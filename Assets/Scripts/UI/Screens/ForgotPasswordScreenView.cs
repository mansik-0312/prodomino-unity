using ProDomino.Core.UI;
using ProDomino.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProDomino.UI.Screens
{
    public class ForgotPasswordScreenView : MonoBehaviour
    {
        private const float ModalHeight = 704f;

        private UITheme theme;
        private LabeledInputField emailField;

        private void Awake()
        {
            theme = AuthScreenLayout.LoadTheme();
            if (theme == null)
            {
                Debug.LogError("ForgotPasswordScreenView: UITheme not found.");
                return;
            }

            var canvas = AuthScreenLayout.EnsureCanvas();
            AuthScreenLayout.BuildBackground(canvas.transform, theme);
            AuthScreenLayout.BuildAuthModal(canvas.transform, theme, ModalHeight, OnCloseClicked, BuildContent);
        }

        private void BuildContent(RectTransform contentRoot)
        {
            AuthScreenLayout.BuildLogoHeader(
                contentRoot,
                theme,
                "Forgot Password?",
                "Enter your email address and we'll send you a link to reset your password.");

            BuildForm(contentRoot);
            BuildActions(contentRoot);
            BuildFooter(contentRoot);
        }

        private void BuildForm(Transform parent)
        {
            var form = AuthScreenLayout.CreateLayoutGroup("Form", parent, true, 24f);
            form.GetComponent<LayoutElement>().flexibleWidth = 1f;

            emailField = AuthScreenLayout.InstantiateInputField(
                form.transform,
                "Email",
                "Enter your email",
                TMP_InputField.ContentType.EmailAddress);
        }

        private void BuildActions(Transform parent)
        {
            var actions = AuthScreenLayout.CreateLayoutGroup("Actions", parent, true, 16f);
            actions.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var buttonPrefab = AuthScreenLayout.LoadAsset<GameObject>("Assets/Prefabs/UI/Buttons/PrimaryButton.prefab");
            if (buttonPrefab == null)
                return;

            var submitGo = Instantiate(buttonPrefab, actions.transform);
            var submitButton = submitGo.GetComponent<PrimaryButton>();
            submitButton.LabelText = "Send Reset Link";
            submitButton.AddListener(OnSubmitClicked);
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

            AuthScreenLayout.CreateText("FooterPrompt", footer, theme, "Remember your password?", UIFontWeight.Regular, theme.bodySize, theme.black100);
            AuthScreenLayout.CreateLinkButton("BackToLogin", footer, theme, "Back to Login", OnBackToLoginClicked);
        }

        private void OnCloseClicked() => OnboardingNavigator.ShowLogin();
        private void OnSubmitClicked()
        {
            string email = emailField?.Text ?? string.Empty;
            Debug.Log($"Forgot password: Send Reset Link clicked for '{email}' (stub).");
            OnboardingNavigator.ShowOtpVerification(email);
        }
        private void OnBackToLoginClicked() => OnboardingNavigator.ShowLogin();
    }
}
