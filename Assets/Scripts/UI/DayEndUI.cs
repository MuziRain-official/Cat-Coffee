using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CatCafe
{
    /// <summary>
    /// 结算界面 + 调试按钮（uGUI）。
    /// - 结算界面：当天盈利 + 「开始下一天」按钮
    /// - 右上角调试：立即结束当天 / +100金币
    /// </summary>
    public class DayEndUI : MonoBehaviour
    {
        private GameObject _dayEndRoot;
        private Text _profitText;

        private void Start()
        {
            BuildUI();
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (_dayEndRoot != null)
                _dayEndRoot.SetActive(gm.State == GameState.DayEnd);

            if (_dayEndRoot != null && _dayEndRoot.activeSelf && _profitText != null)
                _profitText.text = $"今天盈利：{gm.DayProfit} 金币";
        }

        private void BuildUI()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            if (FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasGo = new GameObject("DayEnd_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110; // 覆盖在 HUD 和菜单之上
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.transform.SetParent(transform, false);

            // —— 结算界面 ——
            _dayEndRoot = new GameObject("DayEndRoot", typeof(RectTransform), typeof(Image));
            _dayEndRoot.transform.SetParent(canvasGo.transform, false);
            var rt = _dayEndRoot.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _dayEndRoot.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);

            // 标题
            var title = NewText("Title", _dayEndRoot.transform, font, 64, new Vector2(0, 150), new Vector2(700, 100));
            title.text = "营业结束！";

            // 盈利
            _profitText = NewText("Profit", _dayEndRoot.transform, font, 48, new Vector2(0, 20), new Vector2(700, 80));
            _profitText.color = new Color(1f, 0.85f, 0.4f);

            // 开始下一天按钮
            MakeButton("开始下一天", new Vector2(0, -120), font, () => GameManager.Instance?.StartNextDay());

            _dayEndRoot.SetActive(false);

            // —— 右上角调试按钮（锚点在右上角，用负偏移）——
            MakeSmallButton("立即结束", new Vector2(-220, -50), font, () => GameManager.Instance?.DebugEndDay());
            MakeSmallButton("+100金币", new Vector2(-220, -110), font, () => GameManager.Instance?.DebugAddCoins());
        }

        private void MakeButton(string label, Vector2 centerPos, Font font, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_dayEndRoot.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = centerPos;
            rt.sizeDelta = new Vector2(360, 80);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.45f, 0.35f, 0.25f);
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var t = labelGo.GetComponent<Text>();
            t.font = font;
            t.fontSize = 36;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.text = label;
        }

        private void MakeSmallButton(string label, Vector2 topRightPos, Font font, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_dayEndRoot.transform.parent, false); // 挂 canvas 下，不是结算面板下
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = topRightPos;
            rt.sizeDelta = new Vector2(200, 50);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.3f, 0.5f, 0.4f);
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var t = labelGo.GetComponent<Text>();
            t.font = font;
            t.fontSize = 24;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.text = label;
        }

        private Text NewText(string name, Transform parent, Font font, int size, Vector2 centerPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = centerPos;
            rt.sizeDelta = sizeDelta;
            var t = go.GetComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }
    }
}
