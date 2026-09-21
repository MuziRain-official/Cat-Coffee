namespace CatCafe
{
    /// <summary>
    /// 打奶泡节奏连击小游戏（纯逻辑，可单元测试）。
    /// 8 个节拍依次点亮，在点亮的瞬间按 E 命中。
    /// 命中 8 = 完美，5-7 = 良好，<5 = 勉强。
    /// Tick 推进节拍；Tap 命中当前拍（每拍只能命中一次）。
    /// </summary>
    public class FrothGame
    {
        public const int BeatCount = 8;

        /// <summary>小游戏是否进行中。</summary>
        public bool IsActive { get; private set; }

        /// <summary>当前节拍序号（0..7）。</summary>
        public int CurrentBeat { get; private set; }

        /// <summary>当前节拍是否已被命中。</summary>
        public bool CurrentBeatHit { get; private set; }

        /// <summary>命中次数。</summary>
        public int Hits { get; private set; }

        private float _elapsed;
        private float _beatInterval;

        public void Start(float beatIntervalSeconds)
        {
            IsActive = true;
            CurrentBeat = 0;
            CurrentBeatHit = false;
            Hits = 0;
            _elapsed = 0f;
            _beatInterval = beatIntervalSeconds;
        }

        /// <summary>推进节拍。返回是否结束。</summary>
        public void Tick(float deltaTime)
        {
            if (!IsActive) return;
            _elapsed += deltaTime;
            int beat = (int)(_elapsed / _beatInterval);
            if (beat >= BeatCount)
            {
                IsActive = false;
                CurrentBeat = BeatCount - 1;
                return;
            }
            if (beat != CurrentBeat)
            {
                CurrentBeat = beat;
                CurrentBeatHit = false; // 新拍重置
            }
        }

        /// <summary>玩家按 E 打拍。当前拍未命中过则命中。</summary>
        public bool Tap()
        {
            if (!IsActive) return false;
            if (CurrentBeatHit) return false; // 每拍只能命中一次
            CurrentBeatHit = true;
            Hits++;
            return true;
        }

        /// <summary>结算品质。</summary>
        public BrewQuality Result()
        {
            if (Hits >= 8) return BrewQuality.Perfect;
            if (Hits >= 5) return BrewQuality.Good;
            return BrewQuality.Poor;
        }
    }
}
