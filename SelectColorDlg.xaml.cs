using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFColorLib
{
    public partial class SelectColorDlg : Window
    {
        static List<Color> favColors = new List<Color>();// here we keep favorite colors between dialog calls

        public Color SelectedColor => ((SolidColorBrush)bordNewClr.Background).Color;
        public Brush SelectedBrush => bordNewClr.Background;

        public SelectColorDlg(Color? preselColor = null, string title = null)
        {
            InitializeComponent();
            if (title != null) Title = title;

            foreach(var clr in favColors){
                AddToFavorites(new SolidColorBrush(clr), false);
            }
            if (preselColor.HasValue) {
                var clr = preselColor.Value;
                bordOldClr.Background = new SolidColorBrush(clr);
                txtOldClrHEX.Text = ClrHlp.Color2hex(clr);
                txtOldClrDEC.Text = $"{clr.R},{clr.G},{clr.B}";
            }
            hslColorSel.ColorRGBChanged += HslColorSel_ColorRGBChanged;
        }

        void HslColorSel_ColorRGBChanged(object sender, Tuple<byte, byte, byte> e)
        {
            ShowSelectedColor(Color.FromRgb(e.Item1, e.Item2, e.Item3));
        }

        void ShowSelectedColor(Color clr)
        {
            bordNewClr.Background = new SolidColorBrush(clr);
            txtNewClrHEX.Text = ClrHlp.Color2hex(clr);
            txtNewClrDEC.Text = $"{clr.R}, {clr.G}, {clr.B}";

            GenerateLuminosityPalette(clr);

            btnOK.IsEnabled = true;
        }

        void GenerateLuminosityPalette(Color baseClr)
        {
            pnlLumPalette.Children.Clear();
            var (hue, sat, lum) = ClrHlp.ColorToHSL(baseClr);
            for (int i = 0; i <= 100; i += 10) {
                var clrArr = ClrHlp.HSL2RGB(hue, sat, i);
                var clr = Color.FromRgb((byte)clrArr[0], (byte)clrArr[1], (byte)clrArr[2]);
                var ctrl = new Border { Background = new SolidColorBrush(clr), BorderBrush = Brushes.Black, BorderThickness = thck1, CornerRadius = rad3, Width = 30, Height = 30, Margin = thck2 };
                ctrl.MouseLeftButtonDown += (_, e) => {
                    ShowSelectedColor(clr);
                    e.Handled = true;
                };
                pnlLumPalette.Children.Add(ctrl);
            }
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        void AddFavorites_NewClr_Click(object sender, RoutedEventArgs e)
        {
            AddToFavorites(bordNewClr.Background);
        }

        void CopyNewClrHEX_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtNewClrHEX.Text);
            //this.ShowHint("Copied " + txtNewClrHEX.Text);
        }

        void CopyNewClrDEC_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtNewClrDEC.Text);
            //this.ShowHint("Copied " + txtNewClrDEC.Text);
        }

        void AddFavorites_OldClr_Click(object sender, RoutedEventArgs e)
        {
            AddToFavorites(bordOldClr.Background);   
        }

        void CopyOldClrHEX_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtOldClrHEX.Text);
            //this.ShowHint("Copied " + txtOldClrHEX.Text);
        }

        void CopyOldClrDEC_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtOldClrDEC.Text);
            //this.ShowHint("Copied " + txtOldClrDEC.Text);
        }

        static Thickness thck1 = new Thickness(1);
        static Thickness thck2 = new Thickness(2);
        static CornerRadius rad3 = new CornerRadius(3);

        bool RemoveFavoriteMode;// 'true' when fav.color has RMB-Down
        void AddToFavorites(Brush clr, bool saveColor = true)
        {
            var color = ((SolidColorBrush)clr).Color;
            if (saveColor) favColors.Add(color);

            var ctrl = new Border { Background = clr, BorderBrush = Brushes.Black, BorderThickness = thck1, CornerRadius = rad3, Width = 30, Height = 30, Margin = thck2, Tag = color };
            ctrl.MouseLeftButtonDown += (_, e) => {
                ShowSelectedColor(color);
                e.Handled = true;
            };
            ctrl.PreviewMouseRightButtonDown += (_, e) => {
                pnlFavColors.Children.Remove(ctrl);
                favColors.Remove((Color)ctrl.Tag);
                RemoveFavoriteMode = true;
                e.Handled = true;
            };
            pnlFavColors.Children.Add(ctrl);
        }
        
        /// <summary>When fav.color is removed by RMB-Down, next RMB-Up can appear above empty container and should not call popup menu.</summary>
        void pnlFavColors_RMouUp(object sender, MouseButtonEventArgs e)
        {
            if (RemoveFavoriteMode){
                RemoveFavoriteMode = false;
                e.Handled = true;
            }
        }

        Regex reHex6 = new Regex(@"[0-9a-f]{6}", RegexOptions.Compiled|RegexOptions.IgnoreCase);
        void txtNewClrHEX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            ParseHexRGB();
            e.Handled = true;
        }

        bool ParseHexRGB()
        {
            if (reHex6.IsMatch(txtNewClrHEX.Text)){
                ShowSelectedColor(ClrHlp.Hex2color(txtNewClrHEX.Text));
                return true;
            } else {
                MessageBox.Show("Color must be in RRGGBB hex form", "Error parsing color value", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        Regex reDec3 = new Regex(@"(\d+)\s*,\s*(\d+)\s*,\s*(\d+)", RegexOptions.Compiled);
        void txtNewClrDEC_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            ParseDecRGB();
            e.Handled = true;
        }
        
        bool ParseDecRGB()
        {
            var m = reDec3.Match(txtNewClrDEC.Text);
            if (m.Success){
                try {
                    var clr = Color.FromRgb(byte.Parse(m.Groups[1].Value), byte.Parse(m.Groups[2].Value), byte.Parse(m.Groups[3].Value));
                    ShowSelectedColor(clr);
                    return true;
                } catch {
                    MessageBox.Show("Check values - they must fit BYTE", "Error parsing color value", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else {
                MessageBox.Show("Color must be in 0,0,0 decimal form", "Error parsing color value", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        void PasteNewClrHEX_Click(object sender, RoutedEventArgs e)
        {
            var oldText = txtNewClrHEX.Text;
            txtNewClrHEX.Text = Clipboard.GetText();
            if (!ParseHexRGB()) txtNewClrHEX.Text = oldText;
        }

        void PasteNewClrDEC_Click(object sender, RoutedEventArgs e)
        {
            var oldText = txtNewClrDEC.Text;
            txtNewClrDEC.Text = Clipboard.GetText();
            if (!ParseDecRGB()) txtNewClrDEC.Text = oldText;
        }

        static List<string> paletteApple16 = new List<string> {
            "FFFFFF", "FCF400", "FF6400", "DD0202", "F00285", "4600A5", "0000D5", "00AEE9", 
            "1AB90C", "006407", "572800", "917035", "C1C1C1", "818181", "3E3E3E", "000000", };
        void GenerateAppleColors_Click(object sender, RoutedEventArgs e)
        {
            foreach (var s in paletteApple16)
                AddToFavorites(new SolidColorBrush(ClrHlp.Hex2color(s)));
        }

        static string[] hex6LevelColor = new string[] { "00", "33", "66", "99", "CC", "FF" };
        void GenerateStdColors_Click(object sender, RoutedEventArgs e)
        {
            for(int b=0; b < hex6LevelColor.Length; b++)
                for(int g=0; g < hex6LevelColor.Length; g++)
                    for(int r=0; r < hex6LevelColor.Length; r++){
                        AddToFavorites(new SolidColorBrush(ClrHlp.Hex2color(hex6LevelColor[r] + hex6LevelColor[g] + hex6LevelColor[b])));
                    }
        }

        void ClearFav_Click(object sender, RoutedEventArgs e)
        {
            pnlFavColors.Children.Clear();
            favColors.Clear();
        }
    }
}
