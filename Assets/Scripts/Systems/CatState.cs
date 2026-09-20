using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 猫咪三态（纯逻辑，可单元测试）：
    /// Satiety(饱腹度) / Hygiene(清洁度) / Mood(心情)，范围 0–100，越高越好。
    /// 随时间衰减，互动可恢复。均值高于阈值 → 增益，低于阈值 → 惩罚。
    /// </summary>
    public class CatState
    {
        public float Satiety { get; private set; }
        public float Hygiene { get; private set; }
        public float Mood { get; private set; }

        public CatState(float initial = 100f)
        {
            Satiety = Hygiene = Mood = initial;
        }

        /// <summary>三态均值。</summary>
        public float Average => (Satiety + Hygiene + Mood) / 3f;

        /// <summary>是否处于增益状态。</summary>
        public bool IsBoosted(GameConfigSO c) => Average > c.boostThreshold;

        /// <summary>是否处于惩罚状态。</summary>
        public bool IsPenalized(GameConfigSO c) => Average < c.penaltyThreshold;

        /// <summary>推进衰减：饥饿/清洁/心情随时间下滑，饿或脏时心情加速下滑。</summary>
        public void Tick(float deltaTime, GameConfigSO c)
        {
            Satiety = Clamp(Satiety - deltaTime / c.hungerDecayInterval);
            Hygiene = Clamp(Hygiene - deltaTime / c.hygieneDecayInterval);

            float moodRate = 1f / c.moodDecayInterval;
            if (Satiety < c.penaltyThreshold || Hygiene < c.penaltyThreshold)
                moodRate *= 2f;
            Mood = Clamp(Mood - deltaTime * moodRate);
        }

        public void Feed(GameConfigSO c) => Satiety = Clamp(Satiety + c.feedRecovery);
        public void Clean(GameConfigSO c) => Hygiene = Clamp(Hygiene + c.cleanRecovery);
        public void Pet(GameConfigSO c) => Mood = Clamp(Mood + c.petRecovery);

        private float Clamp(float v) => Mathf.Clamp(v, 0f, 100f);
    }
}
