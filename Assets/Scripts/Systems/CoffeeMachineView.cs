using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 咖啡机表现层：萃取读条（Extracting）时在咖啡机上方显示进度条。
    /// </summary>
    public class CoffeeMachineView : MonoBehaviour
    {
        private WorldBar _bar;

        private void Start()
        {
            // 进度条贴在咖啡机台面上（方块内部偏上，而非飘在上方空中）
            _bar = WorldBar.Create(transform, new Vector3(0f, 0.25f, 0f), 1.2f, 0.1f, new Color(0.9f, 0.6f, 0.2f));
            _bar.SetVisible(false);
        }

        private void LateUpdate()
        {
            var flow = GameManager.Instance?.Flow;
            if (flow == null || _bar == null) return;

            if (flow.Order.Step == OrderStep.Extracting)
            {
                _bar.SetVisible(true);
                _bar.SetProgress(flow.Order.ExtractProgress);
            }
            else
            {
                _bar.SetVisible(false);
            }
        }
    }
}
