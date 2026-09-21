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

        public GameFlow(GameConfigSO config, int seed = 0)
        {
            _config = config;
            _clock = new DayClock();
            _cat = new CatState();
            _ledger = new DayLedger();
            _warmer = new Warmer(config.warmerCapacity);
            _rng = new System.Random(seed);
            _spawnTimer = NextSpawnInterval();
        }

        // —— 只读视图 ——
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
        public bool IsDayOver => _clock.IsDayOver(_config.dayDurationSeconds);
        public int ServedCount { get; private set; }
        public int LeftCount { get; private set; }

        /// <summary>猫咪心情对消费的增益系数。</summary>
        public float MoodMultiplier
        {
            get
            {
                if (_cat.IsBoosted(_config)) return 1.2f;
                if (_cat.IsPenalized(_config)) return 0.8f;
                return 1f;
            }
        }

        // —— 玩家操作 ——

        /// <summary>开始做拿铁或猫爪咖啡（两者都需要萃取）。空闲时有效。</summary>
        public bool Brew(RecipeType recipe = RecipeType.Latte)
        {
            if (_order.Step != OrderStep.None) return false;
            if (recipe != RecipeType.Latte && recipe != RecipeType.CatPaw) return false;

            _order.Start(recipe);
            _order.StartBrewing();

            float bonus = 0f;
            if (_cat.Zone == CatZone.Bar && !_cat.IsCarried)
                bonus = _config.barZonePerfectWidthBonus * _cat.GainStrength(_config);
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
            _frothGame.Start(_config.frothBeatInterval);
            _ledger.RecordCost(_config.cappuccinoCost);
            return true;
        }

        /// <summary>打奶泡连击拍。返回是否命中。</summary>
        public bool TapFroth()
        {
            if (!_frothGame.IsActive) return false;
            bool hit = _frothGame.Tap();
            if (!_frothGame.IsActive) // 游戏刚结束
                _order.CompleteFroth(_frothGame.Result());
            return hit;
        }

        /// <summary>回设备取原料。</summary>
        public bool Pickup()
        {
            if (_order.Step != OrderStep.ReadyToPickup) return false;
            _order.Pickup();
            return true;
        }

        /// <summary>装杯。</summary>
        public bool Cup()
        {
            if (_order.Step != OrderStep.HoldingIngredients) return false;
            _order.Cup();
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

        public void FeedCat() => _cat.Feed(_config);
        public void CleanCat() => _cat.Clean(_config);
        public void PetCat() => _cat.Pet(_config);

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
            _cat.Tick(deltaTime, _config);
            _brewGame.Tick(deltaTime, _config.swingSpeed);
            _frothGame.Tick(deltaTime);
            _latteArtGame.Tick(deltaTime);
            _order.TickExtract(deltaTime, _config.extractSeconds);
            _warmer.Tick(deltaTime, _config.freshDurationSeconds);

            int paidThisFrame = 0;
            SpawnIfDue(deltaTime);

            float patienceSlow = 0f;
            if (_cat.Zone == CatZone.Seat && !_cat.IsCarried)
                patienceSlow = _config.seatZonePatienceSlow * _cat.GainStrength(_config);

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
                spawnBoost = _config.doorZoneSpawnBoost * _cat.GainStrength(_config);

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
