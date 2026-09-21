using NUnit.Framework;
using CatCafe;
using UnityEngine;

namespace CatCafe.Tests
{
    public class CatZoneTests
    {
        private GameConfigSO _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<GameConfigSO>();
        }

        [TearDown]
        public void TearDown()
        {
            ScriptableObject.DestroyImmediate(_config);
        }

        [Test]
        public void NewCat_StartsAtNest_NotCarried()
        {
            var c = new CatState();
            Assert.AreEqual(CatZone.Nest, c.Zone);
            Assert.IsFalse(c.IsCarried);
        }

        [Test]
        public void PickUpAndPutDown_ChangesCarryAndZone()
        {
            var c = new CatState();
            c.PickUp();
            Assert.IsTrue(c.IsCarried);

            c.PutDown(CatZone.Bar);
            Assert.IsFalse(c.IsCarried);
            Assert.AreEqual(CatZone.Bar, c.Zone);
        }

        [Test]
        public void GainStrength_FullWhenMoodHigh()
        {
            var c = new CatState(); // 满心情 100 > 70
            Assert.AreEqual(1f, c.GainStrength(_config), 0.01f);
        }

        [Test]
        public void GainStrength_ZeroWhenMoodLow()
        {
            var c = new CatState();
            // 心情极快下滑到低值
            var lowConfig = ScriptableObject.CreateInstance<GameConfigSO>();
            lowConfig.moodDecayInterval = 0.1f; // 心情猛降
            c.Tick(20f, lowConfig);
            ScriptableObject.DestroyImmediate(lowConfig);

            Assert.Less(c.Mood, _config.penaltyThreshold);
            Assert.AreEqual(0f, c.GainStrength(_config), 0.01f);
        }

        [Test]
        public void GainStrength_HalfWhenMoodMid()
        {
            var c = new CatState();
            // 心情降到 30~70 之间（如 50）：用 moodDecayInterval 调到一个中间值
            var midConfig = ScriptableObject.CreateInstance<GameConfigSO>();
            midConfig.moodDecayInterval = 2f; // 缓慢下滑
            c.Tick(100f, midConfig); // 100 - 100/2 = 50
            ScriptableObject.DestroyImmediate(midConfig);

            Assert.Greater(c.Mood, _config.penaltyThreshold);
            Assert.LessOrEqual(c.Mood, _config.boostThreshold);
            Assert.AreEqual(0.5f, c.GainStrength(_config), 0.01f);
        }
    }
}
