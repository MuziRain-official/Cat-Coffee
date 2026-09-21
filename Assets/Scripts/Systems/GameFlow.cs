using System;
using System.Collections.Generic;

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
        private readonly System.Random _rng;

        private float _spawnTimer;

        public GameFlow(GameConfigSO config, int seed = 0)
        {
            _config = config;
            _clock = new DayClock();
            _cat = new CatState();
            _ledger = new DayLedger();
            _rng = new System.Random(seed);
            _spawnTimer = NextSpawnInterval();
        }

        // —— 只读视图（供 UI / 测试）——
        public DayClock Clock => _clock;
        public CatState Cat => _cat;
        public DayLedger Ledger => _ledger;
        public IReadOnlyList<Customer> Customers => _customers;
        public Order Order => _order;
        public BrewGame BrewGame => _brewGame;
        public bool IsBrewGameActive => _brewGame.IsActive;
        public bool IsDayOver => _clock.IsDayOver(_config.dayDurationSeconds);
        public int ServedCount { get; private set; }
        /// <summary>累计流失顾客数（未付费）。</summary>
        public int LeftCount { get; private set; }

        /// <summary>猫咪心情对消费的增益系数：增益 1.2 / 惩罚 0.8 / 正常 1.0。</summary>
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

        /// <summary>开始萃取（第 1 步）：启动时机条小游戏。</summary>
        public bool Brew()
        {
            if (_order.Step != OrderStep.None) return false;
            _order.StartBrewing();

            // 吧台猫增益：完美区宽度放大
            float bonus = 0f;
            if (_cat.Zone == CatZone.Bar && !_cat.IsCarried)
                bonus = _config.barZonePerfectWidthBonus * _cat.GainStrength(_config);
            _brewGame.PerfectWidthMultiplier = 1f + bonus;
            _brewGame.Start();

            _ledger.RecordCost(_config.coffeeCost); // 制作时扣成本
            return true;
        }

        /// <summary>玩家停指针锁定品质（第 1 步完成）。仅在萃取小游戏进行中有效。</summary>
        public bool StopBrewGame()
        {
            if (!_brewGame.IsActive) return false;
            var quality = _brewGame.Stop(_config);
            _order.CompleteBrew(quality);
            return true;
        }

        /// <summary>回咖啡机取原料。仅在萃取完成后有效。</summary>
        public bool Pickup()
        {
            if (_order.Step != OrderStep.ReadyToPickup) return false;
            _order.Pickup();
            return true;
        }

        /// <summary>装杯（第 2 步，即时）。仅在持原料时有效。</summary>
        public bool Cup()
        {
            if (_order.Step != OrderStep.HoldingIngredients) return false;
            _order.Cup();
            return true;
        }

        /// <summary>上菜给指定顾客。</summary>
        public bool ServeTo(Customer customer)
        {
            if (!_order.Serve(customer, _config.customerEatingSeconds)) return false;
            ServedCount++;
            return true;
        }

        /// <summary>给当前待上菜订单选一个最早等待的顾客。</summary>
        public bool ServeToEarliestWaiting()
        {
            foreach (var c in _customers)
            {
                if (c.Phase == CustomerPhase.Waiting)
                    return ServeTo(c);
            }
            return false;
        }

        public void FeedCat() => _cat.Feed(_config);
        public void CleanCat() => _cat.Clean(_config);
        public void PetCat() => _cat.Pet(_config);

        /// <summary>抱起猫（F 键）。成功返回 true。</summary>
        public bool PickUpCat()
        {
            if (_cat.IsCarried) return false;
            _cat.PickUp();
            return true;
        }

        /// <summary>把猫放到某区域（猫垫）。成功返回 true。</summary>
        public bool PutDownCat(CatZone zone)
        {
            if (!_cat.IsCarried) return false;
            _cat.PutDown(zone);
            return true;
        }

        // —— 推进 ——

        /// <summary>推进一帧。返回本帧新付费的顾客数。</summary>
        public int Tick(float deltaTime)
        {
            if (IsDayOver) return 0;

            _clock.Tick(deltaTime);
            _cat.Tick(deltaTime, _config);
            _brewGame.Tick(deltaTime, _config.swingSpeed); // 推进萃取时机条摆动

            int paidThisFrame = 0;
            SpawnIfDue(deltaTime);

            // 顾客耐心减速：座位区猫增益
            float patienceSlow = 0f;
            if (_cat.Zone == CatZone.Seat && !_cat.IsCarried)
                patienceSlow = _config.seatZonePatienceSlow * _cat.GainStrength(_config);

            // 推进顾客，处理付费/流失
            foreach (var c in _customers)
            {
                c.Tick(deltaTime, patienceSlow);
                if (c.Phase == CustomerPhase.Paid)
                {
                    int price = PriceFor(c.ServedQuality);
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

        /// <summary>售价 = 基础价 × 品质倍率 × 心情系数（向下取整到 0 以上）。</summary>
        private int PriceFor(BrewQuality quality)
        {
            float qMult = quality switch
            {
                BrewQuality.Perfect => _config.perfectPriceMult,
                BrewQuality.Poor => _config.poorPriceMult,
                _ => _config.goodPriceMult,
            };
            return Math.Max(0, (int)Math.Round(_config.coffeePrice * qMult * MoodMultiplier));
        }

        private void SpawnIfDue(float deltaTime)
        {
            // 门口猫增益：生成间隔缩短（加速客流）
            float spawnBoost = 0f;
            if (_cat.Zone == CatZone.Door && !_cat.IsCarried)
                spawnBoost = _config.doorZoneSpawnBoost * _cat.GainStrength(_config);

            _spawnTimer -= deltaTime * (1f + spawnBoost);
            if (_spawnTimer > 0f) return;
            if (_customers.Count >= _config.maxCustomers) return;

            var customer = new Customer();
            customer.PlaceOrder(_config.customerPatienceSeconds);
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
