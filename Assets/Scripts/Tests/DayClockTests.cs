using NUnit.Framework;
using CatCafe;

namespace CatCafe.Tests
{
    public class DayClockTests
    {
        private const float DayDuration = 360f;

        [Test]
        public void Tick_AccumulatesElapsedTime()
        {
            var clock = new DayClock();
            clock.Tick(10f);
            clock.Tick(5f);
            Assert.AreEqual(15f, clock.ElapsedSeconds);
        }

        [Test]
        public void Tick_AppliesTimeScale()
        {
            var clock = new DayClock { TimeScale = 2f };
            clock.Tick(10f);
            Assert.AreEqual(20f, clock.ElapsedSeconds);
        }

        [Test]
        public void Tick_DoesNothingWhilePaused()
        {
            var clock = new DayClock { IsPaused = true };
            clock.Tick(10f);
            Assert.AreEqual(0f, clock.ElapsedSeconds);
        }

        [Test]
        public void IsDayOver_TrueWhenElapsedReachesDuration()
        {
            var clock = new DayClock();
            clock.Tick(360f);
            Assert.IsTrue(clock.IsDayOver(DayDuration));

            // 未到时长则为 false
            var early = new DayClock();
            early.Tick(359f);
            Assert.IsFalse(early.IsDayOver(DayDuration));
        }

        [Test]
        public void RemainingSeconds_ClampsToZero()
        {
            var clock = new DayClock();
            clock.Tick(400f);
            Assert.AreEqual(0f, clock.RemainingSeconds(DayDuration));
        }

        [Test]
        public void Reset_ClearsElapsed()
        {
            var clock = new DayClock();
            clock.Tick(120f);
            clock.Reset();
            Assert.AreEqual(0f, clock.ElapsedSeconds);
        }
    }
}
