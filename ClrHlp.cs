using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WPFColorLib
{
    public class ClrHlp
    {
        public static (int, int, int) ColorToHSL(Color rgb)
        {
	        var r = (rgb.R / 255.0);
	        var g = (rgb.G / 255.0);
	        var b = (rgb.B / 255.0);

	        var min = Math.Min(Math.Min(r, g), b);
	        var max = Math.Max(Math.Max(r, g), b);
	        var delta = max - min;

	        var lum = (max + min) / 2;
            double hue, sat;

	        if (delta == 0)
	        {
		        hue = 0.0;
		        sat = 0.0;
	        }
	        else
	        {
		        sat = (lum <= 0.5) ? (delta / (max + min)) : (delta / (2 - max - min));

		        if (r == max)
		        {
			        hue = ((g - b) / 6.0) / delta;
		        }
		        else if (g == max)
		        {
			        hue = (1.0 / 3.0) + ((b - r) / 6.0) / delta;
		        }
		        else
		        {
			        hue = (2.0 / 3.0) + ((r - g) / 6.0) / delta;
		        }

		        if (hue < 0)
			        hue += 1;
		        if (hue > 1)
			        hue -= 1;
	        }

	        return ((int)(hue*360), (int)(sat*100), (int)(lum*100));
        }

        public static Regex reHexRGB = new Regex(@"([a-f0-9]{2})([a-f0-9]{2})([a-f0-9]{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Color Hex2color(string s)
        {
            var m = reHexRGB.Match(s);
            if (m.Success)
                return Color.FromRgb(Hex2Byte(m.Groups[1].Value), Hex2Byte(m.Groups[2].Value), Hex2Byte(m.Groups[3].Value));
            throw new ArgumentException($"Invalid hex RGB value '{s}'");
        }

        public static string Color2hex(Color clr)
        {
            return clr.R.ToString("X02") + clr.G.ToString("X02") + clr.B.ToString("X02");
        }

        public static byte Hex2Byte(string hex)
        {
            return byte.Parse(hex, NumberStyles.AllowHexSpecifier);
        }

        public static int HSL2RGBInt(int h, int s, int l)
        {
            var rgb = HSL2RGB(h, s, l);
            return rgb[0] << 16 | rgb[1] << 8 | rgb[2];
        }

        public static int[] HSL2RGB(int h, int s, int l)
        {
            var rgb = new int[3];

            var baseColor = (h + 60) % 360 / 120;
            var shift = (h + 60) % 360 - (120 * baseColor + 60);
            var secondaryColor = (baseColor + (shift >= 0 ? 1 : -1) + 3) % 3;

            //Setting Hue
            rgb[baseColor] = 255;
            rgb[secondaryColor] = (int)((Math.Abs(shift) / 60.0f) * 255.0f);

            //Setting Saturation
            for (var i = 0; i < 3; i++)
                rgb[i] += (int)((255 - rgb[i]) * ((100 - s) / 100.0f));

            //Setting Value
            for (var i = 0; i < 3; i++)
                rgb[i] -= (int)(rgb[i] * (100 - l) / 100.0f);

            return rgb;
        }
    }
}
