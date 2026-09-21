using UnityEngine;

namespace CatCafe
{
    /// <summary>猫所在的区域（猫垫）。</summary>
    public enum CatZone
    {
        Nest, // 默认猫窝（无增益）
        Bar,  // 吧台旁：小游戏完美区变宽
        Seat, // 座位区：顾客耐心减速
        Door, // 门口：客流加速
    }

    /// <summary>
    /// 猫咪三态（纯逻辑，可单元测试）：
    /// Satiety(饱腹度) / Hygiene(清洁度) / Mood(心情)，范围 0–100，越高越好。
    /// 随时间衰减，互动可恢复。猫可被抱到不同区域，区域增益强度由心情决定。
    /// </summary>
    public class CatState
    {
        public float Satiety { get; private set; }
        public float Hygiene { get; private set; }
        public float Mood { get; private set; }

        /// <summary>猫当前所在区域。</summary>
        public CatZone Zone { get; private set; } = CatZone.Nest;

        /// <summary>是否正被主角抱着。</summary>
        public bool IsCarried { get; private set; }

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

        /// <summary>区域增益强度：心情 >70 全额 / 30~70 半额 / <30 失效。</summary>
        public float GainStrength(GameConfigSO c)
        {
            if (Mood > c.boostThreshold) return 1f;
            if (Mood > c.penaltyThreshold) return 0.5f;
            return 0f;
        }

        /// <summary>把猫放到某区域（猫垫）。</summary>
        public void PlaceAt(CatZone zone) => Zone = zone;

        /// <summary>抱起猫。</summary>
        public void PickUp() => IsCarried = true;

        /// <summary>放下猫（放下时才确定落在哪个区域）。</summary>
        public void PutDown(CatZone zone)
        {
            IsCarried = false;
            Zone = zone;
        }

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

