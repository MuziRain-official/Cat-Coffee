using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 拉花小游戏（纯逻辑，可单元测试）。
    /// 3×3 格子，指针循环高亮，按 E 停在中心格 = 猫爪成型。
    /// 停在中心 = 完美，停在四边中格 = 良好，停在四角 = 勉强。
    /// </summary>
    public class LatteArtGame
    {
        /// <summary>格子索引 0-8（0,0 为左上，中心是 4）。</summary>
        public int CursorIndex { get; private set; } = 0;

        /// <summary>小游戏是否进行中。</summary>
        public bool IsActive { get; private set; }

        private float _elapsed;
        private float _cellInterval;

        public void Start(float cellIntervalSeconds)
        {
            IsActive = true;
            CursorIndex = 0;
            _elapsed = 0f;
            _cellInterval = cellIntervalSeconds;
        }

        /// <summary>推进光标循环移动。</summary>
        public void Tick(float deltaTime)
        {
            if (!IsActive) return;
            _elapsed += deltaTime;
            int idx = (int)(_elapsed / _cellInterval);
            CursorIndex = idx % 9; // 0..8 循环
        }

        /// <summary>按 E 停下，返回品质。</summary>
        public BrewQuality Stop()
        {
            IsActive = false;
            return Judge(CursorIndex);
        }

        /// <summary>按格子索引判定品质（纯函数）。</summary>
        public static BrewQuality Judge(int index)
        {
            if (index == 4) return BrewQuality.Perfect;      // 中心
            if (index % 2 == 1) return BrewQuality.Good;     // 四边中格（1,3,5,7）
            return BrewQuality.Poor;                          // 四角（0,2,6,8）
        }
    }
}
