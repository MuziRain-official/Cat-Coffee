namespace CatCafe
{
    /// <summary>订单进度步骤（固定工序）。</summary>
    public enum OrderStep
    {
        None,               // 未开始
        Brewing,            // 萃取小游戏进行中（玩家在时机条里）
        ReadyToPickup,      // 品质锁定，咖啡液在咖啡机，需回咖啡机取
        HoldingIngredients, // 主角持原料，需到装杯台装杯
        ReadyToServe,       // 装杯完成，主角持成品，需上菜
    }

    /// <summary>
    /// 订单（纯逻辑，可单元测试）。固定工序 4 步：
    /// 萃取小游戏 → 回咖啡机取原料 → 装杯 → 上菜。
    /// </summary>
    public class Order
    {
        public OrderStep Step { get; private set; } = OrderStep.None;

        /// <summary>本杯咖啡的品质（萃取小游戏锁定，上菜时用于计价）。</summary>
        public BrewQuality Quality { get; private set; } = BrewQuality.Good;

        /// <summary>开始第 1 步：萃取小游戏。</summary>
        public void StartBrewing()
        {
            Step = OrderStep.Brewing;
        }

        /// <summary>小游戏停指针后，锁定品质并进入 ReadyToPickup。仅在萃取中有效。</summary>
        public void CompleteBrew(BrewQuality quality)
        {
            if (Step != OrderStep.Brewing) return;
            Quality = quality;
            Step = OrderStep.ReadyToPickup;
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

        /// <summary>上菜给顾客。成功返回 true 并复位订单，同时把品质写进顾客。</summary>
        public bool Serve(Customer customer, float eatingSeconds)
        {
            if (Step != OrderStep.ReadyToServe) return false;
            if (customer == null || customer.Phase != CustomerPhase.Waiting) return false;

            customer.Serve(eatingSeconds, Quality);
            Step = OrderStep.None;
            return true;
        }
    }
}
