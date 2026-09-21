using NUnit.Framework;
using CatCafe;
using UnityEngine;

namespace CatCafe.Tests
{
    public class BrewGameTests
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
        public void Start_ActivatesGameAtCenter()
        {
            var g = new BrewGame();
            g.Start();
            Assert.IsTrue(g.IsActive);
            Assert.AreEqual(0.5f, g.PointerPosition, 0.001f);
        }

        [Test]
        public void Tick_MovesPointer()
        {
            var g = new BrewGame();
            g.Start();
            g.Tick(0.5f, 3f); // 摆动 1.5 弧度
            Assert.That(g.PointerPosition, Is.Not.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void Judge_PerfectAtCenter()
        {
            Assert.AreEqual(BrewQuality.Perfect, BrewGame.Judge(0.5f, _config));
        }

        [Test]
        public void Judge_GoodInMiddleZone()
        {
            // 离中心 0.15（>0.12 完美区，≤0.30 良好区）
            Assert.AreEqual(BrewQuality.Good, BrewGame.Judge(0.65f, _config));
        }

        [Test]
        public void Judge_PoorFarFromCenter()
        {
            // 离中心 0.35（>0.30 良好区）
            Assert.AreEqual(BrewQuality.Poor, BrewGame.Judge(0.85f, _config));
        }

        [Test]
        public void Stop_LocksResultAndDeactivates()
        {
            var g = new BrewGame();
            g.Start();
            g.Tick(0.01f, 3f);
            var q = g.Stop(_config);
            Assert.AreEqual(q, g.Result.Value);
            Assert.IsFalse(g.IsActive);
        }
    }
}
