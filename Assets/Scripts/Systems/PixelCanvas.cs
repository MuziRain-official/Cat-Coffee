using UnityEngine;

namespace CatCafe
{
    /// <summary>奶油复古像素风调色板。</summary>
    public static class PixelPalette
    {
        public static readonly Color Clear        = new Color(0f, 0f, 0f, 0f);
        public static readonly Color Cream        = new Color(0.96f, 0.90f, 0.82f);
        public static readonly Color CreamDark    = new Color(0.90f, 0.82f, 0.72f);
        public static readonly Color LightWood    = new Color(0.80f, 0.65f, 0.48f);
        public static readonly Color Wood         = new Color(0.56f, 0.41f, 0.28f);
        public static readonly Color DarkWood     = new Color(0.36f, 0.25f, 0.18f);
        public static readonly Color Orange       = new Color(0.91f, 0.59f, 0.35f);
        public static readonly Color LightOrange  = new Color(0.96f, 0.75f, 0.53f);
        public static readonly Color Green        = new Color(0.66f, 0.76f, 0.63f);
        public static readonly Color GreenDark    = new Color(0.55f, 0.66f, 0.52f);
        public static readonly Color White        = Color.white;
        public static readonly Color CoffeeBrown  = new Color(0.35f, 0.22f, 0.12f);
        public static readonly Color MilkFoam     = new Color(0.92f, 0.84f, 0.74f);
        public static readonly Color HairBrown    = new Color(0.42f, 0.28f, 0.18f);
        public static readonly Color Skin         = new Color(0.98f, 0.85f, 0.72f);
        public static readonly Color Red          = new Color(0.85f, 0.40f, 0.38f);
        public static readonly Color Blue         = new Color(0.55f, 0.68f, 0.80f);
    }

    /// <summary>像素画布：在 N×N 网格上逐像素绘制，输出 1 单位 sprite。</summary>
    public class PixelCanvas
    {
        public int Size { get; }
        private readonly Color[] _px;

        public PixelCanvas(int size)
        {
            Size = size;
            _px = new Color[size * size];
            for (int i = 0; i < _px.Length; i++) _px[i] = PixelPalette.Clear;
        }

        public void Set(int x, int y, Color c)
        {
            if (x >= 0 && x < Size && y >= 0 && y < Size) _px[y * Size + x] = c;
        }

        /// <summary>填充实心矩形 [x0,x1) × [y0,y1)。</summary>
        public void FillRect(int x0, int y0, int x1, int y1, Color c)
        {
            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                    Set(x, y, c);
        }

        /// <summary>填充矩形边框。</summary>
        public void RectOutline(int x0, int y0, int x1, int y1, Color c)
        {
            for (int x = x0; x < x1; x++) { Set(x, y0, c); Set(x, y1 - 1, c); }
            for (int y = y0; y < y1; y++) { Set(x0, y, c); Set(x1 - 1, y, c); }
        }

        /// <summary>填充实心圆。</summary>
        public void FillCircle(int cx, int cy, int r, Color c)
        {
            for (int y = cy - r; y <= cy + r; y++)
                for (int x = cx - r; x <= cx + r; x++)
                {
                    int dx = x - cx, dy = y - cy;
                    if (dx * dx + dy * dy <= r * r) Set(x, y, c);
                }
        }

        /// <summary>填充圆环（描边）。</summary>
        public void CircleOutline(int cx, int cy, int r, Color c)
        {
            for (int y = cy - r; y <= cy + r; y++)
                for (int x = cx - r; x <= cx + r; x++)
                {
                    int dx = x - cx, dy = y - cy;
                    int d = dx * dx + dy * dy;
                    if (d <= r * r && d >= (r - 1) * (r - 1)) Set(x, y, c);
                }
        }

        /// <summary>输出为 1 单位宽的像素 sprite（PPU = Size）。</summary>
        public Sprite ToSprite()
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            tex.SetPixels(_px);
            tex.filterMode = FilterMode.Point; // 像素硬边
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }
    }
}
