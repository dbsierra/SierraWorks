using UnityEngine;
using TMPro;

namespace SierraWorks.Param.Samples
{
    public class FloatToText : MonoBehaviour
    {
        public  TextMeshProUGUI text;
        [SerializeField] private float value;

        public float Value
        {
            get => value;
            set
            {
                this.value = value;
                text.text = value.ToString("0.00");
            }
        }

        private void OnValidate()
        {
            Value = value;
        }
    }
}