using ProDomino.Core.UI;
using ProDomino.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProDomino.UI.Screens
{
    public static class LoginScreenBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureLoginScreen()
        {
            if (Object.FindFirstObjectByType<LoginScreenView>() != null)
                return;
            if (Object.FindFirstObjectByType<RegistrationScreenView>() != null)
                return;
            if (Object.FindFirstObjectByType<AccountCreatedScreenView>() != null)
                return;
            if (Object.FindFirstObjectByType<AccountFailedScreenView>() != null)
                return;
            if (Object.FindFirstObjectByType<ForgotPasswordScreenView>() != null)
                return;
            if (Object.FindFirstObjectByType<OtpVerificationScreenView>() != null)
                return;
            if (Object.FindFirstObjectByType<CreatePasswordScreenView>() != null)
                return;
            if (Object.FindFirstObjectByType<PasswordResetSuccessScreenView>() != null)
                return;

            new GameObject("LoginScreen").AddComponent<LoginScreenView>();
        }
    }

    public class LoginScreenView : MonoBehaviour
    {
        private UITheme theme;
        private LabeledInputField passwordField;
        private bool passwordVisible;

        private void Awake()
        {
            theme = AuthScreenLayout.LoadTheme();
            if (theme == null)
            {
                Debug.LogError("LoginScreenView: UITheme not found.");
                return;
            }

            var canvas = AuthScreenLayout.EnsureCanvas();
            AuthScreenLayout.BuildBackground(canvas.transform, theme);
            AuthScreenLayout.BuildAuthModal(canvas.transform, theme, AuthScreenLayout.DefaultModalHeight, OnCloseClicked, BuildContent);
        }

        private void BuildContent(RectTransform contentRoot)
        {
            AuthScreenLayout.BuildLogoHeader(
                contentRoot,
                theme,
                "Welcome Back!\uD83D\uDC4B",
                "Login to continue playing ProDomino with friends & random opponents.");

            BuildForm(contentRoot);
            BuildActions(contentRoot);
            BuildFooter(contentRoot);
        }

        private void BuildForm(Transform parent)
        {
            var form = AuthScreenLayout.CreateLayoutGroup("Form", parent, true, 24f);
            form.GetComponent<LayoutElement>().flexibleWidth = 1f;

            AuthScreenLayout.InstantiateInputField(form.transform, "Email", "Enter your email", TMP_InputField.ContentType.EmailAddress);

            passwordField = AuthScreenLayout.InstantiateInputField(form.transform, "Password", "Enter your password", TMP_InputField.ContentType.Password);
            if (passwordField != null)
                AuthScreenLayout.SetupPasswordToggle(passwordField, theme, () => AuthScreenLayout.TogglePasswordVisibility(passwordField, ref passwordVisible));

            BuildRememberRow(form.transform);
        }

        private void BuildRememberRow(Transform parent)
        {
            var row = AuthScreenLayout.CreateLayoutGroup("RememberRow", parent, false, 0f);
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

            var checkboxPrefab = AuthScreenLayout.LoadAsset<GameObject>("Assets/Prefabs/UI/Inputs/Checkbox.prefab");
            if (checkboxPrefab != null)
            {
                var checkboxGo = Instantiate(checkboxPrefab, row.transform);
                var checkbox = checkboxGo.GetComponent<UICheckbox>();
                checkbox.LabelText = "Remember Me";
                checkboxGo.GetComponent<LayoutElement>().flexibleWidth = 1f;
            }

            var forgotButton = AuthScreenLayout.CreateLinkButton("ForgotPassword", row, theme, "Forgot Password?", OnForgotPasswordClicked);
            forgotButton.GetComponent<LayoutElement>().flexibleWidth = 0f;
        }

        private void BuildActions(Transform parent)
        {
            var actions = AuthScreenLayout.CreateLayoutGroup("Actions", parent, true, 16f);
            actions.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var loginPrefab = AuthScreenLayout.LoadAsset<GameObject>("Assets/Prefabs/UI/Buttons/PrimaryButton.prefab");
            if (loginPrefab != null)
            {
                var loginGo = Instantiate(loginPrefab, actions.transform);
                var loginButton = loginGo.GetComponent<PrimaryButton>();
                loginButton.LabelText = "Login";
                loginButton.AddListener(OnLoginClicked);
            }

            var socialRow = AuthScreenLayout.CreateLayoutGroup("SocialRow", actions.transform, false, 16f);
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
            var footer = AuthScreenLayout.CreateRect("Footer", parent);
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

            AuthScreenLayout.CreateText("FooterPrompt", footer, theme, "Don't have an account?", UIFontWeight.Regular, theme.bodySize, theme.black100);
            AuthScreenLayout.CreateLinkButton("CreateAccount", footer, theme, "Create an Account", OnCreateAccountClicked);
        }

        private void CreateSocialButton(Transform parent, string label, string iconPath, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(HorizontalLayoutGroup), typeof(SecondarySocialButton));
            go.transform.SetParent(parent, false);

            var socialButton = go.GetComponent<SecondarySocialButton>();
            socialButton.SetTheme(theme);
            socialButton.LabelText = label;
            socialButton.IconSprite = AuthScreenLayout.LoadSprite(iconPath);
            socialButton.AddListener(onClick);
        }

        private void OnCloseClicked() => Debug.Log("Login: Close clicked (stub).");
        private void OnForgotPasswordClicked() => OnboardingNavigator.ShowForgotPassword();
        private void OnCreateAccountClicked() => OnboardingNavigator.ShowRegistration();
        private void OnLoginClicked() => Debug.Log("Login: Login clicked (stub).");
        private void OnGoogleLoginClicked() => Debug.Log("Login: Google OAuth clicked (stub).");
        private void OnFacebookLoginClicked() => Debug.Log("Login: Facebook OAuth clicked (stub).");
    }
}
