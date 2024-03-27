using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using Cinemachine;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Coraline {
public class CameraManager : MonoBehaviour
{
    [FormerlySerializedAs("_freeLook")]
    [Header("References")]
    [SerializeField, Anywhere] private InputReader input;
    [SerializeField, Anywhere] private CinemachineFreeLook freeLook;
    
    [Header("Settings")]
    [SerializeField, Range(0f, 10f)] private float rotationSpeed = 10f; 
    bool isRMBPressed;
    bool cameraMovementLock;
    private void OnEnable() {
        input.Look += OnLook;
        input.EnableMouseControlCamera += OnEnableMouseControlCamera;
        input.DisableMouseControlCamera += OnDisableMouseControlCamera;
    }

    private void OnDisable() {
        input.Look -= OnLook;
        input.EnableMouseControlCamera -= OnEnableMouseControlCamera;
        input.DisableMouseControlCamera -= OnDisableMouseControlCamera;
    }
    
    private void OnLook(Vector2 cameraMovement, bool isDeviceMouse) {
        if (!isDeviceMouse) return;

        var deviceMultiplier = Time.fixedDeltaTime;
        var mousePosition = Input.mousePosition;
        var isNotOnBorderX = mousePosition.x > 0 || mousePosition.x < Screen.width;
        var isNotOnBorderY = mousePosition.y > 0 || mousePosition.y < Screen.height;
        freeLook.m_XAxis.m_InputAxisValue = isNotOnBorderX ? 5 * cameraMovement.x * rotationSpeed * deviceMultiplier : 0;
        freeLook.m_YAxis.m_InputAxisValue = isNotOnBorderY ? 5 * cameraMovement.y * rotationSpeed * deviceMultiplier : 0;
    }
    
    void OnEnableMouseControlCamera() {
        isRMBPressed = true;
            
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
            
        StartCoroutine(DisableMouseForFrame());
    }

    void OnDisableMouseControlCamera() {
        isRMBPressed = false;
            
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
            
        freeLook.m_XAxis.m_InputAxisValue = 0f;
        freeLook.m_YAxis.m_InputAxisValue = 0f;
    }

    IEnumerator DisableMouseForFrame() {
        cameraMovementLock = true;
        yield return new WaitForEndOfFrame();
        cameraMovementLock = false;
    }

}
}
