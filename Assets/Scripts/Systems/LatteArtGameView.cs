using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 拉花小游戏表现层（世界空间）。
    /// 在装杯台上方显示 3×3 九宫格，一个高亮格循环扫动，
    /// 按 E 停在中心格(4)=猫爪完美。
    /// </summary>
    public class LatteArtGameView : MonoBehaviour
    {
        private GameObject _grid;               // 九宫格容器
        private SpriteRenderer[] _cells = new SpriteRenderer[9]; // 9 个格子
        private GameObject _cursor;             // 高亮扫动格
        private SpriteRenderer _cursorSR;

        private void Start()
        {
            _grid = new GameObject("LatteArtGrid");
            _grid.transform.SetParent(transform, false);
            _grid.transform.localScale = new Vector3(1f / 1.6f, 1f / 1.6f, 1f); // 补偿父缩放
            _grid.transform.localPosition = new Vector3(0f, 2.2f / 1.6f, 0f);

            float cellSize = 0.4f;
            float gap = 0.1f;
            for (int i = 0; i < 9; i++)
            {
                int col = i % 3;
                int row = i / 3;
                var cell = new GameObject("Cell_" + i, typeof(SpriteRenderer));
                cell.transform.SetParent(_grid.transform, false);
                cell.transform.localScale = new Vector3(cellSize, cellSize, 1f);
                // 中心格(4)在中心，其他格按 col-1, row-1 偏移
                float x = (col - 1) * (cellSize + gap);
                float y = (1 - row) * (cellSize + gap); // row0在上
                cell.transform.localPosition = new Vector3(x, y, 0f);
                var sr = cell.GetComponent<SpriteRenderer>();
                sr.sprite = SpriteUtil.White;
                sr.color = new Color(0.6f, 0.55f, 0.5f, 0.7f);
                sr.sortingOrder = 20;
                _cells[i] = sr;
            }

            // 中心格用猫爪色标记（目标）
            _cells[4].color = new Color(0.85f, 0.6f, 0.5f, 0.9f);

            // 扫动光标
            _cursor = new GameObject("LatteArtCursor", typeof(SpriteRenderer));
            _cursor.transform.SetParent(_grid.transform, false);
            _cursor.transform.localScale = new Vector3(cellSize * 1.2f, cellSize * 1.2f, 1f);
            _cursorSR = _cursor.GetComponent<SpriteRenderer>();
            _cursorSR.sprite = SpriteUtil.White;
            _cursorSR.color = new Color(1f, 0.9f, 0.3f, 0.95f);
            _cursorSR.sortingOrder = 21;

            _grid.SetActive(false);
        }

        private void LateUpdate()
        {
            var flow = GameManager.Instance?.Flow;
            if (flow == null || _grid == null) return;

            bool show = flow.IsLatteArtGameActive;
            _grid.SetActive(show);
            if (!show) return;

            // 光标跟随逻辑层的 CursorIndex
            int idx = flow.LatteArtGame.CursorIndex;
            int col = idx % 3;
            int row = idx / 3;
            float cellSize = 0.4f;
            float gap = 0.1f;
            float x = (col - 1) * (cellSize + gap);
            float y = (1 - row) * (cellSize + gap);
            _cursor.transform.localPosition = new Vector3(x, y, 0f);
        }
    }
}
