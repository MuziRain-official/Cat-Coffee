using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 游戏流程编排器（纯逻辑，可单元测试）——整个营业日的"大脑"。
    /// 连接：时钟 / 猫咪 / 顾客 / 订单 / 账本，暴露玩家操作接口。
    /// 由 GameManager 每帧调用 Tick 推进。
    /// </summary>
    public class GameFlow
    {
        private readonly GameConfigSO _config;
        private readonly DayClock _clock;
        private readonly CatState _cat;
        private readonly DayLedger _ledger;
        private readonly List<Customer> _customers = new List<Customer>();
        private readonly Order _order = new Order();
        private readonly BrewGame _brewGame = new BrewGame();
        private readonly FrothGame _frothGame = new FrothGame();
        private readonly LatteArtGame _latteArtGame = new LatteArtGame();
        private readonly Warmer _warmer;
        private readonly System.Random _rng;

        private float _spawnTimer;
        private int _warmerCursor; // 保温台选杯光标
        private readonly Progress _progress;

        public GameFlow(GameConfigSO config, Progress progress = null, int seed = 0)
        {
            _config = config;
            _progress = progress ?? new Progress();
            _clock = new DayClock();
            _cat = new CatState();
            _ledger = new DayLedger();
            _warmer = new Warmer(config.warmerCapacity);
            _rng = new System.Random(seed);
            _spawnTimer = NextSpawnInterval();
        }

        // —— 只读视图 ——
        public Progress Progress => _progress;
        public bool HasNoWaitItem => _progress.HasItem(ItemType.NoWait);
        public bool HasQuickServeItem => _progress.HasItem(ItemType.QuickServe);
        public DayClock Clock => _clock;
        public CatState Cat => _cat;
        public DayLedger Ledger => _ledger;
        public IReadOnlyList<Customer> Customers => _customers;
        public Order Order => _order;
        public BrewGame BrewGame => _brewGame;
        public FrothGame FrothGame => _frothGame;
        public LatteArtGame LatteArtGame => _latteArtGame;
        public Warmer Warmer => _warmer;
        public bool IsBrewGameActive => _brewGame.IsActive;
        public bool IsFrothGameActive => _frothGame.IsActive;
        public bool IsLatteArtGameActive => _latteArtGame.IsActive;
        public int WarmerCursor => _warmerCursor;
        /// <summary>是否处于保温台选择模式（打开选择栏）。</summary>
        public bool IsWarmerSelecting { get; private set; }
        /// <summary>是否处于咖啡机选菜品模式。</summary>
        public bool IsCoffeeSelecting { get; private set; }
        /// <summary>咖啡机选菜品的当前光标（0=拿铁, 1=猫爪咖啡）。</summary>
        public int CoffeeSelectCursor { get; private set; }
        /// <summary>是否处于商店浏览模式。</summary>
        public bool IsShopOpen { get; private set; }
        /// <summary>商店当前光标（道具索引）。</summary>
        public int ShopCursor { get; private set; }
        public bool IsDayOver => _clock.IsDayOver(_config.dayDurationSeconds);
        public int ServedCount { get; private set; }
        public int LeftCount { get; private set; }

        /// <summary>猫咪心情对消费的增益系数（现固定 1.0，无三态）。</summary>
        public float MoodMultiplier => 1f;

        // —— 玩家操作 ——

        /// <summary>开始做拿铁或猫爪咖啡（两者都需要萃取）。空闲时有效。</summary>
        public bool Brew(RecipeType recipe = RecipeType.Latte)
        {
            if (_order.Step != OrderStep.None) return false;
            if (recipe != RecipeType.Latte && recipe != RecipeType.CatPaw) return false;

            _order.Start(recipe);
            _order.StartBrewing();

            // 吧台猫增益：完美区宽度放大（固定，无三态强度）
            float bonus = 0f;
            if (_cat.Zone == CatZone.Bar && !_cat.IsCarried)
                bonus = 0.3f;
            _brewGame.PerfectWidthMultiplier = 1f + bonus;
            _brewGame.Start();

            _ledger.RecordCost(recipe == RecipeType.CatPaw ? _config.catPawCost : _config.latteCost);
            return true;
        }

        /// <summary>停止萃取时机条，锁定品质。</summary>
        public bool StopBrewGame()
        {
            if (!_brewGame.IsActive) return false;
            var q = _brewGame.Stop(_config);
            _order.CompleteBrew(q);
            return true;
        }

        /// <summary>开始做卡布奇诺（打奶泡）。空闲时有效。</summary>
        public bool Froth()
        {
            if (_order.Step != OrderStep.None) return false;
            _order.Start(RecipeType.Cappuccino);
            _order.StartFrothing();
            _frothGame.Start(_config.frothFallSpeed);
            _ledger.RecordCost(_config.cappuccinoCost);
            return true;
        }

        /// <summary>下落音游按 E 判定。返回判定结果。</summary>
        public NoteJudgement TapFroth()
        {
            if (!_frothGame.IsActive) return NoteJudgement.Miss;
            var j = _frothGame.Tap();
            if (!_frothGame.IsActive) // 游戏刚结束
                _order.CompleteFroth(_frothGame.Result());
            return j;
        }

        /// <summary>回设备取原料。</summary>
        public bool Pickup()
        {
            if (_order.Step != OrderStep.ReadyToPickup) return false;
            _order.Pickup();
            return true;
        }

        /// <summary>装杯。猫爪咖啡装杯后立即启动拉花游戏。</summary>
        public bool Cup()
        {
            if (_order.Step != OrderStep.HoldingIngredients) return false;
            _order.Cup();
            // 猫爪咖啡装杯后直接进入拉花游戏（Order.Cup 已置为 LatteArt 状态）
            if (_order.Step == OrderStep.LatteArt)
                _latteArtGame.Start(_config.latteArtCellInterval);
            return true;
        }

        /// <summary>开始拉花（猫爪装杯后）。</summary>
        public bool StartLatteArt()
        {
            if (_order.Step != OrderStep.ReadyToLatteArt) return false;
            _order.StartLatteArt();
            _latteArtGame.Start(_config.latteArtCellInterval);
            return true;
        }

        /// <summary>停止拉花（按 E 定品质）。</summary>
        public bool StopLatteArt()
        {
            if (!_latteArtGame.IsActive) return false;
            _latteArtGame.Stop();
            _order.CompleteLatteArt();
            return true;
        }

        /// <summary>上菜给指定顾客（菜品需匹配）。</summary>
        public bool ServeTo(Customer customer)
        {
            if (!_order.Serve(customer, _config.customerEatingSeconds)) return false;
            ServedCount++;
            return true;
        }

        /// <summary>
        /// 道具1「自动送餐」：手上持菜品时，自动送到最急需且菜品匹配的顾客。
        /// 按耐心剩余升序（耐心最低最优先）。
        /// </summary>
        public bool QuickServe()
        {
            if (!_progress.HasItem(ItemType.QuickServe)) return false;
            if (_order.Step != OrderStep.ReadyToServe) return false;

            Customer target = null;
            float minPatience = float.MaxValue;
            foreach (var c in _customers)
            {
                if (c.Phase != CustomerPhase.Waiting) continue;
                if (c.OrderedRecipe != _order.Recipe) continue; // 需求不匹配跳过
                if (c.RemainingPatience < minPatience)
                {
                    minPatience = c.RemainingPatience;
                    target = c;
                }
            }
            if (target == null) return false; // 没有对应需求的顾客

            return ServeTo(target);
        }

        /// <summary>把手里成品放入保温台。</summary>
        public bool StoreToWarmer()
        {
            if (_order.Step != OrderStep.ReadyToServe) return false;
            if (_warmer.IsFull) return false;
            _warmer.Store(_order.Recipe, _order.Quality);
            _order.ResetToNone();
            return true;
        }

        /// <summary>从保温台按光标索引取一杯。</summary>
        public bool TakeFromWarmerAt(int index)
        {
            var cup = _warmer.TakeAt(index);
            if (cup == null) return false;
            if (!_order.TakeFromWarmer(cup.Recipe, cup.Quality, cup.Freshness))
            {
                _warmer.ReturnCup(cup);
                return false;
            }
            return true;
        }

        /// <summary>保温台选杯光标左右移动。</summary>
        public void MoveWarmerCursor(int delta)
        {
            if (_warmer.Count == 0) { _warmerCursor = 0; return; }
            _warmerCursor = (_warmerCursor + delta + _warmer.Count) % _warmer.Count;
        }

        /// <summary>打开保温台选择栏（进入选择模式）。</summary>
        public void OpenWarmerSelect()
        {
            if (_warmer.Count == 0) return;
            IsWarmerSelecting = true;
        }

        /// <summary>关闭选择栏（不取）。</summary>
        public void CloseWarmerSelect() => IsWarmerSelecting = false;

        /// <summary>选择模式下确认拿起光标杯。</summary>
        public bool ConfirmWarmerSelect()
        {
            if (!IsWarmerSelecting) return false;
            bool ok = TakeFromWarmerAt(_warmerCursor);
            IsWarmerSelecting = false;
            return ok;
        }

        /// <summary>打开咖啡机选菜品模式（拿铁/猫爪）。</summary>
        public void OpenCoffeeSelect()
        {
            if (_order.Step != OrderStep.None) return;
            IsCoffeeSelecting = true;
            CoffeeSelectCursor = 0;
        }

        /// <summary>关闭咖啡机选菜品。</summary>
        public void CloseCoffeeSelect() => IsCoffeeSelecting = false;

        /// <summary>咖啡机选菜品光标左右移动。</summary>
        public void MoveCoffeeCursor(int delta)
        {
            CoffeeSelectCursor = (CoffeeSelectCursor + delta + 2) % 2; // 0/1 循环
        }

        /// <summary>确认选菜品并开始制作。</summary>
        public bool ConfirmCoffeeSelect()
        {
            if (!IsCoffeeSelecting) return false;
            var recipe = CoffeeSelectCursor == 0 ? RecipeType.Latte : RecipeType.CatPaw;
            IsCoffeeSelecting = false;
            return Brew(recipe);
        }

        /// <summary>打开商店。</summary>
        public void OpenShop()
        {
            IsShopOpen = true;
            ShopCursor = 0;
        }

        /// <summary>关闭商店。</summary>
        public void CloseShop() => IsShopOpen = false;

        /// <summary>商店光标移动。</summary>
        public void MoveShopCursor(int delta)
        {
            ShopCursor = (ShopCursor + delta + ItemDef.All.Length) % ItemDef.All.Length;
        }

        /// <summary>商店确认购买当前道具（由 GameManager 执行购买+存档）。</summary>
        public ItemType CurrentShopItem() => ItemDef.All[ShopCursor];

        public void FeedCat() { }
        public void CleanCat() { }
        public void PetCat() { }

        public bool PickUpCat()
        {
            if (_cat.IsCarried) return false;
            _cat.PickUp();
            return true;
        }

        public bool PutDownCat(CatZone zone)
        {
            if (!_cat.IsCarried) return false;
            _cat.PutDown(zone);
            return true;
        }

        // —— 推进 ——

        public int Tick(float deltaTime)
        {
            if (IsDayOver) return 0;

            _clock.Tick(deltaTime);
            _brewGame.Tick(deltaTime, _config.swingSpeed);
            _frothGame.Tick(deltaTime);
            _latteArtGame.Tick(deltaTime);

            // 吧台猫增益：保鲜度下降减速（新鲜度更持久）
            float freshSlow = (_cat.Zone == CatZone.Bar && !_cat.IsCarried) ? _config.barZoneFreshnessSlow : 0f;
            _warmer.Tick(deltaTime * (1f - freshSlow), _config.freshDurationSeconds);

            // 萃取读条：道具2免读条则直接完成，否则按吧台猫增益加速
            float extractBoost = (_cat.Zone == CatZone.Bar && !_cat.IsCarried) ? _config.barZoneSpeedBoost : 0f;
            if (_order.Step == OrderStep.Extracting)
            {
                if (HasNoWaitItem)
                    _order.TickExtract(1f, 0f); // 免读条：立即完成
                else
                    _order.TickExtract(deltaTime * (1f + extractBoost), _config.extractSeconds);
            }

            // 打奶泡游戏自然结束 → 结算订单；有读条需求（非免读条）时进入读条状态
            if (_order.Step == OrderStep.Frothing && !_frothGame.IsActive)
                _order.CompleteFroth(_frothGame.Result());

            int paidThisFrame = 0;
            SpawnIfDue(deltaTime);

            // 餐桌旁猫增益：顾客停留更久（耐心下降减速）
            float patienceSlow = 0f;
            if (_cat.Zone == CatZone.Seat && !_cat.IsCarried)
                patienceSlow = _config.seatZoneStayLonger;

            foreach (var c in _customers)
            {
                c.Tick(deltaTime, patienceSlow);
                if (c.Phase == CustomerPhase.Paid)
                {
                    int price = PriceFor(c.OrderedRecipe, c.ServedQuality, c.ServedFreshness);
                    _ledger.RecordRevenue(price);
                    paidThisFrame++;
                }
                else if (c.Phase == CustomerPhase.Left)
                {
                    LeftCount++;
                }
            }
            RemoveFinished();

            return paidThisFrame;
        }

        /// <summary>售价 = 菜品基础价 × 品质倍率 × 心情系数 × 新鲜度。</summary>
        private int PriceFor(RecipeType recipe, BrewQuality quality, float freshness = 1f)
        {
            int basePrice = recipe switch
            {
                RecipeType.Latte => _config.lattePrice,
                RecipeType.Cappuccino => _config.cappuccinoPrice,
                RecipeType.CatPaw => _config.catPawPrice,
                _ => _config.lattePrice,
            };
            float qMult = quality switch
            {
                BrewQuality.Perfect => _config.perfectPriceMult,
                BrewQuality.Poor => _config.poorPriceMult,
                _ => _config.goodPriceMult,
            };
            float fresh = Mathf.Clamp01(freshness);
            return Math.Max(0, (int)Math.Round(basePrice * qMult * MoodMultiplier * fresh));
        }

        private void SpawnIfDue(float deltaTime)
        {
            float spawnBoost = 0f;
            if (_cat.Zone == CatZone.Door && !_cat.IsCarried)
                spawnBoost = _config.doorZoneSpawnBoost;

            _spawnTimer -= deltaTime * (1f + spawnBoost);
            if (_spawnTimer > 0f) return;
            if (_customers.Count >= _config.maxCustomers) return;

            // 随机点三种菜品之一
            var customer = new Customer();
            var recipes = Recipe.All;
            var recipe = recipes[_rng.Next(recipes.Length)];
            customer.PlaceOrder(_config.customerPatienceSeconds, recipe);
            _customers.Add(customer);
            _spawnTimer = NextSpawnInterval();
        }

        private float NextSpawnInterval()
        {
            float min = _config.customerSpawnMin;
            float max = _config.customerSpawnMax;
            return min + (float)_rng.NextDouble() * (max - min);
        }

        private void RemoveFinished()
        {
            _customers.RemoveAll(c => c.HasPaid || c.HasLeft);
        }
    }
}
