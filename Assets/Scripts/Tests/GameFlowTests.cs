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
            _config.customerSpawnMax = 10f;
            _flow = new GameFlow(_config, null, seed: 42);
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
        public void Brew_StartsCoffeeMachine()
        {
            Assert.IsTrue(_flow.Brew(RecipeType.Latte));
            Assert.AreEqual(_config.latteCost, _flow.Ledger.Cost);
            Assert.IsTrue(_flow.IsBrewGameActive);
            Assert.AreEqual(DeviceStep.Idle, _flow.CoffeeStep);
        }

        [Test]
        public void StopBrewGame_EntersExtracting()
        {
            _flow.Brew(RecipeType.Latte);
            Assert.IsTrue(_flow.StopBrewGame());
            Assert.AreEqual(DeviceStep.Extracting, _flow.CoffeeStep);
        }

        [Test]
        public void CoffeeReadiesAfterExtractDuration()
        {
            _flow.Brew(RecipeType.Latte);
            _flow.StopBrewGame();
            Advance(_config.extractSeconds);
            Assert.AreEqual(DeviceStep.Ready, _flow.CoffeeStep);
        }

        [Test]
        public void PickupCoffee_AfterReady()
        {
            _flow.Brew(RecipeType.Latte);
            _flow.StopBrewGame();
            Advance(_config.extractSeconds);
            Assert.IsTrue(_flow.PickupCoffee());
            Assert.AreEqual(OrderStep.HoldingIngredients, _flow.Order.Step);
            Assert.AreEqual(RecipeType.Latte, _flow.Order.Recipe);
        }

        [Test]
        public void Froth_StartsFrotherIndependently()
        {
            Assert.IsTrue(_flow.Froth());
            Assert.AreEqual(_config.cappuccinoCost, _flow.Ledger.Cost);
            Assert.IsTrue(_flow.IsFrothGameActive);
        }

        [Test]
        public void CoffeeAndFrother_CanExtractInParallel()
        {
            // 咖啡机开始萃取
            _flow.Brew(RecipeType.Latte);
            _flow.StopBrewGame(); // 咖啡机进入读条
            Assert.AreEqual(DeviceStep.Extracting, _flow.CoffeeStep);

            // 奶泡机同时也能开始（并行）
            Assert.IsTrue(_flow.Froth());
            Assert.IsTrue(_flow.IsFrothGameActive);

            // 推进让咖啡机读条完成
            Advance(_config.extractSeconds);
            Assert.AreEqual(DeviceStep.Ready, _flow.CoffeeStep);
            // 奶泡机不受影响（音游还活跃或已进入读条）
        }

        [Test]
        public void CatPawFullFlow()
        {
            var customer = new Customer();
            customer.PlaceOrder(60f, RecipeType.CatPaw);

            _flow.Brew(RecipeType.CatPaw);
            _flow.StopBrewGame();
            Advance(_config.extractSeconds);
            _flow.PickupCoffee();
            Assert.AreEqual(OrderStep.HoldingIngredients, _flow.Order.Step);

            _flow.Cup();
            Assert.AreEqual(OrderStep.LatteArt, _flow.Order.Step);
            Assert.IsTrue(_flow.IsLatteArtGameActive);

            _flow.StopLatteArt();
            Assert.AreEqual(OrderStep.ReadyToServe, _flow.Order.Step);

            Assert.IsTrue(_flow.ServeTo(customer));
        }

        [Test]
        public void Serve_WrongRecipe_Fails()
        {
            var customer = new Customer();
            customer.PlaceOrder(60f, RecipeType.CatPaw);

            _flow.Brew(RecipeType.Latte);
            _flow.StopBrewGame();
            Advance(_config.extractSeconds);
            _flow.PickupCoffee();
            _flow.Cup(); // 拿铁装杯即 ReadyToServe

            Assert.IsFalse(_flow.ServeTo(customer));
            Assert.AreEqual(OrderStep.ReadyToServe, _flow.Order.Step);
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
        }
    }
}
