using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CatCafe
{
    /// <summary>
    /// 极简 HUD（仅状态显示，无操作按钮——操作改为空间交互）。
    /// 统一「左上角原点、Y 向下」坐标，面板与子元素同锚点。
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        private GameManager _gm;

        private Text _timeText;
        private Text _coinsText;
        private Text _catText;
        private Text _customerText;
        private Text _orderText;
        private Text _hintText;

        // 萃取时机条（表现层）
        private GameObject _brewPanel;
        private RectTransform _brewBar;      // 时机条底槽
        private RectTransform _brewPointer;  // 指针
        private Image _brewPointerImg;
        private RectTransform _perfectZone;  // 完美区高亮
        private Text _brewQualityText;

        private void Start()
        {
            _gm = GameManager.Instance;
            BuildUI();
        }

        private void Update()
        {
            if (_gm == null || _gm.Flow == null) return;
            Refresh();
        }

        private void Refresh()
        {
            var flow = _gm.Flow;
            var cfg = _gm.Config;
            float remain = flow.Clock.RemainingSeconds(cfg.dayDurationSeconds);

            _timeText.text = flow.IsDayOver
                ? "【打烊】"
                : $"剩余 {Mathf.CeilToInt(remain)}s";

            _coinsText.text = $"金币 {flow.Ledger.Coins}  (收入{flow.Ledger.Revenue} 成本{flow.Ledger.Cost})";

            var c = flow.Cat;
            _catText.text = $"猫  饱腹{c.Satiety:F0} 清洁{c.Hygiene:F0} 心情{c.Mood:F0}  [{FlowLabel(flow)}]";

            _customerText.text = $"顾客  在场{flow.Customers.Count}  已服务{flow.ServedCount}  流失{flow.LeftCount}";

            _orderText.text = flow.Order.Step switch
            {
                OrderStep.None => "订单：空闲",
                OrderStep.Brewing => $"订单：萃取中（时机条，再按E停）",
                OrderStep.ReadyToPickup => $"订单：萃取好[{QualityLabel(flow.Order.Quality)}]，回咖啡机取原料",
                OrderStep.HoldingIngredients => "订单：持原料，到装杯台装杯",
                OrderStep.ReadyToServe => "订单：持成品，上菜给顾客",
                _ => ""
            };

            // 时机条显隐 + 指针位置刷新
            bool brewing = flow.IsBrewGameActive;
            if (_brewPanel != null) _brewPanel.SetActive(brewing);
            if (brewing && _brewPointer != null && _brewBar != null)
            {
                float pos = flow.BrewGame.PointerPosition; // 0–1
                _brewPointer.anchoredPosition = new Vector2(pos * _brewBar.sizeDelta.x, 0f);
                _brewQualityText.text = QualityLabel(flow.Order.Quality);
            }
        }

        private string FlowLabel(GameFlow flow)
        {
            if (flow.MoodMultiplier > 1f) return "增益";
            if (flow.MoodMultiplier < 1f) return "惩罚";
            return "正常";
        }

        private static string QualityLabel(BrewQuality q) => q switch
        {
            BrewQuality.Perfect => "完美",
            BrewQuality.Poor => "勉强",
            _ => "良好",
        };

        private void BuildUI()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            if (FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasGo = new GameObject("HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.transform.SetParent(transform, false);

            var topPanel = NewPanel("TopPanel", canvasGo.transform, new Vector2(20, -20), new Vector2(560, 200));
            _timeText     = NewText("TimeText",     topPanel, font, 30, new Vector2(20, -20),  new Vector2(520, 40));
            _coinsText    = NewText("CoinsText",    topPanel, font, 26, new Vector2(20, -70),  new Vector2(520, 36));
            _catText      = NewText("CatText",      topPanel, font, 26, new Vector2(20, -115), new Vector2(520, 36));
            _customerText = NewText("CustomerText", topPanel, font, 26, new Vector2(20, -155), new Vector2(520, 36));

            var bottomPanel = NewPanel("BottomPanel", canvasGo.transform, new Vector2(20, -240), new Vector2(900, 130));
            _orderText = NewText("OrderText", bottomPanel, font, 26, new Vector2(20, -20), new Vector2(500, 36));
            _hintText  = NewText("HintText",  bottomPanel, font, 20, new Vector2(20, -80), new Vector2(860, 44));
            _hintText.color = new Color(1f, 0.9f, 0.5f);
            _hintText.text = "WASD移动 · 咖啡机按E萃取(时机条再按E停) · 猫前:E喂食 Q铲屎 R互动 · 空格暂停";

            BuildBrewGameUI(canvasGo.transform, font);
        }

        /// <summary>构建萃取时机条（屏幕中央下方，默认隐藏，小游戏时显示）。</summary>
        private void BuildBrewGameUI(Transform canvasRoot, Font font)
        {
            _brewPanel = new GameObject("BrewPanel", typeof(RectTransform), typeof(Image));
            _brewPanel.transform.SetParent(canvasRoot, false);
            var pr = _brewPanel.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(0.5f, 1f);
            pr.anchorMax = new Vector2(0.5f, 1f);
            pr.pivot = new Vector2(0.5f, 1f);
            pr.anchoredPosition = new Vector2(0, -460);
            pr.sizeDelta = new Vector2(520, 100);
            _brewPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.55f);

            // 标题
            var title = NewText("BrewTitle", _brewPanel.transform, font, 24, new Vector2(20, -12), new Vector2(480, 30));
            title.text = "萃取时机条：再按 E 停住指针";

            // 底槽（时机条）
            var barGo = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            barGo.transform.SetParent(_brewPanel.transform, false);
            _brewBar = barGo.GetComponent<RectTransform>();
            AnchorTopLeft(_brewBar, new Vector2(20, -52), new Vector2(480, 20));
            barGo.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.3f);

            // 完美区高亮（中央，宽度 = 2*perfectHalfWidth）
            var zoneGo = new GameObject("PerfectZone", typeof(RectTransform), typeof(Image));
            zoneGo.transform.SetParent(barGo.transform, false);
            _perfectZone = zoneGo.GetComponent<RectTransform>();
            _perfectZone.anchorMin = new Vector2(0.5f, 0f);
            _perfectZone.anchorMax = new Vector2(0.5f, 1f);
            _perfectZone.pivot = new Vector2(0.5f, 0.5f);
            _perfectZone.sizeDelta = new Vector2(96f, 20f); // 480 * 0.24 ≈ 完美区总宽
            _perfectZone.anchoredPosition = Vector2.zero;
            zoneGo.GetComponent<Image>().color = new Color(0.3f, 0.9f, 0.4f, 0.9f);

            // 指针
            var ptrGo = new GameObject("Pointer", typeof(RectTransform), typeof(Image));
            ptrGo.transform.SetParent(barGo.transform, false);
            _brewPointer = ptrGo.GetComponent<RectTransform>();
            _brewPointer.anchorMin = new Vector2(0, 0.5f);
            _brewPointer.anchorMax = new Vector2(0, 0.5f);
            _brewPointer.pivot = new Vector2(0.5f, 0.5f);
            _brewPointer.sizeDelta = new Vector2(10f, 32f);
            _brewPointer.anchoredPosition = new Vector2(0, 0f);
            ptrGo.GetComponent<Image>().color = new Color(1f, 0.9f, 0.3f);

            // 品质文字（提示当前停在哪个档）
            _brewQualityText = NewText("BrewQuality", _brewPanel.transform, font, 20, new Vector2(400, -52), new Vector2(100, 24));
            _brewQualityText.alignment = TextAnchor.MiddleRight;
            _brewQualityText.text = "";

            _brewPanel.SetActive(false); // 默认隐藏
        }

        private Transform NewPanel(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            AnchorTopLeft(rt, pos, size);
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            return go.transform;
        }

        private Text NewText(string name, Transform parent, Font font, int size, Vector2 pos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            AnchorTopLeft(rt, pos, sizeDelta);
            var t = go.GetComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.alignment = TextAnchor.MiddleLeft;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private static void AnchorTopLeft(RectTransform rt, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }
    }
}
