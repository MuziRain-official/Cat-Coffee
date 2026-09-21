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
            Assert.IsTrue(w.Store(BrewQuality.Perfect));
            Assert.IsTrue(w.Store(BrewQuality.Good));
            Assert.IsFalse(w.Store(BrewQuality.Poor)); // 已满
            Assert.AreEqual(2, w.Count);
            Assert.IsTrue(w.IsFull);
        }

        [Test]
        public void Take_IsFifo()
        {
            var w = new Warmer(3);
            w.Store(BrewQuality.Perfect);
            w.Store(BrewQuality.Poor);

            var first = w.Take();
            var second = w.Take();
            Assert.AreEqual(BrewQuality.Perfect, first.Quality);
            Assert.AreEqual(BrewQuality.Poor, second.Quality);
            Assert.IsNull(w.Take()); // 空了
        }

        [Test]
        public void Tick_DecaysFreshness()
        {
            var w = new Warmer(1);
            w.Store(BrewQuality.Good);
            w.Tick(45f, 90f); // 45/90 = 0.5 新鲜度减半
            var cup = w.Take();
            Assert.AreEqual(0.5f, cup.Freshness, 0.01f);
        }

        [Test]
        public void Tick_FreshnessClampsAtZero()
        {
            var w = new Warmer(1);
            w.Store(BrewQuality.Good);
            w.Tick(200f, 90f); // 超过时长
            var cup = w.Take();
            Assert.AreEqual(0f, cup.Freshness);
        }

        [Test]
        public void ReturnCup_PreservesFreshness()
        {
            var w = new Warmer(1);
            w.Store(BrewQuality.Good);
            w.Tick(30f, 90f); // 新鲜度 1 - 30/90 = 0.667
            var cup = w.Take();
            float f = cup.Freshness;
            w.ReturnCup(cup);
            Assert.AreEqual(f, w.Take().Freshness, 0.01f);
        }
    }
}
