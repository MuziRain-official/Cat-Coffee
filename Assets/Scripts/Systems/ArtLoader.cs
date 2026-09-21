using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 美术资源加载器。
    /// 默认使用程序化像素占位（干净统一），AI 贴图保留在 Assets/Art 备用。
    /// 切换：把 useProcedural 设为 false 则用 AI 贴图。
    /// </summary>
    public static class ArtLoader
    {
        /// <summary>true = 程序化像素占位；false = AI 生成贴图。</summary>
        private static bool _useProcedural = true;
        public static bool UseProcedural
        {
            get => _useProcedural;
            set => _useProcedural = value;
        }

        /// <summary>尝试加载一张贴图，失败返回 null（调用方回退到色块）。</summary>
        public static Sprite Load(string assetName)
        {
            if (_useProcedural)
                return PixelArtGenerator.Get(assetName);

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
