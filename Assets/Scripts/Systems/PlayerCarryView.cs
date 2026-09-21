using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 主角手中物品可视化（表现层）。
    /// 在主角头顶显示当前持有的东西（原料杯/成品杯/拉花杯）。
    /// 无持有物时隐藏。
    /// </summary>
    public class PlayerCarryView : MonoBehaviour
    {
        private SpriteRenderer _icon;
        private GameObject _iconGo;

        private void Start()
        {
            _iconGo = new GameObject("CarriedItem", typeof(SpriteRenderer));
            _iconGo.transform.SetParent(transform, false);
            _iconGo.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            _iconGo.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
            _icon = _iconGo.GetComponent<SpriteRenderer>();
            _icon.sortingOrder = 9;
            _iconGo.SetActive(false);
        }

        private void Update()
        {
            var flow = GameManager.Instance?.Flow;
            if (flow == null || _iconGo == null) return;

            var order = flow.Order;
            // 哪些状态表示"手里拿着东西"
            bool holding = order.Step == OrderStep.HoldingIngredients
                        || order.Step == OrderStep.ReadyToServe
                        || order.Step == OrderStep.ReadyToLatteArt
                        || order.Step == OrderStep.LatteArt;

            _iconGo.SetActive(holding);
            if (holding)
            {
                // 装杯后显示成品杯（含猫爪拉花），原料阶段显示原料杯
                bool isFinished = order.Step == OrderStep.ReadyToServe
                               || order.Step == OrderStep.ReadyToLatteArt
                               || order.Step == OrderStep.LatteArt;
                _icon.sprite = isFinished
                    ? PixelArtGenerator.CupIcon(order.Recipe)
                    : PixelArtGenerator.IngredientIcon(order.Recipe);
            }
        }
    }
}
