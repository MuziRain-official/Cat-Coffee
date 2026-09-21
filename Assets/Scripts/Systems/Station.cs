using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 设备交互点：咖啡机/奶泡机/装杯台/保温台/猫窝。
    /// 主角靠近后按 E（或 Q/R）触发，转成 GameFlow 的逻辑方法调用。
    /// </summary>
    public class Station : MonoBehaviour, IInteractable
    {
        public StationType Type;
        public float interactRadius = 1.6f;

        private GameFlow Flow => GameManager.Instance != null ? GameManager.Instance.Flow : null;

        public bool IsInteractable => Flow != null;

        public bool IsPlayerNear(Vector3 playerPos) =>
            Vector2.Distance(transform.position, playerPos) <= interactRadius;

        public void OnInteractPrimary()
        {
            switch (Type)
            {
                case StationType.CoffeeMachine: CoffeeMachineInteract(); break;
                case StationType.Frother: FrotherInteract(); break;
                case StationType.Counter: CounterInteract(); break;
                case StationType.Warmer: WarmerInteract(); break;
                case StationType.Shop: Flow?.OpenShop(); break;
                case StationType.CatNest: Flow?.FeedCat(); break;
            }
        }

        /// <summary>咖啡机：空闲→打开选菜品；萃取好→取原料；小游戏中→停指针。</summary>
        private void CoffeeMachineInteract()
        {
            if (Flow == null) return;
            if (Flow.IsBrewGameActive)
                Flow.StopBrewGame();                     // 小游戏进行中 → 停指针
            else if (Flow.Order.Step == OrderStep.ReadyToPickup
                     && (Flow.Order.Recipe == RecipeType.Latte || Flow.Order.Recipe == RecipeType.CatPaw))
                Flow.Pickup();                           // 萃取好了且是咖啡机菜品 → 取原料
            else if (Flow.Order.Step == OrderStep.None)
                Flow.OpenCoffeeSelect();                 // 空闲 → 打开选菜品（拿铁/猫爪）
        }

        /// <summary>奶泡机：空闲→做卡布；音游中→按E判定；ReadyToPickup且是卡布→取原料。</summary>
        private void FrotherInteract()
        {
            if (Flow == null) return;
            if (Flow.IsFrothGameActive)
                Flow.TapFroth();                         // 音游中 → 按E判定
            else if (Flow.Order.Step == OrderStep.ReadyToPickup
                     && Flow.Order.Recipe == RecipeType.Cappuccino)
                Flow.Pickup();                           // 奶泡好且是卡布 → 取原料
            else if (Flow.Order.Step == OrderStep.None)
                Flow.Froth();                            // 空闲 → 做卡布
        }

        /// <summary>装杯台：持原料→装杯；猫爪装杯后→开始拉花；拉花中→停。</summary>
        private void CounterInteract()
        {
            if (Flow == null) return;
            if (Flow.IsLatteArtGameActive)
                Flow.StopLatteArt();                     // 拉花中 → 停
            else if (Flow.Order.Step == OrderStep.HoldingIngredients)
                Flow.Cup();                              // 持原料 → 装杯
            else if (Flow.Order.Step == OrderStep.ReadyToLatteArt)
                Flow.StartLatteArt();                    // 猫爪装杯后 → 开始拉花
        }

        /// <summary>保温台：手里有成品→放入；手里没东西→打开选择栏。</summary>
        private void WarmerInteract()
        {
            if (Flow == null) return;
            if (Flow.Order.Step == OrderStep.ReadyToServe)
                Flow.StoreToWarmer();                    // 手里有成品 → 备餐放入
            else
                Flow.OpenWarmerSelect();                 // 手里没东西 → 打开选择栏
        }

        public void OnInteractSecondary()
        {
            switch (Type)
            {
                case StationType.Warmer: Flow?.MoveWarmerCursor(-1); break; // Q：光标左移
                case StationType.CatNest: Flow?.CleanCat(); break;
            }
        }

        public void OnInteractTertiary()
        {
            switch (Type)
            {
                case StationType.Warmer: Flow?.MoveWarmerCursor(+1); break; // R：光标右移
                case StationType.CatNest: Flow?.PetCat(); break;
            }
        }
    }
}
