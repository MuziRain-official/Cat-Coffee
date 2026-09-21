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
        [Tooltip("萃取读条时长（秒），小游戏完成后咖啡机实际萃取")]
        public float extractSeconds = 5f;

        [Header("猫咪系统（区域固定增益，无三态）")]
        [Tooltip("吧台区：制作时间加速比例（0.2=快20%）")]
        public float barZoneSpeedBoost = 0.2f;
        [Tooltip("吧台区：新鲜度下降减速比例（0.5=慢50%，即保鲜更久）")]
        public float barZoneFreshnessSlow = 0.5f;
        [Tooltip("餐桌旁：顾客停留更久比例（0.3=多等30%）")]
        public float seatZoneStayLonger = 0.3f;
        [Tooltip("门口区：顾客生成加速比例（0.3=多30%客流）")]
        public float doorZoneSpawnBoost = 0.3f;
        [Tooltip("抱猫时主角移动减速比例")]
        public float carryCatSlow = 0.3f;

        [Header("经济系统")]
        [Tooltip("拿铁售价（金币）")]
        public int lattePrice = 12;
        [Tooltip("拿铁成本（金币）")]
        public int latteCost = 4;
        [Tooltip("卡布奇诺售价（金币）")]
        public int cappuccinoPrice = 16;
        [Tooltip("卡布奇诺成本（金币）")]
        public int cappuccinoCost = 6;
        [Tooltip("猫爪咖啡售价（金币）")]
        public int catPawPrice = 22;
        [Tooltip("猫爪咖啡成本（金币）")]
        public int catPawCost = 8;
        [Tooltip("声望转化率（利润 × 比例）")]
        [Range(0f, 1f)]
        public float reputationRate = 0.1f;

        [Header("制作（下落式音游）")]
        [Tooltip("奶泡音符下落速度（y单位/秒）")]
        public float frothFallSpeed = 3f;

        [Header("制作（拉花小游戏）")]
        [Tooltip("拉花格子切换间隔（秒）")]
        public float latteArtCellInterval = 0.18f;

        [Header("备餐系统")]
        [Tooltip("保温台容量（杯）")]
        public int warmerCapacity = 3;
        [Tooltip("新鲜度时长（秒），100→0 匀速下降")]
        public float freshDurationSeconds = 90f;
    }
}
