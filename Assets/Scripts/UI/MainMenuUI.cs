using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CatCafe
{
    /// <summary>
    /// 主菜单 UI（uGUI 全屏覆盖）。
    /// 显示两个选项：开始游戏（新档）/ 继续游戏（读档）。
    /// 菜单状态下覆盖整个画面，进入游戏后隐藏。
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private GameObject _menuRoot;

        private void Start()
        {
            BuildUI();
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || _menuRoot == null) return;
            _menuRoot.SetActive(gm.State == GameState.Menu);
        }

        private void BuildUI()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            if (FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasGo = new GameObject("Menu_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // 覆盖在 HUD 之上
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.transform.SetParent(transform, false);

            _menuRoot = new GameObject("MenuRoot", typeof(RectTransform), typeof(Image));
            _menuRoot.transform.SetParent(canvasGo.transform, false);
            var rt = _menuRoot.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _menuRoot.GetComponent<Image>().color = new Color(0.1f, 0.08f, 0.06f, 0.95f);

            // 标题
            var title = NewText("Title", _menuRoot.transform, font, 72, new Vector2(0, 240), new Vector2(900, 120));
            title.alignment = TextAnchor.MiddleCenter;
            title.text = "猫咪咖啡馆";

            var subtitle = NewText("Subtitle", _menuRoot.transform, font, 32, new Vector2(0, 150), new Vector2(700, 60));
            subtitle.alignment = TextAnchor.MiddleCenter;
            subtitle.color = new Color(1f, 0.9f, 0.6f);
            subtitle.text = "Cat Café";

            // 开始游戏按钮
            MakeButton("开始游戏", new Vector2(0, -20), font, () => GameManager.Instance?.NewGame());
            // 继续游戏按钮
            MakeButton("继续游戏", new Vector2(0, -130), font, () => GameManager.Instance?.ContinueGame());
        }

        private void MakeButton(string label, Vector2 centerPos, Font font, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_menuRoot.transform, false);
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
