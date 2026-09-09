using ProDomino.Core.UI;
using ProDomino.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace ProDomino.UI.Screens
{
    public class OtpVerificationScreenView : MonoBehaviour
    {
        private const float ModalHeight = 704f;

        private UITheme theme;
        private OTPCodeInput otpInput;
        private string emailAddress;

        private void Awake()
        {
            emailAddress = OnboardingNavigator.PendingOtpEmail;
            OnboardingNavigator.ClearPendingOtpEmail();

            theme = AuthScreenLayout.LoadTheme();
            if (theme == null)
            {
                Debug.LogError("OtpVerificationScreenView: UITheme not found.");
                return;
            }

            var canvas = AuthScreenLayout.EnsureCanvas();
            AuthScreenLayout.BuildBackground(canvas.transform, theme);
            AuthScreenLayout.BuildAuthModal(canvas.transform, theme, ModalHeight, OnCloseClicked, BuildContent);
        }

        private void BuildContent(RectTransform contentRoot)
        {
            string subtitle = string.IsNullOrWhiteSpace(emailAddress)
                ? "Please enter the 4 digit code sent to your email."
                : $"Please enter the 4 digit code sent to {emailAddress}";

            AuthScreenLayout.BuildLogoHeader(contentRoot, theme, "OTP Verification", subtitle);

            BuildForm(contentRoot);
            BuildActions(contentRoot);
        }

        private void BuildForm(Transform parent)
        {
            var form = AuthScreenLayout.CreateLayoutGroup("Form", parent, true, 24f);
            form.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var otpRow = AuthScreenLayout.CreateLayoutGroup("OtpRow", form.transform, false, 0f);
            var otpRowLayout = otpRow.GetComponent<LayoutElement>();
            otpRowLayout.flexibleWidth = 1f;
            otpRowLayout.preferredHeight = 70f;
            otpRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var otpGo = new GameObject("OTPCodeInput", typeof(RectTransform), typeof(OTPCodeInput));
            otpGo.transform.SetParent(otpRow.transform, false);
            otpInput = otpGo.GetComponent<OTPCodeInput>();

            var resendGo = new GameObject("ResendCodeLink", typeof(RectTransform), typeof(ResendCodeLink));
            resendGo.transform.SetParent(form.transform, false);
            var resendLink = resendGo.GetComponent<ResendCodeLink>();
            resendLink.OnResendClicked.AddListener(OnResendClicked);
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
            submitButton.LabelText = "Verify Code";
            submitButton.AddListener(OnVerifyClicked);
        }

        private void OnCloseClicked() => OnboardingNavigator.ShowForgotPassword();
        private void OnResendClicked() => Debug.Log("OTP verification: Resend code clicked (stub).");
        private void OnVerifyClicked()
        {
            Debug.Log($"OTP verification: Verify Code clicked with code '{otpInput?.Code}' (stub).");
            OnboardingNavigator.ShowCreatePassword();
        }
    }
}
