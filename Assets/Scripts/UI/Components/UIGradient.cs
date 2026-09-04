using UnityEngine;
using UnityEngine.UI;

namespace ProDomino.UI.Components
{
    [RequireComponent(typeof(Graphic))]
    [DisallowMultipleComponent]
    public class UIGradient : BaseMeshEffect
    {
        public enum Direction
        {
            Horizontal,
            Vertical
        }

        [SerializeField] private Direction direction = Direction.Horizontal;
        [SerializeField] private Color startColor = Color.white;
        [SerializeField] private Color endColor = Color.white;

        public Direction GradientDirection
        {
            get => direction;
            set
            {
                direction = value;
                graphic.SetVerticesDirty();
            }
        }

        public Color StartColor
        {
            get => startColor;
            set
            {
                startColor = value;
                graphic.SetVerticesDirty();
            }
        }

        public Color EndColor
        {
            get => endColor;
            set
            {
                endColor = value;
                graphic.SetVerticesDirty();
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0)
                return;

            UIVertex vertex = new UIVertex();
            vh.PopulateUIVertex(ref vertex, 0);
            float min = direction == Direction.Horizontal ? vertex.position.x : vertex.position.y;
            float max = min;

            for (int i = 1; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                float value = direction == Direction.Horizontal ? vertex.position.x : vertex.position.y;
                if (value < min)
                    min = value;
                if (value > max)
                    max = value;
            }

            float range = max - min;
            if (range <= 0.0001f)
                return;

            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                float value = direction == Direction.Horizontal ? vertex.position.x : vertex.position.y;
                float t = (value - min) / range;
                Color gradient = Color.Lerp(startColor, endColor, t);
                vertex.color *= gradient;
                vh.SetUIVertex(vertex, i);
            }
        }
    }
}
