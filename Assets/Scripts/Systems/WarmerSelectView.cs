using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 保温台选择栏（表现层，世界空间）。
    /// 打开选择模式时，在保温台上方显示：当前选中杯的菜品图标 + 顶部名字 + 左右杯预览。
    /// 玩家 A/D 切换，F 确认。
    /// </summary>
    public class WarmerSelectView : MonoBehaviour
    {
        private GameObject _panel;
        private SpriteRenderer _iconSR;
        private GameObject _nameBg;
        private TextMesh _nameText;

        private void Start()
        {
            // 面板底
            _panel = new GameObject("WarmerSelectPanel", typeof(SpriteRenderer));
            _panel.transform.SetParent(transform, false);
            _panel.transform.localScale = new Vector3(1.8f / 1.6f, 1.6f / 1.6f, 1f); // 世界 1.8x1.6
            _panel.transform.localPosition = new Vector3(0f, 2.2f / 1.6f, 0f);
            var bg = _panel.GetComponent<SpriteRenderer>();
            bg.sprite = SpriteUtil.White;
            bg.color = new Color(0.15f, 0.12f, 0.1f, 0.9f);
            bg.sortingOrder = 20;

            // 菜品图标（面板中央）
            var iconGo = new GameObject("SelectedIcon", typeof(SpriteRenderer));
            iconGo.transform.SetParent(_panel.transform, false);
            iconGo.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            iconGo.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            _iconSR = iconGo.GetComponent<SpriteRenderer>();
            _iconSR.sortingOrder = 21;

            // 顶部名字（用 TextMesh）
            _nameBg = new GameObject("NameBg", typeof(TextMesh));
            _nameBg.transform.SetParent(_panel.transform, false);
            _nameBg.transform.localPosition = new Vector3(0f, 0.62f, 0f);
            _nameText = _nameBg.GetComponent<TextMesh>();
            _nameText.fontSize = 48;
            _nameText.characterSize = 0.02f;
            _nameText.anchor = TextAnchor.MiddleCenter;
            _nameText.alignment = TextAlignment.Center;
            _nameText.color = Color.white;
            _nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var nameSr = _nameBg.GetComponent<MeshRenderer>();
            nameSr.sortingOrder = 22;

            _panel.SetActive(false);
        }

        private void LateUpdate()
        {
            var flow = GameManager.Instance?.Flow;
            if (flow == null || _panel == null) return;

            bool show = flow.IsWarmerSelecting && flow.Warmer.Count > 0;
            _panel.SetActive(show);
            if (!show) return;

            var cup = flow.Warmer.Cups[flow.WarmerCursor];
            _iconSR.sprite = PixelArtGenerator.CupIcon(cup.Recipe);
            _nameText.text = Recipe.Name(cup.Recipe);
        }
    }
}
