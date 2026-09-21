using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 保温台表现层：在保温台上方显示每杯咖啡图标 + 新鲜度进度条。
    /// 每帧从 GameFlow.Warmer 读取杯数，动态生成/销毁咖啡杯图标。
    /// </summary>
    public class WarmerView : MonoBehaviour
    {
        private readonly System.Collections.Generic.List<GameObject> _cupIcons = new System.Collections.Generic.List<GameObject>();

        private void LateUpdate()
        {
            var flow = GameManager.Instance?.Flow;
            if (flow == null) return;

            var warmer = flow.Warmer;
            int count = warmer.Count;

            // 同步图标数量
            while (_cupIcons.Count < count) CreateCupIcon();
            while (_cupIcons.Count > count)
            {
                Destroy(_cupIcons[_cupIcons.Count - 1]);
                _cupIcons.RemoveAt(_cupIcons.Count - 1);
            }

            // 更新每个图标的新鲜度条
            for (int i = 0; i < count; i++)
            {
                var icon = _cupIcons[i];
                var bar = icon.GetComponentInChildren<WorldBar>();
                if (bar != null)
                    bar.SetProgress(warmer.Cups[i].Freshness);
            }
        }

        private void CreateCupIcon()
        {
            var icon = new GameObject("CupIcon", typeof(SpriteRenderer));
            icon.transform.SetParent(transform, false);
            icon.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            var sr = icon.GetComponent<SpriteRenderer>();
            sr.sprite = SpriteUtil.White;
            sr.color = new Color(0.7f, 0.5f, 0.3f); // 咖啡色杯身
            sr.sortingOrder = 6;

            // 图标排成一排（世界坐标，父保温台 scale=1.6，需除以补偿）
            float worldX = (_cupIcons.Count - 1) * 0.6f;
            icon.transform.localPosition = new Vector3(worldX / 1.6f, 1.2f / 1.6f, 0f);

            // 新鲜度条：宽 0.5 与杯身同宽，在杯子上方 0.5 处
            WorldBar.Create(icon.transform, new Vector3(0f, 1.0f, 0f), 0.5f, 0.08f, new Color(0.3f, 0.9f, 0.4f));

            _cupIcons.Add(icon);
        }
    }
}
