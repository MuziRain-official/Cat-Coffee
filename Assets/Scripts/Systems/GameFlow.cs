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

        /// <summary>开始萃取（第 1 步）。设备空闲时有效。</summary>
        public bool Brew()
        {
            if (_order.Step != OrderStep.None) return false;
            _order.StartBrewing();
            _ledger.RecordCost(_config.coffeeCost); // 制作时扣成本
            return true;
        }

        /// <summary>装杯（第 2 步，即时）。仅在萃取完成后有效。</summary>
        public bool Cup()
        {
            if (_order.Step != OrderStep.ReadyToCup) return false;
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

        // —— 推进 ——

        /// <summary>推进一帧。返回本帧新付费的顾客数。</summary>
        public int Tick(float deltaTime)
        {
            if (IsDayOver) return 0;

            _clock.Tick(deltaTime);
            _cat.Tick(deltaTime, _config);
            _order.Tick(deltaTime, _config.brewSeconds);

            int paidThisFrame = 0;
            SpawnIfDue(deltaTime);

            // 推进顾客，处理付费/流失
            foreach (var c in _customers)
            {
                c.Tick(deltaTime);
                if (c.Phase == CustomerPhase.Paid)
                {
                    int price = PriceWithMood();
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

        private int PriceWithMood() =>
            Math.Max(0, (int)Math.Round(_config.coffeePrice * MoodMultiplier));

        private void SpawnIfDue(float deltaTime)
        {
            _spawnTimer -= deltaTime;
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
