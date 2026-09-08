using ProDomino.Core.UI;
using ProDomino.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProDomino.UI.Screens
{
    public static class LoginScreenBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureLoginScreen()
        {
            if (Object.FindFirstObjectByType<LoginScreenView>() != null)
                return;

            var go = new GameObject("LoginScreen");
            go.AddComponent<LoginScreenView>();
        }
    }

    public class LoginScreenView : MonoBehaviour
    {
        private const float ModalWidth = 880f;
        private const float ModalHeight = 860f;
        private const float ContentWidth = 720f;

        private UITheme theme;
        private AuthModal authModal;
        private LabeledInputField passwordField;
        private bool passwordVisible;

        private void Awake()
        {
            theme = LoadAsset<UITheme>("Assets/ScriptableObjects/UI/ProDominoUITheme.asset");
            if (theme == null)
            {
                Debug.LogError("LoginScreenView: UITheme not found.");
                return;
            }

            var canvas = EnsureCanvas();
            BuildBackground(canvas.transform);
            BuildModal(canvas.transform);
        }

        private static T LoadAsset<T>(string assetPath) where T : Object
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
            return null;
#endif
        }

        private static Sprite LoadSprite(string assetPath)
        {
            var sprite = LoadAsset<Sprite>(assetPath);
            if (sprite != null)
                return sprite;

            var texture = LoadAsset<Texture2D>(assetPath);
            if (texture == null)
                return null;

            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        private Canvas EnsureCanvas()
        {
            var existing = FindFirstObjectByType<Canvas>();
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

            var rect = canvasGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            EnsureEventSystem();
            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;

            var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void BuildBackground(Transform parent)
        {
            var root = CreateRect("Background", parent);
            StretchFull(root);

            var fill = CreateImage("Fill", root, theme.backgroundColor);
            StretchFull(fill.rectTransform);

            var bgSprite = LoadSprite("Assets/Art/UI/Login/bg-image.png");
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

        private void BuildModal(Transform parent)
        {
            var host = CreateRect("ModalHost", parent);
            var hostRect = host;
            hostRect.anchorMin = new Vector2(0.5f, 0.5f);
            hostRect.anchorMax = new Vector2(0.5f, 0.5f);
            hostRect.pivot = new Vector2(0.5f, 0.5f);
            hostRect.sizeDelta = new Vector2(ModalWidth, ModalHeight);

            var authModalPrefab = LoadAsset<GameObject>("Assets/Prefabs/UI/Modals/AuthModal.prefab");
            if (authModalPrefab == null)
            {
                Debug.LogError("LoginScreenView: AuthModal prefab not found.");
                return;
            }

            var modalInstance = Instantiate(authModalPrefab, host);
            authModal = modalInstance.GetComponent<AuthModal>();
            if (authModal == null)
            {
                Debug.LogError("LoginScreenView: AuthModal component missing on prefab.");
                return;
            }

            var modalRect = modalInstance.GetComponent<RectTransform>();
            StretchFull(modalRect);

            AddModalBorder(modalInstance);
            AddDominoPattern(modalInstance.transform);

            authModal.CloseButton?.AddListener(OnCloseClicked);
            BuildContent(authModal.ContentRoot);
        }

        private void AddModalBorder(GameObject modal)
        {
            var outline = modal.GetComponent<Outline>();
            if (outline == null)
                outline = modal.AddComponent<Outline>();

            outline.effectColor = theme.modalBorder;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
        }

        private void AddDominoPattern(Transform modalRoot)
        {
            var patternSprite = LoadSprite("Assets/Art/UI/Login/domino-pattern.png");
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
            rect.anchoredPosition = new Vector2(0f, 0f);

            var image = patternGo.GetComponent<Image>();
            image.sprite = patternSprite;
            image.preserveAspect = true;
            image.color = new Color(1f, 1f, 1f, 0.29f);
            image.raycastTarget = false;
        }

        private void BuildContent(RectTransform contentRoot)
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

            BuildHeader(contentRoot);
            BuildForm(contentRoot);
            BuildActions(contentRoot);
            BuildFooter(contentRoot);
        }

        private void BuildHeader(Transform parent)
        {
            var header = CreateLayoutGroup("Header", parent, true, 16f);
            var headerLayout = header.GetComponent<LayoutElement>();
            headerLayout.preferredWidth = ContentWidth;
            headerLayout.flexibleWidth = 1f;

            var logoSprite = LoadSprite("Assets/Art/UI/Login/logo-prodomino.svg");
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
                var logoText = CreateText("LogoText", header.transform, "PRODOMINO", UIFontWeight.Bold, theme.heading4Size, theme.primaryColor);
                logoText.alignment = TextAlignmentOptions.Center;
            }

            CreateText("Title", header.transform, "Welcome Back!\uD83D\uDC4B", UIFontWeight.SemiBold, theme.heading2Size, theme.secondaryColor);
            CreateText("Subtitle", header.transform, "Log in to continue your domino journey.", UIFontWeight.Regular, theme.bodySize, theme.black100);
        }

        private void BuildForm(Transform parent)
        {
            var form = CreateLayoutGroup("Form", parent, true, 24f);
            var formLayout = form.GetComponent<LayoutElement>();
            formLayout.flexibleWidth = 1f;

            var emailPrefab = LoadAsset<GameObject>("Assets/Prefabs/UI/Inputs/LabeledInputField.prefab");
            if (emailPrefab != null)
            {
                var emailGo = Instantiate(emailPrefab, form.transform);
                var email = emailGo.GetComponent<LabeledInputField>();
                email.LabelText = "Email";
                email.PlaceholderText = "Enter your email";
                email.Input.contentType = TMP_InputField.ContentType.EmailAddress;
            }

            var passwordPrefab = emailPrefab;
            if (passwordPrefab != null)
            {
                var passwordGo = Instantiate(passwordPrefab, form.transform);
                passwordField = passwordGo.GetComponent<LabeledInputField>();
                passwordField.LabelText = "Password";
                passwordField.PlaceholderText = "Enter your password";
                passwordField.Input.contentType = TMP_InputField.ContentType.Password;
                SetupPasswordToggle(passwordField);
            }

            BuildRememberRow(form.transform);
        }

        private void BuildRememberRow(Transform parent)
        {
            var row = CreateLayoutGroup("RememberRow", parent, false, 0f);
            var rowLayout = row.GetComponent<LayoutElement>();
            rowLayout.minHeight = theme.checkboxHeight;
            rowLayout.preferredHeight = theme.checkboxHeight;
            rowLayout.flexibleWidth = 1f;

            var horizontal = row.GetComponent<HorizontalLayoutGroup>();
            horizontal.childAlignment = TextAnchor.MiddleLeft;
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = false;
            horizontal.childForceExpandHeight = false;

            var checkboxPrefab = LoadAsset<GameObject>("Assets/Prefabs/UI/Inputs/Checkbox.prefab");
            if (checkboxPrefab != null)
            {
                var checkboxGo = Instantiate(checkboxPrefab, row.transform);
                var checkbox = checkboxGo.GetComponent<UICheckbox>();
                checkbox.LabelText = "Remember Me";
                var checkboxLayout = checkboxGo.GetComponent<LayoutElement>();
                checkboxLayout.flexibleWidth = 1f;
            }

            var forgotButton = CreateLinkButton("ForgotPassword", row.transform, "Forgot Password?", OnForgotPasswordClicked);
            var forgotLayout = forgotButton.GetComponent<LayoutElement>();
            forgotLayout.flexibleWidth = 0f;
        }

        private void BuildActions(Transform parent)
        {
            var actions = CreateLayoutGroup("Actions", parent, true, 16f);
            var actionsLayout = actions.GetComponent<LayoutElement>();
            actionsLayout.flexibleWidth = 1f;

            var loginPrefab = LoadAsset<GameObject>("Assets/Prefabs/UI/Buttons/PrimaryButton.prefab");
            if (loginPrefab != null)
            {
                var loginGo = Instantiate(loginPrefab, actions.transform);
                var loginButton = loginGo.GetComponent<PrimaryButton>();
                loginButton.LabelText = "Login";
                loginButton.AddListener(OnLoginClicked);
            }

            var socialRow = CreateLayoutGroup("SocialRow", actions.transform, false, 16f);
            var socialRowLayout = socialRow.GetComponent<LayoutElement>();
            socialRowLayout.flexibleWidth = 1f;
            socialRowLayout.minHeight = theme.inputFieldHeight;

            var socialHorizontal = socialRow.GetComponent<HorizontalLayoutGroup>();
            socialHorizontal.childAlignment = TextAnchor.MiddleCenter;
            socialHorizontal.childControlWidth = true;
            socialHorizontal.childControlHeight = true;
            socialHorizontal.childForceExpandWidth = true;
            socialHorizontal.childForceExpandHeight = false;

            CreateSocialButton(socialRow.transform, "Login with Google", "Assets/Art/UI/Login/icon-google.svg", OnGoogleLoginClicked);
            CreateSocialButton(socialRow.transform, "Login with Facebook", "Assets/Art/UI/Login/icon-facebook.svg", OnFacebookLoginClicked);
        }

        private void BuildFooter(Transform parent)
        {
            var footer = CreateRect("Footer", parent);
            var footerLayout = footer.gameObject.AddComponent<LayoutElement>();
            footerLayout.flexibleWidth = 1f;
            footerLayout.minHeight = 24f;

            var horizontal = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontal.childAlignment = TextAnchor.MiddleCenter;
            horizontal.spacing = 4f;
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = false;
            horizontal.childForceExpandHeight = false;

            CreateText("FooterPrompt", footer, "Don't have an account?", UIFontWeight.Regular, theme.bodySize, theme.black100);
            CreateLinkButton("CreateAccount", footer, "Create an Account", OnCreateAccountClicked);
        }

        private void CreateSocialButton(Transform parent, string label, string iconPath, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(HorizontalLayoutGroup), typeof(SecondarySocialButton));
            go.transform.SetParent(parent, false);

            var socialButton = go.GetComponent<SecondarySocialButton>();
            socialButton.SetTheme(theme);
            socialButton.LabelText = label;
            socialButton.IconSprite = LoadSprite(iconPath);
            socialButton.AddListener(onClick);
        }

        private void SetupPasswordToggle(LabeledInputField field)
        {
            var slot = field.TrailingIconSlot;
            if (slot == null)
                return;

            slot.gameObject.SetActive(true);

            var iconSprite = LoadSprite("Assets/Art/UI/Login/icon-eye.svg");
            var buttonGo = new GameObject("EyeToggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(slot, false);

            var buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.anchorMin = Vector2.zero;
            buttonRect.anchorMax = Vector2.one;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

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

            button.onClick.AddListener(TogglePasswordVisibility);

            if (iconSprite == null)
            {
                var fallback = CreateText("EyeFallback", buttonGo.transform, "\uD83D\uDC41", UIFontWeight.Regular, 16f, theme.black100);
                fallback.raycastTarget = false;
            }
        }

        private void TogglePasswordVisibility()
        {
            if (passwordField?.Input == null)
                return;

            passwordVisible = !passwordVisible;
            passwordField.Input.contentType = passwordVisible
                ? TMP_InputField.ContentType.Standard
                : TMP_InputField.ContentType.Password;
            passwordField.Input.ForceLabelUpdate();
        }

        private Button CreateLinkButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction onClick)
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

            var layout = go.GetComponent<LayoutElement>();
            layout.flexibleWidth = 0f;

            return button;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, string text, UIFontWeight weight, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var label = go.GetComponent<TextMeshProUGUI>();
            theme.ApplyTo(label, weight, size, color);
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;

            var layout = go.GetComponent<LayoutElement>();
            layout.flexibleWidth = 1f;

            return label;
        }

        private static RectTransform CreateLayoutGroup(string name, Transform parent, bool vertical, float spacing)
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

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }

        private void OnCloseClicked() => Debug.Log("Login: Close clicked (stub).");
        private void OnForgotPasswordClicked() => Debug.Log("Login: Forgot Password clicked (stub).");
        private void OnCreateAccountClicked() => Debug.Log("Login: Create Account clicked (stub).");
        private void OnLoginClicked() => Debug.Log("Login: Login clicked (stub).");
        private void OnGoogleLoginClicked() => Debug.Log("Login: Google OAuth clicked (stub).");
        private void OnFacebookLoginClicked() => Debug.Log("Login: Facebook OAuth clicked (stub).");
    }
}
