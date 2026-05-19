using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float CameraBreathingVolume = 1.0f;

    private AudioSource cameraBreathingSource;
    private AudioSource cameraBreathingBoostSource;
    private AudioSource cameraBreathingHeavyBoostSource;
    private Coroutine cameraBreathingLoadRoutine;
    private bool cameraBreathingWanted;

    private void SetCameraBreathingLoop(bool enabled)
    {
        cameraBreathingWanted = enabled;
        if (!enabled)
        {
            if (cameraBreathingSource != null)
            {
                cameraBreathingSource.Stop();
            }

            if (cameraBreathingBoostSource != null)
            {
                cameraBreathingBoostSource.Stop();
            }

            if (cameraBreathingHeavyBoostSource != null)
            {
                cameraBreathingHeavyBoostSource.Stop();
            }

            return;
        }

        EnsureCameraBreathingSource();
        if (cameraBreathingSource == null)
        {
            return;
        }

        if (cameraBreathingSource.clip != null)
        {
            PlayCameraBreathing();
            return;
        }

        AudioClip clip = FindCameraBreathingResourceClip();
        if (clip != null)
        {
            AssignCameraBreathingClip(clip);
            PlayCameraBreathing();
            return;
        }

        if (cameraBreathingLoadRoutine == null)
        {
            cameraBreathingLoadRoutine = StartCoroutine(LoadCameraBreathingFromFile());
        }
    }

    private void EnsureCameraBreathingSource()
    {
        if (cameraBreathingSource != null || mainCamera == null)
        {
            return;
        }

        GameObject audioObject = new GameObject("Camera Breathing Audio");
        audioObject.SetActive(false);
        audioObject.transform.SetParent(mainCamera.transform, false);

        cameraBreathingSource = audioObject.AddComponent<AudioSource>();
        ConfigureCameraBreathingSource(cameraBreathingSource, 72);
        cameraBreathingBoostSource = audioObject.AddComponent<AudioSource>();
        ConfigureCameraBreathingSource(cameraBreathingBoostSource, 73);
        cameraBreathingHeavyBoostSource = audioObject.AddComponent<AudioSource>();
        ConfigureCameraBreathingSource(cameraBreathingHeavyBoostSource, 74);
        audioObject.SetActive(true);
    }

    private void ConfigureCameraBreathingSource(AudioSource source, int priority)
    {
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = CameraBreathingVolume;
        source.priority = priority;
    }

    private IEnumerator LoadCameraBreathingFromFile()
    {
        string path = FindCameraBreathingFile();
        AudioType audioType = CameraBreathingAudioType(path);
        if (!string.IsNullOrEmpty(path) && audioType != AudioType.UNKNOWN)
        {
            string uri = "file:///" + path.Replace("\\", "/");
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
            {
                yield return request.SendWebRequest();
                AudioClip clip = ReadCameraBreathingClip(request);
                if (clip != null)
                {
                    AssignCameraBreathingClip(clip);
                }
            }
        }

        if (cameraBreathingSource != null && cameraBreathingSource.clip == null)
        {
            AssignCameraBreathingClip(CreateCameraBreathingFallbackClip());
        }

        cameraBreathingLoadRoutine = null;
        if (cameraBreathingWanted)
        {
            PlayCameraBreathing();
        }
    }

    private AudioClip FindCameraBreathingResourceClip()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sfx/CameraBreathing");
        if (clip != null)
        {
            return clip;
        }

        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sfx");
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name.Equals("CameraBreathing", System.StringComparison.OrdinalIgnoreCase))
            {
                return clips[i];
            }
        }

        return null;
    }

    private string FindCameraBreathingFile()
    {
        string folder = Path.Combine(Application.dataPath, "Resources/Sfx");
        string[] names =
        {
            "CameraBreathing.mp3",
            "CameraBreathing.ogg",
            "CameraBreathing.wav",
            "CameraBreathing.m4a"
        };

        for (int i = 0; i < names.Length; i++)
        {
            string path = Path.Combine(folder, names[i]);
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    private AudioType CameraBreathingAudioType(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return AudioType.UNKNOWN;
        }

        string extension = Path.GetExtension(path).ToLowerInvariant();
        if (extension == ".mp3" || extension == ".m4a")
        {
            return AudioType.MPEG;
        }

        if (extension == ".ogg")
        {
            return AudioType.OGGVORBIS;
        }

        return extension == ".wav" ? AudioType.WAV : AudioType.UNKNOWN;
    }

    private AudioClip ReadCameraBreathingClip(UnityWebRequest request)
    {
        if (request.result != UnityWebRequest.Result.Success)
        {
            return null;
        }

        try
        {
            return DownloadHandlerAudioClip.GetContent(request);
        }
        catch
        {
            return null;
        }
    }

    private void AssignCameraBreathingClip(AudioClip clip)
    {
        cameraBreathingSource.clip = clip;
        cameraBreathingSource.loop = true;
        cameraBreathingSource.volume = CameraBreathingVolume;
        if (cameraBreathingBoostSource != null)
        {
            cameraBreathingBoostSource.clip = clip;
            cameraBreathingBoostSource.loop = true;
            cameraBreathingBoostSource.volume = CameraBreathingVolume;
        }

        if (cameraBreathingHeavyBoostSource != null)
        {
            cameraBreathingHeavyBoostSource.clip = clip;
            cameraBreathingHeavyBoostSource.loop = true;
            cameraBreathingHeavyBoostSource.volume = CameraBreathingVolume;
        }
    }

    private void PlayCameraBreathing()
    {
        if (cameraBreathingSource == null || cameraBreathingSource.isPlaying)
        {
            return;
        }

        cameraBreathingSource.Play();
        if (cameraBreathingBoostSource != null)
        {
            cameraBreathingBoostSource.timeSamples = cameraBreathingSource.timeSamples;
            cameraBreathingBoostSource.Play();
        }

        if (cameraBreathingHeavyBoostSource != null)
        {
            cameraBreathingHeavyBoostSource.timeSamples = cameraBreathingSource.timeSamples;
            cameraBreathingHeavyBoostSource.Play();
        }
    }

    private AudioClip CreateCameraBreathingFallbackClip()
    {
        const int sampleRate = 22050;
        const int seconds = 4;
        float[] data = new float[sampleRate * seconds];
        for (int i = 0; i < data.Length; i++)
        {
            float t = i / (float)sampleRate;
            float breath = 0.5f + Mathf.Sin(t * Mathf.PI * 2f * 0.28f) * 0.5f;
            float hum = Mathf.Sin(t * Mathf.PI * 2f * 74f) * 0.028f;
            float hiss = (Mathf.PerlinNoise(t * 18f, 0.37f) - 0.5f) * 0.030f;
            data[i] = (hum + hiss) * Mathf.Lerp(0.24f, 1f, breath);
        }

        AudioClip clip = AudioClip.Create("CameraBreathingFallback", data.Length, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
