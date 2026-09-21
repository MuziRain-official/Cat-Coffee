using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 下落式音游表现层：在奶泡机上方用世界坐标绘制下落音符 + 判定线。
    /// 音符挂在场景根下（不继承奶泡机 scale），世界坐标直接定位，避免缩放失真。
    /// </summary>
    public class FrothGameView : MonoBehaviour
    {
        /// <summary>判定线在奶泡机上方的世界高度偏移。</summary>
        public float judgeLineWorldY = 1.5f;

        /// <summary>音符最高点相对判定线的世界高度。</summary>
        public float spawnHeight = 3.0f;

        private GameObject[] _notes = new GameObject[FrothGame.NoteCount];
        private SpriteRenderer[] _noteSRs = new SpriteRenderer[FrothGame.NoteCount];
        private GameObject _judgeLine;
        private WorldBar _extractBar; // 奶泡读条进度条

        private void Start()
        {
            // 音符挂场景根下，世界坐标定位
            for (int i = 0; i < FrothGame.NoteCount; i++)
            {
                var note = new GameObject("FrothNote_" + i, typeof(SpriteRenderer));
                note.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
                var sr = note.GetComponent<SpriteRenderer>();
                sr.sprite = SpriteUtil.White;
                sr.color = new Color(1f, 0.85f, 0.4f);
                sr.sortingOrder = 10;
                note.SetActive(false);
                _notes[i] = note;
                _noteSRs[i] = sr;
            }

            _judgeLine = new GameObject("FrothJudgeLine", typeof(SpriteRenderer));
            _judgeLine.transform.localScale = new Vector3(1.4f, 0.1f, 1f);
            var lineSR = _judgeLine.GetComponent<SpriteRenderer>();
            lineSR.sprite = SpriteUtil.White;
            lineSR.color = new Color(1f, 0.4f, 0.4f, 0.95f);
            lineSR.sortingOrder = 9;
            _judgeLine.SetActive(false);

            // 奶泡读条进度条（挂场景根，世界坐标，在奶泡机上方）
            var barGo = new GameObject("FrothExtractBar");
            barGo.transform.position = transform.position + new Vector3(0f, 1.5f, 0f);
            _extractBar = WorldBar.Create(barGo.transform, Vector3.zero, 1.2f, 0.1f, new Color(0.9f, 0.6f, 0.2f));
            _extractBar.SetVisible(false);
        }

        private void Update()
        {
            var flow = GameManager.Instance?.Flow;
            if (flow == null) return;

            bool active = flow.IsFrothGameActive;
            _judgeLine.SetActive(active);
            for (int i = 0; i < FrothGame.NoteCount; i++)
                _notes[i].SetActive(false);

            // 奶泡读条进度条：奶泡机读条时显示
            bool extracting = flow.FrotherStep == DeviceStep.Extracting;
            if (_extractBar != null)
            {
                _extractBar.SetVisible(extracting);
                if (extracting) _extractBar.SetProgress(flow.FrotherProgress);
            }

            if (!active) return;

            // 判定线世界位置（奶泡机上方）
            var basePos = transform.position; // 奶泡机世界位置
            _judgeLine.transform.position = new Vector3(basePos.x, basePos.y + judgeLineWorldY, basePos.z);

            // 更新音符世界位置
            var game = flow.FrothGame;
            for (int i = 0; i < FrothGame.NoteCount; i++)
            {
                float pos = game.NotePositions[i];
                bool judged = game.Judgements[i] != NoteJudgement.Pending;
                bool spawned = pos > -1f;
                if (!spawned || judged) continue;

                _notes[i].SetActive(true);
                float worldY = basePos.y + judgeLineWorldY + (pos / 5f) * spawnHeight;
                _notes[i].transform.position = new Vector3(basePos.x, worldY, basePos.z);

                // 越接近判定线越亮
                float closeness = Mathf.Clamp01(1f - Mathf.Abs(pos) / 1.5f);
                _noteSRs[i].color = Color.Lerp(new Color(1f, 0.85f, 0.4f), new Color(0.4f, 1f, 0.5f), closeness);
            }
        }
    }
}
