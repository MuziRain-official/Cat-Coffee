namespace CatCafe
{
    /// <summary>订单进度步骤（固定工序）。</summary>
    public enum OrderStep
    {
        None,               // 未开始
        Brewing,            // 萃取时机条进行中
        Extracting,         // 咖啡机实际萃取读条中
        Frothing,           // 打奶泡节奏连击进行中
        ReadyToPickup,      // 原料在设备，需回去取
        HoldingIngredients, // 主角持原料，需装杯
        ReadyToLatteArt,    // 装杯完成，需拉花
        LatteArt,           // 拉花小游戏进行中
        ReadyToServe,       // 成品完成，需上菜
    }

    /// <summary>
    /// 订单（纯逻辑，可单元测试）。根据菜品类型跑不同工序链：
    /// 拿铁：萃取→读条→取料→装杯
    /// 卡布：打奶泡→取料→装杯
    /// 猫爪：萃取→读条→取料→装杯→拉花
    /// </summary>
    public class Order
    {
        public OrderStep Step { get; private set; } = OrderStep.None;

        /// <summary>当前制作的菜品。</summary>
        public RecipeType Recipe { get; private set; } = RecipeType.Latte;

        /// <summary>本杯咖啡的品质（小游戏锁定，上菜时用于计价）。</summary>
        public BrewQuality Quality { get; private set; } = BrewQuality.Good;

        /// <summary>本杯咖啡的新鲜度（1=现做；从保温台取会 <1）。</summary>
        public float Freshness { get; private set; } = 1f;

        /// <summary>萃取读条进度 0–1（Extracting 状态用）。</summary>
        public float ExtractProgress { get; private set; }

        /// <summary>开始制作某个菜品。</summary>
        public void Start(RecipeType recipe)
        {
            Step = OrderStep.None;
            Recipe = recipe;
            Freshness = 1f;
            ExtractProgress = 0f;
        }

        /// <summary>开始萃取时机条（拿铁/猫爪的第 1 步）。</summary>
        public void StartBrewing()
        {
            Step = OrderStep.Brewing;
        }

        /// <summary>萃取小游戏完成，锁定品质并进入读条。</summary>
        public void CompleteBrew(BrewQuality quality)
        {
            if (Step != OrderStep.Brewing) return;
            Quality = quality;
            Step = OrderStep.Extracting;
        }

        /// <summary>推进萃取读条，完成后进入 ReadyToPickup。</summary>
        public void TickExtract(float deltaTime, float extractSeconds)
        {
            if (Step != OrderStep.Extracting) return;
            if (extractSeconds <= 0f) { Step = OrderStep.ReadyToPickup; ExtractProgress = 1f; return; }
            ExtractProgress += deltaTime / extractSeconds;
            if (ExtractProgress >= 1f - 1e-4f)
            {
                ExtractProgress = 1f;
                Step = OrderStep.ReadyToPickup;
            }
        }

        /// <summary>开始打奶泡（卡布的第 1 步）。</summary>
        public void StartFrothing()
        {
            Step = OrderStep.Frothing;
        }

        /// <summary>打奶泡完成，锁定品质并进入 ReadyToPickup。</summary>
        public void CompleteFroth(BrewQuality quality)
        {
            if (Step != OrderStep.Frothing) return;
            Quality = quality;
            Step = OrderStep.ReadyToPickup;
        }

        /// <summary>回设备取原料。</summary>
        public void Pickup()
        {
            if (Step != OrderStep.ReadyToPickup) return;
            Step = OrderStep.HoldingIngredients;
        }

        /// <summary>装杯。猫爪咖啡装杯后进入 ReadyToLatteArt（需拉花），否则 ReadyToServe。</summary>
        public void Cup()
        {
            if (Step != OrderStep.HoldingIngredients) return;
            var t = Recipe;
            Step = CatCafe.Recipe.NeedsLatteArt(t) ? OrderStep.ReadyToLatteArt : OrderStep.ReadyToServe;
        }

        /// <summary>开始拉花（猫爪的收尾步骤）。</summary>
        public void StartLatteArt()
        {
            if (Step != OrderStep.ReadyToLatteArt) return;
            Step = OrderStep.LatteArt;
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

        /// <summary>上菜给顾客。需菜品匹配。</summary>
        public bool Serve(Customer customer, float eatingSeconds)
        {
            if (Step != OrderStep.ReadyToServe) return false;
            if (customer == null || customer.Phase != CustomerPhase.Waiting) return false;
            if (customer.OrderedRecipe != Recipe) return false; // 上错菜

            customer.Serve(eatingSeconds, Quality, Freshness);
            Step = OrderStep.None;
            return true;
        }
    }
}
