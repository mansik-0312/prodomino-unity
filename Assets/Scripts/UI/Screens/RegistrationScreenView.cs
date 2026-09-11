using System;
using HelperSharedLibrary;
using ProDomino.Authentication;
using ProDomino.Core.UI;
using ProDomino.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProDomino.UI.Screens
{
    public class RegistrationScreenView : MonoBehaviour
    {
        private const float ModalHeight = 984f;
        private const string CreateAccountLabel = "Create Account";
        private const string CreatingAccountLabel = "Creating...";

        private UITheme theme;
        private LabeledInputField emailField;
        private LabeledInputField passwordField;
        private LabeledInputField confirmPasswordField;
        private UICheckbox termsCheckbox;
        private UICheckbox dataTreatmentCheckbox;
        private PrimaryButton registerButton;
        private bool passwordVisible;
        private bool confirmPasswordVisible;
        private bool isSubmitting;

        private void Awake()
        {
            theme = AuthScreenLayout.LoadTheme();
            if (theme == null)
            {
                Debug.LogError("RegistrationScreenView: UITheme not found.");
                return;
            }

            var canvas = AuthScreenLayout.EnsureCanvas();
            AuthScreenLayout.BuildBackground(canvas.transform, theme);
            AuthScreenLayout.BuildAuthModal(canvas.transform, theme, ModalHeight, OnCloseClicked, BuildContent);
            RefreshSubmitState();
        }

        private void BuildContent(RectTransform contentRoot)
        {
            AuthScreenLayout.BuildLogoHeader(
                contentRoot,
                theme,
                "Create your Account",
                "Start playing ProDomino with friends & random opponents.");

            BuildForm(contentRoot);
            BuildActions(contentRoot);
            BuildFooter(contentRoot);
        }

        private void BuildForm(Transform parent)
        {
            var form = AuthScreenLayout.CreateLayoutGroup("Form", parent, true, 24f);
            form.GetComponent<LayoutElement>().flexibleWidth = 1f;

            emailField = AuthScreenLayout.InstantiateInputField(form.transform, "Email", "Enter your email", TMP_InputField.ContentType.EmailAddress);
            BindField(emailField);

            passwordField = AuthScreenLayout.InstantiateInputField(form.transform, "Password", "Enter Password", TMP_InputField.ContentType.Password);
            if (passwordField != null)
                AuthScreenLayout.SetupPasswordToggle(passwordField, theme, () => AuthScreenLayout.TogglePasswordVisibility(passwordField, ref passwordVisible));
            BindField(passwordField);

            confirmPasswordField = AuthScreenLayout.InstantiateInputField(form.transform, "Confirm Password", "Enter Password", TMP_InputField.ContentType.Password);
            if (confirmPasswordField != null)
                AuthScreenLayout.SetupPasswordToggle(confirmPasswordField, theme, () => AuthScreenLayout.TogglePasswordVisibility(confirmPasswordField, ref confirmPasswordVisible));
            BindField(confirmPasswordField);

            BuildLegalCheckboxes(form.transform);
        }

        private void BuildLegalCheckboxes(Transform parent)
        {
            var checkboxPrefab = AuthScreenLayout.LoadAsset<GameObject>("Assets/Prefabs/UI/Inputs/Checkbox.prefab");
            if (checkboxPrefab == null)
                return;

            var termsGo = Instantiate(checkboxPrefab, parent);
            termsCheckbox = termsGo.GetComponent<UICheckbox>();
            termsCheckbox.LabelText = "I agree to the Terms & Conditions of ProDomino";
            termsCheckbox.AddListener(_ => RefreshSubmitState());

            var dataGo = Instantiate(checkboxPrefab, parent);
            dataTreatmentCheckbox = dataGo.GetComponent<UICheckbox>();
            dataTreatmentCheckbox.LabelText = "I agree to the ProDomino Data Treatment";
            dataTreatmentCheckbox.AddListener(_ => RefreshSubmitState());
        }

        private void BuildActions(Transform parent)
        {
            var actions = AuthScreenLayout.CreateLayoutGroup("Actions", parent, true, 16f);
            actions.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var buttonPrefab = AuthScreenLayout.LoadAsset<GameObject>("Assets/Prefabs/UI/Buttons/PrimaryButton.prefab");
            if (buttonPrefab == null)
                return;

            var registerGo = Instantiate(buttonPrefab, actions.transform);
            registerButton = registerGo.GetComponent<PrimaryButton>();
            registerButton.LabelText = CreateAccountLabel;
            registerButton.Interactable = false;
            registerButton.AddListener(OnRegisterClicked);
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

            AuthScreenLayout.CreateText("FooterPrompt", footer, theme, "Already have an account?", UIFontWeight.Regular, theme.bodySize, theme.black100);
            AuthScreenLayout.CreateLinkButton("LoginLink", footer, theme, "Login", OnLoginClicked);
        }

        private void BindField(LabeledInputField field)
        {
            if (field?.Input == null)
                return;

            field.Input.onValueChanged.AddListener(_ => RefreshSubmitState());
        }

        private void RefreshSubmitState()
        {
            if (registerButton == null || isSubmitting)
                return;

            registerButton.Interactable = IsFormValid();
        }

        private bool IsFormValid()
        {
            return CredentialsValidator.IsValidToSignUp(
                emailField?.Text?.Trim(),
                passwordField?.Text,
                confirmPasswordField?.Text,
                termsCheckbox != null && termsCheckbox.IsOn,
                dataTreatmentCheckbox != null && dataTreatmentCheckbox.IsOn);
        }

        private void OnCloseClicked() => OnboardingNavigator.ShowLogin();

        private async void OnRegisterClicked()
        {
            if (isSubmitting || !IsFormValid())
                return;

            isSubmitting = true;
            if (registerButton != null)
            {
                registerButton.Interactable = false;
                registerButton.LabelText = CreatingAccountLabel;
            }

            var email = emailField.Text.Trim();
            var password = passwordField.Text;
            var username = CredentialsValidator.DeriveUsernameFromEmail(email);

            try
            {
                var result = await AuthManager.SignUpWithCredentialsAsync(username, email, password);
                if (this == null)
                    return;

                if (result.Success)
                    OnboardingNavigator.ShowAccountCreated();
                else
                    OnboardingNavigator.ShowAccountFailed(result.ErrorMessage);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Registration submit failed: {exception.Message}");
                if (this != null)
                    OnboardingNavigator.ShowAccountFailed(exception.Message);
            }
        }

        private void OnLoginClicked() => OnboardingNavigator.ShowLogin();
    }
}
