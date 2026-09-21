namespace CatCafe
{
    /// <summary>订单进度步骤（固定工序）。</summary>
    public enum OrderStep
    {
        None,               // 未开始
        Brewing,            // 萃取小游戏进行中（玩家在时机条里）
        Extracting,         // 小游戏完成，咖啡机实际萃取中（读条，可离开）
        ReadyToPickup,      // 萃取完成，咖啡液在咖啡机，需回咖啡机取
        HoldingIngredients, // 主角持原料，需到装杯台装杯
        ReadyToServe,       // 装杯完成，主角持成品，需上菜
    }

    /// <summary>
    /// 订单（纯逻辑，可单元测试）。固定工序 5 步：
    /// 萃取小游戏 → 咖啡机萃取读条 → 回咖啡机取原料 → 装杯 → 上菜。
    /// </summary>
    public class Order
    {
        public OrderStep Step { get; private set; } = OrderStep.None;

        /// <summary>本杯咖啡的品质（萃取小游戏锁定，上菜时用于计价）。</summary>
        public BrewQuality Quality { get; private set; } = BrewQuality.Good;

        /// <summary>本杯咖啡的新鲜度（1=现做；从保温台取会 <1）。</summary>
        public float Freshness { get; private set; } = 1f;

        /// <summary>萃取读条进度 0–1（Extracting 状态用）。</summary>
        public float ExtractProgress { get; private set; }

        /// <summary>开始第 1 步：萃取小游戏。</summary>
        public void StartBrewing()
        {
            Step = OrderStep.Brewing;
            Freshness = 1f; // 现做默认新鲜
            ExtractProgress = 0f;
        }

        /// <summary>小游戏停指针后，锁定品质并进入 Extracting（咖啡机开始实际萃取）。仅在萃取中有效。</summary>
        public void CompleteBrew(BrewQuality quality)
        {
            if (Step != OrderStep.Brewing) return;
            Quality = quality;
            Step = OrderStep.Extracting;
        }

        /// <summary>推进萃取读条，完成后进入 ReadyToPickup。仅在 Extracting 有效。</summary>
        public void TickExtract(float deltaTime, float extractSeconds)
        {
            if (Step != OrderStep.Extracting) return;
            if (extractSeconds <= 0f)
            {
                Step = OrderStep.ReadyToPickup;
                ExtractProgress = 1f;
                return;
            }
            ExtractProgress += deltaTime / extractSeconds;
            if (ExtractProgress >= 1f - 1e-4f) // 浮点容差
            {
                ExtractProgress = 1f;
                Step = OrderStep.ReadyToPickup;
            }
        }

        /// <summary>回咖啡机取原料。仅在萃取完成后有效。</summary>
        public void Pickup()
        {
            if (Step != OrderStep.ReadyToPickup) return;
            Step = OrderStep.HoldingIngredients;
        }

        /// <summary>第 2 步：装杯（即时）。仅在持原料时有效。</summary>
        public void Cup()
        {
            if (Step != OrderStep.HoldingIngredients) return;
            Step = OrderStep.ReadyToServe;
        }

        /// <summary>从保温台取一杯端到手里。仅在空闲时有效。成功返回 true。</summary>
        public bool TakeFromWarmer(BrewQuality quality, float freshness)
        {
            if (Step != OrderStep.None) return false;
            Quality = quality;
            Freshness = freshness;
            Step = OrderStep.ReadyToServe;
            return true;
        }

        /// <summary>复位订单到空闲（用于成品入保温台后清空手中状态）。</summary>
        public void ResetToNone() => Step = OrderStep.None;

        /// <summary>上菜给顾客。成功返回 true 并复位订单，同时把品质与新鲜度写进顾客。</summary>
        public bool Serve(Customer customer, float eatingSeconds)
        {
            if (Step != OrderStep.ReadyToServe) return false;
            if (customer == null || customer.Phase != CustomerPhase.Waiting) return false;

            customer.Serve(eatingSeconds, Quality, Freshness);
            Step = OrderStep.None;
            return true;
        }
    }
}
