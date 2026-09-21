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
            var flow = GameManager.Instance?.Flow;

            // 保温台选择模式：A/D 移动，F 确认，E 关闭
            if (flow != null && flow.IsWarmerSelecting)
            {
                if (Input.GetKeyDown(KeyCode.A)) flow.MoveWarmerCursor(-1);
                if (Input.GetKeyDown(KeyCode.D)) flow.MoveWarmerCursor(+1);
                if (Input.GetKeyDown(KeyCode.F)) flow.ConfirmWarmerSelect();
                if (Input.GetKeyDown(KeyCode.E)) flow.CloseWarmerSelect();
                return;
            }

            // 咖啡机选菜品模式：A/D 移动，F 确认，E 关闭
            if (flow != null && flow.IsCoffeeSelecting)
            {
                if (Input.GetKeyDown(KeyCode.A)) flow.MoveCoffeeCursor(-1);
                if (Input.GetKeyDown(KeyCode.D)) flow.MoveCoffeeCursor(+1);
                if (Input.GetKeyDown(KeyCode.F)) flow.ConfirmCoffeeSelect();
                if (Input.GetKeyDown(KeyCode.E)) flow.CloseCoffeeSelect();
                return;
            }

            // 商店模式：A/D 移动，F 购买，E 关闭
            if (flow != null && flow.IsShopOpen)
            {
                if (Input.GetKeyDown(KeyCode.A)) flow.MoveShopCursor(-1);
                if (Input.GetKeyDown(KeyCode.D)) flow.MoveShopCursor(+1);
                if (Input.GetKeyDown(KeyCode.F)) BuyCurrentItem();
                if (Input.GetKeyDown(KeyCode.E)) flow.CloseShop();
                return;
            }

            if (Input.GetKeyDown(KeyCode.E)) Interact(0);
            if (Input.GetKeyDown(KeyCode.Q)) Interact(1);
            if (Input.GetKeyDown(KeyCode.R)) Interact(2);
            if (Input.GetKeyDown(KeyCode.F)) Carry();
        }

        /// <summary>商店购买当前道具。</summary>
        private void BuyCurrentItem()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var item = gm.Flow.CurrentShopItem();
            gm.BuyItem(item);
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

        /// <summary>F 键：抱猫 / 放猫（放猫必须在猫垫旁）。</summary>
        private void Carry()
        {
            var flow = GameManager.Instance?.Flow;
            if (flow == null) return;

            if (flow.Cat.IsCarried)
            {
                // 抱猫时：找主角附近的猫垫放下
                var pad = FindNearestPad();
                if (pad != null) flow.PutDownCat(pad.Zone);
            }
            else
            {
                // 没抱猫：找最近的猫实体抱起
                var cat = FindNearestCat();
                if (cat != null) flow.PickUpCat();
            }
        }

        /// <summary>找主角附近的猫实体（抱猫用）。</summary>
        private CatView FindNearestCat()
        {
            CatView best = null;
            float bestDist = float.MaxValue;
            Vector2 pos = transform.position;
            foreach (var cat in FindObjectsOfType<CatView>())
            {
                float d = Vector2.Distance(cat.transform.position, pos);
                if (d <= 1.8f && d < bestDist)
                {
                    bestDist = d;
                    best = cat;
                }
            }
            return best;
        }

        /// <summary>找主角交互范围内的猫垫（放猫用）。</summary>
        private CatPad FindNearestPad()
        {
            CatPad best = null;
            float bestDist = float.MaxValue;
            Vector2 pos = transform.position;

            foreach (var pad in FindObjectsOfType<CatPad>())
            {
                float d = Vector2.Distance(pad.transform.position, pos);
                if (d <= 1.8f && d < bestDist)
                {
                    bestDist = d;
                    best = pad;
                }
            }
            return best;
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
