using UnityEngine;

namespace CatCafe
{
    /// <summary>共享的 1×1 白色 Sprite（供所有代码自建物体使用）。</summary>
    public static class SpriteUtil
    {
        private static Sprite _white;

        public static Sprite White
        {
            get
            {
                if (_white == null)
                {
                    var tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, Color.white);
                    tex.Apply();
                    _white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                }
                return _white;
            }
        }
    }
}
