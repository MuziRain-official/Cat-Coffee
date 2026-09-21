using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 保温台选择栏（表现层，世界坐标，挂在场景根避免嵌套缩放）。
    /// 长背景框 + 3 个餐位并排，A/D 选取，选中餐位黄色背景高亮，F 确认。
    /// </summary>
    public class WarmerSelectView : MonoBehaviour
    {
        private GameObject _panel;
        private GameObject[] _slots = new GameObject[3];
        private SpriteRenderer[] _slotSRs = new SpriteRenderer[3];
        private SpriteRenderer[] _slotIcons = new SpriteRenderer[3];
        private TextMesh[] _slotNames = new TextMesh[3];

        private void Start()
        {
            // 面板容器：scale=1，挂场景根，世界位置
            _panel = new GameObject("WarmerSelectPanel");
            _panel.transform.position = transform.position + new Vector3(0f, 2.2f, 0f);
            _panel.transform.localScale = Vector3.one; // 关键：不缩放容器

            // 背景框（子 sprite 单独缩放成 2.6 x 1.4）
            var bgGo = new GameObject("BG", typeof(SpriteRenderer));
            bgGo.transform.SetParent(_panel.transform, false);
            bgGo.transform.localScale = new Vector3(2.6f, 1.4f, 1f);
            var bg = bgGo.GetComponent<SpriteRenderer>();
            bg.sprite = SpriteUtil.White;
            bg.color = new Color(0.15f, 0.12f, 0.1f, 0.95f);
            bg.sortingOrder = 20;

            // 3 个餐位并排（世界尺寸 0.7x0.85，间距 0.8）
            for (int i = 0; i < 3; i++)
            {
                var slot = new GameObject("Slot_" + i, typeof(SpriteRenderer));
                slot.transform.SetParent(_panel.transform, false);
                slot.transform.localScale = new Vector3(0.7f, 0.85f, 1f);
                slot.transform.localPosition = new Vector3((i - 1) * 0.8f, 0f, 0f);
                _slotSRs[i] = slot.GetComponent<SpriteRenderer>();
                _slotSRs[i].sprite = SpriteUtil.White;
                _slotSRs[i].color = new Color(0.3f, 0.28f, 0.25f, 0.9f);
                _slotSRs[i].sortingOrder = 21;
                _slots[i] = slot;

                // 菜品图标（世界 0.5）
                var icon = new GameObject("Icon", typeof(SpriteRenderer));
                icon.transform.SetParent(slot.transform, false);
                icon.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                icon.transform.localPosition = new Vector3(0f, 0.12f, 0f);
                _slotIcons[i] = icon.GetComponent<SpriteRenderer>();
                _slotIcons[i].sortingOrder = 22;

                // 头顶菜品名字（世界字符大小 0.05）
                var nameGo = new GameObject("Name", typeof(TextMesh));
                nameGo.transform.SetParent(slot.transform, false);
                nameGo.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                var tm = nameGo.GetComponent<TextMesh>();
                tm.fontSize = 40;
                tm.characterSize = 0.05f;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.color = Color.white;
                tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                nameGo.GetComponent<MeshRenderer>().sortingOrder = 23;
                _slotNames[i] = tm;
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
                    _slotNames[i].text = Recipe.Name(warmer.Cups[i].Recipe);
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
