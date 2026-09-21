using UnityEngine;

namespace CatCafe
{
    /// <summary>萃取品质（三档）。</summary>
    public enum BrewQuality
    {
        Poor,    // 勉强
        Good,    // 良好
        Perfect, // 完美
    }

    /// <summary>
    /// 萃取时机条小游戏（纯逻辑，可单元测试）。
    /// 指针在 [0,1] 区间正弦摆动；玩家按 E 停住后，按指针离中心距离判定品质。
    /// 无失败，只有好坏——做砸了也能卖，只是少赚。
    /// </summary>
    public class BrewGame
    {
        /// <summary>指针位置 0–1（0.5 为中心/完美区）。</summary>
        public float PointerPosition { get; private set; } = 0.5f;

        /// <summary>小游戏是否进行中。</summary>
        public bool IsActive { get; private set; }

        /// <summary>本次结果（停止后才有）。</summary>
        public BrewQuality? Result { get; private set; }

        private float _phase;

        public void Start()
        {
            IsActive = true;
            Result = null;
            _phase = 0f;
            PointerPosition = 0.5f;
        }

        /// <summary>推进指针摆动。</summary>
        public void Tick(float deltaTime, float swingSpeed)
        {
            if (!IsActive) return;
            _phase += deltaTime * swingSpeed;
            PointerPosition = (Mathf.Sin(_phase) + 1f) / 2f;
        }

        /// <summary>玩家按 E 停住指针，锁定品质。</summary>
        public BrewQuality Stop(GameConfigSO config)
        {
            if (!IsActive) return BrewQuality.Good;
            var q = Judge(PointerPosition, config);
            Result = q;
            IsActive = false;
            return q;
        }

        /// <summary>按指针位置判定品质（纯函数）。</summary>
        public static BrewQuality Judge(float position, GameConfigSO config)
        {
            float dist = Mathf.Abs(position - 0.5f);
            if (dist <= config.perfectHalfWidth) return BrewQuality.Perfect;
            if (dist <= config.goodHalfWidth) return BrewQuality.Good;
            return BrewQuality.Poor;
        }
    }
}
