using System.Collections.Generic;

namespace CatCafe
{
    /// <summary>一杯放在保温台的成品咖啡。</summary>
    public class WarmCup
    {
        public RecipeType Recipe;
        public BrewQuality Quality;
        /// <summary>新鲜度 0–1（1=刚出炉，随时间下降）。</summary>
        public float Freshness;
    }

    /// <summary>
    /// 保温台（纯逻辑，可单元测试）。
    /// 提前备好的成品咖啡存放在此，新鲜度随时间下降。
    /// FIFO：先放的先取出，自然形成"先备先用"循环。
    /// </summary>
    public class Warmer
    {
        private readonly List<WarmCup> _cups = new List<WarmCup>();
        private readonly int _capacity;

        public Warmer(int capacity) => _capacity = capacity;

        /// <summary>当前杯数。</summary>
        public int Count => _cups.Count;

        /// <summary>只读杯列表（表现层读新鲜度用）。</summary>
        public IReadOnlyList<WarmCup> Cups => _cups;

        /// <summary>是否已满。</summary>
        public bool IsFull => _cups.Count >= _capacity;

        /// <summary>放入一杯（成功返回 true）。</summary>
        public bool Store(RecipeType recipe, BrewQuality quality)
        {
            if (IsFull) return false;
            _cups.Add(new WarmCup { Recipe = recipe, Quality = quality, Freshness = 1f });
            return true;
        }

        /// <summary>取出一杯（FIFO）。空则返回 null。</summary>
        public WarmCup Take()
        {
            if (_cups.Count == 0) return null;
            var cup = _cups[0];
            _cups.RemoveAt(0);
            return cup;
        }

        /// <summary>按索引取出一杯（选杯用）。越界返回 null。</summary>
        public WarmCup TakeAt(int index)
        {
            if (index < 0 || index >= _cups.Count) return null;
            var cup = _cups[index];
            _cups.RemoveAt(index);
            return cup;
        }

        /// <summary>把一杯放回队首（保留新鲜度）。用于取出后无顾客可上菜的撤回。</summary>
        public void ReturnCup(WarmCup cup)
        {
            if (cup == null) return;
            _cups.Insert(0, cup);
        }

        /// <summary>所有杯新鲜度随时间下降。</summary>
        public void Tick(float deltaTime, float freshDurationSeconds)
        {
            if (freshDurationSeconds <= 0f) return;
            foreach (var cup in _cups)
                cup.Freshness = UnityEngine.Mathf.Max(0f, cup.Freshness - deltaTime / freshDurationSeconds);
        }
    }
}
