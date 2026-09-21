using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 商店面板（表现层，世界坐标，挂场景根避免嵌套缩放）。
    /// 打开时显示道具图标 + 名字 + 价格 + 描述。
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
            _panel.transform.position = transform.position + new Vector3(0f, 2.6f, 0f);
            _panel.transform.localScale = new Vector3(2.8f, 3.2f, 1f); // 世界尺寸
            var bg = _panel.GetComponent<SpriteRenderer>();
            bg.sprite = SpriteUtil.White;
            bg.color = new Color(0.15f, 0.12f, 0.1f, 0.95f);
            bg.sortingOrder = 30;

            // 名字（顶部，世界字符大小 0.07）
            _nameText = CreateText("Name", _panel.transform, new Vector3(0f, 1.2f, 0f), 48, 0.07f, Color.white, 31);
            // 价格
            _priceText = CreateText("Price", _panel.transform, new Vector3(0f, -0.4f, 0f), 40, 0.06f, new Color(1f, 0.85f, 0.4f), 31);
            // 描述（底部，多行）
            _descText = CreateText("Desc", _panel.transform, new Vector3(0f, -0.9f, 0f), 36, 0.055f, new Color(0.9f, 0.9f, 0.9f), 31);

            // 道具图标（名字下方居中，世界 0.8）
            var iconGo = new GameObject("ItemIcon", typeof(SpriteRenderer));
            iconGo.transform.SetParent(_panel.transform, false);
            iconGo.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
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
            _iconSR.sprite = PixelArtGenerator.ItemIcon(item);
            int price = ItemDef.Price(item);
            _priceText.text = flow.Progress.HasItem(item) ? "已拥有" : $"价格 {price} 金币";
            _descText.text = WrapText(ItemDef.Desc(item), 12) + "\n(A/D 切换  F 购买  E 关闭)";
        }

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
