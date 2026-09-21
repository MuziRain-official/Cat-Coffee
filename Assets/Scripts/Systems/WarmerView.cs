using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 保温台表现层：成品咖啡杯放在保温台台面上（方块内部），
    /// 每杯显示菜品图标 + 新鲜度条 + 选中光标高亮。
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

            // 更新每个图标：菜品图标 + 新鲜度条 + 光标高亮
            for (int i = 0; i < count; i++)
            {
                var icon = _cupIcons[i];
                var cup = warmer.Cups[i];

                // 菜品图标
                var sr = icon.GetComponent<SpriteRenderer>();
                sr.sprite = PixelArtGenerator.CupIcon(cup.Recipe);

                // 新鲜度条
                var bar = icon.GetComponentInChildren<WorldBar>();
                if (bar != null) bar.SetProgress(cup.Freshness);

                // 光标高亮：选中杯放大 + 亮边
                bool selected = (i == flow.WarmerCursor);
                float s = selected ? 0.62f / 1.6f : 0.5f / 1.6f;
                icon.transform.localScale = new Vector3(s, s, 1f);
                sr.color = selected ? new Color(1f, 0.9f, 0.5f) : Color.white;
            }
        }

        private void CreateCupIcon()
        {
            var icon = new GameObject("CupIcon", typeof(SpriteRenderer));
            icon.transform.SetParent(transform, false);
            float s = 0.5f / 1.6f;
            icon.transform.localScale = new Vector3(s, s, 1f);
            var sr = icon.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 6;

            // 新鲜度条：贴在杯子上缘
            WorldBar.Create(icon.transform, new Vector3(0f, 0.5f, 0f), 0.5f, 0.08f, new Color(0.3f, 0.9f, 0.4f));

            _cupIcons.Add(icon);
        }

        private void LayoutCups()
        {
            int n = _cupIcons.Count;
            if (n == 0) return;
            for (int i = 0; i < n; i++)
            {
                float worldX = (i - (n - 1) / 2f) * 0.55f;
                float worldY = 0.1f;
                _cupIcons[i].transform.localPosition = new Vector3(worldX / 1.6f, worldY / 1.6f, 0f);
            }
        }
    }
}
