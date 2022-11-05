# WPFColorLib

This library is created for "WPF/.NET FW 4.8" and intended to work with colors.

-------------------------------------------------------------------------------------
Standard WPF lacks of color selection dialog and WPFColorLib fills this niche with brand new approach:

![Picture of dialog]( https://i.imgur.com/yeyOuX5_d.webp?maxwidth=760&fidelity=grand )

Description of functionality:

- First, user selects desirable Hue level using top slider
- Saturation/Luminosity gradient below shows color variations of current hue
- Clicking/moving mouse on gradient selects one specific color (and button `OK` become enabled)
  and its RGB value is reflected in Hex and Dec fields below. Contrast circle on gradient
  shows the place of selection
- Right from "New color" block there is "Old color" if dialog was called with some original color (to compare)
- Each value, hex or dec, can be typed directly (and confirmed with `Enter`) or pasted from
  Clipboard (using button on the left of the field)
  > Note that pasted text can be any free text taken from CSS, for example. Strings like
  `#p1 {background-color: #ff0000;}   /* red */` will be correctly parsed, catchig first matching
  RGB hex value (here `ff0000`)
- Right from Dec/Hex fields there is ❍ (copy to Clipboard) button. Old color also can be copied
- Once color is selected, "Luminosity palette" is filled with 11 variations of current color,
  where H/S is the same and "Luminosity" level vary from 0 to 100 with step 10 - these
  samples also can be selected
- Selected color can be added to "Favorite colors", clicking on ⊕ button right from color sample
- When you click with *left* mouse button on "Favorite color" sample, its color become selected.
  *Right* click deletes this sample
- "Favorite colors" also can be *generated*: right click on favorite area and select 16-color
  Apple's palette or click "Standard 6 level" to generate 216-colors palette, where each
  color has 6-level variations of RGB components /easier to try than explain :) /.
  From the same popup you can clear favorites
- "Favorite colors" are saved between dialog calls, but lost when you exit application

Usage:

Reference necessary DLLs: [System.Memory](https://www.nuget.org/packages/System.Memory) 
and [System.Runtime.CompilerServices.Unsafe](https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe/)

```C#
var dlg = new SelectColorDlg(Colors.Bisque) { Owner = this };
if (dlg.ShowDialog() == true) {
    var color = dlg.SelectedColor;// as Color struct
    var brush = dlg.SelectedBrush;// the same, but as SolidColorBrush
}
```

-------------------------------------------------------------------------------------

WPFColorLib also contains a few helpers in a static class `ClrHlp`:

- ColorToHSL - convert `Color` struct to (H, S, L) presentation (H=0..359, S=0..100, L=0..100)
- Hex2color - takes free text with hex RGB presentation and returns created `Color`
- Color2hex - takes `Color` and returns hex RGB presentation
- Hex2Byte - parse hex byte presentation to native `byte` type
- HSL2RGB - convert (H,S,L) color to its RGB presentation (as array of 3 `int`s)
- HSL2RGBInt - the same as `HSL2RGB`, but returns RGB components packed into integer
