using TMPro;
using UnityEngine;

namespace ProDomino.Core.UI
{
    [CreateAssetMenu(fileName = "ProDominoUITheme", menuName = "ProDomino/UI Theme")]
    public class UITheme : ScriptableObject
    {
        [Header("Brand")]
        public Color primaryColor = Parse("#FDC553");
        public Color secondaryColor = Parse("#FFFFFF");
        public Color backgroundColor = Parse("#01010C");
        public Color primaryButtonStart = Parse("#FFA501");
        public Color primaryButtonEnd = Parse("#FDC653");
        public Color primaryButtonBorder = Parse("#FDC653");
        public Color primaryButtonLabel = Parse("#01010C");

        [Header("Black Scale")]
        public Color black50 = Parse("#E6E6E7");
        public Color black100 = Parse("#B0B0B4");
        public Color black200 = Parse("#8A8A8F");
        public Color black300 = Parse("#55555C");
        public Color black400 = Parse("#34343D");
        public Color black500 = Parse("#01010C");
        public Color black600 = Parse("#01010B");
        public Color black700 = Parse("#010109");
        public Color black800 = Parse("#010107");
        public Color black900 = Parse("#000005");

        [Header("Yellow Scale")]
        public Color yellow50 = Parse("#FFF9EE");
        public Color yellow100 = Parse("#FEEDCA");
        public Color yellow200 = Parse("#FEE4B0");
        public Color yellow300 = Parse("#FED88C");
        public Color yellow400 = Parse("#FDD175");
        public Color yellow500 = Parse("#FDC553");
        public Color yellow600 = Parse("#E6B34C");
        public Color yellow700 = Parse("#B48C3B");
        public Color yellow800 = Parse("#8B6C2E");
        public Color yellow900 = Parse("#6A5323");

        [Header("Surfaces")]
        public Color inputFill = Parse("#212129");
        public Color inputBorder = Parse("#2E2E38");
        public Color modalStart = Parse("#27272C");
        public Color modalEnd = Parse("#01010C");
        public Color modalBorder = Parse("#37373D");
        public Color closeButtonFill = Parse("#34343D");
        public Color closeButtonBorder = Parse("#3E3E3E");
        public Color checkboxFill = new Color(52f / 255f, 52f / 255f, 61f / 255f, 0.3f);

        [Header("Typography Sizes")]
        public float heading2Size = 36f;
        public float heading4Size = 24f;
        public float heading6Size = 16f;
        public float bodySize = 16f;

        [Header("Heights (Figma)")]
        public float primaryButtonHeight = 56f;
        public float inputFieldHeight = 48f;
        public float labeledInputHeight = 80f;
        public float checkboxHeight = 24f;
        public float closeButtonSize = 50f;

        [Header("Radii & Modal Padding")]
        public float buttonCornerRadius = 10f;
        public float inputCornerRadius = 10f;
        public float modalCornerRadius = 12f;
        public float closeButtonCornerRadius = 8f;
        public float checkboxCornerRadius = 4f;
        public float modalPaddingHorizontal = 80f;
        public float modalPaddingTop = 100f;
        public float modalPaddingBottom = 100f;
        public float labeledInputSpacing = 12f;

        [Header("Montserrat TMP Fonts (assign when available)")]
        public TMP_FontAsset fontRegular;
        public TMP_FontAsset fontMedium;
        public TMP_FontAsset fontSemiBold;
        public TMP_FontAsset fontBold;

        public TMP_FontAsset GetFont(UIFontWeight weight)
        {
            TMP_FontAsset selected = weight switch
            {
                UIFontWeight.Medium => fontMedium,
                UIFontWeight.SemiBold => fontSemiBold,
                UIFontWeight.Bold => fontBold,
                _ => fontRegular
            };

            if (selected != null)
                return selected;

            if (fontRegular != null)
                return fontRegular;
            if (fontMedium != null)
                return fontMedium;
            if (fontSemiBold != null)
                return fontSemiBold;
            if (fontBold != null)
                return fontBold;

            return TMP_Settings.defaultFontAsset;
        }

        public void ApplyTo(TMP_Text text, UIFontWeight weight, float size, Color color)
        {
            if (text == null)
                return;

            TMP_FontAsset font = GetFont(weight);
            if (font != null)
                text.font = font;

            text.fontSize = size;
            text.color = color;
        }

        private static Color Parse(string html)
        {
            return ColorUtility.TryParseHtmlString(html, out Color color) ? color : Color.magenta;
        }
    }
}
