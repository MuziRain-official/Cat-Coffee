namespace CatCafe
{
    /// <summary>订单进度步骤（固定工序）。</summary>
    public enum OrderStep
    {
        None,          // 未开始
        Brewing,       // 第 1 步：萃取中（读条）
        ReadyToCup,    // 萃取完成，待装杯
        ReadyToServe,  // 装杯完成，待上菜
    }

    /// <summary>
    /// 订单（纯逻辑，可单元测试）。固定工序 2 步：萃取(读条) → 装杯(即时) → 上菜。
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

        /// <summary>推进萃取读条，完成后进入 ReadyToCup。</summary>
        public void Tick(float deltaTime, float brewSeconds)
        {
            if (Step != OrderStep.Brewing) return;
            BrewProgress += deltaTime / brewSeconds;
            if (BrewProgress >= 1f)
            {
                BrewProgress = 1f;
                Step = OrderStep.ReadyToCup;
            }
        }

        /// <summary>第 2 步：装杯（即时）。仅在萃取完成后有效。</summary>
        public void Cup()
        {
            if (Step != OrderStep.ReadyToCup) return;
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
