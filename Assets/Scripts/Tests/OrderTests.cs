using NUnit.Framework;
using CatCafe;

namespace CatCafe.Tests
{
    public class OrderTests
    {
        private const float ExtractSeconds = 5f;

        [Test]
        public void Start_SetsRecipe()
        {
            var o = new Order();
            o.Start(RecipeType.Latte);
            Assert.AreEqual(RecipeType.Latte, o.Recipe);
            Assert.AreEqual(OrderStep.None, o.Step);
        }

        [Test]
        public void LatteFlow_BrewExtractPickupCup()
        {
            var o = new Order();
            o.Start(RecipeType.Latte);
            o.StartBrewing();
            Assert.AreEqual(OrderStep.Brewing, o.Step);

            o.CompleteBrew(BrewQuality.Good);
            Assert.AreEqual(OrderStep.Extracting, o.Step);

            o.TickExtract(ExtractSeconds, ExtractSeconds);
            Assert.AreEqual(OrderStep.ReadyToPickup, o.Step);

            o.Pickup();
            Assert.AreEqual(OrderStep.HoldingIngredients, o.Step);

            o.Cup();
            Assert.AreEqual(OrderStep.ReadyToServe, o.Step); // 拿铁装杯即完成
        }

        [Test]
        public void CatPawFlow_RequiresLatteArt()
        {
            var o = new Order();
            o.Start(RecipeType.CatPaw);
            o.StartBrewing();
            o.CompleteBrew(BrewQuality.Perfect);
            o.TickExtract(ExtractSeconds, ExtractSeconds);
            o.Pickup();
            o.Cup();
            Assert.AreEqual(OrderStep.LatteArt, o.Step); // 猫爪装杯即进拉花

            o.CompleteLatteArt();
            Assert.AreEqual(OrderStep.ReadyToServe, o.Step);
        }

        [Test]
        public void CappuccinoFlow_FrothThenExtractThenPickupCup()
        {
            var o = new Order();
            o.Start(RecipeType.Cappuccino);
            o.StartFrothing();
            Assert.AreEqual(OrderStep.Frothing, o.Step);

            o.CompleteFroth(BrewQuality.Good);
            Assert.AreEqual(OrderStep.Extracting, o.Step); // 奶泡完成 → 读条

            o.TickExtract(ExtractSeconds, ExtractSeconds); // 读条完成
            Assert.AreEqual(OrderStep.ReadyToPickup, o.Step);

            o.Pickup();
            o.Cup();
            Assert.AreEqual(OrderStep.ReadyToServe, o.Step); // 卡布装杯即完成
        }

        [Test]
        public void Serve_RequiresMatchingRecipe()
        {
            var o = new Order();
            var customer = new Customer();
            customer.PlaceOrder(45f, RecipeType.Latte);

            o.Start(RecipeType.Latte);
            o.StartBrewing();
            o.CompleteBrew(BrewQuality.Perfect);
            o.TickExtract(ExtractSeconds, ExtractSeconds);
            o.Pickup();
            o.Cup();

            bool served = o.Serve(customer, 12f);
            Assert.IsTrue(served);
            Assert.AreEqual(CustomerPhase.Eating, customer.Phase);
            Assert.AreEqual(OrderStep.None, o.Step);
        }

        [Test]
        public void Serve_WrongRecipe_Fails()
        {
            var o = new Order();
            var customer = new Customer();
            customer.PlaceOrder(45f, RecipeType.CatPaw); // 顾客要猫爪

            o.Start(RecipeType.Latte); // 做的是拿铁
            o.StartBrewing();
            o.CompleteBrew(BrewQuality.Good);
            o.TickExtract(ExtractSeconds, ExtractSeconds);
            o.Pickup();
            o.Cup();

            bool served = o.Serve(customer, 12f);
            Assert.IsFalse(served); // 上错菜失败
            Assert.AreEqual(OrderStep.ReadyToServe, o.Step);
        }

        [Test]
        public void TakeFromWarmer_SetsRecipeQualityFreshness()
        {
            var o = new Order();
            bool ok = o.TakeFromWarmer(RecipeType.Cappuccino, BrewQuality.Good, 0.7f);
            Assert.IsTrue(ok);
            Assert.AreEqual(OrderStep.ReadyToServe, o.Step);
            Assert.AreEqual(RecipeType.Cappuccino, o.Recipe);
            Assert.AreEqual(0.7f, o.Freshness, 0.01f);
        }
    }
}
