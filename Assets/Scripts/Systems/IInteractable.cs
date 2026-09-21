using UnityEngine;

namespace CatCafe
{
    /// <summary>设备/交互点类型。</summary>
    public enum StationType
    {
        CoffeeMachine, // 萃取（拿铁/猫爪）
        Frother,       // 奶泡机（卡布奇诺）
        Counter,       // 装杯
        Warmer,        // 保温台（备餐/选杯取餐）
        CatNest        // 猫咪（E喂食/Q铲屎/R互动）
    }

    /// <summary>
    /// 可交互对象接口：主角靠近后按 E/Q/R 触发。
    /// 由 InteractionController 检测最近目标并分发。
    /// </summary>
    public interface IInteractable
    {
        /// <summary>是否当前可交互（业务状态允许）。</summary>
        bool IsInteractable { get; }

        /// <summary>主角是否在交互范围内。</summary>
        bool IsPlayerNear(Vector3 playerPos);

        void OnInteractPrimary();   // E
        void OnInteractSecondary(); // Q
        void OnInteractTertiary();  // R
    }
}
