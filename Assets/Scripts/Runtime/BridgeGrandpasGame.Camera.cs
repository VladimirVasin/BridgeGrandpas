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
    private const float CameraMinZoom = 1.30f;
    private const float CameraMaxZoom = 5.65f;
    private const float CameraDefaultZoom = 7.15f;
    private const float CameraVhsDefaultZoom = 2.35f;
    private const float CameraMinPanX = -7.2f;
    private const float CameraMaxPanX = 7.2f;
    private const float CameraMinPanY = -0.55f;
    private const float CameraMaxPanY = 1.15f;
    private const float CameraVhsMinPanY = -0.40f;
    private const float CameraVhsMaxPanY = 4.65f;
    private const float CameraVhsVerticalPanScale = 0.92f;
    private const float CameraVhsDragVerticalPanScale = 0.58f;
    private const float CameraCloseZoomPanBonus = 1.55f;
    private const float CameraVerticalPanScale = 0.28f;

    private Vector3 cameraHomePosition;
    private Quaternion cameraHomeRotation;
    private Vector2 cameraPanOffset;
    private Vector2 cameraPanVelocity;
    private Vector3 cameraGroundRight;
    private Vector3 cameraGroundForward;
    private float cameraZoomTarget;
    private float vhsSavedZoom;
    private float delayedVhsZoomAmount;
    private float delayedVhsZoomWait;
    private bool cameraDragActive;
    private Vector2 cameraDragPointer;

    private void SetupCameraControls()
    {
        if (mainCamera == null)
        {
            return;
        }

        cameraHomePosition = mainCamera.transform.position;
        cameraHomeRotation = mainCamera.transform.rotation;
        cameraZoomTarget = CameraDefaultZoom;
        mainCamera.orthographicSize = cameraZoomTarget;
        vhsSavedZoom = CameraVhsDefaultZoom;
        cameraGroundRight = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;
        cameraGroundForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
        cameraPanOffset = Vector2.zero;
        ApplyCameraPose(true);
    }

    private void UpdateCameraControls(float deltaTime)
    {
        if (mainCamera == null)
        {
            return;
        }

        RefreshInteractionMode();
        if (interactionMode == GameInteractionMode.Dialog || interactionMode == GameInteractionMode.Notebook)
        {
            cameraDragActive = false;
            cameraPanVelocity = Vector2.zero;
            ApplyCameraPose(false);
            return;
        }

        Vector2 input = ReadCameraKeyboardInput();
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        input.y *= vhsModeEnabled ? CameraVhsVerticalPanScale : CameraVerticalPanScale;
        float zoomFactor = Mathf.Lerp(0.85f, 1.25f, Mathf.InverseLerp(CameraMinZoom, CameraMaxZoom, cameraZoomTarget));
        cameraPanVelocity = Vector2.Lerp(cameraPanVelocity, input * CameraKeyboardPanSpeed * zoomFactor, deltaTime * 10f);
        cameraPanOffset += cameraPanVelocity * deltaTime;

        HandleCameraMouseInput();
        HandleCameraKeyboardZoom(deltaTime);
        UpdateDelayedVhsZoom(deltaTime);
        ClampCameraPan();

        if (WasCameraResetPressed())
        {
            cameraPanOffset = Vector2.zero;
            cameraPanVelocity = Vector2.zero;
            cameraZoomTarget = vhsModeEnabled ? CameraVhsDefaultZoom : CameraDefaultZoom;
            if (vhsModeEnabled)
            {
                vhsSavedZoom = cameraZoomTarget;
            }
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
            float dragVerticalScale = vhsModeEnabled ? CameraVhsDragVerticalPanScale : CameraVerticalPanScale;
            cameraPanOffset -= new Vector2(delta.x, delta.y * dragVerticalScale) * CameraDragPanSpeed * (cameraZoomTarget / CameraDefaultZoom);
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
        if (!vhsModeEnabled)
        {
            return;
        }

        QueueDelayedVhsZoom(amount);
    }

    private void ApplyCameraZoomNow(float amount, bool instant)
    {
        float previousZoom = cameraZoomTarget;
        cameraZoomTarget = Mathf.Clamp(cameraZoomTarget - amount, CameraMinZoom, CameraMaxZoom);
        if (Mathf.Abs(previousZoom - cameraZoomTarget) > 0.001f)
        {
            TriggerVhsZoomPulse();
        }

        if (instant && mainCamera != null)
        {
            mainCamera.orthographicSize = cameraZoomTarget;
        }
    }

    private void QueueDelayedVhsZoom(float amount)
    {
        delayedVhsZoomAmount += amount * 0.85f;
        delayedVhsZoomAmount = Mathf.Clamp(delayedVhsZoomAmount, -2.4f, 2.4f);
        delayedVhsZoomWait = 0.07f;
        TriggerVhsZoomPulse();
    }

    private void UpdateDelayedVhsZoom(float deltaTime)
    {
        if (!vhsModeEnabled)
        {
            delayedVhsZoomAmount = 0f;
            delayedVhsZoomWait = 0f;
            return;
        }

        if (delayedVhsZoomWait > 0f)
        {
            delayedVhsZoomWait -= deltaTime;
            return;
        }

        if (Mathf.Abs(delayedVhsZoomAmount) <= 0.001f)
        {
            return;
        }

        float speed = 2.25f + Mathf.Clamp01(Mathf.Abs(delayedVhsZoomAmount)) * 1.45f;
        float step = Mathf.Sign(delayedVhsZoomAmount) * Mathf.Min(Mathf.Abs(delayedVhsZoomAmount), speed * deltaTime);
        delayedVhsZoomAmount -= step;
        ApplyCameraZoomNow(step, false);
        vhsSavedZoom = cameraZoomTarget;
    }

    private void RestoreVhsCameraZoom()
    {
        delayedVhsZoomAmount = 0f;
        delayedVhsZoomWait = 0f;
        cameraZoomTarget = Mathf.Clamp(vhsSavedZoom, CameraMinZoom, CameraMaxZoom);
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = cameraZoomTarget;
        }
    }

    private void ReturnToNormalCameraZoom()
    {
        vhsSavedZoom = Mathf.Clamp(cameraZoomTarget, CameraMinZoom, CameraMaxZoom);
        delayedVhsZoomAmount = 0f;
        delayedVhsZoomWait = 0f;
        cameraZoomTarget = CameraDefaultZoom;
        if (mainCamera != null)
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
            return vhsModeEnabled
                ? Mouse.current.middleButton.isPressed
                : Mouse.current.middleButton.isPressed || Mouse.current.rightButton.isPressed;
        }
