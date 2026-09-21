using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CatCafe
{
    /// <summary>
    /// 极简 HUD：只显示剩余时间、赚了多少钱、猫位置。
    /// 小游戏（萃取时机条/下落音游/拉花）进行中时，显示对应提示面板。
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        private GameManager _gm;

        private Text _timeText;
        private Text _coinsText;
        private Text _catText;

        // 萃取时机条
        private GameObject _brewPanel;
        private RectTransform _brewBar;
        private RectTransform _brewPointer;

        // 下落音游提示
        private GameObject _frothPanel;
        private Text _frothText;

        // 拉花提示
        private GameObject _latteArtPanel;
        private Text _latteArtText;

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

            // 1. 剩余时间
            float remain = flow.Clock.RemainingSeconds(cfg.dayDurationSeconds);
            _timeText.text = flow.IsDayOver ? "打烊" : $"第{flow.Progress.Day}天 剩余 {Mathf.CeilToInt(remain)}s";

            // 2. 赚了多少钱（利润 = 收入 - 成本）+ 总金币
            _coinsText.text = $"本日赚 {flow.Ledger.Profit} | 总金币 {flow.Progress.TotalCoins}";

            // 3. 猫位置
            _catText.text = $"猫：{ZoneLabel(flow.Cat)}";

            // 小游戏面板显隐
            bool brewing = flow.IsBrewGameActive;
            if (_brewPanel != null) _brewPanel.SetActive(brewing);
            if (brewing && _brewPointer != null && _brewBar != null)
                _brewPointer.anchoredPosition = new Vector2(flow.BrewGame.PointerPosition * _brewBar.sizeDelta.x, 0f);

            bool frothing = flow.IsFrothGameActive;
            if (_frothPanel != null) _frothPanel.SetActive(frothing);
            if (frothing && _frothText != null)
            {
                int judged = 0;
                for (int i = 0; i < FrothGame.NoteCount; i++)
                    if (flow.FrothGame.Judgements[i] != NoteJudgement.Pending) judged++;
                _frothText.text = $"奶泡下落中！在判定线按 E\n已判定 {judged}/{FrothGame.NoteCount}";
            }

            bool latteArt = flow.IsLatteArtGameActive;
            if (_latteArtPanel != null) _latteArtPanel.SetActive(latteArt);
            if (latteArt && _latteArtText != null)
                _latteArtText.text = $"拉花中！光标格 {flow.LatteArtGame.CursorIndex}，在中心格(4)按 E";
        }

        private static string ZoneLabel(CatState cat) => cat.IsCarried
            ? "被抱着"
            : cat.Zone switch
            {
                CatZone.Bar => "吧台（制作快·保鲜久）",
                CatZone.Seat => "餐桌旁（顾客停留久）",
                CatZone.Door => "门口（客流多）",
                _ => "猫窝",
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

            // 顶部左上角：只显示三行（紧凑，不挡视野）
            var panel = NewPanel("HudPanel", canvasGo.transform, new Vector2(16, -16), new Vector2(320, 120));
            _timeText = NewText("TimeText", panel, font, 30, new Vector2(16, -14), new Vector2(288, 36));
            _coinsText = NewText("CoinsText", panel, font, 26, new Vector2(16, -54), new Vector2(288, 32));
            _catText = NewText("CatText", panel, font, 24, new Vector2(16, -90), new Vector2(288, 30));

            // 萃取时机条（屏幕中央上方，默认隐藏）
            BuildBrewPanel(canvasGo.transform, font);

            // 下落音游提示（奶泡机上方，默认隐藏）
            BuildFrothPanel(canvasGo.transform, font);

            // 拉花提示（默认隐藏）
            BuildLatteArtPanel(canvasGo.transform, font);
        }

        private void BuildBrewPanel(Transform root, Font font)
        {
            _brewPanel = new GameObject("BrewPanel", typeof(RectTransform), typeof(Image));
            _brewPanel.transform.SetParent(root, false);
            var pr = _brewPanel.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(0.5f, 1f);
            pr.anchorMax = new Vector2(0.5f, 1f);
            pr.pivot = new Vector2(0.5f, 1f);
            pr.anchoredPosition = new Vector2(0, -80);
            pr.sizeDelta = new Vector2(520, 80);
            _brewPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.55f);

            var title = NewText("BrewTitle", _brewPanel.transform, font, 24, new Vector2(20, -10), new Vector2(480, 30));
            title.text = "萃取：再按 E 停住指针";

            var barGo = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            barGo.transform.SetParent(_brewPanel.transform, false);
            _brewBar = barGo.GetComponent<RectTransform>();
            AnchorTopLeft(_brewBar, new Vector2(20, -46), new Vector2(480, 18));
            barGo.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.3f);

            var zoneGo = new GameObject("PerfectZone", typeof(RectTransform), typeof(Image));
            zoneGo.transform.SetParent(barGo.transform, false);
            var zone = zoneGo.GetComponent<RectTransform>();
            zone.anchorMin = new Vector2(0.5f, 0f);
            zone.anchorMax = new Vector2(0.5f, 1f);
            zone.pivot = new Vector2(0.5f, 0.5f);
            zone.sizeDelta = new Vector2(96f, 18f);
            zone.anchoredPosition = Vector2.zero;
            zoneGo.GetComponent<Image>().color = new Color(0.3f, 0.9f, 0.4f, 0.9f);

            var ptrGo = new GameObject("Pointer", typeof(RectTransform), typeof(Image));
            ptrGo.transform.SetParent(barGo.transform, false);
            _brewPointer = ptrGo.GetComponent<RectTransform>();
            _brewPointer.anchorMin = new Vector2(0, 0.5f);
            _brewPointer.anchorMax = new Vector2(0, 0.5f);
            _brewPointer.pivot = new Vector2(0.5f, 0.5f);
            _brewPointer.sizeDelta = new Vector2(10f, 28f);
            _brewPointer.anchoredPosition = Vector2.zero;
            ptrGo.GetComponent<Image>().color = new Color(1f, 0.9f, 0.3f);

            _brewPanel.SetActive(false);
        }

        private void BuildFrothPanel(Transform root, Font font)
        {
            _frothPanel = new GameObject("FrothPanel", typeof(RectTransform), typeof(Image));
            _frothPanel.transform.SetParent(root, false);
            var pr = _frothPanel.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(0.5f, 1f);
            pr.anchorMax = new Vector2(0.5f, 1f);
            pr.pivot = new Vector2(0.5f, 1f);
            pr.anchoredPosition = new Vector2(0, -80);
            pr.sizeDelta = new Vector2(420, 70);
            _frothPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.55f);

            _frothText = NewText("FrothText", _frothPanel.transform, font, 24, new Vector2(16, -14), new Vector2(388, 50));
            _frothText.alignment = TextAnchor.MiddleCenter;
            _frothPanel.SetActive(false);
        }

        private void BuildLatteArtPanel(Transform root, Font font)
        {
            _latteArtPanel = new GameObject("LatteArtPanel", typeof(RectTransform), typeof(Image));
            _latteArtPanel.transform.SetParent(root, false);
            var pr = _latteArtPanel.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(0.5f, 1f);
            pr.anchorMax = new Vector2(0.5f, 1f);
            pr.pivot = new Vector2(0.5f, 1f);
            pr.anchoredPosition = new Vector2(0, -80);
            pr.sizeDelta = new Vector2(420, 70);
            _latteArtPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.55f);

            _latteArtText = NewText("LatteArtText", _latteArtPanel.transform, font, 24, new Vector2(16, -14), new Vector2(388, 50));
            _latteArtText.alignment = TextAnchor.MiddleCenter;
            _latteArtPanel.SetActive(false);
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
