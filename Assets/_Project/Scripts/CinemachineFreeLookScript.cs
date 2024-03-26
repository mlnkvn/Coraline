using Cinemachine;
using UnityEngine;

namespace Coraline
{
    public class CinemachineFreeLookScript : MonoBehaviour
    {
        private CinemachineFreeLook _freeLookScript;

        public float horizontalAimingSpeed = 20f;
        public float verticalAimingSpeed = 20f;

        [Tooltip("This depends on your Free Look rigs setup, use to correct Y sensitivity,"
                 + " about 1.5 - 2 results in good Y-X square responsiveness")]
        public float yCorrection = 2f;

        private float xAxisValue;
        private float yAxisValue;

        private void Awake()
        {
            _freeLookScript = GetComponent<CinemachineFreeLook>();
        }

        private void Update()
        {
            float mouseX = Input.GetAxis("Mouse X") * horizontalAimingSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * verticalAimingSpeed * Time.deltaTime;

            // Correction for Y
            mouseY /= 360f;
            mouseY *= yCorrection;

            xAxisValue += mouseX;
            yAxisValue = Mathf.Clamp01(yAxisValue - mouseY);

            _freeLookScript.m_XAxis.Value = xAxisValue;
            _freeLookScript.m_YAxis.Value = yAxisValue;
        }
    }
}