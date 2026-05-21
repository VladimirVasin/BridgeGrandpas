using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private bool fakeUnityErrorMusicSuppressed;
    private bool fakeUnityErrorMusicEscapeMenuVisited;

    private void SuppressBackgroundMusicAfterFakeUnityError()
    {
        fakeUnityErrorMusicSuppressed = true;
        fakeUnityErrorMusicEscapeMenuVisited = false;
        StopSuppressedFakeUnityErrorMusic();
        WriteDebugLog("FAKE_UNITY_ERROR_MUSIC", "Background music suppressed until Escape menu is opened and closed.");
    }

    private bool IsBackgroundMusicSuppressedAfterFakeUnityError()
    {
        return fakeUnityErrorMusicSuppressed;
    }

    private void MarkEscapeMenuVisitedForFakeUnityErrorMusic()
    {
        if (!fakeUnityErrorMusicSuppressed)
        {
            return;
        }

        fakeUnityErrorMusicEscapeMenuVisited = true;
        WriteDebugLog("FAKE_UNITY_ERROR_MUSIC", "Escape menu visited while music was suppressed.");
    }

    private void RestoreBackgroundMusicAfterFakeUnityErrorEscapeReturn()
    {
        if (!fakeUnityErrorMusicSuppressed || !fakeUnityErrorMusicEscapeMenuVisited)
        {
            return;
        }

        fakeUnityErrorMusicSuppressed = false;
        fakeUnityErrorMusicEscapeMenuVisited = false;
        waitingForNextMusic = true;
        nextMusicStartAt = Time.time + BackgroundMusicStartDelay;
        WriteDebugLog("FAKE_UNITY_ERROR_MUSIC", "Background music restored after Escape menu return.");
    }

    private void RestoreBackgroundMusicImmediatelyAfterFakeUnityError()
    {
        fakeUnityErrorMusicSuppressed = false;
        fakeUnityErrorMusicEscapeMenuVisited = false;
        waitingForNextMusic = true;
        nextMusicStartAt = Time.time;

        if (musicSource != null && ingameMusicClips != null && ingameMusicClips.Length > 0)
        {
            PlayNextBackgroundMusicTrack();
        }

        WriteDebugLog("FAKE_UNITY_ERROR_MUSIC", "Background music restored immediately after fake webcam menu.");
    }

    private void StopSuppressedFakeUnityErrorMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }

        waitingForNextMusic = true;
        nextMusicStartAt = float.PositiveInfinity;
    }
}
