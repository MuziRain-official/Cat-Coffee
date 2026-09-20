using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 游戏主控制器（单例）。持有全局配置与各系统，驱动每帧 Tick。
    /// 后续时间/顾客/猫咪/订单/结算系统都挂在这里统一驱动。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("全局配置")]
        [SerializeField] private GameConfigSO config;

        // 运行时状态（非序列化，纯逻辑）
        public DayClock Clock { get; private set; } = new DayClock();

        /// <summary>是否正在营业中（打烊后为 false）</summary>
        public bool IsRunning => !Clock.IsDayOver(config.dayDurationSeconds);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            // 打烊后停止推进
            if (!IsRunning) return;

            Clock.Tick(Time.deltaTime);
        }

        /// <summary>开始/重启一个营业日。</summary>
        public void StartDay()
        {
            Clock.Reset();
            Clock.IsPaused = false;
            Clock.TimeScale = 1f;
        }

        /// <summary>切换暂停。</summary>
        public void TogglePause() => Clock.IsPaused = !Clock.IsPaused;

        /// <summary>切换快进（2x）。</summary>
        public void ToggleFastForward() =>
            Clock.TimeScale = Clock.TimeScale > 1f ? 1f : config.fastForwardMultiplier;
    }
}
