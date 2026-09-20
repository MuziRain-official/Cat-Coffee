using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CatCafe
{
    /// <summary>
    /// 极简 HUD（代码自建 UI，零序列化依赖）。
    /// 统一采用「左上角原点、Y 向下」坐标系：面板与其所有子元素
    /// 都用同一套 TopLeft 锚点，避免 anchor/pivot 错位。
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

            _hintText.text = flow.IsDayOver
                ? "营业结束。点击『重开一天』再来一局。"
                : "顾客进店→点『萃取』→读条完『装杯』→『上菜』；猫饿了点『喂食/铲屎/互动』。";
        }

        private string FlowLabel(GameFlow flow)
        {
            if (flow.MoodMultiplier > 1f) return "增益";
            if (flow.MoodMultiplier < 1f) return "惩罚";
            return "正常";
        }

        // —— 代码构建 UI ——
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

            // 顶部状态面板（左上角）
            var topPanel = NewPanel("TopPanel", canvasGo.transform, new Vector2(20, -20), new Vector2(560, 200));
            _timeText     = NewText("TimeText",     topPanel, font, 30, new Vector2(20, -20),  new Vector2(520, 40));
            _coinsText    = NewText("CoinsText",    topPanel, font, 26, new Vector2(20, -70),  new Vector2(520, 36));
            _catText      = NewText("CatText",      topPanel, font, 26, new Vector2(20, -115), new Vector2(520, 36));
            _customerText = NewText("CustomerText", topPanel, font, 26, new Vector2(20, -155), new Vector2(520, 36));

            // 底部操作面板（左上角，紧跟顶部面板下方）
            var bottomPanel = NewPanel("BottomPanel", canvasGo.transform, new Vector2(20, -240), new Vector2(900, 300));
            _orderText = NewText("OrderText", bottomPanel, font, 26, new Vector2(20, -20), new Vector2(500, 36));

            // 咖啡操作行
            MakeButton("萃取", bottomPanel, font, new Vector2(20, -70), OnBrew);
            MakeButton("装杯", bottomPanel, font, new Vector2(160, -70), OnCup);
            MakeButton("上菜", bottomPanel, font, new Vector2(300, -70), OnServe);

            // 猫咪操作行
            MakeButton("喂食", bottomPanel, font, new Vector2(20, -140), OnFeed);
            MakeButton("铲屎", bottomPanel, font, new Vector2(160, -140), OnClean);
            MakeButton("互动", bottomPanel, font, new Vector2(300, -140), OnPet);

            // 控制行
            MakeButton("暂停/继续", bottomPanel, font, new Vector2(480, -70), OnPause);
            MakeButton("快进",      bottomPanel, font, new Vector2(620, -70), OnFast);
            MakeButton("重开一天",  bottomPanel, font, new Vector2(480, -140), OnRestart);

            _hintText = NewText("HintText", bottomPanel, font, 20, new Vector2(20, -220), new Vector2(860, 44));
            _hintText.color = new Color(1f, 0.9f, 0.5f);
        }

        // —— 布局辅助：统一 TopLeft 锚点（pivot = 0,1，Y 向下）——

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

        private void MakeButton(string label, Transform parent, Font font, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            AnchorTopLeft(rt, pos, new Vector2(120, 48));

            var img = go.GetComponent<Image>();
            img.color = new Color(0.35f, 0.65f, 0.55f);

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            // 文字铺满按钮
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var t = labelGo.GetComponent<Text>();
            t.font = font;
            t.fontSize = 22;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.text = label;
        }

        /// <summary>将 RectTransform 锚定到父级左上角，pivot=(0,1)，Y 向下。</summary>
        private static void AnchorTopLeft(RectTransform rt, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos; // (右, 下) 偏移
            rt.sizeDelta = size;
        }

        // —— 回调 ——
        private void OnBrew() => _gm?.Brew();
        private void OnCup() => _gm?.Cup();
        private void OnServe() => _gm?.Serve();
        private void OnFeed() => _gm?.FeedCat();
        private void OnClean() => _gm?.CleanCat();
        private void OnPet() => _gm?.PetCat();
        private void OnPause() => _gm?.TogglePause();
        private void OnFast() => _gm?.ToggleFastForward();
        private void OnRestart() => _gm?.StartDay();
    }
}
