namespace CatCafe
{
    /// <summary>单个下落音符的判定结果。</summary>
    public enum NoteJudgement
    {
        Pending,  // 未判定
        Perfect,  // 精准命中
        Good,     // 命中（略偏）
        Miss,     // 漏掉
    }

    /// <summary>
    /// 下落式节奏音游（纯逻辑，可单元测试）。
    /// 10 个音符从上方下落，到达判定线(0)时按 E。
    /// 按精准度判定 Perfect/Good/Miss。
    /// 品质：Perfect ≥9 = 完美，Perfect+Good ≥6 = 良好，否则勉强。
    /// </summary>
    public class FrothGame
    {
        public const int NoteCount = 10;
        public const float JudgeLineY = 0f;

        /// <summary>每个音符的下落位置（y，越大越靠上，0=判定线，-1=未生成）。</summary>
        public float[] NotePositions { get; private set; } = new float[NoteCount];

        /// <summary>每个音符的判定结果。</summary>
        public NoteJudgement[] Judgements { get; private set; } = new NoteJudgement[NoteCount];

        public bool IsActive { get; private set; }

        private float _fallSpeed;
        private float _elapsed;
        private int _nextNoteIndex;

        public void Start(float fallSpeed)
        {
            IsActive = true;
            _fallSpeed = fallSpeed;
            _elapsed = 0f;
            _nextNoteIndex = 0;
            for (int i = 0; i < NoteCount; i++)
            {
                NotePositions[i] = -1f;
                Judgements[i] = NoteJudgement.Pending;
            }
        }

        /// <summary>推进下落。返回是否结束。</summary>
        public void Tick(float deltaTime)
        {
            if (!IsActive) return;
            _elapsed += deltaTime;

            // 音符逐个生成（间距 0.5 秒，从高处下落）
            if (_nextNoteIndex < NoteCount && _elapsed >= _nextNoteIndex * 0.5f)
            {
                NotePositions[_nextNoteIndex] = 5f;
                _nextNoteIndex++;
            }

            // 所有已生成音符下落
            for (int i = 0; i < NoteCount; i++)
            {
                if (NotePositions[i] <= -1f) continue;
                if (Judgements[i] != NoteJudgement.Pending) continue;
                NotePositions[i] -= _fallSpeed * deltaTime;
                if (NotePositions[i] < -0.3f)
                    Judgements[i] = NoteJudgement.Miss;
            }

            if (AllJudged()) IsActive = false;
        }

        /// <summary>按 E：判定离判定线最近且未判定的音符。</summary>
        public NoteJudgement Tap()
        {
            if (!IsActive) return NoteJudgement.Miss;

            int best = -1;
            float bestDist = float.MaxValue;
            for (int i = 0; i < NoteCount; i++)
            {
                if (NotePositions[i] <= -1f) continue;
                if (Judgements[i] != NoteJudgement.Pending) continue;
                float dist = System.Math.Abs(NotePositions[i] - JudgeLineY);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = i;
                }
            }
            if (best < 0) return NoteJudgement.Miss;

            // 距离太远不判定（避免误伤远处音符）
            if (bestDist > 0.45f) return NoteJudgement.Miss;

            NoteJudgement result = bestDist <= 0.15f ? NoteJudgement.Perfect : NoteJudgement.Good;
            Judgements[best] = result;

            if (AllJudged()) IsActive = false;
            return result;
        }

        private bool AllJudged()
        {
            if (_nextNoteIndex < NoteCount) return false; // 还有音符没生成
            for (int i = 0; i < NoteCount; i++)
                if (Judgements[i] == NoteJudgement.Pending)
                    return false;
            return true;
        }

        /// <summary>结算品质。</summary>
        public BrewQuality Result()
        {
            int perfect = 0, good = 0;
            for (int i = 0; i < NoteCount; i++)
            {
                if (Judgements[i] == NoteJudgement.Perfect) perfect++;
                else if (Judgements[i] == NoteJudgement.Good) good++;
            }
            if (perfect >= 9) return BrewQuality.Perfect;
            if (perfect + good >= 6) return BrewQuality.Good;
            return BrewQuality.Poor;
        }
    }
}
