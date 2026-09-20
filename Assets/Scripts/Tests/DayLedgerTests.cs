using NUnit.Framework;
using CatCafe;

namespace CatCafe.Tests
{
    public class DayLedgerTests
    {
        [Test]
        public void RecordCostAndRevenue_Accumulate()
        {
            var ledger = new DayLedger();
            ledger.RecordCost(4);
            ledger.RecordCost(4);
            ledger.RecordRevenue(12);
            ledger.RecordRevenue(12);

            Assert.AreEqual(8, ledger.Cost);
            Assert.AreEqual(24, ledger.Revenue);
        }

        [Test]
        public void Profit_SubtractsCostFromRevenue()
        {
            var ledger = new DayLedger();
            ledger.RecordRevenue(36);
            ledger.RecordCost(12);
            Assert.AreEqual(24, ledger.Profit);
        }

        [Test]
        public void Coins_EqualsRevenueMinusCost()
        {
            var ledger = new DayLedger();
            Assert.AreEqual(0, ledger.Coins);

            ledger.RecordRevenue(12);
            Assert.AreEqual(12, ledger.Coins);

            ledger.RecordCost(4);
            Assert.AreEqual(8, ledger.Coins);
        }

        [Test]
        public void Reputation_AppliesRateToProfit()
        {
            var ledger = new DayLedger();
            ledger.RecordRevenue(120);
            ledger.RecordCost(40); // 利润 80
            Assert.AreEqual(8, ledger.Reputation(0.1f)); // 80 × 10%
        }
    }
}
