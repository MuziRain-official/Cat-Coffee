namespace CatCafe
{
    /// <summary>
    /// 经济纯函数计算器：不依赖 MonoBehaviour / ScriptableObject / 场景，
    /// 全部静态纯函数，可直接单元测试。
    /// </summary>
    public static class EconomyCalculator
    {
        /// <summary>总收入 = 售价 × 数量</summary>
        public static int TotalRevenue(int price, int count) => price * count;

        /// <summary>总成本 = 成本 × 数量</summary>
        public static int TotalCost(int cost, int count) => cost * count;

        /// <summary>利润 = 收入 − 成本</summary>
        public static int Profit(int revenue, int cost) => revenue - cost;

        /// <summary>声望 = 利润 × 转化率（向下取整）</summary>
        public static int ReputationFromProfit(int profit, float rate) => (int)(profit * rate);
    }
}
