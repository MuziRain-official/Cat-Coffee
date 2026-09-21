using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 保温台选择栏（表现层，世界空间）。
    /// 长背景框 + 3 个餐位并排，A/D 选取，选中餐位黄色背景高亮，F 确认。
    /// </summary>
    public class WarmerSelectView : MonoBehaviour
    {
        private GameObject _panel;
        private GameObject[] _slots = new GameObject[3];        // 3 个餐位
        private SpriteRenderer[] _slotSRs = new SpriteRenderer[3];
        private SpriteRenderer[] _slotIcons = new SpriteRenderer[3]; // 每餐位菜品图标

        private void Start()
        {
            // 长背景框（世界尺寸 2.6 x 1.2）
            _panel = new GameObject("WarmerSelectPanel", typeof(SpriteRenderer));
            _panel.transform.SetParent(transform, false);
            _panel.transform.localScale = new Vector3(2.6f / 1.6f, 1.2f / 1.6f, 1f);
            _panel.transform.localPosition = new Vector3(0f, 2.1f / 1.6f, 0f);
            var bg = _panel.GetComponent<SpriteRenderer>();
            bg.sprite = SpriteUtil.White;
            bg.color = new Color(0.15f, 0.12f, 0.1f, 0.9f);
            bg.sortingOrder = 20;

            // 3 个餐位并排
            for (int i = 0; i < 3; i++)
            {
                var slot = new GameObject("Slot_" + i, typeof(SpriteRenderer));
                slot.transform.SetParent(_panel.transform, false);
                slot.transform.localScale = new Vector3(0.7f, 0.8f, 1f);
                float x = (i - 1) * 0.75f;
                slot.transform.localPosition = new Vector3(x, 0f, 0f);
                _slotSRs[i] = slot.GetComponent<SpriteRenderer>();
                _slotSRs[i].sprite = SpriteUtil.White;
                _slotSRs[i].color = new Color(0.3f, 0.28f, 0.25f, 0.9f);
                _slotSRs[i].sortingOrder = 21;
                _slots[i] = slot;

                // 菜品图标（餐位中央）
                var icon = new GameObject("Icon", typeof(SpriteRenderer));
                icon.transform.SetParent(slot.transform, false);
                icon.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                icon.transform.localPosition = Vector3.zero;
                _slotIcons[i] = icon.GetComponent<SpriteRenderer>();
                _slotIcons[i].sortingOrder = 22;
            }

            _panel.SetActive(false);
        }

        private void LateUpdate()
        {
            var flow = GameManager.Instance?.Flow;
            if (flow == null || _panel == null) return;

            bool show = flow.IsWarmerSelecting && flow.Warmer.Count > 0;
            _panel.SetActive(show);
            if (!show) return;

            var warmer = flow.Warmer;
            for (int i = 0; i < 3; i++)
            {
                if (i < warmer.Count)
                {
                    _slots[i].SetActive(true);
                    _slotIcons[i].sprite = PixelArtGenerator.CupIcon(warmer.Cups[i].Recipe);
                    // 选中餐位黄色背景
                    _slotSRs[i].color = (i == flow.WarmerCursor)
                        ? new Color(0.95f, 0.85f, 0.3f, 0.95f)  // 黄色高亮
                        : new Color(0.3f, 0.28f, 0.25f, 0.9f);  // 普通
                }
                else
                {
                    _slots[i].SetActive(false);
                }
            }
        }
    }
}
