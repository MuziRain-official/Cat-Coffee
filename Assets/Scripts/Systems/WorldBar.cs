using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 世界空间进度条（表现层）。背景 + 填充两个 SpriteRenderer，统一左 pivot。
    /// 通过补偿父物体缩放，使宽/高/位置均以世界单位精确渲染，
    /// 不受父物体 localScale 影响。
    /// </summary>
    public class WorldBar : MonoBehaviour
    {
        private Transform _fill;
        private float _width;
        private float _height;

        /// <summary>
        /// 创建进度条。localPos / width / height 均按父物体坐标系的世界单位解释，
        /// 内部自动补偿父缩放。
        /// </summary>
        public static WorldBar Create(Transform parent, Vector3 localPos, float width, float height, Color fillColor)
        {
            var go = new GameObject("WorldBar");
            go.transform.SetParent(parent, false);

            // 补偿父物体缩放：让进度条自身在世界上是 1:1 尺寸
            var parentScale = parent.lossyScale;
            go.transform.localScale = new Vector3(
                1f / Mathf.Max(parentScale.x, 1e-4f),
                1f / Mathf.Max(parentScale.y, 1e-4f),
                1f);
            // localPosition 同样除以父缩放，使最终世界偏移 = localPos
            go.transform.localPosition = new Vector3(
                localPos.x / Mathf.Max(parentScale.x, 1e-4f),
                localPos.y / Mathf.Max(parentScale.y, 1e-4f),
                localPos.z);

            // 背景：左 pivot，世界尺寸 = width × height
            var bg = NewQuad("BG", go.transform, new Color(0.2f, 0.2f, 0.2f, 0.9f), -1);
            bg.localPosition = Vector3.zero;
            bg.localScale = new Vector3(width, height, 1f);

            // 填充：左 pivot，初始 0 宽
            var fill = NewQuad("Fill", go.transform, fillColor, 0);
            fill.localPosition = Vector3.zero;

            var bar = go.AddComponent<WorldBar>();
            bar._fill = fill;
            bar._width = width;
            bar._height = height;
            bar.SetProgress(0f);

            return bar;
        }

        /// <summary>设置进度 0–1。</summary>
        public void SetProgress(float p)
        {
            p = Mathf.Clamp01(p);
            if (_fill != null)
                _fill.localScale = new Vector3(_width * p, _height, 1f);
        }

        public void SetVisible(bool visible)
        {
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
                sr.enabled = visible;
        }

        private static Transform NewQuad(string name, Transform parent, Color color, int order)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = SpriteUtil.WhiteLeftPivot; // 左 pivot
            sr.color = color;
            sr.sortingOrder = order;
            return go.transform;
        }
    }
}
