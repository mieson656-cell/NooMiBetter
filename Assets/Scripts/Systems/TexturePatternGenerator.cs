using UnityEngine;

namespace NooMiBetter
{
    public static class TexturePatternGenerator
    {
        public static Texture2D Create(int width, int height, Color baseColor, Color accent, int seed = 1)
        {
            width = Mathf.Clamp(width, 16, 1024);
            height = Mathf.Clamp(height, 16, 1024);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var random = new System.Random(seed);
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                bool stripe = ((x + y + seed) / 12) % 2 == 0;
                float noise = (float)random.NextDouble() * .06f;
                pixels[y * width + x] = (stripe ? baseColor : accent) * (1f - noise);
            }
            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }
    }
}
