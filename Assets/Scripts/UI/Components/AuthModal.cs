using ProDomino.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ProDomino.UI.Components
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(UIGradient))]
    [RequireComponent(typeof(LayoutElement))]
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class AuthModal : MonoBehaviour
    {
        [SerializeField] private UITheme theme;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectTransform closeButtonSlot;
        [SerializeField] private CloseButton closeButton;

        private Image background;
        private UIGradient gradient;
        private LayoutElement layoutElement;
        private VerticalLayoutGroup layoutGroup;

        public RectTransform ContentRoot => contentRoot;
        public RectTransform CloseButtonSlot => closeButtonSlot;
        public CloseButton CloseButton => closeButton;

        private void Awake()
        {
            Cache();
            EnsureHierarchy();
            ApplyLayout();
            ApplyTheme();
        }

        private void OnValidate()
        {
            Cache();
            EnsureHierarchy();
            ApplyLayout();
            ApplyTheme();
        }

        private void Cache()
        {
            background = GetComponent<Image>();
            gradient = GetComponent<UIGradient>();
            layoutElement = GetComponent<LayoutElement>();
            layoutGroup = GetComponent<VerticalLayoutGroup>();
        }

        private void EnsureHierarchy()
        {
            if (closeButtonSlot == null)
            {
                Transform existing = transform.Find("CloseButtonSlot");
                if (existing != null)
                    closeButtonSlot = existing as RectTransform;
            }

            if (closeButtonSlot == null)
            {
                var go = new GameObject("CloseButtonSlot", typeof(RectTransform), typeof(LayoutElement));
                go.transform.SetParent(transform, false);
                closeButtonSlot = go.GetComponent<RectTransform>();
                var slotLayout = go.GetComponent<LayoutElement>();
                slotLayout.ignoreLayout = true;
            }

            closeButtonSlot.SetAsLastSibling();
            AnchorCloseSlot();

            if (closeButton == null)
                closeButton = closeButtonSlot.GetComponentInChildren<CloseButton>(true);

            if (contentRoot == null)
            {
                Transform existing = transform.Find("Content");
                if (existing != null)
                    contentRoot = existing as RectTransform;
            }

            if (contentRoot == null)
            {
                var go = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
                go.transform.SetParent(transform, false);
                go.transform.SetAsFirstSibling();
                contentRoot = go.GetComponent<RectTransform>();
                var contentLayout = go.GetComponent<LayoutElement>();
                contentLayout.flexibleWidth = 1f;
                contentLayout.flexibleHeight = 1f;
                var contentGroup = go.GetComponent<VerticalLayoutGroup>();
                contentGroup.childAlignment = TextAnchor.UpperCenter;
                contentGroup.childControlWidth = true;
                contentGroup.childControlHeight = false;
                contentGroup.childForceExpandWidth = true;
                contentGroup.childForceExpandHeight = false;
                contentGroup.spacing = 36f;
            }
        }

        private void AnchorCloseSlot()
        {
            float size = theme != null ? theme.closeButtonSize : 50f;
            closeButtonSlot.anchorMin = new Vector2(1f, 1f);
            closeButtonSlot.anchorMax = new Vector2(1f, 1f);
            closeButtonSlot.pivot = new Vector2(1f, 1f);
            closeButtonSlot.sizeDelta = new Vector2(size, size);
            closeButtonSlot.anchoredPosition = new Vector2(-20f, -20f);
        }

        private void ApplyLayout()
        {
            int horizontal = theme != null ? Mathf.RoundToInt(theme.modalPaddingHorizontal) : 80;
            int top = theme != null ? Mathf.RoundToInt(theme.modalPaddingTop) : 100;
            int bottom = theme != null ? Mathf.RoundToInt(theme.modalPaddingBottom) : 100;

            layoutGroup.padding = new RectOffset(horizontal, horizontal, top, bottom);
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 0f;

            var rect = (RectTransform)transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            layoutElement.flexibleWidth = 1f;
            layoutElement.flexibleHeight = 1f;

            AnchorCloseSlot();
        }

        private void ApplyTheme()
        {
            if (theme == null)
                return;

            background.color = Color.white;
            gradient.GradientDirection = UIGradient.Direction.Vertical;
            gradient.StartColor = theme.modalStart;
            gradient.EndColor = theme.modalEnd;
        }
    }
}
