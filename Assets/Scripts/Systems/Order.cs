namespace CatCafe
{
    /// <summary>订单进度步骤（固定工序）。</summary>
    public enum OrderStep
    {
        None,               // 未开始
        Brewing,            // 第 1 步：萃取中（读条，原料在咖啡机里）
        ReadyToPickup,      // 萃取完成，原料在咖啡机，需回咖啡机取
        HoldingIngredients, // 主角持原料，需到装杯台装杯
        ReadyToServe,       // 装杯完成，主角持成品，需上菜
    }

    /// <summary>
    /// 订单（纯逻辑，可单元测试）。固定工序 4 步：
    /// 萃取(读条) → 回咖啡机取原料 → 装杯 → 上菜。
    /// </summary>
    public class Order
    {
        public OrderStep Step { get; private set; } = OrderStep.None;

        /// <summary>萃取读条进度 0–1。</summary>
        public float BrewProgress { get; private set; }

        /// <summary>开始第 1 步：萃取。</summary>
        public void StartBrewing()
        {
            Step = OrderStep.Brewing;
            BrewProgress = 0f;
        }

        /// <summary>推进萃取读条，完成后进入 ReadyToPickup（原料留在咖啡机）。</summary>
        public void Tick(float deltaTime, float brewSeconds)
        {
            if (Step != OrderStep.Brewing) return;
            BrewProgress += deltaTime / brewSeconds;
            if (BrewProgress >= 1f)
            {
                BrewProgress = 1f;
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

        /// <summary>上菜给顾客。成功返回 true 并复位订单。</summary>
        public bool Serve(Customer customer, float eatingSeconds)
        {
            if (Step != OrderStep.ReadyToServe) return false;
            if (customer == null || customer.Phase != CustomerPhase.Waiting) return false;

            customer.Serve(eatingSeconds);
            Step = OrderStep.None;
            return true;
        }
    }
}
