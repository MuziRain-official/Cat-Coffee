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
            g.Start(3f);
            Assert.IsTrue(g.IsActive);
            Assert.AreEqual(FrothGame.NoteCount, g.Judgements.Length);
            Assert.AreEqual(FrothGame.NoteCount, g.NotePositions.Length);
        }

        [Test]
        public void Tick_SpawnsNotesOverTime()
        {
            var g = new FrothGame();
            g.Start(3f);
            g.Tick(0.1f);
            Assert.Greater(g.NotePositions[0], 0f); // 第一个音符已生成（在判定线上方）

            g.Tick(1.0f);
            // 约 2 个音符生成
            Assert.Greater(g.NotePositions[1], 0f);
        }

        [Test]
        public void Tap_JudgesNearestNote()
        {
            var g = new FrothGame();
            g.Start(3f);
            g.Tick(0.1f); // 生成第一个音符
            // 手动把它放到判定线上
            g.NotePositions[0] = 0f;
            var result = g.Tap();
            Assert.AreEqual(NoteJudgement.Perfect, result);
            Assert.AreEqual(NoteJudgement.Perfect, g.Judgements[0]);
        }

        [Test]
        public void Tap_GoodWhenSlightlyOff()
        {
            var g = new FrothGame();
            g.Start(3f);
            g.Tick(0.1f);
            g.NotePositions[0] = 0.3f; // 离判定线 0.3
            var result = g.Tap();
            Assert.AreEqual(NoteJudgement.Good, result);
        }

        [Test]
        public void Tap_TooFarIsMiss()
        {
            var g = new FrothGame();
            g.Start(3f);
            g.Tick(0.1f);
            g.NotePositions[0] = 2f; // 太远
            var result = g.Tap();
            Assert.AreEqual(NoteJudgement.Miss, result);
            Assert.AreEqual(NoteJudgement.Pending, g.Judgements[0]); // 未判定
        }

        [Test]
        public void Result_PerfectWhenNinePerfect()
        {
            var g = new FrothGame();
            g.Start(3f);
            // 手动让 10 个音符全部 Perfect
            for (int i = 0; i < FrothGame.NoteCount; i++)
                g.Judgements[i] = NoteJudgement.Perfect;
            Assert.AreEqual(BrewQuality.Perfect, g.Result());
        }

        [Test]
        public void Result_GoodWhenSixHits()
        {
            var g = new FrothGame();
            g.Start(3f);
            for (int i = 0; i < 6; i++)
                g.Judgements[i] = NoteJudgement.Good;
            for (int i = 6; i < FrothGame.NoteCount; i++)
                g.Judgements[i] = NoteJudgement.Miss;
            Assert.AreEqual(BrewQuality.Good, g.Result());
        }

        [Test]
        public void Result_PoorWhenFewHits()
        {
            var g = new FrothGame();
            g.Start(3f);
            for (int i = 0; i < 3; i++)
                g.Judgements[i] = NoteJudgement.Good;
            for (int i = 3; i < FrothGame.NoteCount; i++)
                g.Judgements[i] = NoteJudgement.Miss;
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
            for (int i = 0; i < 8; i++) g.Tick(0.18f);
            Assert.AreEqual(0, g.CursorIndex);
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
            Assert.AreEqual(BrewQuality.Good, LatteArtGame.Judge(7));
        }

        [Test]
        public void Judge_CornerIsPoor()
        {
            Assert.AreEqual(BrewQuality.Poor, LatteArtGame.Judge(0));
            Assert.AreEqual(BrewQuality.Poor, LatteArtGame.Judge(8));
        }
    }
}
