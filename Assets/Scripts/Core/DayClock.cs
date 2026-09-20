namespace CatCafe
{
    /// <summary>
    /// 营业日时钟（纯逻辑，可单元测试）。
    /// 由 GameManager 每帧调用 Tick 推进。
    /// </summary>
    public class DayClock
    {
        /// <summary>本局已流逝时间（秒），受时间倍率影响</summary>
        public float ElapsedSeconds { get; private set; }

        /// <summary>是否暂停（暂停时 Tick 不推进）</summary>
        public bool IsPaused { get; set; }

        /// <summary>时间倍率（1 = 常速，2 = 快进）</summary>
        public float TimeScale { get; set; } = 1f;

        /// <summary>推进时间：deltaTime × 倍率，暂停时忽略。</summary>
        public void Tick(float deltaTime)
        {
            if (IsPaused) return;
            ElapsedSeconds += deltaTime * TimeScale;
        }

        /// <summary>剩余时间（秒），不小于 0。</summary>
        public float RemainingSeconds(float dayDuration) =>
            dayDuration > ElapsedSeconds ? dayDuration - ElapsedSeconds : 0f;

        /// <summary>营业日是否结束。</summary>
        public bool IsDayOver(float dayDuration) => ElapsedSeconds >= dayDuration;

        /// <summary>重置本局时间。</summary>
        public void Reset() => ElapsedSeconds = 0f;
    }
}
