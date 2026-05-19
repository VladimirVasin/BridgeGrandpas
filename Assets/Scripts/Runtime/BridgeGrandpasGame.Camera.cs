using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float CameraKeyboardPanSpeed = 4.2f;
    private const float CameraDragPanSpeed = 0.012f;
    private const float CameraWheelZoomStep = 0.72f;
    private const float CameraKeyboardZoomSpeed = 2.4f;
    private const float CameraMinZoom = 2.75f;
    private const float CameraMaxZoom = 7.15f;
    private const float CameraMinPanX = -4.4f;
    private const float CameraMaxPanX = 4.4f;
    private const float CameraMinPanY = -3.2f;
    private const float CameraMaxPanY = 5.0f;
    private const float CameraCloseZoomPanBonus = 1.35f;

    private Vector3 cameraHomePosition;
    private Vector2 cameraPanOffset;
    private Vector2 cameraPanVelocity;
    private Vector3 cameraGroundRight;
    private Vector3 cameraGroundForward;
    private float cameraZoomTarget;
    private bool cameraDragActive;
    private Vector2 cameraDragPointer;

    private void SetupCameraControls()
    {
        if (mainCamera == null)
        {
            return;
        }

        cameraHomePosition = mainCamera.transform.position;
        cameraZoomTarget = mainCamera.orthographicSize;
        cameraGroundRight = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;
        cameraGroundForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
        cameraPanOffset = new Vector2(0.6f, -0.9f);
        ApplyCameraPose(true);
    }

    private void UpdateCameraControls(float deltaTime)
    {
        if (mainCamera == null)
        {
            return;
        }

        Vector2 input = ReadCameraKeyboardInput();
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        float zoomFactor = Mathf.Lerp(0.85f, 1.25f, Mathf.InverseLerp(CameraMinZoom, CameraMaxZoom, cameraZoomTarget));
        cameraPanVelocity = Vector2.Lerp(cameraPanVelocity, input * CameraKeyboardPanSpeed * zoomFactor, deltaTime * 10f);
        cameraPanOffset += cameraPanVelocity * deltaTime;

        HandleCameraMouseInput();
        HandleCameraKeyboardZoom(deltaTime);
        ClampCameraPan();

        if (WasCameraResetPressed())
        {
            cameraPanOffset = Vector2.zero;
            cameraPanVelocity = Vector2.zero;
            cameraZoomTarget = 5.7f;
        }

        ApplyCameraPose(false);
    }

    private Vector2 ReadCameraKeyboardInput()
    {
        Vector2 input = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return input;
        }

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            input.x -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            input.x += 1f;
        }

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            input.y += 1f;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            input.y -= 1f;
        }
#else
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
#endif
        return input;
    }

    private void HandleCameraMouseInput()
    {
        bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        Vector2 pointer = GetPointerPosition();
        float wheel = ReadMouseWheelNotches();

        if (Mathf.Abs(wheel) > 0.01f && CanCameraZoomAt(pointer))
        {
            ApplyCameraZoom(wheel * CameraWheelZoomStep, true);
        }

        bool dragPressed = IsCameraDragPressed();
        if (!overUi && dragPressed && !cameraDragActive)
        {
            cameraDragActive = true;
            cameraDragPointer = pointer;
        }

        if (!dragPressed)
        {
            cameraDragActive = false;
        }

        if (cameraDragActive)
        {
            Vector2 delta = pointer - cameraDragPointer;
            cameraPanOffset -= new Vector2(delta.x, delta.y) * CameraDragPanSpeed * (cameraZoomTarget / 6f);
            cameraDragPointer = pointer;
        }
    }

    private void HandleCameraKeyboardZoom(float deltaTime)
    {
        float input = ReadCameraKeyboardZoomInput();
        if (Mathf.Abs(input) > 0.01f)
        {
            ApplyCameraZoom(input * CameraKeyboardZoomSpeed * deltaTime, false);
        }
    }

    private void ApplyCameraZoom(float amount, bool instant)
    {
        cameraZoomTarget = Mathf.Clamp(cameraZoomTarget - amount, CameraMinZoom, CameraMaxZoom);
        if (instant && mainCamera != null)
        {
            mainCamera.orthographicSize = cameraZoomTarget;
        }
    }

    private bool CanCameraZoomAt(Vector2 pointer)
    {
        if (eventModal != null && eventModal.gameObject.activeSelf)
        {
            return false;
        }

        if (victoryModal != null && victoryModal.gameObject.activeSelf)
        {
            return false;
        }

        if (trayOpen && trayPanel != null &&
            RectTransformUtility.RectangleContainsScreenPoint(trayPanel, pointer, null))
        {
            return false;
        }

        return true;
    }

    private float ReadMouseWheelNotches()
    {
        float inputSystemWheel = 0f;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            inputSystemWheel = NormalizeWheelValue(Mouse.current.scroll.ReadValue().y);
        }
#endif
        float legacyWheel = ReadLegacyMouseWheel();
        return Mathf.Abs(inputSystemWheel) >= Mathf.Abs(legacyWheel) ? inputSystemWheel : legacyWheel;
    }

    private float NormalizeWheelValue(float raw)
    {
        if (Mathf.Abs(raw) > 10f)
        {
            return raw / 120f;
        }

        return raw;
    }

    private float ReadLegacyMouseWheel()
    {
        try
        {
            return Input.mouseScrollDelta.y;
        }
        catch (InvalidOperationException)
        {
            return 0f;
        }
    }

    private float ReadCameraKeyboardZoomInput()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return 0f;
        }

        float input = 0f;
        if (keyboard.equalsKey.isPressed || keyboard.numpadPlusKey.isPressed || keyboard.pageUpKey.isPressed)
        {
            input += 1f;
        }

        if (keyboard.minusKey.isPressed || keyboard.numpadMinusKey.isPressed || keyboard.pageDownKey.isPressed)
        {
            input -= 1f;
        }

        return input;
#else
        float input = 0f;
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.PageUp))
        {
            input += 1f;
        }

        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.PageDown))
        {
            input -= 1f;
        }

        return input;
#endif
    }

    private bool IsCameraDragPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.middleButton.isPressed || Mouse.current.rightButton.isPressed;
        }
#endif
        return Input.GetMouseButton(1) || Input.GetMouseButton(2);
    }

    private bool WasCameraResetPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.homeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Home);
#endif
    }

    private void ClampCameraPan()
    {
        float closeZoom = 1f - Mathf.InverseLerp(CameraMinZoom, CameraMaxZoom, cameraZoomTarget);
        float bonus = closeZoom * CameraCloseZoomPanBonus;
        cameraPanOffset.x = Mathf.Clamp(cameraPanOffset.x, CameraMinPanX - bonus, CameraMaxPanX + bonus);
        cameraPanOffset.y = Mathf.Clamp(cameraPanOffset.y, CameraMinPanY - bonus * 0.65f, CameraMaxPanY + bonus);
    }

    private void ApplyCameraPose(bool instant)
    {
        Vector3 targetPosition = cameraHomePosition +
            cameraGroundRight * cameraPanOffset.x +
            cameraGroundForward * cameraPanOffset.y;

        float lerp = instant ? 1f : 1f - Mathf.Exp(-Time.deltaTime * 12f);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, lerp);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, cameraZoomTarget, lerp);
    }
}
