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
                OrderStep.Brewing => $"订单：萃取中 {Mathf.CeilToInt(flow.Order.BrewProgress * 100)}%",
                OrderStep.ReadyToCup => "订单：待装杯",
                OrderStep.ReadyToServe => "订单：待上菜",
                _ => ""
            };
        }

        private string FlowLabel(GameFlow flow)
        {
            if (flow.MoodMultiplier > 1f) return "增益";
            if (flow.MoodMultiplier < 1f) return "惩罚";
            return "正常";
        }

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
            _hintText.text = "WASD移动 · 靠近设备/猫/顾客按E交互 · 猫前:E喂食 Q铲屎 R互动 · 空格暂停";
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
