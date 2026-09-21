using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 咖啡机选菜品面板（表现层，世界空间）。
    /// 打开选择模式时显示：拿铁 / 猫爪咖啡 二选一，A/D 切换，F 确认，E 取消。
    /// </summary>
    public class CoffeeSelectView : MonoBehaviour
    {
        private GameObject _panel;
        private SpriteRenderer _iconSR;
        private TextMesh _nameText;
        private TextMesh _hintText;

        private static readonly RecipeType[] Options = { RecipeType.Latte, RecipeType.CatPaw };

        private void Start()
        {
            _panel = new GameObject("CoffeeSelectPanel", typeof(SpriteRenderer));
            _panel.transform.SetParent(transform, false);
            _panel.transform.localScale = new Vector3(1.8f / 1.6f, 1.8f / 1.6f, 1f);
            _panel.transform.localPosition = new Vector3(0f, 2.3f / 1.6f, 0f);
            var bg = _panel.GetComponent<SpriteRenderer>();
            bg.sprite = SpriteUtil.White;
            bg.color = new Color(0.15f, 0.12f, 0.1f, 0.9f);
            bg.sortingOrder = 20;

            // 图标
            var iconGo = new GameObject("Icon", typeof(SpriteRenderer));
            iconGo.transform.SetParent(_panel.transform, false);
            iconGo.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            iconGo.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            _iconSR = iconGo.GetComponent<SpriteRenderer>();
            _iconSR.sortingOrder = 21;

            // 名字
            var nameGo = new GameObject("Name", typeof(TextMesh));
            nameGo.transform.SetParent(_panel.transform, false);
            nameGo.transform.localPosition = new Vector3(0f, 0.62f, 0f);
            _nameText = nameGo.GetComponent<TextMesh>();
            _nameText.fontSize = 48;
            _nameText.characterSize = 0.02f;
            _nameText.anchor = TextAnchor.MiddleCenter;
            _nameText.alignment = TextAlignment.Center;
            _nameText.color = Color.white;
            _nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameGo.GetComponent<MeshRenderer>().sortingOrder = 22;

            // 提示
            var hintGo = new GameObject("Hint", typeof(TextMesh));
            hintGo.transform.SetParent(_panel.transform, false);
            hintGo.transform.localPosition = new Vector3(0f, -0.62f, 0f);
            _hintText = hintGo.GetComponent<TextMesh>();
            _hintText.fontSize = 32;
            _hintText.characterSize = 0.016f;
            _hintText.anchor = TextAnchor.MiddleCenter;
            _hintText.alignment = TextAlignment.Center;
            _hintText.color = new Color(1f, 0.9f, 0.5f);
            _hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hintGo.GetComponent<MeshRenderer>().sortingOrder = 22;
            _hintText.text = "A/D 切换  F 确认  E 取消";

            _panel.SetActive(false);
        }

        private void LateUpdate()
        {
            var flow = GameManager.Instance?.Flow;
            if (flow == null || _panel == null) return;

            bool show = flow.IsCoffeeSelecting;
            _panel.SetActive(show);
            if (!show) return;

            var recipe = Options[flow.CoffeeSelectCursor];
            _iconSR.sprite = PixelArtGenerator.CupIcon(recipe);
            _nameText.text = Recipe.Name(recipe);
        }
    }
}
