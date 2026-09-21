using NUnit.Framework;
using CatCafe;

namespace CatCafe.Tests
{
    public class WarmerTests
    {
        [Test]
        public void Store_AddsCupUntilFull()
        {
            var w = new Warmer(2);
            Assert.IsTrue(w.Store(RecipeType.Latte, BrewQuality.Perfect));
            Assert.IsTrue(w.Store(RecipeType.CatPaw, BrewQuality.Good));
            Assert.IsFalse(w.Store(RecipeType.Latte, BrewQuality.Poor)); // 已满
            Assert.AreEqual(2, w.Count);
            Assert.IsTrue(w.IsFull);
        }

        [Test]
        public void Take_IsFifo()
        {
            var w = new Warmer(3);
            w.Store(RecipeType.Latte, BrewQuality.Perfect);
            w.Store(RecipeType.CatPaw, BrewQuality.Poor);

            var first = w.Take();
            var second = w.Take();
            Assert.AreEqual(BrewQuality.Perfect, first.Quality);
            Assert.AreEqual(RecipeType.Latte, first.Recipe);
            Assert.AreEqual(BrewQuality.Poor, second.Quality);
            Assert.AreEqual(RecipeType.CatPaw, second.Recipe);
            Assert.IsNull(w.Take()); // 空了
        }

        [Test]
        public void TakeAt_TakesSpecificIndex()
        {
            var w = new Warmer(3);
            w.Store(RecipeType.Latte, BrewQuality.Perfect);
            w.Store(RecipeType.CatPaw, BrewQuality.Good);
            w.Store(RecipeType.Cappuccino, BrewQuality.Poor);

            var cup = w.TakeAt(1); // 拿中间那杯
            Assert.AreEqual(RecipeType.CatPaw, cup.Recipe);
            Assert.AreEqual(2, w.Count);
            Assert.AreEqual(RecipeType.Latte, w.Cups[0].Recipe);
            Assert.AreEqual(RecipeType.Cappuccino, w.Cups[1].Recipe);
        }

        [Test]
        public void Tick_DecaysFreshness()
        {
            var w = new Warmer(1);
            w.Store(RecipeType.Latte, BrewQuality.Good);
            w.Tick(45f, 90f); // 45/90 = 0.5 新鲜度减半
            var cup = w.Take();
            Assert.AreEqual(0.5f, cup.Freshness, 0.01f);
        }

        [Test]
        public void Tick_FreshnessClampsAtZero()
        {
            var w = new Warmer(1);
            w.Store(RecipeType.Latte, BrewQuality.Good);
            w.Tick(200f, 90f); // 超过时长
            var cup = w.Take();
            Assert.AreEqual(0f, cup.Freshness);
        }

        [Test]
        public void ReturnCup_PreservesFreshness()
        {
            var w = new Warmer(1);
            w.Store(RecipeType.Latte, BrewQuality.Good);
            w.Tick(30f, 90f);
            var cup = w.Take();
            float f = cup.Freshness;
            w.ReturnCup(cup);
            Assert.AreEqual(f, w.Take().Freshness, 0.01f);
        }
    }
}
