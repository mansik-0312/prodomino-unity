using ProDomino.Core.UI;
using ProDomino.UI.Components;
using UnityEngine;

namespace ProDomino.UI.Screens
{
    public class PasswordResetSuccessScreenView : MonoBehaviour
    {
        private UITheme theme;

        private void Awake()
        {
            theme = AuthScreenLayout.LoadTheme();
            if (theme == null)
            {
                Debug.LogError("PasswordResetSuccessScreenView: UITheme not found.");
                return;
            }

            var config = StatusResultConfig.PasswordResetSuccess(OnContinueClicked, OnGoToLoginClicked);
            var canvas = AuthScreenLayout.EnsureCanvas();
            AuthScreenLayout.BuildBackground(canvas.transform, theme);
            AuthScreenLayout.BuildAuthModal(
                canvas.transform,
                theme,
                config.ModalHeight,
                OnCloseClicked,
                contentRoot => StatusResultModal.BuildContent(contentRoot, theme, config));
        }

        private void OnCloseClicked() => OnboardingNavigator.ShowLogin();
        private void OnContinueClicked() => Debug.Log("Password reset success: Continue clicked (stub).");
        private void OnGoToLoginClicked() => OnboardingNavigator.ShowLogin();
    }
}
