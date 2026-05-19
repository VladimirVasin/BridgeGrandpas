using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const string BridgeCarEngineAssetPath =
        "Assets/Vehicle_Essentials/Vehicle_Car/Vehicle_Car_Engine/Vehicle_Car_Engine_1000_RPM_Rear_Exterior_Loop.wav";
    private const string BridgeCarEngineResourcesPath =
        "Vehicle_Essentials/Vehicle_Car/Vehicle_Car_Engine/Vehicle_Car_Engine_1000_RPM_Rear_Exterior_Loop";

    private AudioClip bridgeCarEngineClip;

    private void AttachBridgeCarEngineAudio(Transform carRoot)
    {
        AudioClip clip = LoadBridgeCarEngineClip();
        if (clip == null || carRoot == null)
        {
            return;
        }

        GameObject audioObject = new GameObject("Passing Car Engine Audio");
        audioObject.transform.SetParent(carRoot, false);
        audioObject.transform.localPosition = new Vector3(-0.62f, 0.28f, 0f);

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 4.5f;
        source.maxDistance = 38f;
        source.volume = Random.Range(0.28f, 0.38f);
        source.pitch = Random.Range(0.88f, 1.08f);
        source.priority = 120;
        source.dopplerLevel = 0.12f;
        source.reverbZoneMix = 0.48f;
        RouteAudioSource(source, BridgeAudioBus.Ambience);
        AddBridgeCarEngineEffects(audioObject);
        source.Play();
    }

    private AudioClip LoadBridgeCarEngineClip()
    {
        if (bridgeCarEngineClip != null)
        {
            return bridgeCarEngineClip;
        }

        bridgeCarEngineClip = Resources.Load<AudioClip>(BridgeCarEngineResourcesPath);
#if UNITY_EDITOR
        if (bridgeCarEngineClip == null)
        {
            bridgeCarEngineClip = AssetDatabase.LoadAssetAtPath<AudioClip>(BridgeCarEngineAssetPath);
        }
#endif

        if (bridgeCarEngineClip == null)
        {
            Debug.LogWarning("[BridgeGrandpas] Bridge car engine clip not found.");
        }

        return bridgeCarEngineClip;
    }

    private void AddBridgeCarEngineEffects(GameObject audioObject)
    {
        AudioReverbFilter reverb = audioObject.AddComponent<AudioReverbFilter>();
        reverb.reverbPreset = AudioReverbPreset.User;
        reverb.dryLevel = 0f;
        reverb.room = -1120;
        reverb.roomHF = -2150;
        reverb.decayTime = 1.72f;
        reverb.decayHFRatio = 0.46f;
        reverb.reflectionsLevel = -1450f;
        reverb.reverbLevel = -880f;
    }
}
