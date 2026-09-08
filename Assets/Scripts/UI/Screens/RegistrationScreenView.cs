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

        private UITheme theme;
        private LabeledInputField passwordField;
        private LabeledInputField confirmPasswordField;
        private bool passwordVisible;
        private bool confirmPasswordVisible;

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

            AuthScreenLayout.InstantiateInputField(form.transform, "Email", "Enter your email", TMP_InputField.ContentType.EmailAddress);

            passwordField = AuthScreenLayout.InstantiateInputField(form.transform, "Password", "Enter Password", TMP_InputField.ContentType.Password);
            if (passwordField != null)
                AuthScreenLayout.SetupPasswordToggle(passwordField, theme, () => AuthScreenLayout.TogglePasswordVisibility(passwordField, ref passwordVisible));

            confirmPasswordField = AuthScreenLayout.InstantiateInputField(form.transform, "Confirm Password", "Enter Password", TMP_InputField.ContentType.Password);
            if (confirmPasswordField != null)
                AuthScreenLayout.SetupPasswordToggle(confirmPasswordField, theme, () => AuthScreenLayout.TogglePasswordVisibility(confirmPasswordField, ref confirmPasswordVisible));

            BuildLegalCheckboxes(form.transform);
        }

        private void BuildLegalCheckboxes(Transform parent)
        {
            var checkboxPrefab = AuthScreenLayout.LoadAsset<GameObject>("Assets/Prefabs/UI/Inputs/Checkbox.prefab");
            if (checkboxPrefab == null)
                return;

            var termsGo = Instantiate(checkboxPrefab, parent);
            termsGo.GetComponent<UICheckbox>().LabelText = "I agree to the Terms & Conditions of ProDomino";

            var dataGo = Instantiate(checkboxPrefab, parent);
            dataGo.GetComponent<UICheckbox>().LabelText = "I agree to the ProDomino Data Treatment";
        }

        private void BuildActions(Transform parent)
        {
            var actions = AuthScreenLayout.CreateLayoutGroup("Actions", parent, true, 16f);
            actions.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var buttonPrefab = AuthScreenLayout.LoadAsset<GameObject>("Assets/Prefabs/UI/Buttons/PrimaryButton.prefab");
            if (buttonPrefab == null)
                return;

            var registerGo = Instantiate(buttonPrefab, actions.transform);
            var registerButton = registerGo.GetComponent<PrimaryButton>();
            registerButton.LabelText = "Create Account";
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

        private void OnCloseClicked() => Debug.Log("Registration: Close clicked (stub).");
        private void OnRegisterClicked() => OnboardingNavigator.ShowAccountCreated();
        private void OnLoginClicked() => OnboardingNavigator.ShowLogin();
    }
}
