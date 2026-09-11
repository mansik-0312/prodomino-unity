using ProDomino.Core.UI;
using ProDomino.UI.Components;
using UnityEngine;

namespace ProDomino.UI.Screens
{
    public class AccountFailedScreenView : MonoBehaviour
    {
        private UITheme theme;

        private void Awake()
        {
            theme = AuthScreenLayout.LoadTheme();
            if (theme == null)
            {
                Debug.LogError("AccountFailedScreenView: UITheme not found.");
                return;
            }

            var config = StatusResultConfig.AccountCreatedFailed(
                OnTryAgainClicked,
                OnGoToLoginClicked,
                OnboardingNavigator.PendingSignUpError);
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
        private void OnTryAgainClicked() => OnboardingNavigator.ShowRegistration();
        private void OnGoToLoginClicked() => OnboardingNavigator.ShowLogin();
    }
}
