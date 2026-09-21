using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 商店面板（表现层，世界空间）。
    /// 打开时显示当前道具名 + 价格 + 描述 + 是否已拥有。
    /// A/D 切换，F 购买，E 关闭。
    /// </summary>
    public class ShopView : MonoBehaviour
    {
        private GameObject _panel;
        private TextMesh _nameText;
        private TextMesh _priceText;
        private TextMesh _descText;
        private SpriteRenderer _iconSR;

        private void Start()
        {
            _panel = new GameObject("ShopPanel", typeof(SpriteRenderer));
            _panel.transform.SetParent(transform, false);
            _panel.transform.localScale = new Vector3(2.6f / 1.6f, 3.0f / 1.6f, 1f); // 加高
            _panel.transform.localPosition = new Vector3(0f, 2.8f / 1.6f, 0f);
            var bg = _panel.GetComponent<SpriteRenderer>();
            bg.sprite = SpriteUtil.White;
            bg.color = new Color(0.15f, 0.12f, 0.1f, 0.95f);
            bg.sortingOrder = 30;

            // 名字（顶部）
            _nameText = CreateText("Name", _panel.transform, new Vector3(0f, 1.15f, 0f), 60, 0.024f, Color.white, 31);
            // 价格（图标下方）
            _priceText = CreateText("Price", _panel.transform, new Vector3(0f, -0.35f, 0f), 44, 0.02f, new Color(1f, 0.85f, 0.4f), 31);
            // 描述（底部，两行，字号加大）
            _descText = CreateText("Desc", _panel.transform, new Vector3(0f, -0.85f, 0f), 40, 0.02f, new Color(0.9f, 0.9f, 0.9f), 31);

            // 道具图标（名字下方居中）
            var iconGo = new GameObject("ItemIcon", typeof(SpriteRenderer));
            iconGo.transform.SetParent(_panel.transform, false);
            iconGo.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            iconGo.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            _iconSR = iconGo.GetComponent<SpriteRenderer>();
            _iconSR.sortingOrder = 31;

            _panel.SetActive(false);
        }

        private TextMesh CreateText(string name, Transform parent, Vector3 pos, int size, float charSize, Color color, int order)
        {
            var go = new GameObject(name, typeof(TextMesh));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var tm = go.GetComponent<TextMesh>();
            tm.fontSize = size;
            tm.characterSize = charSize;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            go.GetComponent<MeshRenderer>().sortingOrder = order;
            return tm;
        }

        private void LateUpdate()
        {
            var flow = GameManager.Instance?.Flow;
            if (flow == null || _panel == null) return;

            bool show = flow.IsShopOpen;
            _panel.SetActive(show);
            if (!show) return;

            var item = flow.CurrentShopItem();
            _nameText.text = ItemDef.Name(item);
            _iconSR.sprite = PixelArtGenerator.ItemIcon(item); // 道具图标
            int price = ItemDef.Price(item);
            _priceText.text = flow.Progress.HasItem(item) ? "已拥有" : $"价格 {price} 金币";
            // 描述分行显示，避免挤在一行
            _descText.text = WrapText(ItemDef.Desc(item), 14) + "\n(A/D 切换  F 购买  E 关闭)";
        }

        /// <summary>按指定宽度（字符数）自动换行。</summary>
        private static string WrapText(string text, int charsPerLine)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= charsPerLine) return text;
            var sb = new System.Text.StringBuilder();
            int count = 0;
            foreach (var ch in text)
            {
                sb.Append(ch);
                count++;
                if (count >= charsPerLine)
                {
                    sb.Append('\n');
                    count = 0;
                }
            }
            return sb.ToString().TrimEnd('\n');
        }
    }
}
