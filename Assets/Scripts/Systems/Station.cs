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
                case StationType.CoffeeMachine: Flow?.Brew(); break;
                case StationType.Counter: Flow?.Cup(); break;
                case StationType.CatNest: Flow?.FeedCat(); break;
            }
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
