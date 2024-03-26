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
    [SerializeField, Anywhere] private Rigidbody player;
    
    [Header("Settings")]
    [SerializeField, Range(0f, 10f)] private float rotationSpeed = 10f; 
    
    // Start is called before the first frame update
    void OnEnable() {
        input.Look += OnLook;
    }
        
    void OnDisable() {
        input.Look -= OnLook;
    }
    
    private const float LimitAngle = 75;
    private const float AngleStep = 1;

    private void Update()
    {
    }
    
    private void OnLook(Vector2 cameraMovement, bool isDeviceMouse) {
        if (!isDeviceMouse) return;

        var deviceMultiplier = Time.fixedDeltaTime;
        var mousePosition = Input.mousePosition;
        var isOnBorder = mousePosition.x <= 0 || mousePosition.x >= Screen.width;
        if (!isOnBorder) {
            freeLook.m_XAxis.m_InputAxisValue = cameraMovement.x * rotationSpeed * deviceMultiplier;
        }
        else
        {
            freeLook.m_XAxis.m_InputAxisValue = 0;
        }
        
    }

}
}
