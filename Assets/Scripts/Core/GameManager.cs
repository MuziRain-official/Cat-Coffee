using UnityEngine;

namespace CatCafe
{
    /// <summary>游戏状态。</summary>
    public enum GameState
    {
        Menu,    // 主菜单
        Playing, // 经营中
    }

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
        public Progress Progress { get; private set; }
        public GameState State { get; private set; } = GameState.Menu;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Config = config != null ? config : ScriptableObject.CreateInstance<GameConfigSO>();
            Progress = ProgressStore.Load(); // 读档
            Flow = new GameFlow(Config, Progress);
            State = GameState.Menu; // 启动进入主菜单
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

        private bool _dayEnded;

        private void Update()
        {
            if (State != GameState.Playing) return; // 菜单时不跑游戏

            Flow.Tick(Time.deltaTime);

            // 打烊自动结算并进入下一天（只结算一次）
            if (Flow.IsDayOver && !_dayEnded)
            {
                _dayEnded = true;
                EndDay();
            }

            if (Input.GetKeyDown(KeyCode.Space))
                TogglePause();
        }

        // —— 主菜单操作 ——

        /// <summary>开始游戏：全新进度（进度归零）。</summary>
        public void NewGame()
        {
            Progress = new Progress();           // 进度 0
            ProgressStore.Save(Progress);        // 覆盖存档
            Flow = new GameFlow(Config, Progress);
            _dayEnded = false;
            State = GameState.Playing;
            ApplyExtraTables();                  // 按进度显示/隐藏加桌
        }

        /// <summary>继续游戏：加载存档。</summary>
        public void ContinueGame()
        {
            Progress = ProgressStore.Load();     // 读档
            Flow = new GameFlow(Config, Progress);
            _dayEnded = false;
            State = GameState.Playing;
            ApplyExtraTables();
        }

        /// <summary>按进度显示/隐藏道具3的加桌。</summary>
        private void ApplyExtraTables()
        {
            bool has = Progress.HasItem(ItemType.ExtraTables);
            foreach (var name in new[] { "Table_D", "Table_E", "Seat_D1", "Seat_D2", "Seat_E1", "Seat_E2" })
            {
                var go = GameObject.Find(name);
                if (go != null) go.SetActive(has);
            }
        }

        /// <summary>回到主菜单。</summary>
        public void BackToMenu()
        {
            State = GameState.Menu;
        }

        // —— 暴露给 UI 的操作 ——
        public void StartDay() { Flow = new GameFlow(Config, Progress); }
        public void TogglePause() => Flow.Clock.IsPaused = !Flow.Clock.IsPaused;
        public void ToggleFastForward() =>
            Flow.Clock.TimeScale = Flow.Clock.TimeScale > 1f ? 1f : Config.fastForwardMultiplier;

        /// <summary>购买道具（永久，存档）。</summary>
        public bool BuyItem(ItemType item)
        {
            if (!Progress.BuyItem(item)) return false;
            ProgressStore.Save(Progress);
            return true;
        }

        /// <summary>结束营业日：结算利润→总金币→存档→下一天。</summary>
        public void EndDay()
        {
            int profit = Flow.Ledger.Profit;
            Progress.EndDay(profit);
            ProgressStore.Save(Progress);
            Flow = new GameFlow(Config, Progress); // 新的一天
            _dayEnded = false;
        }

        public void Brew() => Flow.Brew();
        public void Cup() => Flow.Cup();
        public void FeedCat() => Flow.FeedCat();
        public void CleanCat() => Flow.CleanCat();
        public void PetCat() => Flow.PetCat();
    }
}
