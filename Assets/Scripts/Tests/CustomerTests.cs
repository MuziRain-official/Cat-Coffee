using NUnit.Framework;
using CatCafe;

namespace CatCafe.Tests
{
    public class CustomerTests
    {
        [Test]
        public void NewCustomer_StartsEntering()
        {
            var c = new Customer();
            Assert.AreEqual(CustomerPhase.Entering, c.Phase);
        }

        [Test]
        public void PlaceOrder_MovesToWaiting()
        {
            var c = new Customer();
            c.PlaceOrder(45f);
            Assert.AreEqual(CustomerPhase.Waiting, c.Phase);
            Assert.AreEqual(45f, c.RemainingPatience);
        }

        [Test]
        public void Serve_MovesToEating_OnlyWhenWaiting()
        {
            var c = new Customer();
            c.Serve(12f); // 未点单，忽略
            Assert.AreEqual(CustomerPhase.Entering, c.Phase);

            c.PlaceOrder(45f);
            c.Serve(12f);
            Assert.AreEqual(CustomerPhase.Eating, c.Phase);
            Assert.AreEqual(12f, c.RemainingEating);
        }

        [Test]
        public void Tick_WaitingOut_LeavesWithoutPaying()
        {
            var c = new Customer();
            c.PlaceOrder(45f);
            c.Tick(45f);
            Assert.AreEqual(CustomerPhase.Left, c.Phase);
            Assert.IsTrue(c.HasLeft);
            Assert.IsFalse(c.HasPaid);
        }

        [Test]
        public void Tick_EatingDone_Pays()
        {
            var c = new Customer();
            c.PlaceOrder(45f);
            c.Serve(12f);
            c.Tick(12f);
            Assert.AreEqual(CustomerPhase.Paid, c.Phase);
            Assert.IsTrue(c.HasPaid);
        }

        [Test]
        public void Tick_WaitingDoesNotDecayBeforeOrder()
        {
            var c = new Customer();
            c.Tick(100f); // Entering 阶段无反应
            Assert.AreEqual(CustomerPhase.Entering, c.Phase);
        }
    }
}
