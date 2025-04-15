using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using Color = Spectre.Console.Color;

public class TextureUtils {

    static Random rnd = new Random();

    // Returns a pseudo-random value in [-1, 1] based on input integer coordinates
    private static double Hash(int x, int y) {
        int n = x + y * 57;
        n = (n << 13) ^ n;
        return 1.0 - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0;
    }

    private static double Lerp(double a, double b, double t) => a + t * (b - a);

    // Smooth noise at (x, y) using bilinear interpolation
    private static double SmoothNoise(double x, double y) {
        int xInt = (int)Math.Floor(x);
        int yInt = (int)Math.Floor(y);
        double xFrac = x - xInt;
        double yFrac = y - yInt;
        
        double n00 = Hash(xInt, yInt);
        double n10 = Hash(xInt + 1, yInt);
        double n01 = Hash(xInt, yInt + 1);
        double n11 = Hash(xInt + 1, yInt + 1);
        
        double i1 = Lerp(n00, n10, xFrac);
        double i2 = Lerp(n01, n11, xFrac);
        return Lerp(i1, i2, yFrac);
    }

    // Fractal noise by summing several octaves
    private static double FractalNoise(double x, double y, int octaves, double persistence) {
        double total = 0.0;
        double frequency = 1.0;
        double amplitude = 1.0;
        double maxValue = 0.0;
        for (int i = 0; i < octaves; i++) {
            total += SmoothNoise(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2.0;
        }
        return total / maxValue;
    }

    // Generate a warped texture pattern
    // colors: list of colours to blend
    // weightings: relative weight for each colour (should have same count as colors)
    // width, height: texture resolution
    // warpFactor: how much the noise warps the coordinates (default 50)
    public static Color[,] GenerateWarpedTexturePattern(List<Color> colors, List<float> weightings,
                                                        int width, int height, double warpFactor = 5.0) {
        if (colors == null || weightings == null || colors.Count != weightings.Count || colors.Count == 0)
            throw new ArgumentException("Colors and weightings must be non-null and have the same non-zero length.");

        Color[,] texture = new Color[width, height];

        // Build a cumulative distribution function from the weightings
        int count = weightings.Count;
        double[] cdf = new double[count];
        double sum = 0.0, cum = 0.0;
        for (int i = 0; i < count; i++) sum += weightings[i];
        for (int i = 0; i < count; i++) {
            cum += weightings[i] / sum;
            cdf[i] = cum;
        }

        // For each pixel, compute a noise value
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                double u = (double)x / width, v = (double)y / height;
                
                // First, generate a base low-frequency noise to warp coordinates
                double warpX = u + warpFactor * FractalNoise(u, v, octaves: 3, persistence: 0.5);
                double warpY = v + warpFactor * FractalNoise(u + 100, v + 100, octaves: 3, persistence: 0.5);

                // Use the warped coordinates in a second noise pass
                // This can act like a musgrave/voronoi style pattern
                double noiseValue = FractalNoise(warpX, warpY, octaves: 4, persistence: 0.5);
                // Normalize noise value into [0,1] (SmoothNoise returns roughly in [-1,1])
                noiseValue = (noiseValue + 1.0) / 2.0;
                noiseValue = Math.Min(1.0, Math.Max(0.0, noiseValue));

                // Find which interval of the CDF the noise value falls into
                // We then blend between the two adjacent colours
                int lowerIndex = 0;
                int upperIndex = 0;
                for (int i = 0; i < count; i++) {
                    if (noiseValue <= cdf[i]) {
                        upperIndex = i;
                        lowerIndex = (i == 0) ? 0 : i - 1;
                        break;
                    }
                }
                // Compute the blend factor between lowerIndex and upperIndex
                double lowerBound = (lowerIndex == 0) ? 0.0 : cdf[lowerIndex];
                double upperBound = cdf[upperIndex];
                double factor = (upperBound - lowerBound) > 0.0 ? (noiseValue - lowerBound) / (upperBound - lowerBound) : 0.0;

                Color finalColor = colors[lowerIndex].Blend(colors[upperIndex], (float)factor);
                texture[x, y] = finalColor;
            }
        }
        return texture;
    }

    public static Color[,] LoadSphericalTexture(string filePath, int outW, int outH) {
        #pragma warning disable CA1416
        if (!File.Exists(filePath)) {
            TUI.Err("TEXLOAD", $"Failed to load texture from {filePath}");
            return GenerateCheckerboardTexture(outW, outH, Color.Magenta1);
        }

        using Bitmap bmp = new(filePath);
        Color[,] res = new Color[outW, outH];

        for (int x = 0; x < outW; x++)
        for (int y = 0; y < outH; y++) {
            double xn = x / (double)outW;
            double yn = y / (double)outH;

            System.Drawing.Color p = bmp.GetPixel((int)(xn*bmp.Width), (int)(yn*bmp.Height));
            res[x, y] = new Color(p.R, p.G, p.B);
        }

        return res;
        #pragma warning restore CA1416
    }

    public static Color[,] GenerateCheckerboardTexture(int w, int h, Color? a = null, Color? b = null) {
        Color[,] res = new Color[w, h];
        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++) {
            res[x, y] = (x+y)%2 == 0 ? (a ?? Color.White) : (b ?? Color.Black);
        }

        return res;
    }

    public static Color[,] GenerateNoiseTexture(int w, int h) {
        Color[,] res = new Color[w, h];
        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++) {
            byte[] c = new byte[3]; rnd.NextBytes(c);
            res[x, y] = new Color(c[0], c[1], c[2]);
        }

        return res;
    }

    public static Color[,] GenerateTextureFromSeed(string seed) {
        // Hash seed string
        byte[] hash;
        using (SHA256 sha = SHA256.Create()) { hash = sha.ComputeHash(Encoding.UTF8.GetBytes(seed)); }

        // Convert hash to random seed
        int randomSeed = BitConverter.ToInt32(hash, 0);
        Random rand = new Random(randomSeed);

        // Generate palette
        List<Color> palette = new List<Color>();
        for (int i = 0; i < 3; i++) {
            byte r = (byte)rand.Next(0, 256);
            byte g = (byte)rand.Next(0, 256);
            byte b = (byte)rand.Next(0, 256);
            palette.Add(new Color(r, g, b));
        }

        // Generate weights
        List<float> weights = new List<float>();
        for (int i = 0; i < 3; i++) { weights.Add((float)(rand.NextDouble() * 50.0 + 1.0)); }

        // Generate warp factor
        double warpFactor = rand.NextDouble() * 45.0 + 5.0;

        // Generate texture using utility
        return GenerateWarpedTexturePattern(palette, weights, 100, 50, warpFactor);
    }
}