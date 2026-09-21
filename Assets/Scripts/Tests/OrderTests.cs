using NUnit.Framework;
using CatCafe;

namespace CatCafe.Tests
{
    public class OrderTests
    {
        [Test]
        public void HoldIngredients_SetsRecipeAndQuality()
        {
            var o = new Order();
            o.HoldIngredients(RecipeType.Latte, BrewQuality.Perfect);
            Assert.AreEqual(OrderStep.HoldingIngredients, o.Step);
            Assert.AreEqual(RecipeType.Latte, o.Recipe);
            Assert.AreEqual(BrewQuality.Perfect, o.Quality);
        }

        [Test]
        public void Cup_LatteGoesReadyToServe()
        {
            var o = new Order();
            o.HoldIngredients(RecipeType.Latte, BrewQuality.Good);
            o.Cup();
            Assert.AreEqual(OrderStep.ReadyToServe, o.Step);
        }

        [Test]
        public void Cup_CatPawGoesLatteArt()
        {
            var o = new Order();
            o.HoldIngredients(RecipeType.CatPaw, BrewQuality.Good);
            o.Cup();
            Assert.AreEqual(OrderStep.LatteArt, o.Step);

            o.CompleteLatteArt();
            Assert.AreEqual(OrderStep.ReadyToServe, o.Step);
        }

        [Test]
        public void Serve_RequiresMatchingRecipe()
        {
            var o = new Order();
            var customer = new Customer();
            customer.PlaceOrder(60f, RecipeType.Latte);

            o.HoldIngredients(RecipeType.Latte, BrewQuality.Good);
            o.Cup();

            Assert.IsTrue(o.Serve(customer, 12f));
            Assert.AreEqual(OrderStep.None, o.Step);
        }

        [Test]
        public void Serve_WrongRecipe_Fails()
        {
            var o = new Order();
            var customer = new Customer();
            customer.PlaceOrder(60f, RecipeType.CatPaw);

            o.HoldIngredients(RecipeType.Latte, BrewQuality.Good);
            o.Cup();

            Assert.IsFalse(o.Serve(customer, 12f));
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
