using UnityEngine;

namespace CatCafe
{
    /// <summary>猫所在的区域（猫垫），决定增益类型。</summary>
    public enum CatZone
    {
        Nest, // 默认猫窝（无增益）
        Bar,  // 吧台旁：制作加速 + 保鲜度更高
        Seat, // 餐桌旁：顾客停留更久
        Door, // 门口：吸引更多顾客
    }

    /// <summary>
    /// 猫咪（纯逻辑）。无三态数值，猫是纯策略道具：
    /// 玩家抱猫(F)放到不同区域，产生对应固定增益。
    /// </summary>
    public class CatState
    {
        /// <summary>猫当前所在区域。</summary>
        public CatZone Zone { get; private set; } = CatZone.Nest;

        /// <summary>是否正被主角抱着。</summary>
        public bool IsCarried { get; private set; }

        /// <summary>抱起猫。</summary>
        public void PickUp() => IsCarried = true;

        /// <summary>放下猫（放下时确定落在哪个区域）。</summary>
        public void PutDown(CatZone zone)
        {
            IsCarried = false;
            Zone = zone;
        }
    }
}