#endif
        return vhsModeEnabled ? Input.GetMouseButton(2) : Input.GetMouseButton(1) || Input.GetMouseButton(2);
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
        float minY = vhsModeEnabled ? CameraVhsMinPanY : CameraMinPanY;
        float maxY = vhsModeEnabled ? CameraVhsMaxPanY : CameraMaxPanY;
        cameraPanOffset.x = Mathf.Clamp(cameraPanOffset.x, CameraMinPanX - bonus, CameraMaxPanX + bonus);
        cameraPanOffset.y = Mathf.Clamp(cameraPanOffset.y, minY - bonus * 0.65f, maxY + bonus);
    }

    private void ApplyCameraPose(bool instant)
    {
        Vector3 verticalPanAxis = vhsModeEnabled ? Vector3.up : cameraGroundForward;
        Vector3 targetPosition = cameraHomePosition +
            cameraGroundRight * cameraPanOffset.x +
            verticalPanAxis * cameraPanOffset.y;

        float positionLerp = instant ? 1f : 1f - Mathf.Exp(-Time.deltaTime * 12f);
        float zoomSpeed = vhsModeEnabled ? 5.2f : 12f;
        float zoomLerp = instant && !vhsModeEnabled ? 1f : 1f - Mathf.Exp(-Time.deltaTime * zoomSpeed);
        Vector3 swayedPosition = targetPosition;
        Quaternion swayedRotation = cameraHomeRotation;
        ApplyNotebookCameraPose(ref swayedPosition, ref swayedRotation);
        if (vhsModeEnabled)
        {
            ApplyVhsCameraSway(ref swayedPosition, ref swayedRotation);
        }

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, swayedPosition, positionLerp);
        mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, swayedRotation, positionLerp);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, cameraZoomTarget, zoomLerp);
    }

    private void ApplyVhsCameraSway(ref Vector3 position, ref Quaternion rotation)
    {
        float zoom01 = Mathf.InverseLerp(CameraMaxZoom, CameraMinZoom, mainCamera.orthographicSize);
        float strength = Mathf.Lerp(0.18f, 1.0f, zoom01);
        float slowX = Mathf.Sin(Time.time * 1.35f) * 0.018f;
        float slowY = Mathf.Sin(Time.time * 1.85f + 1.4f) * 0.014f;
        float handX = (Mathf.PerlinNoise(Time.time * 5.8f, 2.7f) - 0.5f) * 0.035f;
        float handY = (Mathf.PerlinNoise(8.1f, Time.time * 5.2f) - 0.5f) * 0.026f;
        position += cameraGroundRight * (slowX + handX) * strength;
        position += Vector3.up * (slowY + handY) * strength;

        float pitch = (Mathf.PerlinNoise(Time.time * 2.6f, 4.2f) - 0.5f) * 1.2f * strength;
        float yaw = (Mathf.PerlinNoise(6.5f, Time.time * 2.2f) - 0.5f) * 1.6f * strength;
        float roll = Mathf.Sin(Time.time * 1.15f) * 0.85f * strength;
        rotation = cameraHomeRotation * Quaternion.Euler(pitch, yaw, roll);
    }
}
