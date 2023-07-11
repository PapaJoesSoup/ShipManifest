using ShipManifest.InternalObjects.Settings;
using UnityEngine;


namespace ShipManifest
{
  // ReSharper disable once InconsistentNaming
  internal static class SMStyle
  {
    #region Properties

    internal static GUISkin SMSkin;
    internal static GUISkin KSPSkin;
    internal static GUISkin UnitySkin;
    internal static GUIStyle WindowStyle;
    internal static GUIStyle PopupStyle;
    internal static GUIStyle IconStyle;
    internal static GUIStyle ButtonSourceStyle;
    internal static GUIStyle ButtonTargetStyle;
    internal static GUIStyle ButtonToggledSourceStyle;
    internal static GUIStyle ButtonToggledTargetStyle;
    internal static GUIStyle ButtonStyle;
    internal static GUIStyle ButtonToggledStyle;
    internal static GUIStyle ButtonStyleLeft;
    internal static GUIStyle ButtonToggledStyleLeft;
    internal static GUIStyle ButtonOptionStyle;
    internal static GUIStyle ToggleStyleHeader;
    internal static GUIStyle ErrorLabelRedStyle;
    internal static GUIStyle LabelStyle;
    internal static GUIStyle LabelTabHeader;
    internal static GUIStyle LabelStyleBold;
    internal static GUIStyle LabelStyleCenter;
    internal static GUIStyle LabelStyleRed;
    internal static GUIStyle LabelStyleCyan;
    internal static GUIStyle LabelStyleYellow;
    internal static GUIStyle LabelStyleGreen;
    internal static GUIStyle LabelStyleNoWrap;
    internal static GUIStyle LabelStyleNoPad;
    internal static GUIStyle LabelStyleHardRule;
    internal static GUIStyle ToolTipStyle;
    internal static GUIStyle ScrollStyle;
    #endregion properties

    internal static void SetupGuiStyles()
    {
      SMSettings.LoadColors();
      SetStyles();
    }

