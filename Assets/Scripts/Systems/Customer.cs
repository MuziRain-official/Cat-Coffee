namespace CatCafe
{
    /// <summary>顾客生命周期相位。</summary>
    public enum CustomerPhase
    {
        Entering, // 进店
        Waiting,  // 已点单，等待上菜（耐心倒计时）
        Eating,   // 已上菜，用餐中
        Left,     // 耐心耗尽流失（未付费）
        Paid      // 用餐完毕，付费离开
    }

    /// <summary>
    /// 顾客（纯逻辑状态机，可单元测试）。
    /// 不依赖 MonoBehaviour/ScriptableObject，参数由外部传入。
    /// </summary>
    public class Customer
    {
        public CustomerPhase Phase { get; private set; } = CustomerPhase.Entering;

        /// <summary>剩余耐心（秒），仅 Waiting 有效。</summary>
        public float RemainingPatience { get; private set; }

        /// <summary>剩余用餐时间（秒），仅 Eating 有效。</summary>
        public float RemainingEating { get; private set; }

        /// <summary>本单咖啡品质（上菜时锁定，付费时用于计价）。</summary>
        public BrewQuality ServedQuality { get; private set; } = BrewQuality.Good;

        /// <summary>是否已付费（成功完成一单）。</summary>
        public bool HasPaid => Phase == CustomerPhase.Paid;

        /// <summary>是否流失（未付费）。</summary>
        public bool HasLeft => Phase == CustomerPhase.Left;

        /// <summary>落座点单，进入等待。</summary>
        public void PlaceOrder(float patienceSeconds)
        {
            Phase = CustomerPhase.Waiting;
            RemainingPatience = patienceSeconds;
        }

        /// <summary>上菜，进入用餐并锁定品质。仅在等待中有效。</summary>
        public void Serve(float eatingSeconds, BrewQuality quality)
        {
            if (Phase != CustomerPhase.Waiting) return;
            Phase = CustomerPhase.Eating;
            RemainingEating = eatingSeconds;
            ServedQuality = quality;
        }

        /// <summary>推进状态机。patienceSlow 为耐心下降减速比例（0=不减速，0.25=慢25%）。</summary>
        public void Tick(float deltaTime, float patienceSlow = 0f)
        {
            switch (Phase)
            {
                case CustomerPhase.Waiting:
                    RemainingPatience -= deltaTime * (1f - patienceSlow);
                    if (RemainingPatience <= 0f) Phase = CustomerPhase.Left;
                    break;

                case CustomerPhase.Eating:
                    RemainingEating -= deltaTime;
                    if (RemainingEating <= 0f) Phase = CustomerPhase.Paid;
                    break;
            }
        }
    }
}
