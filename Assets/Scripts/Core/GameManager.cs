using UnityEngine;

namespace CatCafe
{
    /// <summary>游戏状态。</summary>
    public enum GameState
    {
        Menu,    // 主菜单
        Playing, // 经营中
        DayEnd,  // 当天结束，显示结算
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
        /// <summary>当天结算的利润（结算界面显示用）。</summary>
        public int DayProfit { get; private set; }

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
            if (Flow == null) return; // 防御：未初始化时跳过
            if (State != GameState.Playing) return; // 菜单/结算时不跑游戏

            Flow.Tick(Time.deltaTime);

            // 打烊自动进入结算界面（暂停，不结算）
            if (Flow.IsDayOver && !_dayEnded)
            {
                _dayEnded = true;
                ShowDayEnd();
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
            ApplyExtraTables();
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
            WorldBuilder.SetExtraTablesVisible(Progress.HasItem(ItemType.ExtraTables));
        }

        /// <summary>回到主菜单。</summary>
        public void BackToMenu()
        {
            State = GameState.Menu;
        }

        // —— 结算日流程 ——

        /// <summary>进入结算界面：记录当天利润，暂停游戏。</summary>
        public void ShowDayEnd()
        {
            DayProfit = Flow.Ledger.Profit;
            State = GameState.DayEnd;
        }

        /// <summary>开始下一天：结算利润→总金币→存档→新的一天。</summary>
        public void StartNextDay()
        {
            Progress.EndDay(DayProfit);          // 当天利润入账
            ProgressStore.Save(Progress);
            Flow = new GameFlow(Config, Progress); // 新的一天
            _dayEnded = false;
            State = GameState.Playing;
            ApplyExtraTables();
        }

        // —— 调试按钮 ——

        /// <summary>调试：立即结束当天（进入结算界面）。</summary>
        public void DebugEndDay()
        {
            if (State != GameState.Playing) return;
            _dayEnded = true;
            ShowDayEnd();
        }

        /// <summary>调试：加 100 金币到总金币。</summary>
        public void DebugAddCoins()
        {
            if (State != GameState.Playing) return;
            // 通过 EndDay 机制：直接给 Progress 加钱（临时借道，不推进天数）
            var p = Progress;
            // 用反射不可行，直接构造新 Progress 或加个方法
            // 简化：给 Progress 加公开方法 AddCoins
            p.AddCoins(100);
            ProgressStore.Save(Progress);
        }

        // —— 暴露给 UI 的操作 ——
        public void StartDay() { Flow = new GameFlow(Config, Progress); }
        public void TogglePause() => Flow.Clock.IsPaused = !Flow.Clock.IsPaused;
        public void ToggleFastForward() =>
            Flow.Clock.TimeScale = Flow.Clock.TimeScale > 1f ? 1f : Config.fastForwardMultiplier;

        /// <summary>购买道具（永久，存档）。购买后立即应用效果。</summary>
        public bool BuyItem(ItemType item)
        {
            if (!Progress.BuyItem(item)) return false;
            ProgressStore.Save(Progress);
            if (item == ItemType.ExtraTables)
                ApplyExtraTables(); // 立即显示加桌
            return true;
        }

        /// <summary>结束营业日（已废弃，改用 ShowDayEnd + StartNextDay）。</summary>
        public void EndDay()
        {
            ShowDayEnd();
        }

        public void Brew() => Flow.Brew();
        public void Cup() => Flow.Cup();
        public void FeedCat() => Flow.FeedCat();
        public void CleanCat() => Flow.CleanCat();
        public void PetCat() => Flow.PetCat();
    }
}
