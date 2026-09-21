using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 设备交互点：咖啡机 / 前台 / 猫窝。
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
                case StationType.Counter: Flow?.Cup(); break;
                case StationType.CatNest: Flow?.FeedCat(); break;
            }
        }

        /// <summary>咖啡机智能分发：小游戏中按E=停指针，萃取好按E=取原料，否则=开始萃取。</summary>
        private void CoffeeMachineInteract()
        {
            if (Flow == null) return;
            if (Flow.IsBrewGameActive)
                Flow.StopBrewGame();                     // 小游戏进行中 → 停指针锁定品质
            else if (Flow.Order.Step == OrderStep.ReadyToPickup)
                Flow.Pickup();                           // 萃取好了 → 回来取原料
            else
                Flow.Brew();                             // 空闲 → 开始萃取小游戏
        }

        public void OnInteractSecondary()
        {
            if (Type == StationType.CatNest) Flow?.CleanCat();
        }

        public void OnInteractTertiary()
        {
            if (Type == StationType.CatNest) Flow?.PetCat();
        }
    }
}
