using ProDomino.Core.UI;
using ProDomino.UI.Components;
using UnityEngine;

namespace ProDomino.UI.Screens
{
    public class AccountCreatedScreenView : MonoBehaviour
    {
        private UITheme theme;

        private void Awake()
        {
            theme = AuthScreenLayout.LoadTheme();
            if (theme == null)
            {
                Debug.LogError("AccountCreatedScreenView: UITheme not found.");
                return;
            }

            var config = StatusResultConfig.AccountCreatedSuccess(OnContinueClicked, OnGoToLoginClicked);
            var canvas = AuthScreenLayout.EnsureCanvas();
            AuthScreenLayout.BuildBackground(canvas.transform, theme);
            AuthScreenLayout.BuildAuthModal(
                canvas.transform,
                theme,
                config.ModalHeight,
                OnCloseClicked,
                contentRoot => StatusResultModal.BuildContent(contentRoot, theme, config));
        }

        private void OnCloseClicked() => Debug.Log("Account created: Close clicked (stub).");
        private void OnContinueClicked() => Debug.Log("Account created: Continue clicked (stub).");
        private void OnGoToLoginClicked() => OnboardingNavigator.ShowLogin();
    }
}
