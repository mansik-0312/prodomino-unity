using ProDomino.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProDomino.UI.Components
{
    public static class AuthScreenLayout
    {
        public const float ModalWidth = 880f;
        public const float DefaultModalHeight = 860f;
        public const float StatusModalHeight = 531f;
        public const float ContentWidth = 720f;

        public const string ThemePath = "Assets/ScriptableObjects/UI/ProDominoUITheme.asset";
        public const string AuthModalPrefabPath = "Assets/Prefabs/UI/Modals/AuthModal.prefab";
        public const string BgImagePath = "Assets/Art/UI/Login/bg-image.png";
        public const string DominoPatternPath = "Assets/Art/UI/Login/domino-pattern.png";
        public const string LogoPath = "Assets/Art/UI/Login/logo-prodomino.svg";
        public const string EyeIconPath = "Assets/Art/UI/Login/icon-eye.svg";

        public static UITheme LoadTheme()
        {
            return LoadAsset<UITheme>(ThemePath);
        }

        public static T LoadAsset<T>(string assetPath) where T : Object
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
            return null;
#endif
        }

        public static Sprite LoadSprite(string assetPath)
        {
            var sprite = LoadAsset<Sprite>(assetPath);
            if (sprite != null)
                return sprite;

            var texture = LoadAsset<Texture2D>(assetPath);
            if (texture == null)
                return null;

            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public static Canvas EnsureCanvas()
        {
            var existing = Object.FindFirstObjectByType<Canvas>();
            if (existing != null)
                return existing;

            var canvasGo = new GameObject("LoginCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            StretchFull(canvasGo.GetComponent<RectTransform>());
            EnsureEventSystem();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public static void BuildBackground(Transform parent, UITheme theme)
        {
            var root = CreateRect("Background", parent);
            StretchFull(root);

            var fill = CreateImage("Fill", root, theme.backgroundColor);
            StretchFull(fill.rectTransform);

            var bgSprite = LoadSprite(BgImagePath);
            if (bgSprite != null)
            {
                var bgImage = CreateImage("BgImage", root, Color.white);
                StretchFull(bgImage.rectTransform);
                bgImage.sprite = bgSprite;
                bgImage.preserveAspect = false;
                bgImage.color = new Color(1f, 1f, 1f, 0.3f);
                bgImage.raycastTarget = false;
            }
        }

        public static AuthModal BuildAuthModal(
            Transform parent,
            UITheme theme,
            float modalHeight,
            UnityAction onClose,
            System.Action<RectTransform> buildContent)
        {
            var host = CreateRect("ModalHost", parent);
            host.anchorMin = new Vector2(0.5f, 0.5f);
            host.anchorMax = new Vector2(0.5f, 0.5f);
            host.pivot = new Vector2(0.5f, 0.5f);
            host.sizeDelta = new Vector2(ModalWidth, modalHeight);

            var authModalPrefab = LoadAsset<GameObject>(AuthModalPrefabPath);
            if (authModalPrefab == null)
            {
                Debug.LogError("AuthScreenLayout: AuthModal prefab not found.");
                return null;
            }

            var modalInstance = Object.Instantiate(authModalPrefab, host);
            var authModal = modalInstance.GetComponent<AuthModal>();
            if (authModal == null)
            {
                Debug.LogError("AuthScreenLayout: AuthModal component missing on prefab.");
                return null;
            }

            StretchFull(modalInstance.GetComponent<RectTransform>());
            AddModalBorder(modalInstance, theme);
            AddDominoPattern(modalInstance.transform);
            authModal.CloseButton?.AddListener(onClose);

            PrepareContentRoot(authModal.ContentRoot);
            buildContent?.Invoke(authModal.ContentRoot);

            return authModal;
        }

        public static void PrepareContentRoot(RectTransform contentRoot)
        {
            ClearChildren(contentRoot);

            var contentLayout = contentRoot.GetComponent<VerticalLayoutGroup>();
            if (contentLayout != null)
            {
                contentLayout.spacing = 36f;
                contentLayout.childAlignment = TextAnchor.UpperCenter;
                contentLayout.childControlWidth = true;
                contentLayout.childControlHeight = true;
                contentLayout.childForceExpandWidth = true;
                contentLayout.childForceExpandHeight = false;
            }
        }

        public static void BuildLogoHeader(Transform parent, UITheme theme, string title, string subtitle)
        {
            var header = CreateLayoutGroup("Header", parent, true, 16f);
            var headerLayout = header.GetComponent<LayoutElement>();
            headerLayout.preferredWidth = ContentWidth;
            headerLayout.flexibleWidth = 1f;

            var logoSprite = LoadSprite(LogoPath);
            if (logoSprite != null)
            {
                var logo = CreateImage("Logo", header.transform, Color.white);
                var logoLayout = logo.gameObject.AddComponent<LayoutElement>();
                logoLayout.preferredWidth = 320f;
                logoLayout.preferredHeight = 40f;
                logoLayout.flexibleWidth = 0f;
                logo.sprite = logoSprite;
                logo.preserveAspect = true;
                logo.raycastTarget = false;
            }
            else
            {
                var logoText = CreateText("LogoText", header.transform, theme, "PRODOMINO", UIFontWeight.Bold, theme.heading4Size, theme.primaryColor);
                logoText.alignment = TextAlignmentOptions.Center;
            }

            CreateText("Title", header.transform, theme, title, UIFontWeight.SemiBold, theme.heading2Size, theme.secondaryColor);
            CreateText("Subtitle", header.transform, theme, subtitle, UIFontWeight.Regular, theme.bodySize, theme.black100);
        }

        public static LabeledInputField InstantiateInputField(Transform parent, string label, string placeholder, TMP_InputField.ContentType contentType)
        {
            var prefab = LoadAsset<GameObject>("Assets/Prefabs/UI/Inputs/LabeledInputField.prefab");
            if (prefab == null)
                return null;

            var instance = Object.Instantiate(prefab, parent);
            var field = instance.GetComponent<LabeledInputField>();
            field.LabelText = label;
            field.PlaceholderText = placeholder;
            field.Input.contentType = contentType;
            return field;
        }

        public static void SetupPasswordToggle(LabeledInputField field, UITheme theme, UnityAction onToggle)
        {
            var slot = field.TrailingIconSlot;
            if (slot == null)
                return;

            slot.gameObject.SetActive(true);

            var iconSprite = LoadSprite(EyeIconPath);
            var buttonGo = new GameObject("EyeToggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(slot, false);

            StretchFull(buttonGo.GetComponent<RectTransform>());

            var image = buttonGo.GetComponent<Image>();
            if (iconSprite != null)
            {
                image.sprite = iconSprite;
                image.preserveAspect = true;
                image.color = theme.black100;
            }
            else
            {
                image.color = Color.clear;
            }

            var button = buttonGo.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = theme.black100;
            colors.pressedColor = theme.primaryColor;
            colors.selectedColor = Color.white;
            button.colors = colors;
            button.onClick.AddListener(onToggle);

            if (iconSprite == null)
            {
                var fallback = CreateText("EyeFallback", buttonGo.transform, theme, "\uD83D\uDC41", UIFontWeight.Regular, 16f, theme.black100);
                fallback.raycastTarget = false;
            }
        }

        public static void TogglePasswordVisibility(LabeledInputField field, ref bool visible)
        {
            if (field?.Input == null)
                return;

            visible = !visible;
            field.Input.contentType = visible
                ? TMP_InputField.ContentType.Standard
                : TMP_InputField.ContentType.Password;
            field.Input.ForceLabelUpdate();
        }

        public static Button CreateLinkButton(string name, Transform parent, UITheme theme, string label, UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var text = go.GetComponent<TextMeshProUGUI>();
            theme.ApplyTo(text, UIFontWeight.Medium, theme.bodySize, theme.primaryColor);
            text.text = label;
            text.alignment = TextAlignmentOptions.Center;

            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = theme.primaryColor;
            colors.highlightedColor = theme.yellow400;
            colors.pressedColor = theme.yellow600;
            colors.selectedColor = theme.primaryColor;
            button.colors = colors;
            button.onClick.AddListener(onClick);

            go.GetComponent<LayoutElement>().flexibleWidth = 0f;
            return button;
        }

        public static TextMeshProUGUI CreateText(string name, Transform parent, UITheme theme, string text, UIFontWeight weight, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var label = go.GetComponent<TextMeshProUGUI>();
            theme.ApplyTo(label, weight, size, color);
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;

            go.GetComponent<LayoutElement>().flexibleWidth = 1f;
            return label;
        }

        public static RectTransform CreateLayoutGroup(string name, Transform parent, bool vertical, float spacing)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            if (vertical)
            {
                var group = go.AddComponent<VerticalLayoutGroup>();
                group.spacing = spacing;
                group.childAlignment = TextAnchor.UpperCenter;
                group.childControlWidth = true;
                group.childControlHeight = true;
                group.childForceExpandWidth = true;
                group.childForceExpandHeight = false;
            }
            else
            {
                var group = go.AddComponent<HorizontalLayoutGroup>();
                group.spacing = spacing;
                group.childAlignment = TextAnchor.MiddleLeft;
                group.childControlWidth = true;
                group.childControlHeight = true;
                group.childForceExpandWidth = false;
                group.childForceExpandHeight = false;
            }

            return go.GetComponent<RectTransform>();
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        public static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Object.Destroy(parent.GetChild(i).gameObject);
        }

        private static void AddModalBorder(GameObject modal, UITheme theme)
        {
            var outline = modal.GetComponent<Outline>();
            if (outline == null)
                outline = modal.AddComponent<Outline>();

            outline.effectColor = theme.modalBorder;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
        }

        private static void AddDominoPattern(Transform modalRoot)
        {
            var patternSprite = LoadSprite(DominoPatternPath);
            if (patternSprite == null)
                return;

            var patternGo = new GameObject("DominoPattern", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            patternGo.transform.SetParent(modalRoot, false);
            patternGo.transform.SetAsFirstSibling();

            var rect = patternGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(358f, 358f);
            rect.anchoredPosition = Vector2.zero;

            var image = patternGo.GetComponent<Image>();
            image.sprite = patternSprite;
            image.preserveAspect = true;
            image.color = new Color(1f, 1f, 1f, 0.29f);
            image.raycastTarget = false;
        }
    }
}
