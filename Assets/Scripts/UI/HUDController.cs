using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CatCafe
{
    /// <summary>
    /// 极简 HUD（代码自建 UI，零序列化依赖）。
    /// 启动时构建 Canvas + EventSystem + 状态文本 + 操作按钮，每帧刷新。
    /// 用 legacy uGUI + 内置字体，保证全新项目也能直接渲染。
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
            // Unity 2022+ 内置字体为 LegacyRuntime.ttf（旧版 Arial.ttf 已失效）
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // EventSystem
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            // Canvas
            var canvasGo = new GameObject("HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.transform.SetParent(transform, false);

            // 顶部状态面板
            var panel = NewUI("Panel", canvasGo.transform, TextAnchor.UpperLeft);
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.5f);
            var pr = panel.GetComponent<RectTransform>();
            pr.anchoredPosition = new Vector2(20, -20);
            pr.sizeDelta = new Vector2(560, 200);

            _timeText = NewText("TimeText", panel.transform, font, 30, new Vector2(20, -20), new Vector2(520, 40), TextAnchor.MiddleLeft);
            _coinsText = NewText("CoinsText", panel.transform, font, 26, new Vector2(20, -70), new Vector2(520, 36), TextAnchor.MiddleLeft);
            _catText = NewText("CatText", panel.transform, font, 26, new Vector2(20, -115), new Vector2(520, 36), TextAnchor.MiddleLeft);
            _customerText = NewText("CustomerText", panel.transform, font, 26, new Vector2(20, -155), new Vector2(520, 36), TextAnchor.MiddleLeft);

            // 底部操作面板
            var bottom = NewUI("Buttons", canvasGo.transform, TextAnchor.LowerLeft);
            var bImg = bottom.AddComponent<Image>();
            bImg.color = new Color(0, 0, 0, 0.5f);
            var br = bottom.GetComponent<RectTransform>();
            br.anchoredPosition = new Vector2(20, 20);
            br.sizeDelta = new Vector2(900, 260);

            _orderText = NewText("OrderText", bottom.transform, font, 26, new Vector2(20, -20), new Vector2(500, 36), TextAnchor.MiddleLeft);

            MakeButton("萃取", bottom.transform, font, new Vector2(20, -80), OnBrew);
            MakeButton("装杯", bottom.transform, font, new Vector2(160, -80), OnCup);
            MakeButton("上菜", bottom.transform, font, new Vector2(300, -80), OnServe);

            MakeButton("喂食", bottom.transform, font, new Vector2(20, -150), OnFeed);
            MakeButton("铲屎", bottom.transform, font, new Vector2(160, -150), OnClean);
            MakeButton("互动", bottom.transform, font, new Vector2(300, -150), OnPet);

            MakeButton("暂停/继续", bottom.transform, font, new Vector2(480, -80), OnPause);
            MakeButton("快进", bottom.transform, font, new Vector2(620, -80), OnFast);
            MakeButton("重开一天", bottom.transform, font, new Vector2(480, -150), OnRestart);

            _hintText = NewText("HintText", bottom.transform, font, 20, new Vector2(20, -220), new Vector2(860, 40), TextAnchor.MiddleLeft);
            _hintText.color = new Color(1f, 0.9f, 0.5f);
        }

        private GameObject NewUI(string name, Transform parent, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = AnchorVector(anchor);
            return go;
        }

        private static Vector2 AnchorVector(TextAnchor a) => a switch
        {
            TextAnchor.UpperLeft => new Vector2(0, 1),
            TextAnchor.LowerLeft => new Vector2(0, 0),
            TextAnchor.MiddleCenter => new Vector2(0.5f, 0.5f),
            _ => new Vector2(0.5f, 0.5f),
        };

        private Text NewText(string name, Transform parent, Font font, int size, Vector2 pos, Vector2 sizeDelta, TextAnchor align)
        {
            var go = NewUI(name, parent, TextAnchor.MiddleCenter);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            var t = go.AddComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.alignment = align;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private void MakeButton(string label, Transform parent, Font font, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var go = NewUI("Btn_" + label, parent, TextAnchor.MiddleCenter);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(120, 48);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.35f, 0.65f, 0.55f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var labelGo = NewUI("Label", go.transform, TextAnchor.MiddleCenter);
            var lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            var t = labelGo.AddComponent<Text>();
            t.font = font; t.fontSize = 22; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
            t.text = label;
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
