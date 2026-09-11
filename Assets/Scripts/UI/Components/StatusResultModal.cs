using ProDomino.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProDomino.UI.Components
{
    public enum StatusResultVariant
    {
        Success,
        Error
    }

    public readonly struct StatusResultAction
    {
        public string Label { get; }
        public UnityAction OnClick { get; }
        public bool UsePrimaryStyle { get; }

        public StatusResultAction(string label, UnityAction onClick, bool usePrimaryStyle = true)
        {
            Label = label;
            OnClick = onClick;
            UsePrimaryStyle = usePrimaryStyle;
        }
    }

    public sealed class StatusResultConfig
    {
        public StatusResultVariant Variant;
        public Sprite IconOverride;
        public string Title;
        public string Body;
        public StatusResultAction PrimaryAction;
        public StatusResultAction? SecondaryAction;
        public float ModalHeight = AuthScreenLayout.StatusModalHeight;

        public static StatusResultConfig AccountCreatedSuccess(UnityAction onContinue, UnityAction onGoToLogin)
        {
            return new StatusResultConfig
            {
                Variant = StatusResultVariant.Success,
                Title = "Account Creation Successfully!",
                Body = "Welcome to ProDomino - Start playing ProDomino with friends & random opponents.",
                PrimaryAction = new StatusResultAction("Continue", onContinue),
                SecondaryAction = new StatusResultAction("Go to Login", onGoToLogin, false)
            };
        }

        public static StatusResultConfig AccountCreatedFailed(UnityAction onTryAgain, UnityAction onGoToLogin, string body = null)
        {
            return new StatusResultConfig
            {
                Variant = StatusResultVariant.Error,
                Title = "Account Creation Failed",
                Body = string.IsNullOrWhiteSpace(body)
                    ? "Something went wrong while creating your account. Please try again."
                    : body,
                PrimaryAction = new StatusResultAction("Try Again", onTryAgain),
                SecondaryAction = new StatusResultAction("Go to Login", onGoToLogin, false)
            };
        }

        public static StatusResultConfig PasswordResetSuccess(UnityAction onContinue, UnityAction onGoToLogin)
        {
            return new StatusResultConfig
            {
                Variant = StatusResultVariant.Success,
                ModalHeight = 603f,
                Title = "Password Reset Successfully!",
                Body = "Your password has been updated. You can now log in with your new password.",
                PrimaryAction = new StatusResultAction("Continue", onContinue),
                SecondaryAction = new StatusResultAction("Go to Login", onGoToLogin, false)
            };
        }
    }

    public static class StatusResultModal
    {
        private const string SuccessIconPath = "Assets/Art/UI/AccountStatus/icon-success.svg";
        private const string ErrorIconPath = "Assets/Art/UI/AccountStatus/icon-error.svg";

        // Temporary hardcoded tint until UITheme gains semantic error/success badge colors.
        private static readonly Color SuccessBadgeFill = new Color(0.12f, 0.55f, 0.28f, 0.35f);
        private static readonly Color ErrorBadgeFill = new Color(0.90f, 0.28f, 0.31f, 0.35f);
        private static readonly Color ErrorBadgeBorder = new Color(0.90f, 0.28f, 0.31f, 1f);

        public static Sprite GetDefaultIcon(StatusResultVariant variant)
        {
            string path = variant == StatusResultVariant.Success ? SuccessIconPath : ErrorIconPath;
            return AuthScreenLayout.LoadSprite(path);
        }

        public static void BuildContent(RectTransform contentRoot, UITheme theme, StatusResultConfig config)
        {
            AuthScreenLayout.PrepareContentRoot(contentRoot);

            var root = AuthScreenLayout.CreateLayoutGroup("StatusContent", contentRoot, true, 36f);
            var rootLayout = root.GetComponent<LayoutElement>();
            rootLayout.flexibleWidth = 1f;

            BuildIconBlock(root.transform, theme, config);
            BuildTextBlock(root.transform, theme, config);
            BuildActions(root.transform, theme, config);
        }

        private static void BuildIconBlock(Transform parent, UITheme theme, StatusResultConfig config)
        {
            var block = AuthScreenLayout.CreateLayoutGroup("IconBlock", parent, true, 32f);
            block.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var badgeGo = new GameObject("IconBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            badgeGo.transform.SetParent(block, false);

            var badgeLayout = badgeGo.GetComponent<LayoutElement>();
            badgeLayout.preferredWidth = 110f;
            badgeLayout.preferredHeight = 110f;
            badgeLayout.flexibleWidth = 0f;

            var badgeImage = badgeGo.GetComponent<Image>();
            bool isSuccess = config.Variant == StatusResultVariant.Success;
            badgeImage.color = isSuccess ? SuccessBadgeFill : ErrorBadgeFill;

            var iconSprite = config.IconOverride ?? GetDefaultIcon(config.Variant);
            if (iconSprite != null)
            {
                var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconGo.transform.SetParent(badgeGo.transform, false);
                var iconRect = iconGo.GetComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = new Vector2(30f, 30f);
                iconRect.offsetMax = new Vector2(-30f, -30f);
                var iconImage = iconGo.GetComponent<Image>();
                iconImage.sprite = iconSprite;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
                iconImage.color = isSuccess ? theme.primaryColor : ErrorBadgeBorder;
            }
            else
            {
                var fallback = AuthScreenLayout.CreateText(
                    "IconFallback",
                    badgeGo.transform,
                    theme,
                    isSuccess ? "\u2713" : "\u00D7",
                    UIFontWeight.Bold,
                    40f,
                    isSuccess ? theme.primaryColor : ErrorBadgeBorder);
                fallback.alignment = TextAlignmentOptions.Center;
                fallback.raycastTarget = false;
            }
        }

        private static void BuildTextBlock(Transform parent, UITheme theme, StatusResultConfig config)
        {
            var textBlock = AuthScreenLayout.CreateLayoutGroup("TextBlock", parent, true, 8f);
            textBlock.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var title = AuthScreenLayout.CreateText("Title", textBlock.transform, theme, config.Title, UIFontWeight.SemiBold, theme.heading2Size, theme.secondaryColor);
            title.textWrappingMode = TextWrappingModes.Normal;

            var body = AuthScreenLayout.CreateText("Body", textBlock.transform, theme, config.Body, UIFontWeight.Regular, theme.bodySize, theme.black100);
            body.textWrappingMode = TextWrappingModes.Normal;
        }

        private static void BuildActions(Transform parent, UITheme theme, StatusResultConfig config)
        {
            var actions = AuthScreenLayout.CreateLayoutGroup("Actions", parent, true, 16f);
            actions.GetComponent<LayoutElement>().flexibleWidth = 1f;

            if (config.SecondaryAction.HasValue)
            {
                var row = AuthScreenLayout.CreateLayoutGroup("ButtonRow", actions.transform, false, 12f);
                var rowLayout = row.GetComponent<LayoutElement>();
                rowLayout.flexibleWidth = 1f;
                rowLayout.minHeight = theme.primaryButtonHeight;

                var secondary = config.SecondaryAction.Value;
                CreateActionButton(row.transform, theme, secondary, true);

                var primary = config.PrimaryAction;
                CreateActionButton(row.transform, theme, primary, true);
            }
            else
            {
                CreateActionButton(actions.transform, theme, config.PrimaryAction, false);
            }
        }

        private static void CreateActionButton(Transform parent, UITheme theme, StatusResultAction action, bool halfWidth)
        {
            if (action.UsePrimaryStyle)
            {
                var prefab = AuthScreenLayout.LoadAsset<GameObject>("Assets/Prefabs/UI/Buttons/PrimaryButton.prefab");
                if (prefab == null)
                    return;

                var go = Object.Instantiate(prefab, parent);
                var primaryBtn = go.GetComponent<PrimaryButton>();
                primaryBtn.LabelText = action.Label;
                primaryBtn.AddListener(action.OnClick);

                if (halfWidth)
                {
                    var layout = go.GetComponent<LayoutElement>();
                    layout.flexibleWidth = 1f;
                    layout.minWidth = 120f;
                }

                return;
            }

            var secondaryGo = new GameObject(action.Label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            secondaryGo.transform.SetParent(parent, false);

            var background = secondaryGo.GetComponent<Image>();
            background.color = theme.inputFill;

            var layoutElement = secondaryGo.GetComponent<LayoutElement>();
            layoutElement.minHeight = theme.primaryButtonHeight;
            layoutElement.preferredHeight = theme.primaryButtonHeight;
            layoutElement.flexibleWidth = 1f;
            if (halfWidth)
                layoutElement.minWidth = 120f;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(secondaryGo.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelGo.GetComponent<TextMeshProUGUI>();
            theme.ApplyTo(label, UIFontWeight.Bold, theme.heading4Size, theme.secondaryColor);
            label.text = action.Label;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            var button = secondaryGo.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = theme.inputFill;
            colors.highlightedColor = theme.black400;
            colors.pressedColor = theme.black300;
            colors.selectedColor = theme.inputFill;
            button.colors = colors;
            button.onClick.AddListener(action.OnClick);
        }
    }
}
