using ProDomino.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProDomino.UI.Components
{
    public class UIThemeBinder : MonoBehaviour
    {
        [SerializeField] private UITheme theme;
        [SerializeField] private Image[] images;
        [SerializeField] private Color[] imageColors;
        [SerializeField] private TMP_Text[] labels;
        [SerializeField] private Color[] labelColors;
        [SerializeField] private UIFontWeight[] labelWeights;
        [SerializeField] private float[] labelSizes;

        public UITheme Theme => theme;

        private void OnEnable()
        {
            Apply();
        }

        public void Apply()
        {
            if (theme == null)
                return;

            if (images != null)
            {
                int count = Mathf.Min(images.Length, imageColors != null ? imageColors.Length : 0);
                for (int i = 0; i < count; i++)
                {
                    if (images[i] != null)
                        images[i].color = imageColors[i];
                }
            }

            if (labels != null)
            {
                int count = labels.Length;
                for (int i = 0; i < count; i++)
                {
                    if (labels[i] == null)
                        continue;

                    Color color = labelColors != null && i < labelColors.Length ? labelColors[i] : theme.secondaryColor;
                    UIFontWeight weight = labelWeights != null && i < labelWeights.Length ? labelWeights[i] : UIFontWeight.Regular;
                    float size = labelSizes != null && i < labelSizes.Length ? labelSizes[i] : theme.bodySize;
                    theme.ApplyTo(labels[i], weight, size, color);
                }
            }
        }
    }
}
