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

        public void EnsureScrollableContent()
        {
            Cache();
            EnsureHierarchy();
            if (contentRoot == null || (contentRoot.parent != null && contentRoot.parent.GetComponent<RectMask2D>() != null))
                return;

            var scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(LayoutElement), typeof(ScrollRect));
            scrollGo.transform.SetParent(transform, false);
            scrollGo.transform.SetSiblingIndex(contentRoot.GetSiblingIndex());

            var scrollLayout = scrollGo.GetComponent<LayoutElement>();
            scrollLayout.flexibleWidth = 1f;
            scrollLayout.flexibleHeight = 1f;
            scrollLayout.minHeight = 120f;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewport = viewportGo.GetComponent<RectTransform>();
            AuthScreenLayout.StretchFull(viewport);
            var viewportImage = viewportGo.GetComponent<Image>();
            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = true;

            contentRoot.SetParent(viewport, false);
            var contentRect = contentRoot;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            var fitter = contentRoot.GetComponent<ContentSizeFitter>();
            if (fitter == null)
                fitter = contentRoot.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var contentLayout = contentRoot.GetComponent<LayoutElement>();
            if (contentLayout != null)
            {
                contentLayout.flexibleWidth = 1f;
                contentLayout.flexibleHeight = 0f;
            }

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRect;
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;
        }

        public void ApplyResponsiveStyle(bool compact, float scaleFactor, UITheme overrideTheme = null)
        {
            Cache();
            if (overrideTheme != null)
                theme = overrideTheme;

            float scale = Mathf.Max(0.0001f, scaleFactor);
            int horizontal;
            int top;
            int bottom;
            if (compact)
            {
                horizontal = Mathf.RoundToInt(24f / scale);
                top = Mathf.RoundToInt(56f / scale);
                bottom = Mathf.RoundToInt(24f / scale);
            }
            else
            {
                horizontal = theme != null ? Mathf.RoundToInt(theme.modalPaddingHorizontal) : 80;
                top = theme != null ? Mathf.RoundToInt(theme.modalPaddingTop) : 100;
                bottom = theme != null ? Mathf.RoundToInt(theme.modalPaddingBottom) : 100;
            }

            if (layoutGroup != null)
            {
                layoutGroup.padding = new RectOffset(horizontal, horizontal, top, bottom);
                var contentGroup = contentRoot != null ? contentRoot.GetComponent<VerticalLayoutGroup>() : null;
                if (contentGroup != null)
                    contentGroup.spacing = compact ? 24f : 36f;
            }

            AnchorCloseSlot(compact, scale);
        }

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
            AnchorCloseSlot(false, 1f);

            if (closeButton == null)
                closeButton = closeButtonSlot.GetComponentInChildren<CloseButton>(true);

            if (contentRoot == null)
            {
                Transform existing = transform.Find("Content")
                    ?? transform.Find("ScrollView/Viewport/Content");
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

        private void AnchorCloseSlot(bool compact = false, float scale = 1f)
        {
            if (closeButtonSlot == null)
                return;

            float scaleSafe = Mathf.Max(0.0001f, scale);
            float size = theme != null ? theme.closeButtonSize : 50f;
            float offset = compact ? 12f / scaleSafe : 20f;
            if (compact)
                size = Mathf.Max(size, 44f / scaleSafe);

            closeButtonSlot.anchorMin = new Vector2(1f, 1f);
            closeButtonSlot.anchorMax = new Vector2(1f, 1f);
            closeButtonSlot.pivot = new Vector2(1f, 1f);
            closeButtonSlot.sizeDelta = new Vector2(size, size);
            closeButtonSlot.anchoredPosition = new Vector2(-offset, -offset);
        }

        private void ApplyLayout()
        {
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
