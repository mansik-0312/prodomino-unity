using ProDomino.Core.UI;
using ProDomino.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProDomino.UI.Screens
{
    public class CreatePasswordScreenView : MonoBehaviour
    {
        private const float ModalHeight = 727f;

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
                Debug.LogError("CreatePasswordScreenView: UITheme not found.");
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
                "Create New Password",
                "Enter and confirm your new password below.");

            BuildForm(contentRoot);
            BuildActions(contentRoot);
        }

        private void BuildForm(Transform parent)
        {
            var form = AuthScreenLayout.CreateLayoutGroup("Form", parent, true, 24f);
            form.GetComponent<LayoutElement>().flexibleWidth = 1f;

            passwordField = AuthScreenLayout.InstantiateInputField(form.transform, "New Password", "Enter Password", TMP_InputField.ContentType.Password);
            if (passwordField != null)
                AuthScreenLayout.SetupPasswordToggle(passwordField, theme, () => AuthScreenLayout.TogglePasswordVisibility(passwordField, ref passwordVisible));

            confirmPasswordField = AuthScreenLayout.InstantiateInputField(form.transform, "Confirm Password", "Enter Password", TMP_InputField.ContentType.Password);
            if (confirmPasswordField != null)
                AuthScreenLayout.SetupPasswordToggle(confirmPasswordField, theme, () => AuthScreenLayout.TogglePasswordVisibility(confirmPasswordField, ref confirmPasswordVisible));
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
            submitButton.LabelText = "Reset Password";
            submitButton.AddListener(OnResetPasswordClicked);
        }

        private void OnCloseClicked() => OnboardingNavigator.ShowOtpVerification();
        private void OnResetPasswordClicked()
        {
            string password = passwordField?.Text ?? string.Empty;
            string confirm = confirmPasswordField?.Text ?? string.Empty;
            Debug.Log($"Create password: Reset Password clicked (stub). Match={password == confirm}");
            OnboardingNavigator.ShowPasswordResetSuccess();
        }
    }
}
