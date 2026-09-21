using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 美术资源加载器：从 Resources 或直接路径加载像素贴图。
    /// 图放在 Assets/Art/，通过 Resources.Load 需要放 Resources 目录，
    /// 这里改用 AssetDatabase（仅编辑器）+ 运行时兜底白色方块。
    /// 实际运行时推荐把 Art 移到 Assets/Resources/Art。
    /// </summary>
    public static class ArtLoader
    {
        /// <summary>尝试加载一张贴图，失败返回 null（调用方回退到色块）。</summary>
        public static Sprite Load(string assetName)
        {
#if UNITY_EDITOR
            var tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(
                $"Assets/Art/{assetName}.png");
            if (tex == null) return null;
            // PPU = 贴图宽度，让 sprite 世界尺寸 = 1 单位，实际大小由物体 localScale 控制
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), tex.width);
#else
            var tex = Resources.Load<Texture2D>("Art/" + assetName);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), tex.width);
#endif
        }

        /// <summary>给 SpriteRenderer 应用贴图，失败则保持色块。</summary>
        public static void Apply(SpriteRenderer sr, string assetName)
        {
            var sprite = Load(assetName);
            if (sprite != null)
            {
                sr.sprite = sprite;
                sr.color = Color.white; // 贴图本身带色
            }
        }
    }
}
