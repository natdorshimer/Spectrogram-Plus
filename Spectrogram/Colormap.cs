﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Spectrogram
{
    public class Colormap
    {
        public static Colormap Argo => new Colormap(new Colormaps.Argo());
        public static Colormap Blues => new Colormap(new Colormaps.Blues());
        public static Colormap Grayscale => new Colormap(new Colormaps.Grayscale());
        public static Colormap GrayscaleReversed => new Colormap(new Colormaps.Grayscale());
        public static Colormap Greens => new Colormap(new Colormaps.Greens());
        public static Colormap Inferno => new Colormap(new Colormaps.Inferno());
        public static Colormap Lopora => new Colormap(new Colormaps.Lopora());
        public static Colormap Magma => new Colormap(new Colormaps.Magma());
        public static Colormap Plasma => new Colormap(new Colormaps.Plasma());
        public static Colormap Turbo => new Colormap(new Colormaps.Turbo());
        public static Colormap Viridis => new Colormap(new Colormaps.Viridis());

        private readonly IColormap cmap;
        public readonly string Name;
        private BitmapPalette palette;

        public BitmapPalette GetBitmapPalette()
        {
            return palette;
        }

        public Colormap(IColormap colormap)
        {
            cmap = colormap ?? new Colormaps.Grayscale();
            Name = cmap.GetType().Name;

            IList<System.Windows.Media.Color> color_list = new List<System.Windows.Media.Color>();
            for (int i = 0; i < 256; i++)
            {
                System.Drawing.Color c = GetColor((byte)i);
                System.Windows.Media.Color MediaColor = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
                color_list.Add(MediaColor);
            }
            palette = new BitmapPalette(color_list);
        }

        public override string ToString()
        {
            return $"Colormap {Name}";
        }

        public static Colormap[] GetColormaps()
        {
            IColormap[] ics = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(s => s.GetTypes())
                                .Where(p => p.IsInterface == false)
                                .Where(p => p.ToString().StartsWith("Spectrogram.Colormaps."))
                                .Select(x => x.ToString())
                                .Select(path => (IColormap)Activator.CreateInstance(Type.GetType(path)))
                                .ToArray();

            return ics.Select(x => new Colormap(x)).ToArray();
        }

        public static string[] GetColormapNames()
        {
            return GetColormaps().Select(x => x.Name).ToArray();
        }

        public static Colormap GetColormap(string colormapName)
        {
            foreach (Colormap cmap in GetColormaps())
                if (string.Equals(cmap.Name, colormapName, StringComparison.InvariantCultureIgnoreCase))
                    return cmap;

            throw new ArgumentException($"Colormap does not exist: {colormapName}");
        }

        public (byte r, byte g, byte b) GetRGB(byte value)
        {
            return cmap.GetRGB(value);
        }

        public (byte r, byte g, byte b) GetRGB(double fraction)
        {
            fraction = Math.Max(fraction, 0);
            fraction = Math.Min(fraction, 1);
            return cmap.GetRGB((byte)(fraction * 255));
        }

        public int GetInt32(byte value)
        {
            var (r, g, b) = GetRGB(value);
            return 255 << 24 | r << 16 | g << 8 | b;
        }

        public int GetInt32(double fraction)
        {
            var (r, g, b) = GetRGB(fraction);
            return 255 << 24 | r << 16 | g << 8 | b;
        }

        public Color GetColor(byte value)
        {
            return Color.FromArgb(GetInt32(value));
        }

        public Color GetColor(double fraction)
        {
            return Color.FromArgb(GetInt32(fraction));
        }

        public void Apply(Bitmap bmp)
        {
            System.Drawing.Imaging.ColorPalette pal = bmp.Palette;
            for (int i = 0; i < 256; i++)
                pal.Entries[i] = GetColor((byte)i);
            bmp.Palette = pal;
        }
    }
}
