using ProDomino.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ProDomino.UI.Components
{
    /// <summary>
    /// Sizes the auth modal from the live canvas: centered card on web/desktop,
    /// full-bleed sheet on narrow/mobile, with safe-area and keyboard insets.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class AuthResponsiveLayout : MonoBehaviour
    {
        public const float CompactPixelWidth = 900f;
        public const float DesktopMarginPixels = 24f;
        public const float CompactMarginPixels = 16f;

        [SerializeField] private float preferredWidth = AuthScreenLayout.ModalWidth;
        [SerializeField] private float preferredHeight = AuthScreenLayout.DefaultModalHeight;
        [SerializeField] private UITheme theme;

        private RectTransform rect;
        private Canvas canvas;
        private AuthModal modal;
        private Vector2 lastScreenSize;
        private float lastKeyboardInset = -1f;
        private bool lastCompact;
        private bool applied;
        private bool isApplying;

        public bool IsCompact { get; private set; }

        public void Configure(float width, float height, UITheme uiTheme)
        {
            preferredWidth = width;
            preferredHeight = height;
            theme = uiTheme;
            Cache();
            Apply();
        }

        private void Awake()
        {
            Cache();
        }

        private void OnEnable()
        {
            Cache();
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
                return;

            Apply();
        }

        private void LateUpdate()
        {
            var screen = new Vector2(Screen.width, Screen.height);
            float keyboard = KeyboardInsetCanvas();
            if (applied
                && screen == lastScreenSize
                && Mathf.Approximately(keyboard, lastKeyboardInset)
                && IsCompact == lastCompact)
                return;

            Apply();
        }

        private void Cache()
        {
            if (rect == null)
                rect = transform as RectTransform;

            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            if (modal == null)
                modal = GetComponentInChildren<AuthModal>(true);
        }

        private void Apply()
        {
            if (isApplying)
                return;

            Cache();
            if (rect == null || canvas == null)
                return;

            isApplying = true;
            try
            {
                float scale = Mathf.Max(0.0001f, canvas.scaleFactor);
                GetSafeInsets(scale, out float safeLeft, out float safeRight, out float safeBottom, out float safeTop);

                float keyboard = KeyboardInsetCanvas();
                float availableWidth = Screen.width / scale - safeLeft - safeRight;
                float availableHeight = Screen.height / scale - safeTop - safeBottom - keyboard;

                IsCompact = Screen.width < CompactPixelWidth
                    || (canvas.pixelRect.width / scale) < CompactPixelWidth;

                float margin = (IsCompact ? CompactMarginPixels : DesktopMarginPixels) / scale;

                if (IsCompact)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.offsetMin = new Vector2(safeLeft + margin, safeBottom + margin + keyboard);
                    rect.offsetMax = new Vector2(-(safeRight + margin), -(safeTop + margin));
                }
                else
                {
                    float width = Mathf.Min(preferredWidth, Mathf.Max(280f, availableWidth - margin * 2f));
                    float height = Mathf.Min(preferredHeight, Mathf.Max(320f, availableHeight - margin * 2f));

                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.sizeDelta = new Vector2(width, height);

                    float x = (safeLeft - safeRight) * 0.5f;
                    float y = (safeBottom + keyboard - safeTop) * 0.5f;
                    rect.anchoredPosition = new Vector2(x, y);
                }

                modal?.ApplyResponsiveStyle(IsCompact, scale, theme);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                lastScreenSize = new Vector2(Screen.width, Screen.height);
                lastKeyboardInset = keyboard;
                lastCompact = IsCompact;
                applied = true;
            }
            finally
            {
                isApplying = false;
            }
        }

        private static void GetSafeInsets(float scale, out float left, out float right, out float bottom, out float top)
        {
            Rect safe = Screen.safeArea;
            left = safe.xMin / scale;
            right = (Screen.width - safe.xMax) / scale;
            bottom = safe.yMin / scale;
            top = (Screen.height - safe.yMax) / scale;
        }

        private float KeyboardInsetCanvas()
        {
            if (canvas == null || !TouchScreenKeyboard.visible)
                return 0f;

            float scale = Mathf.Max(0.0001f, canvas.scaleFactor);
            float height = TouchScreenKeyboard.area.height;
            if (height <= 1f)
                return 0f;

            return height / scale;
        }
    }
}
