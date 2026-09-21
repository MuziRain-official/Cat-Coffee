using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 保温台表现层：成品咖啡杯放在保温台台面上（方块内部），
    /// 每杯上方贴着一条新鲜度进度条。
    /// 父物体(保温台 Station) localScale=1.6，这里用局部坐标 + 缩放补偿。
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

            // 重新排布（居中，放在台面内部）
            LayoutCups();

            // 更新每个图标的新鲜度条
            for (int i = 0; i < count; i++)
            {
                var bar = _cupIcons[i].GetComponentInChildren<WorldBar>();
                if (bar != null)
                    bar.SetProgress(warmer.Cups[i].Freshness);
            }
        }

        private void CreateCupIcon()
        {
            var icon = new GameObject("CupIcon", typeof(SpriteRenderer));
            icon.transform.SetParent(transform, false);
            // 杯身世界尺寸 0.5×0.5，父 scale=1.6 → 局部 0.3125
            float s = 0.5f / 1.6f;
            icon.transform.localScale = new Vector3(s, s, 1f);
            var sr = icon.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 6;
            ArtLoader.Apply(sr, "coffee_cup"); // 咖啡杯贴图，失败回退色块
            if (sr.sprite == SpriteUtil.White)
                sr.color = new Color(0.7f, 0.5f, 0.3f); // 回退：咖啡色杯身

            // 新鲜度条：贴在杯子上缘（杯图标局部坐标系，杯半高=0.5）
            WorldBar.Create(icon.transform, new Vector3(0f, 0.5f, 0f), 0.5f, 0.08f, new Color(0.3f, 0.9f, 0.4f));

            _cupIcons.Add(icon);
        }

        private void LayoutCups()
        {
            int n = _cupIcons.Count;
            if (n == 0) return;
            // 世界坐标：杯子居中排布在台面上，间距 0.55
            for (int i = 0; i < n; i++)
            {
                float worldX = (i - (n - 1) / 2f) * 0.55f;
                float worldY = 0.1f; // 台面中心略偏上
                _cupIcons[i].transform.localPosition = new Vector3(worldX / 1.6f, worldY / 1.6f, 0f);
            }
        }
    }
}
