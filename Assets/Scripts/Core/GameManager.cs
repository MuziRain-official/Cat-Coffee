using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 游戏主控制器（单例）。持有 GameFlow（纯逻辑编排器），每帧驱动。
    /// config 为空时自动创建运行时实例，保证场景零配置即可运行。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("全局配置（留空则运行时自动创建默认值）")]
        [SerializeField] private GameConfigSO config;

        public GameFlow Flow { get; private set; }
        public GameConfigSO Config { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Config = config != null ? config : ScriptableObject.CreateInstance<GameConfigSO>();
            Flow = new GameFlow(Config);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                // 释放运行时自动创建的 config，避免泄漏
                if (config == null && Config != null)
                    Destroy(Config);
            }
        }

        private void Update()
        {
            Flow.Tick(Time.deltaTime);
        }

        // —— 暴露给 UI 的操作 ——
        public void StartDay() { Flow = new GameFlow(Config); }
        public void TogglePause() => Flow.Clock.IsPaused = !Flow.Clock.IsPaused;
        public void ToggleFastForward() =>
            Flow.Clock.TimeScale = Flow.Clock.TimeScale > 1f ? 1f : Config.fastForwardMultiplier;

        public void Brew() => Flow.Brew();
        public void Cup() => Flow.Cup();
        public void Serve() => Flow.ServeToEarliestWaiting();
        public void FeedCat() => Flow.FeedCat();
        public void CleanCat() => Flow.CleanCat();
        public void PetCat() => Flow.PetCat();
    }
}
