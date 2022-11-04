using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFColorLib
{
    public partial class HSLColorSelector : UserControl
    {
        public HSLColorSelector()
        {
            InitializeComponent();

            slidHue.ValueChanged += SlidHue_ValueChanged;
        }

        public event EventHandler<Tuple<byte, byte, byte>> ColorRGBChanged;
        public event EventHandler<Tuple<int, int, int>> ColorHSLChanged;// 0..360, 0..100, 0..100

        void Control_Resized(object sender, SizeChangedEventArgs e)
        {
            #region recreate hue bar
            var imgW = (int)(brdHue.ActualWidth - 2);
            var imgH = (int)(brdHue.ActualHeight - 2);
            imgHue.Source = new WriteableBitmap(imgW, imgH, 96, 96, PixelFormats.Bgr32, null);
            DrawHueBar();
            #endregion

            #region recreate Saturation/Lightness area
            imgW = (int)(brdSaturLight.ActualWidth - 2);
            imgH = (int)(brdSaturLight.ActualHeight - 2);
            imgSaturLight.Source = new WriteableBitmap(imgW, imgH, 96, 96, PixelFormats.Bgr32, null);
            RedrawSaturLight(slidHue.Value);
            #endregion
        }

        void DrawHueBar()
        {
            var recW = (int)(brdHue.ActualWidth - 2);
            var recH = (int)(brdHue.ActualHeight - 2);
            var k = 360.0 / recW;
            var bmp = (WriteableBitmap)imgHue.Source;
            bmp.Lock();
            unsafe {
                var buf = bmp.BackBuffer;
                // fill the first line of hue bar
                for(int x=0; x < recW; x++){
                    int hue = (int)Math.Floor(x * k);
                    *((int*)(buf + x * 4)) = ClrHlp.HSL2RGBInt(hue, 100, 100);
                }
                var origColorLine = new Span<byte>(buf.ToPointer(), bmp.BackBufferStride);
                for (int y = 1; y < recH; y++) {
                    var destColorLine = new Span<byte>((buf + y * bmp.BackBufferStride).ToPointer(), bmp.BackBufferStride);
                    origColorLine.CopyTo(destColorLine);
                }
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, recW, recH));
            bmp.Unlock();
        }

        void SlidHue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RedrawSaturLight(e.NewValue);

            // calc contrast color for ellipse(target above color)
            var contrastHue = ((int)e.NewValue + 180) % 360;// opposite side of color ring
            var contrastColor = ClrHlp.HSL2RGB(contrastHue, 100, 100);
            ellClrTarget.Stroke = new SolidColorBrush(Color.FromRgb((byte)contrastColor[0], (byte)contrastColor[1], (byte)contrastColor[2]));
        }

        void RedrawSaturLight(double newHue)
        {
            var hue = (int)newHue;
            var recW = (int)(brdSaturLight.ActualWidth - 2);
            var recH = (int)(brdSaturLight.ActualHeight - 2);
            var kX = 100.0 / recW;
            var kY = 100.0 / recH;
            var bmp = (WriteableBitmap)imgSaturLight.Source;
            bmp.Lock();
            unsafe {
                var buf = bmp.BackBuffer;
                for (int y = 0; y < recH; y++) {
                    var lineStart = buf + y * bmp.BackBufferStride;
                    for (int x = 0; x < recW; x++) {
                        *((int*)(lineStart + x * 4)) = ClrHlp.HSL2RGBInt(hue, (int)(kX * x), (int)(kY * y));
                    }
                }
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, recW, recH));
            bmp.Unlock();
        }

        bool trackColorMode = false;

        void imgSaturLight_MouDown(object sender, MouseButtonEventArgs e)
        {
            trackColorMode = true;
            imgSaturLight.CaptureMouse();

            SelectNewColor(e.GetPosition(imgSaturLight));
        }

        void imgSaturLight_MouMove(object sender, MouseEventArgs e)
        {
            if (!trackColorMode) return;

            SelectNewColor(e.GetPosition(imgSaturLight));
        }

        void SelectNewColor(Point mou)
        {
            var recW = (int)(brdSaturLight.ActualWidth - 2);
            var recH = (int)(brdSaturLight.ActualHeight - 2);
            var kX = 100.0 / recW;
            var kY = 100.0 / recH;

            if (mou.X < 0)
                mou.X = 0;
            else if (mou.X >= recW)
                mou.X = recW - 1;
            if (mou.Y < 0)
                mou.Y = 0;
            else if (mou.Y >= recH)
                mou.Y = recH - 1;

            Canvas.SetLeft(ellClrTarget, mou.X-2);
            Canvas.SetTop(ellClrTarget, mou.Y-2);

            var hue = (int)slidHue.Value;
            var sat = (int)(mou.X * kX);
            var lum = (int)(mou.Y * kY);
            ColorHSLChanged?.Invoke(this, new Tuple<int, int, int>(hue, sat, lum));

            if (ColorRGBChanged != null) {
                var clr = ClrHlp.HSL2RGB(hue, sat, lum);
                ColorRGBChanged.Invoke(this, new Tuple<byte, byte, byte>((byte)clr[0], (byte)clr[1], (byte)clr[2]));
            }
        }

        void imgSaturLight_MouUp(object sender, MouseButtonEventArgs e)
        {
            trackColorMode = false;
            imgSaturLight.ReleaseMouseCapture();
        }
    }
}
