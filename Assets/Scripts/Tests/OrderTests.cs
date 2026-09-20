using NUnit.Framework;
using CatCafe;

namespace CatCafe.Tests
{
    public class OrderTests
    {
        private const float BrewSeconds = 2.5f;

        [Test]
        public void StartBrewing_SetsBrewingStep()
        {
            var o = new Order();
            o.StartBrewing();
            Assert.AreEqual(OrderStep.Brewing, o.Step);
            Assert.AreEqual(0f, o.BrewProgress);
        }

        [Test]
        public void Tick_CompletesBrewAfterFullDuration()
        {
            var o = new Order();
            o.StartBrewing();
            o.Tick(1.25f, BrewSeconds);
            Assert.AreEqual(OrderStep.Brewing, o.Step);
            Assert.AreEqual(0.5f, o.BrewProgress, 0.01f);

            o.Tick(1.25f, BrewSeconds);
            Assert.AreEqual(OrderStep.ReadyToCup, o.Step);
            Assert.AreEqual(1f, o.BrewProgress);
        }

        [Test]
        public void Cup_OnlyWorksAfterBrew()
        {
            var o = new Order();
            o.Cup(); // 未萃取，忽略
            Assert.AreEqual(OrderStep.None, o.Step);

            o.StartBrewing();
            o.Cup(); // 萃取未完成，忽略
            Assert.AreEqual(OrderStep.Brewing, o.Step);

            o.Tick(BrewSeconds, BrewSeconds);
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
            o.Tick(BrewSeconds, BrewSeconds);
            o.Cup();

            bool served = o.Serve(customer, 12f);
            Assert.IsTrue(served);
            Assert.AreEqual(CustomerPhase.Eating, customer.Phase);
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
            o.Tick(BrewSeconds, BrewSeconds);
            o.Cup();

            bool served = o.Serve(customer, 12f);
            Assert.IsFalse(served);
            Assert.AreEqual(OrderStep.ReadyToServe, o.Step); // 未消费，保持待上菜
        }
    }
}
