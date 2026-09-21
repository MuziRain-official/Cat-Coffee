using System.Collections.Generic;

namespace CatCafe
{
    /// <summary>
    /// 经营进度（纯逻辑，可单元测试）。
    /// 跨天持久：第几天 / 总金币 / 已购道具。
    /// 每天经营结束后结算利润并入总金币，自动存档。
    /// </summary>
    public class Progress
    {
        /// <summary>当前是经营第几天（从 1 开始）。</summary>
        public int Day { get; private set; } = 1;

        /// <summary>总金币（跨天累积）。</summary>
        public int TotalCoins { get; private set; }

        /// <summary>已购道具。</summary>
        private readonly HashSet<ItemType> _ownedItems = new HashSet<ItemType>();

        public Progress() { }

        public Progress(int day, int totalCoins, IEnumerable<ItemType> owned)
        {
            Day = day;
            TotalCoins = totalCoins;
            if (owned != null)
                foreach (var item in owned)
                    _ownedItems.Add(item);
        }

        /// <summary>是否拥有某道具。</summary>
        public bool HasItem(ItemType item) => _ownedItems.Contains(item);

        /// <summary>所有已购道具。</summary>
        public IReadOnlyCollection<ItemType> OwnedItems => _ownedItems;

        /// <summary>购买道具（成功扣款返回 true）。</summary>
        public bool BuyItem(ItemType item)
        {
            if (HasItem(item)) return false;
            int price = ItemDef.Price(item);
            if (TotalCoins < price) return false;
            TotalCoins -= price;
            _ownedItems.Add(item);
            return true;
        }

        /// <summary>结算一天：把当天利润并入总金币，天数 +1。</summary>
        public void EndDay(int profit)
        {
            TotalCoins += profit;
            Day++;
        }

        /// <summary>调试：直接加金币。</summary>
        public void AddCoins(int amount)
        {
            TotalCoins += amount;
        }

        // —— 序列化 ——

        [System.Serializable]
        private class SaveData
        {
            public int day;
            public int totalCoins;
            public List<int> ownedItems;
        }

        public string ToJson()
        {
            var d = new SaveData
            {
                day = Day,
                totalCoins = TotalCoins,
                ownedItems = new List<int>(),
            };
            foreach (var item in _ownedItems) d.ownedItems.Add((int)item);
            return UnityEngine.JsonUtility.ToJson(d);
        }

        public static Progress FromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return new Progress();
            var d = UnityEngine.JsonUtility.FromJson<SaveData>(json);
            if (d == null) return new Progress();
            var items = new List<ItemType>();
            if (d.ownedItems != null)
                foreach (var i in d.ownedItems) items.Add((ItemType)i);
            return new Progress(d.day, d.totalCoins, items);
        }
    }
}
