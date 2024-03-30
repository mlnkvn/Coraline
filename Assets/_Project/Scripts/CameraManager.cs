using System;
using System.Collections;
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
    [SerializeField, Anywhere] private Shader grayscaleShader;
    [SerializeField, Anywhere] private Shader defaultShader;
    [SerializeField, Anywhere] private Shader hintShader;
    [Header("Settings")]
    [SerializeField, Range(0f, 10f)] private float rotationSpeed = 10f; 
    
    private void OnEnable() {
        defaultShader = Shader.Find("Universal Render Pipeline/Lit");
        input.Look += OnLook;
        input.LookThroughStone += OnLookThroughStone;
    }

    private void OnDisable() {
        input.Look -= OnLook;
        input.LookThroughStone -= OnLookThroughStone;
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

    private readonly string[] targetedObjects = { "Hidden Door" };
    
    private void OnLookThroughStone(bool isLookingThroughStone)
    {
        var player = GameObject.Find("Player").GetComponent<PlayerController>();
        if (!player.hasItem || !player.myHands.transform.GetChild(0).CompareTag("Magic Stone"))
        {
            return;
        }
        var bigRoom = GameObject.FindWithTag("BigRoom");
        foreach (var obj in bigRoom.GetComponentsInChildren<Transform>())
        {
            if (obj.CompareTag("Hidden Door"))
            {
                Debug.Log("wsdkeikfc");
            }
            var objRenderer = obj.GetComponent<Renderer>();
            if (objRenderer == null) continue;
            foreach (var material in objRenderer.materials)
            {
                var shader = Array.IndexOf(targetedObjects, obj.tag) == -1 ? grayscaleShader : hintShader;
                material.shader = isLookingThroughStone ? shader : defaultShader;
            }
        }
        
    }
}
}
