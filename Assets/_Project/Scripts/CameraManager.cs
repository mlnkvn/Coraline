using System;
using System.Collections;
using Cinemachine;
using KBCore.Refs;
using UnityEngine;

namespace Coraline
{
    public class CameraManager : ValidatedMonoBehaviour
    {
        [Header("References")] 
        [SerializeField, Anywhere] InputReader input;
        [SerializeField, Anywhere] CinemachineFreeLook freeLookVCam;

        [Header("Settings")]
        [SerializeField, Range(0.5f, 3f)] float speedMult = 1f;

        bool isRightMouseButtonPressed;
        bool cameraMovementLock;

        IEnumerator DisableMouseForFrame()
        {
            cameraMovementLock = true;
            yield return new WaitForEndOfFrame();
            cameraMovementLock = false;
        }
        
        void OnEnableMouseControlCamera()
        {
            isRightMouseButtonPressed = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            StartCoroutine(DisableMouseForFrame());
        }
        
        void OnDisableMouseControlCamera()
        {
            isRightMouseButtonPressed = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            freeLookVCam.m_XAxis.m_InputAxisValue = 0f;
            freeLookVCam.m_YAxis.m_InputAxisValue = 0f;
        }

        void OnLook(Vector2 cameraMove, bool isDeviceMouse)
        {
            if (cameraMovementLock) return;
            if (isDeviceMouse && !isRightMouseButtonPressed) return;

            float deviceMult = isDeviceMouse ? Time.fixedDeltaTime : Time.deltaTime;
            freeLookVCam.m_XAxis.m_InputAxisValue = cameraMove.x * speedMult * deviceMult;
            freeLookVCam.m_YAxis.m_InputAxisValue = cameraMove.y * speedMult * deviceMult;
        }
        void OnEnable()
        {
            input.Look += OnLook;
            input.EnableMouseControlCamera += OnEnableMouseControlCamera;
            input.DisableMouseControlCamera += OnDisableMouseControlCamera;
        }
        
        void OnDisable()
        {
            input.Look -= OnLook;
            input.EnableMouseControlCamera -= OnEnableMouseControlCamera;
            input.DisableMouseControlCamera -= OnDisableMouseControlCamera;
        }
    }
}