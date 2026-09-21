using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatCafe
{
    /// <summary>设备读条状态。</summary>
    public enum DeviceStep
    {
        Idle,      // 空闲
        Extracting,// 读条中
        Ready,     // 读条完成，原料在设备，待取
    }

    /// <summary>
    /// 游戏流程编排器（纯逻辑，可单元测试）。
    /// 咖啡机与奶泡机完全独立，可并行读条。
    /// 手持订单只负责"玩家手上拿的东西"。
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

        // 咖啡机独立状态
        private DeviceStep _coffeeStep = DeviceStep.Idle;
        private RecipeType _coffeeRecipe = RecipeType.Latte;
        private BrewQuality _coffeeQuality = BrewQuality.Good;
        private float _coffeeProgress;

        // 奶泡机独立状态
        private DeviceStep _frotherStep = DeviceStep.Idle;
        private BrewQuality _frotherQuality = BrewQuality.Good;
        private float _frotherProgress;

        private float _spawnTimer;
        private int _warmerCursor;
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

        // 设备状态视图（供表现层读条显示）
        public DeviceStep CoffeeStep => _coffeeStep;
        public float CoffeeProgress => _coffeeProgress;
        public DeviceStep FrotherStep => _frotherStep;
        public float FrotherProgress => _frotherProgress;

        public bool IsWarmerSelecting { get; private set; }
        public bool IsCoffeeSelecting { get; private set; }
        public int CoffeeSelectCursor { get; private set; }
        public bool IsShopOpen { get; private set; }
        public int ShopCursor { get; private set; }
        public bool IsDayOver => _clock.IsDayOver(_config.dayDurationSeconds);
        public int ServedCount { get; private set; }
        public int LeftCount { get; private set; }
        public float MoodMultiplier => 1f;

        // —— 咖啡机 ——

        /// <summary>开始做拿铁/猫爪（萃取时机条）。咖啡机空闲时有效。</summary>
        public bool Brew(RecipeType recipe = RecipeType.Latte)
        {
            if (_coffeeStep != DeviceStep.Idle) return false;
            if (recipe != RecipeType.Latte && recipe != RecipeType.CatPaw) return false;

            _coffeeRecipe = recipe;
            _brewGame.Start();
            _ledger.RecordCost(recipe == RecipeType.CatPaw ? _config.catPawCost : _config.latteCost);
            return true;
        }

        /// <summary>停止萃取时机条，锁定品质，进入读条。</summary>
        public bool StopBrewGame()
        {
            if (!_brewGame.IsActive) return false;
            _coffeeQuality = _brewGame.Stop(_config);
            _coffeeStep = DeviceStep.Extracting;
            _coffeeProgress = 0f;
            return true;
        }

        /// <summary>从咖啡机取原料（读条完成后）。</summary>
        public bool PickupCoffee()
        {
            if (_coffeeStep != DeviceStep.Ready) return false;
            _order.HoldIngredients(_coffeeRecipe, _coffeeQuality);
            _coffeeStep = DeviceStep.Idle;
            return true;
        }

        // —— 奶泡机 ——

        /// <summary>开始做卡布奇诺（下落音游）。奶泡机空闲时有效。</summary>
        public bool Froth()
        {
            if (_frotherStep != DeviceStep.Idle) return false;
            _frothGame.Start(_config.frothFallSpeed);
            _ledger.RecordCost(_config.cappuccinoCost);
            return true;
        }

        /// <summary>下落音游按 E 判定。</summary>
        public NoteJudgement TapFroth()
        {
            if (!_frothGame.IsActive) return NoteJudgement.Miss;
            var j = _frothGame.Tap();
            if (!_frothGame.IsActive) // 音游结束 → 进入读条
            {
                _frotherQuality = _frothGame.Result();
                _frotherStep = DeviceStep.Extracting;
                _frotherProgress = 0f;
            }
            return j;
        }

        /// <summary>从奶泡机取原料（读条完成后）。</summary>
        public bool PickupFroth()
        {
            if (_frotherStep != DeviceStep.Ready) return false;
            _order.HoldIngredients(RecipeType.Cappuccino, _frotherQuality);
            _frotherStep = DeviceStep.Idle;
            return true;
        }

        // —— 装杯台 ——

        /// <summary>装杯。猫爪装杯后自动进拉花。</summary>
        public bool Cup()
        {
            if (_order.Step != OrderStep.HoldingIngredients) return false;
            _order.Cup();
            if (_order.Step == OrderStep.LatteArt)
                _latteArtGame.Start(_config.latteArtCellInterval);
            return true;
        }

        /// <summary>停止拉花。</summary>
        public bool StopLatteArt()
        {
            if (!_latteArtGame.IsActive) return false;
            _latteArtGame.Stop();
            _order.CompleteLatteArt();
            return true;
        }

        // —— 上菜/备餐 ——

        public bool ServeTo(Customer customer)
        {
            if (!_order.Serve(customer, _config.customerEatingSeconds)) return false;
            ServedCount++;
            return true;
        }

        public bool QuickServe()
        {
            if (!_progress.HasItem(ItemType.QuickServe)) return false;
            if (_order.Step != OrderStep.ReadyToServe) return false;

            Customer target = null;
            float minPatience = float.MaxValue;
            foreach (var c in _customers)
            {
                if (c.Phase != CustomerPhase.Waiting) continue;
                if (c.OrderedRecipe != _order.Recipe) continue;
                if (c.RemainingPatience < minPatience)
                {
                    minPatience = c.RemainingPatience;
                    target = c;
                }
            }
            if (target == null) return false;
            return ServeTo(target);
        }

        public bool StoreToWarmer()
        {
            if (_order.Step != OrderStep.ReadyToServe) return false;
            if (_warmer.IsFull) return false;
            _warmer.Store(_order.Recipe, _order.Quality);
            _order.ResetToNone();
            return true;
        }

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

        public void MoveWarmerCursor(int delta)
        {
            if (_warmer.Count == 0) { _warmerCursor = 0; return; }
            _warmerCursor = (_warmerCursor + delta + _warmer.Count) % _warmer.Count;
        }

        public void OpenWarmerSelect() { if (_warmer.Count > 0) IsWarmerSelecting = true; }
        public void CloseWarmerSelect() => IsWarmerSelecting = false;
        public bool ConfirmWarmerSelect()
        {
            if (!IsWarmerSelecting) return false;
            bool ok = TakeFromWarmerAt(_warmerCursor);
            IsWarmerSelecting = false;
            return ok;
        }

        public void OpenCoffeeSelect()
        {
            if (_coffeeStep != DeviceStep.Idle) return;
            IsCoffeeSelecting = true;
            CoffeeSelectCursor = 0;
        }
        public void CloseCoffeeSelect() => IsCoffeeSelecting = false;
        public void MoveCoffeeCursor(int delta) => CoffeeSelectCursor = (CoffeeSelectCursor + delta + 2) % 2;
        public bool ConfirmCoffeeSelect()
        {
            if (!IsCoffeeSelecting) return false;
            var recipe = CoffeeSelectCursor == 0 ? RecipeType.Latte : RecipeType.CatPaw;
            IsCoffeeSelecting = false;
            return Brew(recipe);
        }

        public void OpenShop() { IsShopOpen = true; ShopCursor = 0; }
        public void CloseShop() => IsShopOpen = false;
        public void MoveShopCursor(int delta) => ShopCursor = (ShopCursor + delta + ItemDef.All.Length) % ItemDef.All.Length;
        public ItemType CurrentShopItem() => ItemDef.All[ShopCursor];

        public void FeedCat() { }
        public void CleanCat() { }
        public void PetCat() { }

        public bool PickUpCat() { if (_cat.IsCarried) return false; _cat.PickUp(); return true; }
        public bool PutDownCat(CatZone zone) { if (!_cat.IsCarried) return false; _cat.PutDown(zone); return true; }

        // —— 推进 ——

        public int Tick(float deltaTime)
        {
            if (IsDayOver) return 0;

            _clock.Tick(deltaTime);
            _brewGame.Tick(deltaTime, _config.swingSpeed);
            _frothGame.Tick(deltaTime);
            _latteArtGame.Tick(deltaTime);

            // 吧台猫增益
            float freshSlow = (_cat.Zone == CatZone.Bar && !_cat.IsCarried) ? _config.barZoneFreshnessSlow : 0f;
            _warmer.Tick(deltaTime * (1f - freshSlow), _config.freshDurationSeconds);

            float extractBoost = (_cat.Zone == CatZone.Bar && !_cat.IsCarried) ? _config.barZoneSpeedBoost : 0f;

            // 咖啡机读条（独立）
            if (_coffeeStep == DeviceStep.Extracting)
            {
                if (HasNoWaitItem)
                    _coffeeProgress = 1f;
                else
                    _coffeeProgress += (deltaTime * (1f + extractBoost)) / _config.extractSeconds;
                if (_coffeeProgress >= 1f) { _coffeeProgress = 1f; _coffeeStep = DeviceStep.Ready; }
            }

            // 奶泡机读条（独立）
            if (_frotherStep == DeviceStep.Extracting)
            {
                if (HasNoWaitItem)
                    _frotherProgress = 1f;
                else
                    _frotherProgress += (deltaTime * (1f + extractBoost)) / _config.extractSeconds;
                if (_frotherProgress >= 1f) { _frotherProgress = 1f; _frotherStep = DeviceStep.Ready; }
            }

            int paidThisFrame = 0;
            SpawnIfDue(deltaTime);

            float patienceSlow = (_cat.Zone == CatZone.Seat && !_cat.IsCarried) ? _config.seatZoneStayLonger : 0f;

            foreach (var c in _customers)
            {
                c.Tick(deltaTime, patienceSlow);
                if (c.Phase == CustomerPhase.Paid)
                {
                    _ledger.RecordRevenue(PriceFor(c.OrderedRecipe, c.ServedQuality, c.ServedFreshness));
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
            return Math.Max(0, (int)Math.Round(basePrice * qMult * MoodMultiplier * Mathf.Clamp01(freshness)));
        }

        private void SpawnIfDue(float deltaTime)
        {
            float spawnBoost = (_cat.Zone == CatZone.Door && !_cat.IsCarried) ? _config.doorZoneSpawnBoost : 0f;
            _spawnTimer -= deltaTime * (1f + spawnBoost);
            if (_spawnTimer > 0f) return;
            if (_customers.Count >= _config.maxCustomers) return;

            var customer = new Customer();
            var recipes = Recipe.All;
            customer.PlaceOrder(_config.customerPatienceSeconds, recipes[_rng.Next(recipes.Length)]);
            _customers.Add(customer);
            _spawnTimer = NextSpawnInterval();
        }

        private float NextSpawnInterval()
        {
            return _config.customerSpawnMin + (float)_rng.NextDouble() * (_config.customerSpawnMax - _config.customerSpawnMin);
        }

        private void RemoveFinished()
        {
            _customers.RemoveAll(c => c.HasPaid || c.HasLeft);
        }
    }
}
