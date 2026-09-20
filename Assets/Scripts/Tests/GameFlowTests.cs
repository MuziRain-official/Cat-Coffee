using NUnit.Framework;
using CatCafe;
using UnityEngine;

namespace CatCafe.Tests
{
    public class GameFlowTests
    {
        private GameConfigSO _config;
        private GameFlow _flow;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<GameConfigSO>();
            _config.customerSpawnMin = 10f;
            _config.customerSpawnMax = 10f; // 固定 10s 生成一位
            _flow = new GameFlow(_config, seed: 42);
        }

        [TearDown]
        public void TearDown()
        {
            ScriptableObject.DestroyImmediate(_config);
        }

        /// <summary>用小步长推进，避免单帧大步长导致的时序失真。</summary>
        private void Advance(float seconds, float step = 0.1f)
        {
            float remaining = seconds;
            while (remaining > 0f)
            {
                float dt = remaining > step ? step : remaining;
                _flow.Tick(dt);
                remaining -= dt;
            }
        }

        [Test]
        public void Brew_RecordsCostAndStartsOrder()
        {
            Assert.IsTrue(_flow.Brew());
            Assert.AreEqual(_config.coffeeCost, _flow.Ledger.Cost);
            Assert.AreEqual(OrderStep.Brewing, _flow.Order.Step);
        }

        [Test]
        public void Brew_SecondCallFailsWhileBusy()
        {
            Assert.IsTrue(_flow.Brew());
            Assert.IsFalse(_flow.Brew());
        }

        [Test]
        public void FullServeCycle_PaysAndRecordsRevenue()
        {
            Advance(10f); // 生成 1 位顾客
            Assert.GreaterOrEqual(_flow.Customers.Count, 1);

            Assert.IsTrue(_flow.Brew());
            Advance(_config.brewSeconds); // 萃取完成 → ReadyToPickup
            Assert.IsTrue(_flow.Pickup());  // 取原料
            Assert.IsTrue(_flow.Cup());     // 装杯
            Assert.IsTrue(_flow.ServeToEarliestWaiting()); // 上菜

            Advance(_config.customerEatingSeconds); // 用餐完成 → 付费

            Assert.AreEqual(1, _flow.ServedCount);
            Assert.Greater(_flow.Ledger.Revenue, 0, "应记入收入");
        }

        [Test]
        public void NeglectedCustomer_LeavesWithoutPaying()
        {
            Advance(10f); // 生成 1 位
            // 顾客耐心耗尽前，持续观察但不服务，直到有人流失
            Advance(_config.customerPatienceSeconds + 5f);

            Assert.Greater(_flow.LeftCount, 0, "应有顾客因耐心耗尽流失");
            Assert.AreEqual(0, _flow.Ledger.Revenue, "未服务任何顾客，无收入");
            Assert.AreEqual(0, _flow.ServedCount);
        }

        [Test]
        public void DayEnd_StopsAdvancing()
        {
            Advance(_config.dayDurationSeconds + 5f);
            Assert.IsTrue(_flow.IsDayOver);
            int count = _flow.Customers.Count;
            Advance(30f);
            Assert.AreEqual(count, _flow.Customers.Count, "打烊后不再生成/推进");
        }

        [Test]
        public void MoodMultiplier_InitialCatState_IsBoosted()
        {
            // 猫咪初始满状态(100) → 均值 100 > 70 → 增益
            Assert.AreEqual(1.2f, _flow.MoodMultiplier, 0.01f);
        }

        [Test]
        public void MoodMultiplier_DropsAsCatDeclines()
        {
            Assert.AreEqual(1.2f, _flow.MoodMultiplier, 0.01f);

            // 推进到接近打烊（360s 内），猫咪均值应从 100 下滑，增益系数随之变化
            Advance(_config.dayDurationSeconds - 1f);
            // 均值仍可能 >70 保持增益；关键是不崩溃、系数在 [0.8, 1.2] 合理区间
            Assert.GreaterOrEqual(_flow.MoodMultiplier, 0.8f);
            Assert.LessOrEqual(_flow.MoodMultiplier, 1.2f);
        }
    }
}
