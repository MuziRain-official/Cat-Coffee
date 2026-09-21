using NUnit.Framework;
using CatCafe;

namespace CatCafe.Tests
{
    public class ProgressTests
    {
        [Test]
        public void NewProgress_StartsDay1_ZeroCoins()
        {
            var p = new Progress();
            Assert.AreEqual(1, p.Day);
            Assert.AreEqual(0, p.TotalCoins);
            Assert.IsFalse(p.HasItem(ItemType.QuickServe));
        }

        [Test]
        public void EndDay_AddsProfitAndIncrementsDay()
        {
            var p = new Progress();
            p.EndDay(80);
            Assert.AreEqual(80, p.TotalCoins);
            Assert.AreEqual(2, p.Day);
        }

        [Test]
        public void BuyItem_DeductsCoinsAndMarksOwned()
        {
            var p = new Progress(1, 500, null);
            Assert.IsTrue(p.BuyItem(ItemType.QuickServe)); // 100
            Assert.AreEqual(400, p.TotalCoins);
            Assert.IsTrue(p.HasItem(ItemType.QuickServe));
        }

        [Test]
        public void BuyItem_FailsWhenInsufficientCoins()
        {
            var p = new Progress(1, 50, null);
            Assert.IsFalse(p.BuyItem(ItemType.ExtraTables)); // 300
            Assert.AreEqual(50, p.TotalCoins);
            Assert.IsFalse(p.HasItem(ItemType.ExtraTables));
        }

        [Test]
        public void BuyItem_FailsWhenAlreadyOwned()
        {
            var p = new Progress(1, 500, new[] { ItemType.NoWait });
            Assert.IsFalse(p.BuyItem(ItemType.NoWait)); // 已拥有
            Assert.AreEqual(500, p.TotalCoins);
        }

        [Test]
        public void Json_RoundTrips()
        {
            var p = new Progress();
            p.EndDay(120);
            p.BuyItem(ItemType.QuickServe);

            var json = p.ToJson();
            var restored = Progress.FromJson(json);

            Assert.AreEqual(p.Day, restored.Day);
            Assert.AreEqual(p.TotalCoins, restored.TotalCoins);
            Assert.IsTrue(restored.HasItem(ItemType.QuickServe));
        }

        [Test]
        public void FromJson_EmptyOrNull_ReturnsNew()
        {
            Assert.AreEqual(1, Progress.FromJson(null).Day);
            Assert.AreEqual(1, Progress.FromJson("").Day);
        }
    }
}
