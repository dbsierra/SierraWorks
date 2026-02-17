using UnityEngine;

namespace SierraWorks.Param.Samples
{
    public class CubeController : MonoBehaviour
    {
        [SerializeField] private Transform transform;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private float yaw = 0f;
        [SerializeField] private float saturation = 1f;
        [SerializeField] private float hue = 0f;


        public float Yaw
        {
            get { return yaw; }
            set { yaw = value; }
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

        private MaterialPropertyBlock _materialBlock;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _materialBlock = new MaterialPropertyBlock();
        }

        // Update is called once per frame
        void Update()
        {
            transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up);

            Color cubeColor = Color.HSVToRGB(hue, saturation, 1f);

            _materialBlock.SetColor("_BaseColor", cubeColor);
            meshRenderer.SetPropertyBlock(_materialBlock);
        }
    }
}