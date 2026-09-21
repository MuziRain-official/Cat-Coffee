using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 主角交互控制器（挂在 Player 上）。
    /// 检测最近的 IInteractable，按 E/Q/R 分发对应动作。
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E)) Interact(0);
            if (Input.GetKeyDown(KeyCode.Q)) Interact(1);
            if (Input.GetKeyDown(KeyCode.R)) Interact(2);
            if (Input.GetKeyDown(KeyCode.F)) Carry();
        }

        private void Interact(int slot)
        {
            var target = FindNearest();
            if (target == null) return;

            switch (slot)
            {
                case 0: target.OnInteractPrimary(); break;
                case 1: target.OnInteractSecondary(); break;
                case 2: target.OnInteractTertiary(); break;
            }
        }

        /// <summary>F 键：抱猫/放猫（仅猫实体响应）。</summary>
        private void Carry()
        {
            var target = FindNearest();
            if (target is CatView cat)
                cat.ToggleCarry();
        }

        /// <summary>找距离最近且在范围内的可交互对象。</summary>
        private IInteractable FindNearest()
        {
            IInteractable best = null;
            float bestDist = float.MaxValue;
            Vector2 pos = transform.position;

            var monos = FindObjectsOfType<MonoBehaviour>();
            foreach (var m in monos)
            {
                if (m is IInteractable it && it.IsInteractable && it.IsPlayerNear(pos))
                {
                    float d = Vector2.Distance(m.transform.position, pos);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = it;
                    }
                }
            }
            return best;
        }
    }
}