    internal static void SetStyles()
    {
      // Set up GUI Styles
      KSPSkin = Object.Instantiate(HighLogic.Skin);
      UnitySkin = Object.Instantiate(GUI.skin);
      SMSkin = CurrSettings.UseUnityStyle ? UnitySkin : KSPSkin;

      // Scale skin fonts
      int newFontSize = (int)(12 * CurrSettings.CurrentUIScale);

      SMSkin.box.fontSize = newFontSize;
      SMSkin.button.fontSize = newFontSize;
      SMSkin.horizontalScrollbar.fontSize = newFontSize;
      SMSkin.horizontalScrollbarThumb.fontSize = newFontSize;
      SMSkin.horizontalScrollbarLeftButton.fontSize = newFontSize;
      SMSkin.horizontalScrollbarRightButton.fontSize = newFontSize;
      SMSkin.horizontalSlider.fontSize = newFontSize;
      SMSkin.horizontalSliderThumb.fontSize = newFontSize;
      SMSkin.label.fontSize = newFontSize;
      SMSkin.scrollView.fontSize = newFontSize;
      SMSkin.textArea.fontSize = newFontSize;
      SMSkin.textField.fontSize = newFontSize;
      SMSkin.toggle.fontSize = newFontSize;
      SMSkin.verticalScrollbar.fontSize = newFontSize;
      SMSkin.verticalScrollbarThumb.fontSize = newFontSize;
      SMSkin.verticalScrollbarUpButton.fontSize = newFontSize;
      SMSkin.verticalScrollbarDownButton.fontSize = newFontSize;
      SMSkin.verticalSlider.fontSize = newFontSize;
      SMSkin.verticalSliderThumb.fontSize = newFontSize;
      SMSkin.window.fontSize = newFontSize;
      
      WindowStyle = new GUIStyle(SMSkin.window);
      PopupStyle = new GUIStyle(SMSkin.window);
      IconStyle = new GUIStyle();

      ButtonStyle = new GUIStyle(SMSkin.button)
      {
        normal = {textColor = Color.white},
        hover = {textColor = Color.blue},
        padding =
        {
          top = 0,
          bottom = 0
        },
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.MiddleCenter,
        clipping = TextClipping.Clip
      };

      ButtonToggledStyle = new GUIStyle(SMSkin.button)
      {
        normal = {textColor = Color.green},
        fontSize = newFontSize,
        padding =
        {
          top = 0,
          bottom = 0
        },
        hover = {textColor = Color.blue}
      };

      ButtonToggledStyle.normal.background = ButtonToggledStyle.onActive.background;
      ButtonToggledStyle.fontStyle = FontStyle.Normal;
      ButtonToggledStyle.alignment = TextAnchor.MiddleCenter;
      ButtonToggledStyle.clipping = TextClipping.Clip;

      ButtonStyleLeft = new GUIStyle(SMSkin.button)
      {
        normal = {textColor = Color.white},
        hover = {textColor = Color.green},
        padding =
        {
          top = 0,
          bottom = 0
        },
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.MiddleLeft,
        clipping = TextClipping.Clip
      };

      ButtonToggledStyleLeft = new GUIStyle(SMSkin.button)
      {
        normal = {textColor = Color.green},
        padding =
        {
          top = 0,
          bottom = 0
        },
        hover = {textColor = Color.white}
      };
      ButtonToggledStyleLeft.normal.background = ButtonToggledStyleLeft.onActive.background;
      ButtonToggledStyleLeft.fontStyle = FontStyle.Normal;
      ButtonToggledStyleLeft.alignment = TextAnchor.MiddleLeft;
      ButtonToggledStyleLeft.clipping = TextClipping.Clip;

      ButtonSourceStyle = new GUIStyle(SMSkin.button)
      {
        normal = {textColor = Color.white},
        padding =
        {
          top = 0,
          bottom = 0
        },
        hover = {textColor = Color.blue},
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.MiddleLeft,
        clipping = TextClipping.Clip
      };

      ButtonToggledSourceStyle = new GUIStyle(SMSkin.button)
      {
        normal = {textColor = SMSettings.Colors[CurrSettings.SourcePartColor]},
        padding =
        {
          top = 0,
          bottom = 0
        },
        hover = {textColor = Color.blue}
      };
      ButtonToggledSourceStyle.normal.background = ButtonToggledSourceStyle.onActive.background;
      ButtonToggledSourceStyle.fontStyle = FontStyle.Normal;
      ButtonToggledSourceStyle.alignment = TextAnchor.MiddleLeft;
      ButtonToggledSourceStyle.clipping = TextClipping.Clip;

      ButtonTargetStyle = new GUIStyle(SMSkin.button)
      {
        normal = {textColor = Color.white},
        padding =
        {
          top = 0,
          bottom = 0
        },
        hover = {textColor = Color.blue},
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.MiddleLeft,
        clipping = TextClipping.Clip
      };

      ButtonOptionStyle = new GUIStyle(SMSkin.button)
      {
        normal = {textColor = Color.white},
        fixedHeight = 20 * CurrSettings.CurrentUIScale,
        padding =
        {
          top = 0,
          bottom = 0,
          left = 0,
          right = 0
        },
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        clipping = TextClipping.Clip
      };

      ButtonToggledTargetStyle = new GUIStyle(SMSkin.button)
      {
        normal = {textColor = SMSettings.Colors[CurrSettings.TargetPartColor]},
        padding =
        {
          top = 0,
          bottom = 0
        },
        hover = {textColor = Color.blue}
      };
      ButtonToggledTargetStyle.normal.background = ButtonToggledSourceStyle.onActive.background;
      ButtonToggledTargetStyle.fontStyle = FontStyle.Normal;
      ButtonToggledTargetStyle.clipping = TextClipping.Clip;
      ButtonToggledTargetStyle.alignment = TextAnchor.MiddleLeft;

      ToggleStyleHeader = new GUIStyle(SMSkin.toggle)
      {
        padding =
        {
          top = 10,
          bottom = 6
        },
        wordWrap = false,
        fontStyle = FontStyle.Bold,
        margin = new RectOffset(0, 0, 0, 0),
        alignment = TextAnchor.LowerLeft
      };

      ErrorLabelRedStyle = new GUIStyle(SMSkin.label) {normal = {textColor = Color.red}};

      LabelStyle = new GUIStyle(SMSkin.label);

      LabelTabHeader = new GUIStyle(SMSkin.label)
      {
        padding =
        {
          top = 10,
          bottom = 6
        },
        wordWrap = false,
        fontStyle = FontStyle.Bold,
        margin = new RectOffset(0, 0, 0, 0)
      };

      LabelStyleHardRule = new GUIStyle(SMSkin.label)
      {
        padding =
        {
          top = 0,
          bottom = 6
        },
        wordWrap = false,
        alignment = TextAnchor.LowerLeft,
        fontStyle = FontStyle.Bold,
        margin = new RectOffset(0, 0, 0, 0)
      };

      LabelStyleNoWrap = new GUIStyle(SMSkin.label)
      {
        wordWrap = false,
        clipping = TextClipping.Clip
      };

      LabelStyleNoPad = new GUIStyle(SMSkin.label)
      {
        padding =
        {
          top = 0,
          bottom = 0
        },
        wordWrap = false
      };

      LabelStyleBold = new GUIStyle(SMSkin.label)
      {
        fontStyle = FontStyle.Bold
      };

      LabelStyleCenter = new GUIStyle(SMSkin.label)
      {
        alignment = TextAnchor.UpperCenter,
        fontStyle = FontStyle.Bold
      };


      LabelStyleRed = new GUIStyle(LabelStyle) {normal = {textColor = Color.red}};

      LabelStyleCyan = new GUIStyle(LabelStyle) {normal = {textColor = Color.cyan}};

      LabelStyleYellow = new GUIStyle(LabelStyle) {normal = {textColor = Color.yellow}};

      LabelStyleGreen = new GUIStyle(LabelStyle) {normal = {textColor = Color.green}};
      ScrollStyle = new GUIStyle(SMSkin.box);

      //if (GUI.skin != null) GUI.skin = null;

      // ReSharper disable once PossibleNullReferenceException
      ToolTipStyle = new GUIStyle(SMSkin.textArea)
      {
        border = new RectOffset(4, 4, 4, 4),
        padding = new RectOffset(5, 5, 5, 5),
        alignment = TextAnchor.MiddleLeft,
        fontStyle = FontStyle.Italic,
        wordWrap = false,
        normal = {textColor = Color.green},
        hover = {textColor = Color.green}
      };
      ToolTipStyle.hover.background = ToolTipStyle.normal.background;

    }
  }
}
