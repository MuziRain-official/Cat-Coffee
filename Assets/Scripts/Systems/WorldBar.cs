using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 世界空间进度条（表现层）。用两个 SpriteRenderer 组成：背景 + 填充。
    /// 填充的 localScale.x 按进度缩放（0–1），左对齐。
    /// </summary>
    public class WorldBar : MonoBehaviour
    {
        private Transform _fill;
        private float _width;

        /// <summary>创建一个进度条并挂到父物体上方。</summary>
        public static WorldBar Create(Transform parent, Vector3 localPos, float width, float height, Color fillColor)
        {
            var go = new GameObject("WorldBar");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            // 背景
            var bg = NewQuad("BG", go.transform, width, height, new Color(0.2f, 0.2f, 0.2f, 0.9f), -1);

            // 填充（左对齐，pivot 在左）
            var fill = NewQuad("Fill", go.transform, width, height, fillColor, 0);
            fill.localPosition = new Vector3(-width / 2f, 0f, 0f);
            var fillSr = fill.GetComponent<SpriteRenderer>();
            fillSr.sprite = SpriteUtil.WhiteLeftPivot;

            var bar = go.AddComponent<WorldBar>();
            bar._fill = fill;
            bar._width = width;
            bar.SetProgress(0f);

            // 背景也左对齐
            bg.localPosition = new Vector3(-width / 2f, 0f, 0f);

            return bar;
        }

        /// <summary>设置进度 0–1。</summary>
        public void SetProgress(float p)
        {
            p = Mathf.Clamp01(p);
            if (_fill != null)
                _fill.localScale = new Vector3(_width * p, 1f, 1f);
        }

        public void SetVisible(bool visible)
        {
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
                sr.enabled = visible;
        }

        private static Transform NewQuad(string name, Transform parent, float width, float height, Color color, int order)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(width, height, 1f);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = SpriteUtil.White;
            sr.color = color;
            sr.sortingOrder = order;
            return go.transform;
        }
    }
}
