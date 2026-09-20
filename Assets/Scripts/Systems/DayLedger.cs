namespace CatCafe
{
    /// <summary>
    /// 营业日账本（纯逻辑，可单元测试）。
    /// 记录真实现金流：制作时扣成本，顾客付费时记收入。
    /// 结算时汇总利润与声望。
    /// </summary>
    public class DayLedger
    {
        /// <summary>总营业额。</summary>
        public int Revenue { get; private set; }

        /// <summary>总成本。</summary>
        public int Cost { get; private set; }

        /// <summary>当前金币（局内可用余额）。</summary>
        public int Coins => Revenue - Cost;

        /// <summary>利润 = 营业额 − 成本。</summary>
        public int Profit => EconomyCalculator.Profit(Revenue, Cost);

        /// <summary>记录一笔成本（制作时调用）。</summary>
        public void RecordCost(int amount) => Cost += amount;

        /// <summary>记录一笔收入（顾客付费时调用）。</summary>
        public void RecordRevenue(int amount) => Revenue += amount;

        /// <summary>结算声望 = 利润 × 转化率（向下取整）。</summary>
        public int Reputation(float rate) => EconomyCalculator.ReputationFromProfit(Profit, rate);
    }
}
