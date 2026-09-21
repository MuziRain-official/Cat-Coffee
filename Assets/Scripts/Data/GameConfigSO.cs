using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 全局数值配置（唯一数据源，所有可调数值都放这里）。
    /// 逻辑代码只引用本 SO，不硬编码数值。调平衡 = 改 .asset，不碰代码。
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Cat Cafe/Game Config")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("时间系统")]
        [Tooltip("营业日时长（秒）")]
        public float dayDurationSeconds = 360f;
        [Tooltip("快进倍率")]
        public float fastForwardMultiplier = 2f;

        [Header("顾客系统（均衡紧张度）")]
        [Tooltip("生成间隔下限（秒）")]
        public float customerSpawnMin = 8f;
        [Tooltip("生成间隔上限（秒）")]
        public float customerSpawnMax = 14f;
        [Tooltip("同时在场上限（座位数）")]
        public int maxCustomers = 6;
        [Tooltip("耐心值（秒），点单后未上菜则流失")]
        public float customerPatienceSeconds = 45f;
        [Tooltip("用餐时长（秒），上菜后付费离开")]
        public float customerEatingSeconds = 12f;

        [Header("制作（萃取时机条小游戏）")]
        [Tooltip("指针摆动速度（弧度/秒）")]
        public float swingSpeed = 3f;
        [Tooltip("完美区半宽（指针离中心 0.5 多近算完美）")]
        public float perfectHalfWidth = 0.12f;
        [Tooltip("良好区半宽（超过完美区但在此内算良好）")]
        public float goodHalfWidth = 0.30f;
        [Tooltip("完美品质售价倍率")]
        public float perfectPriceMult = 1.3f;
        [Tooltip("良好品质售价倍率")]
        public float goodPriceMult = 1.0f;
        [Tooltip("勉强品质售价倍率")]
        public float poorPriceMult = 0.7f;

        [Header("猫咪系统")]
        public float catStatMin = 0f;
        public float catStatMax = 100f;
        [Tooltip("饥饿每 N 秒 -1")]
        public float hungerDecayInterval = 9f;
        [Tooltip("清洁每 N 秒 -1")]
        public float hygieneDecayInterval = 18f;
        [Tooltip("心情每 N 秒 -1（基础；饿或脏时翻倍）")]
        public float moodDecayInterval = 12f;
        [Tooltip("喂食恢复量")]
        public float feedRecovery = 50f;
        [Tooltip("铲屎恢复量")]
        public float cleanRecovery = 50f;
        [Tooltip("互动恢复量（心情）")]
        public float petRecovery = 25f;
        [Tooltip("三态均值 > 此阈值 → 增益")]
        public float boostThreshold = 70f;
        [Tooltip("三态均值 < 此阈值 → 惩罚")]
        public float penaltyThreshold = 30f;

        [Header("猫咪区域增益")]
        [Tooltip("吧台区：萃取小游戏完美区宽度加成比例")]
        public float barZonePerfectWidthBonus = 0.3f;
        [Tooltip("座位区：顾客耐心下降减速比例")]
        public float seatZonePatienceSlow = 0.25f;
        [Tooltip("门口区：顾客生成加速比例")]
        public float doorZoneSpawnBoost = 0.2f;
        [Tooltip("抱猫时主角移动减速比例")]
        public float carryCatSlow = 0.3f;

        [Header("经济系统")]
        [Tooltip("咖啡售价（金币）")]
        public int coffeePrice = 12;
        [Tooltip("咖啡成本（金币）")]
        public int coffeeCost = 4;
        [Tooltip("声望转化率（利润 × 比例）")]
        [Range(0f, 1f)]
        public float reputationRate = 0.1f;
    }
}
