namespace CatCafe
{
    /// <summary>手持订单进度步骤（设备读条已独立出去）。</summary>
    public enum OrderStep
    {
        None,               // 手上没东西
        HoldingIngredients, // 主角持原料，需装杯
        LatteArt,           // 拉花小游戏进行中
        ReadyToServe,       // 成品完成，需上菜
    }

    /// <summary>
    /// 手持订单（纯逻辑）。只负责"玩家手上拿的东西"。
    /// 设备读条（萃取/奶泡）已独立到 GameFlow 的设备状态，互不阻塞。
    /// </summary>
    public class Order
    {
        public OrderStep Step { get; private set; } = OrderStep.None;

        /// <summary>当前手上的菜品。</summary>
        public RecipeType Recipe { get; private set; } = RecipeType.Latte;

        /// <summary>品质（从小游戏/设备读条继承）。</summary>
        public BrewQuality Quality { get; private set; } = BrewQuality.Good;

        /// <summary>新鲜度（现做=1；从保温台取会 <1）。</summary>
        public float Freshness { get; private set; } = 1f;

        /// <summary>手持某菜品的原料。</summary>
        public void HoldIngredients(RecipeType recipe, BrewQuality quality)
        {
            Step = OrderStep.HoldingIngredients;
            Recipe = recipe;
            Quality = quality;
            Freshness = 1f;
        }

        /// <summary>装杯。猫爪咖啡装杯后直接进入拉花。</summary>
        public void Cup()
        {
            if (Step != OrderStep.HoldingIngredients) return;
            var t = Recipe;
            Step = CatCafe.Recipe.NeedsLatteArt(t) ? OrderStep.LatteArt : OrderStep.ReadyToServe;
        }

        /// <summary>拉花完成，成品完成。</summary>
        public void CompleteLatteArt()
        {
            if (Step != OrderStep.LatteArt) return;
            Step = OrderStep.ReadyToServe;
        }

        /// <summary>从保温台取一杯端到手里。</summary>
        public bool TakeFromWarmer(RecipeType recipe, BrewQuality quality, float freshness)
        {
            if (Step != OrderStep.None) return false;
            Recipe = recipe;
            Quality = quality;
            Freshness = freshness;
            Step = OrderStep.ReadyToServe;
            return true;
        }

        /// <summary>复位订单到空闲。</summary>
        public void ResetToNone() => Step = OrderStep.None;

        /// <summary>上菜给顾客（菜品需匹配）。</summary>
        public bool Serve(Customer customer, float eatingSeconds)
        {
            if (Step != OrderStep.ReadyToServe) return false;
            if (customer == null || customer.Phase != CustomerPhase.Waiting) return false;
            if (customer.OrderedRecipe != Recipe) return false;

            customer.Serve(eatingSeconds, Quality, Freshness);
            Step = OrderStep.None;
            return true;
        }
    }
}
