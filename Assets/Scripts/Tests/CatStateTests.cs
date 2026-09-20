using NUnit.Framework;
using CatCafe;
using UnityEngine;

namespace CatCafe.Tests
{
    public class CatStateTests
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
        public void Tick_DecaysAllThreeStats()
        {
            var cat = new CatState(100f);
            cat.Tick(9f, _config); // 饥饿 -1 → 99；清洁 -0.5 → 99.5
            Assert.AreEqual(99f, cat.Satiety, 0.01f);
            Assert.AreEqual(99.5f, cat.Hygiene, 0.01f);

            cat.Tick(9f, _config); // 再 -1 / -0.5
            Assert.AreEqual(98f, cat.Satiety, 0.01f);
            Assert.AreEqual(99.0f, cat.Hygiene, 0.01f);
        }

        [Test]
        public void Feed_RecoversSatietyAndClamps()
        {
            var cat = new CatState(20f);
            cat.Feed(_config);
            Assert.AreEqual(70f, cat.Satiety, 0.01f); // 20 + 50

            cat.Feed(_config);
            cat.Feed(_config);
            Assert.AreEqual(100f, cat.Satiety, 0.01f); // 封顶 100
        }

        [Test]
        public void Clean_RecoversHygiene()
        {
            var cat = new CatState(10f);
            cat.Clean(_config);
            Assert.AreEqual(60f, cat.Hygiene, 0.01f);
        }

        [Test]
        public void Pet_RecoversMood()
        {
            var cat = new CatState(40f);
            cat.Pet(_config);
            Assert.AreEqual(65f, cat.Mood, 0.01f); // 40 + 25
        }

        [Test]
        public void IsBoosted_TrueWhenAverageAboveThreshold()
        {
            var cat = new CatState(80f); // 均值 80 > 70
            Assert.IsTrue(cat.IsBoosted(_config));
        }

        [Test]
        public void IsPenalized_TrueWhenAverageBelowThreshold()
        {
            var cat = new CatState(20f); // 均值 20 < 30
            Assert.IsTrue(cat.IsPenalized(_config));
        }

        [Test]
        public void Tick_MoodDecaysFasterWhenHungry()
        {
            var cat = new CatState(20f); // 饱腹度 20 < 30，触发加速
            float moodBefore = cat.Mood;
            cat.Tick(12f, _config); // 基础 -1，加速翻倍 → -2
            Assert.AreEqual(moodBefore - 2f, cat.Mood, 0.01f);
        }
    }
}
