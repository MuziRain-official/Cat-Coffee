using NUnit.Framework;
using CatCafe;

namespace CatCafe.Tests
{
    public class OrderTests
    {
        private const float ExtractSeconds = 5f;

        [Test]
        public void StartBrewing_SetsBrewingStep()
        {
            var o = new Order();
            o.StartBrewing();
            Assert.AreEqual(OrderStep.Brewing, o.Step);
        }

        [Test]
        public void CompleteBrew_OnlyWorksWhileBrewing()
        {
            var o = new Order();
            o.CompleteBrew(BrewQuality.Perfect); // 未萃取，忽略
            Assert.AreEqual(OrderStep.None, o.Step);

            o.StartBrewing();
            o.CompleteBrew(BrewQuality.Perfect);
            Assert.AreEqual(OrderStep.Extracting, o.Step); // 小游戏完 → 萃取读条
            Assert.AreEqual(BrewQuality.Perfect, o.Quality);
        }

        [Test]
        public void TickExtract_MovesToReadyToPickupAfterFullDuration()
        {
            var o = new Order();
            o.StartBrewing();
            o.CompleteBrew(BrewQuality.Good);

            o.TickExtract(2.5f, ExtractSeconds);
            Assert.AreEqual(OrderStep.Extracting, o.Step);
            Assert.AreEqual(0.5f, o.ExtractProgress, 0.01f);

            o.TickExtract(2.5f, ExtractSeconds);
            Assert.AreEqual(OrderStep.ReadyToPickup, o.Step);
            Assert.AreEqual(1f, o.ExtractProgress);
        }

        [Test]
        public void Pickup_OnlyWorksAfterExtractDone()
        {
            var o = new Order();
            o.StartBrewing();
            o.Pickup(); // 未萃取完，忽略
            Assert.AreEqual(OrderStep.Brewing, o.Step);

            o.CompleteBrew(BrewQuality.Good);
            o.Pickup(); // 萃取读条中，忽略
            Assert.AreEqual(OrderStep.Extracting, o.Step);

            o.TickExtract(ExtractSeconds, ExtractSeconds);
            o.Pickup();
            Assert.AreEqual(OrderStep.HoldingIngredients, o.Step);
        }

        [Test]
        public void Cup_OnlyWorksWhenHoldingIngredients()
        {
            var o = new Order();
            o.Cup(); // 未持原料，忽略
            Assert.AreEqual(OrderStep.None, o.Step);

            o.StartBrewing();
            o.CompleteBrew(BrewQuality.Good);
            o.TickExtract(ExtractSeconds, ExtractSeconds);
            o.Cup(); // 没取原料，忽略
            Assert.AreEqual(OrderStep.ReadyToPickup, o.Step);

            o.Pickup();
            o.Cup();
            Assert.AreEqual(OrderStep.ReadyToServe, o.Step);
        }

        [Test]
        public void Serve_RequiresReadyToServeAndWaitingCustomer()
        {
            var o = new Order();
            var customer = new Customer();
            customer.PlaceOrder(45f);

            o.StartBrewing();
            o.CompleteBrew(BrewQuality.Perfect);
            o.TickExtract(ExtractSeconds, ExtractSeconds);
            o.Pickup();
            o.Cup();

            bool served = o.Serve(customer, 12f);
            Assert.IsTrue(served);
            Assert.AreEqual(CustomerPhase.Eating, customer.Phase);
            Assert.AreEqual(BrewQuality.Perfect, customer.ServedQuality);
            Assert.AreEqual(OrderStep.None, o.Step); // 订单复位
        }

        [Test]
        public void Serve_FailsIfNotReady()
        {
            var o = new Order();
            var customer = new Customer();
            customer.PlaceOrder(45f);

            bool served = o.Serve(customer, 12f); // 未制作
            Assert.IsFalse(served);
            Assert.AreEqual(CustomerPhase.Waiting, customer.Phase);
        }

        [Test]
        public void Serve_FailsIfCustomerNotWaiting()
        {
            var o = new Order();
            var customer = new Customer(); // 未点单

            o.StartBrewing();
            o.CompleteBrew(BrewQuality.Good);
            o.TickExtract(ExtractSeconds, ExtractSeconds);
            o.Pickup();
            o.Cup();

            bool served = o.Serve(customer, 12f);
            Assert.IsFalse(served);
            Assert.AreEqual(OrderStep.ReadyToServe, o.Step); // 未消费，保持待上菜
        }

        [Test]
        public void FullFlow_FollowsFiveStepSequence()
        {
            var o = new Order();
            Assert.AreEqual(OrderStep.None, o.Step);

            o.StartBrewing();
            Assert.AreEqual(OrderStep.Brewing, o.Step);

            o.CompleteBrew(BrewQuality.Good);
            Assert.AreEqual(OrderStep.Extracting, o.Step);

            o.TickExtract(ExtractSeconds, ExtractSeconds);
            Assert.AreEqual(OrderStep.ReadyToPickup, o.Step);

            o.Pickup();
            Assert.AreEqual(OrderStep.HoldingIngredients, o.Step);

            o.Cup();
            Assert.AreEqual(OrderStep.ReadyToServe, o.Step);
        }
    }
}
