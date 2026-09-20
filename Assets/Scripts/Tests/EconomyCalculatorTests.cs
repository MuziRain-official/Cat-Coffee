using NUnit.Framework;
using CatCafe;

namespace CatCafe.Tests
{
    public class EconomyCalculatorTests
    {
        [Test]
        public void Profit_SubtractsCostFromRevenue()
        {
            Assert.AreEqual(8, EconomyCalculator.Profit(12, 4));
        }

        [Test]
        public void TotalRevenue_MultipliesPriceByCount()
        {
            Assert.AreEqual(36, EconomyCalculator.TotalRevenue(12, 3));
        }

        [Test]
        public void TotalCost_MultipliesCostByCount()
        {
            Assert.AreEqual(12, EconomyCalculator.TotalCost(4, 3));
        }

        [Test]
        public void ReputationFromProfit_AppliesRateAndFloors()
        {
            // 利润 24 × 10% = 2
            Assert.AreEqual(2, EconomyCalculator.ReputationFromProfit(24, 0.1f));
            // 利润 9 × 10% = 0（向下取整）
            Assert.AreEqual(0, EconomyCalculator.ReputationFromProfit(9, 0.1f));
        }
    }
}
