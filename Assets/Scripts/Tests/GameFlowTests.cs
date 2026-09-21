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
            Assert.IsTrue(_flow.Brew(RecipeType.Latte));
            Assert.AreEqual(_config.latteCost, _flow.Ledger.Cost);
            Assert.AreEqual(OrderStep.Brewing, _flow.Order.Step);
            Assert.AreEqual(RecipeType.Latte, _flow.Order.Recipe);
            Assert.IsTrue(_flow.IsBrewGameActive);
        }

        [Test]
        public void Brew_CatPawCostsMore()
        {
            Assert.IsTrue(_flow.Brew(RecipeType.CatPaw));
            Assert.AreEqual(_config.catPawCost, _flow.Ledger.Cost);
            Assert.AreEqual(RecipeType.CatPaw, _flow.Order.Recipe);
        }

        [Test]
        public void Brew_SecondCallFailsWhileBusy()
        {
            Assert.IsTrue(_flow.Brew(RecipeType.Latte));
            Assert.IsFalse(_flow.Brew(RecipeType.Latte));
        }

        [Test]
        public void StopBrewGame_LocksQualityAndStartsExtracting()
        {
            Assert.IsTrue(_flow.Brew(RecipeType.Latte));
            Advance(0.5f);
            Assert.IsTrue(_flow.StopBrewGame());
            Assert.IsFalse(_flow.IsBrewGameActive);
            Assert.AreEqual(OrderStep.Extracting, _flow.Order.Step);
        }

        [Test]
        public void FullLatteServeCycle_Pays()
        {
            Advance(10f);
            Assert.GreaterOrEqual(_flow.Customers.Count, 1);
            var customer = _flow.Customers[0];
            customer.PlaceOrder(45f, RecipeType.Latte); // 强制点拿铁

            Assert.IsTrue(_flow.Brew(RecipeType.Latte));
            Assert.IsTrue(_flow.StopBrewGame());
            Advance(_config.extractSeconds);
            Assert.IsTrue(_flow.Pickup());
            Assert.IsTrue(_flow.Cup());
            Assert.AreEqual(OrderStep.ReadyToServe, _flow.Order.Step);
            Assert.IsTrue(_flow.ServeTo(customer));

            Advance(_config.customerEatingSeconds);
            Assert.AreEqual(1, _flow.ServedCount);
            Assert.Greater(_flow.Ledger.Revenue, 0);
        }

        [Test]
        public void CatPaw_RequiresLatteArtBeforeServe()
        {
            var customer = new Customer();
            customer.PlaceOrder(45f, RecipeType.CatPaw);

            Assert.IsTrue(_flow.Brew(RecipeType.CatPaw));
            Assert.IsTrue(_flow.StopBrewGame());
            Advance(_config.extractSeconds);
            Assert.IsTrue(_flow.Pickup());
            Assert.IsTrue(_flow.Cup());
            // 猫爪装杯后直接进入拉花游戏
            Assert.AreEqual(OrderStep.LatteArt, _flow.Order.Step);
            Assert.IsTrue(_flow.IsLatteArtGameActive);

            Assert.IsTrue(_flow.StopLatteArt());
            Assert.AreEqual(OrderStep.ReadyToServe, _flow.Order.Step);
        }

        [Test]
        public void Serve_WrongRecipe_Fails()
        {
            var customer = new Customer();
            customer.PlaceOrder(45f, RecipeType.CatPaw); // 顾客要猫爪

            _flow.Brew(RecipeType.Latte); // 但做的是拿铁
            _flow.StopBrewGame();
            Advance(_config.extractSeconds);
            _flow.Pickup();
            _flow.Cup();
            // 上错菜应失败
            Assert.IsFalse(_flow.ServeTo(customer));
            Assert.AreEqual(OrderStep.ReadyToServe, _flow.Order.Step); // 订单保留
        }

        [Test]
        public void Froth_RecordsCostAndStartsFrothGame()
        {
            Assert.IsTrue(_flow.Froth());
            Assert.AreEqual(_config.cappuccinoCost, _flow.Ledger.Cost);
            Assert.AreEqual(RecipeType.Cappuccino, _flow.Order.Recipe);
            Assert.IsTrue(_flow.IsFrothGameActive);
        }

        [Test]
        public void NeglectedCustomer_LeavesWithoutPaying()
        {
            Advance(10f);
            Advance(_config.customerPatienceSeconds + 5f);
            Assert.Greater(_flow.LeftCount, 0);
            Assert.AreEqual(0, _flow.Ledger.Revenue);
            Assert.AreEqual(0, _flow.ServedCount);
        }

        [Test]
        public void DayEnd_StopsAdvancing()
        {
            Advance(_config.dayDurationSeconds + 5f);
            Assert.IsTrue(_flow.IsDayOver);
            int count = _flow.Customers.Count;
            Advance(30f);
            Assert.AreEqual(count, _flow.Customers.Count);
        }

        [Test]
        public void MoodMultiplier_IsAlwaysOne_NoStats()
        {
            // 三态已移除，消费系数固定 1.0
            Assert.AreEqual(1f, _flow.MoodMultiplier, 0.01f);
        }
    }
}
