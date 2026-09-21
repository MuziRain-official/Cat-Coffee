using NUnit.Framework;
using CatCafe;

namespace CatCafe.Tests
{
    public class FrothGameTests
    {
        [Test]
        public void Start_ActivatesGame()
        {
            var g = new FrothGame();
            g.Start(0.25f);
            Assert.IsTrue(g.IsActive);
            Assert.AreEqual(0, g.Hits);
            Assert.AreEqual(0, g.CurrentBeat);
        }

        [Test]
        public void Tick_AdvancesBeats()
        {
            var g = new FrothGame();
            g.Start(0.25f);
            Assert.AreEqual(0, g.CurrentBeat); // 第 0 拍立即亮

            g.Tick(0.25f);
            Assert.AreEqual(1, g.CurrentBeat); // 推进到第 1 拍
        }

        [Test]
        public void Tap_HitsOncePerBeat()
        {
            var g = new FrothGame();
            g.Start(0.25f);
            Assert.IsTrue(g.Tap());       // 命中第 0 拍
            Assert.AreEqual(1, g.Hits);
            Assert.IsFalse(g.Tap());      // 同拍连点不命中
            Assert.AreEqual(1, g.Hits);

            g.Tick(0.25f);                // 推进到第 1 拍
            Assert.IsTrue(g.Tap());       // 命中第 1 拍
            Assert.AreEqual(2, g.Hits);
        }

        [Test]
        public void Result_PerfectWhenAllHits()
        {
            var g = new FrothGame();
            g.Start(0.25f);
            for (int i = 0; i < 8; i++)
            {
                Assert.IsTrue(g.Tap());   // 命中当前拍
                g.Tick(0.25f);            // 推进到下一拍
            }
            Assert.AreEqual(BrewQuality.Perfect, g.Result());
            Assert.IsFalse(g.IsActive);
        }

        [Test]
        public void Result_PoorWhenFewHits()
        {
            var g = new FrothGame();
            g.Start(0.25f);
            g.Tap(); // 只命中 1 次
            // 推进到结束
            for (int i = 0; i < 10; i++) g.Tick(0.25f);
            Assert.AreEqual(BrewQuality.Poor, g.Result());
        }
    }

    public class LatteArtGameTests
    {
        [Test]
        public void Start_CursorAtZero()
        {
            var g = new LatteArtGame();
            g.Start(0.18f);
            Assert.IsTrue(g.IsActive);
            Assert.AreEqual(0, g.CursorIndex);
        }

        [Test]
        public void Tick_MovesCursorCyclically()
        {
            var g = new LatteArtGame();
            g.Start(0.18f);
            g.Tick(0.18f);
            Assert.AreEqual(1, g.CursorIndex);
            // 9 格循环
            for (int i = 0; i < 8; i++) g.Tick(0.18f);
            Assert.AreEqual(0, g.CursorIndex); // 回到 0
        }

        [Test]
        public void Judge_CenterIsPerfect()
        {
            Assert.AreEqual(BrewQuality.Perfect, LatteArtGame.Judge(4));
        }

        [Test]
        public void Judge_EdgeMiddleIsGood()
        {
            Assert.AreEqual(BrewQuality.Good, LatteArtGame.Judge(1));
            Assert.AreEqual(BrewQuality.Good, LatteArtGame.Judge(3));
            Assert.AreEqual(BrewQuality.Good, LatteArtGame.Judge(5));
            Assert.AreEqual(BrewQuality.Good, LatteArtGame.Judge(7));
        }

        [Test]
        public void Judge_CornerIsPoor()
        {
            Assert.AreEqual(BrewQuality.Poor, LatteArtGame.Judge(0));
            Assert.AreEqual(BrewQuality.Poor, LatteArtGame.Judge(2));
            Assert.AreEqual(BrewQuality.Poor, LatteArtGame.Judge(6));
            Assert.AreEqual(BrewQuality.Poor, LatteArtGame.Judge(8));
        }

        [Test]
        public void Stop_Deactivates()
        {
            var g = new LatteArtGame();
            g.Start(0.18f);
            g.Stop();
            Assert.IsFalse(g.IsActive);
        }
    }
}
