using UnityEngine;

namespace SierraWorks.PARAM.Samples
{
    public class ExampleContentController : MonoBehaviour
    {
        [SerializeField] private Transform rotationTransform;
        [SerializeField] private UnityEngine.UI.Image image;
        [SerializeField] private float rotation = 0f;
        [SerializeField] private float saturation = 1f;
        [SerializeField] private float hue = 0f;


        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public float Saturation
        {
            get { return saturation; }
            set { saturation = value; }
        }

        public float Hue
        {
            get { return hue; }
            set { hue = value; }
        }

        void Update()
        {
            rotationTransform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);

            Color imageColor = Color.HSVToRGB(hue, saturation, 1f);

            image.color = imageColor;
        }
    }
}