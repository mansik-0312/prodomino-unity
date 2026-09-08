using UnityEngine;

namespace ProDomino.UI.Screens
{
    public static class OnboardingNavigator
    {
        public static string PendingOtpEmail { get; private set; }

        public static void ShowLogin()
        {
            DestroyOnboardingScreens();
            new GameObject("LoginScreen").AddComponent<LoginScreenView>();
        }

        public static void ShowRegistration()
        {
            DestroyOnboardingScreens();
            new GameObject("RegistrationScreen").AddComponent<RegistrationScreenView>();
        }

        public static void ShowAccountCreated()
        {
            DestroyOnboardingScreens();
            new GameObject("AccountCreatedScreen").AddComponent<AccountCreatedScreenView>();
        }

        public static void ShowAccountFailed()
        {
            DestroyOnboardingScreens();
            new GameObject("AccountFailedScreen").AddComponent<AccountFailedScreenView>();
        }

        public static void ShowForgotPassword()
        {
            DestroyOnboardingScreens();
            new GameObject("ForgotPasswordScreen").AddComponent<ForgotPasswordScreenView>();
        }

        public static void ShowOtpVerification(string email = null)
        {
            PendingOtpEmail = email;
            DestroyOnboardingScreens();
            new GameObject("OtpVerificationScreen").AddComponent<OtpVerificationScreenView>();
        }

        public static void ShowCreatePassword()
        {
            DestroyOnboardingScreens();
            new GameObject("CreatePasswordScreen").AddComponent<CreatePasswordScreenView>();
        }

        public static void ShowPasswordResetSuccess()
        {
            DestroyOnboardingScreens();
            new GameObject("PasswordResetSuccessScreen").AddComponent<PasswordResetSuccessScreenView>();
        }

        public static void ClearPendingOtpEmail() => PendingOtpEmail = null;

        private static void DestroyOnboardingScreens()
        {
            var login = Object.FindFirstObjectByType<LoginScreenView>();
            if (login != null)
                Object.Destroy(login.gameObject);

            var registration = Object.FindFirstObjectByType<RegistrationScreenView>();
            if (registration != null)
                Object.Destroy(registration.gameObject);

            var accountCreated = Object.FindFirstObjectByType<AccountCreatedScreenView>();
            if (accountCreated != null)
                Object.Destroy(accountCreated.gameObject);

            var accountFailed = Object.FindFirstObjectByType<AccountFailedScreenView>();
            if (accountFailed != null)
                Object.Destroy(accountFailed.gameObject);

            var forgotPassword = Object.FindFirstObjectByType<ForgotPasswordScreenView>();
            if (forgotPassword != null)
                Object.Destroy(forgotPassword.gameObject);

            var otpVerification = Object.FindFirstObjectByType<OtpVerificationScreenView>();
            if (otpVerification != null)
                Object.Destroy(otpVerification.gameObject);

            var createPassword = Object.FindFirstObjectByType<CreatePasswordScreenView>();
            if (createPassword != null)
                Object.Destroy(createPassword.gameObject);

            var passwordResetSuccess = Object.FindFirstObjectByType<PasswordResetSuccessScreenView>();
            if (passwordResetSuccess != null)
                Object.Destroy(passwordResetSuccess.gameObject);
        }
    }
}
