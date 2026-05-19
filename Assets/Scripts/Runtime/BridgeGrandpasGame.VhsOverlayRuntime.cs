using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private void UpdateVhsFrame()
    {
        float height = vhsRoot.rect.height;
        float width = vhsRoot.rect.width;
        Rect viewport = mainCamera == null ? new Rect(0f, 0f, 1f, 1f) : mainCamera.rect;
        float left = viewport.xMin * width;
        float right = (1f - viewport.xMax) * width;
        float bottom = viewport.yMin * height;
        float top = (1f - viewport.yMax) * height;
        float frameWidth = viewport.width * width;
        float frameHeight = viewport.height * height;

        vhsLeftMatte.anchorMin = new Vector2(0f, 0f);
        vhsLeftMatte.anchorMax = new Vector2(0f, 1f);
        vhsLeftMatte.pivot = new Vector2(0f, 0.5f);
        vhsLeftMatte.sizeDelta = new Vector2(left, 0f);
        vhsLeftMatte.anchoredPosition = Vector2.zero;
        vhsRightMatte.anchorMin = new Vector2(1f, 0f);
        vhsRightMatte.anchorMax = new Vector2(1f, 1f);
        vhsRightMatte.pivot = new Vector2(1f, 0.5f);
        vhsRightMatte.sizeDelta = new Vector2(right, 0f);
        vhsRightMatte.anchoredPosition = Vector2.zero;

        vhsTopMatte.anchorMin = new Vector2(0f, 1f);
        vhsTopMatte.anchorMax = new Vector2(1f, 1f);
        vhsTopMatte.pivot = new Vector2(0.5f, 1f);
        vhsTopMatte.sizeDelta = new Vector2(0f, top);
        vhsTopMatte.anchoredPosition = Vector2.zero;
        vhsBottomMatte.anchorMin = new Vector2(0f, 0f);
        vhsBottomMatte.anchorMax = new Vector2(1f, 0f);
        vhsBottomMatte.pivot = new Vector2(0.5f, 0f);
        vhsBottomMatte.sizeDelta = new Vector2(0f, bottom);
        vhsBottomMatte.anchoredPosition = Vector2.zero;
        vhsFrameRoot.anchoredPosition = new Vector2((left - right) * 0.5f, (bottom - top) * 0.5f);
        vhsFrameRoot.sizeDelta = new Vector2(frameWidth, frameHeight);
    }

    private void UpdateVhsReadouts()
    {
        int totalSeconds = Mathf.FloorToInt(vhsRecordTime);
        int frames = Mathf.FloorToInt((vhsRecordTime - totalSeconds) * 25f);
        int seconds = totalSeconds % 60;
        int minutes = totalSeconds / 60 % 60;
        int hours = totalSeconds / 3600;
        vhsTimeText.text = hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + frames.ToString("00") + "  23.10.1998";

        float zoom = mainCamera == null ? 1f : CameraDefaultZoom / Mathf.Max(0.01f, mainCamera.orthographicSize);
        vhsZoomText.text = vhsZoomPulse > 0.02f ? "ZOOM x" + zoom.ToString("0.0") : "AUTO TRACKING";
        UpdateVhsZoomSlider();
        UpdateVhsObservationReadout();
        float blink = Mathf.PingPong(Time.time * 2.4f, 1f);
        vhsRecDot.color = new Color(1f, 0.08f, 0.05f, 0.35f + blink * 0.60f);
    }

    private void UpdateVhsZoomSlider()
    {
        if (mainCamera == null || vhsZoomFill == null)
        {
            return;
        }

        float zoom01 = Mathf.InverseLerp(CameraMaxZoom, CameraMinZoom, mainCamera.orthographicSize);
        vhsZoomFill.sizeDelta = new Vector2(Mathf.Lerp(14f, 164f, zoom01), 0f);
    }

    private void UpdateVhsMotion()
    {
        vhsFrameRoot.anchoredPosition = Vector2.zero;
        vhsGroup.alpha = 1f;
        vhsScanlineGroup.alpha = 0.55f + Mathf.Sin(Time.time * 9.3f) * 0.18f;
        float bandY = Mathf.Sin(Time.time * 0.65f) * vhsRoot.rect.height * 0.28f;
        vhsTrackingBand.anchoredPosition = new Vector2(0f, bandY);
        vhsTrackingBand.sizeDelta = new Vector2(0f, 12f + vhsTrackingPulse * 42f);
        UpdateVhsNoise();
    }

    private void UpdateVhsNoise()
    {
        float frameWidth = vhsFrameRoot.rect.width;
        float frameHeight = vhsFrameRoot.rect.height;
        float tracking = Mathf.PerlinNoise(Time.time * 2.1f, 8.2f);
        if (vhsTrackingImage != null)
        {
            vhsTrackingImage.color = new Color(0.70f, 0.88f, 1f, 0.08f + tracking * 0.13f + vhsTrackingPulse * 0.18f);
        }

        if (vhsHeadSwitchBand != null)
        {
            float crawl = Mathf.Sin(Time.time * 4.8f) * 6f;
            vhsHeadSwitchBand.anchoredPosition = new Vector2(crawl, 18f + Mathf.Sin(Time.time * 14f) * 3f);
            vhsHeadSwitchBand.sizeDelta = new Vector2(0f, 18f + tracking * 24f);
        }

        if (vhsHeadSwitchImage != null)
        {
            vhsHeadSwitchImage.color = new Color(0.70f, 0.90f, 1f, 0.07f + tracking * 0.12f);
        }

        float tear = Mathf.PerlinNoise(4.4f, Time.time * 1.9f);
        if (vhsWhiteTear != null)
        {
            float y = Mathf.Lerp(-frameHeight * 0.33f, frameHeight * 0.36f, tear);
            vhsWhiteTear.anchoredPosition = new Vector2(Mathf.Sin(Time.time * 18f) * 18f, y);
            vhsWhiteTear.sizeDelta = new Vector2(0f, 3f + tear * 14f + vhsTrackingPulse * 12f);
        }

        if (vhsWhiteTearImage != null)
        {
            float alpha = Mathf.Max(0f, tear - 0.68f) * 0.75f + vhsTrackingPulse * 0.06f;
            vhsWhiteTearImage.color = new Color(0.88f, 0.96f, 1f, alpha);
        }

        for (int i = 0; i < vhsNoiseBlocks.Length; i++)
        {
            float n = Mathf.PerlinNoise(i * 0.37f, Time.time * (1.7f + i * 0.03f));
            RectTransform block = vhsNoiseBlocks[i];
            block.anchoredPosition = new Vector2(
                Mathf.Lerp(-frameWidth * 0.46f, frameWidth * 0.46f, Mathf.Repeat(n + i * 0.173f, 1f)),
                Mathf.Lerp(-frameHeight * 0.44f, frameHeight * 0.44f, Mathf.Repeat(n * 1.7f + i * 0.097f, 1f)));
            block.sizeDelta = new Vector2(8f + n * 54f, 2f + n * 13f);
            if (vhsNoiseImages[i] != null)
            {
                float alpha = n > 0.72f ? 0.10f + (n - 0.72f) * 0.9f : 0.0f;
                vhsNoiseImages[i].color = new Color(0.82f, 0.92f, 1f, alpha);
            }
        }
    }
}
