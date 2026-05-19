using UnityEngine;
using UnityEngine.Audio;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private enum BridgeAudioBus
    {
        Music,
        Ambience,
        Footsteps,
        Vhs
    }

    private AudioMixerGroup musicMixerGroup;
    private AudioMixerGroup ambienceMixerGroup;
    private AudioMixerGroup footstepMixerGroup;
    private AudioMixerGroup vhsMixerGroup;

    private void SetupAudioRouting()
    {
        AudioMixer mixer = Resources.Load<AudioMixer>("Audio/BridgeGrandpasAudioMixer");
        if (mixer == null)
        {
            return;
        }

        musicMixerGroup = FindMixerGroup(mixer, "Music");
        ambienceMixerGroup = FindMixerGroup(mixer, "Ambience");
        footstepMixerGroup = FindMixerGroup(mixer, "Footsteps");
        vhsMixerGroup = FindMixerGroup(mixer, "VHS");
    }

    private static AudioMixerGroup FindMixerGroup(AudioMixer mixer, string name)
    {
        AudioMixerGroup[] groups = mixer.FindMatchingGroups(name);
        return groups != null && groups.Length > 0 ? groups[0] : null;
    }

    private void RouteAudioSource(AudioSource source, BridgeAudioBus bus)
    {
        if (source == null)
        {
            return;
        }

        AudioMixerGroup group = null;
        switch (bus)
        {
            case BridgeAudioBus.Music:
                group = musicMixerGroup;
                break;
            case BridgeAudioBus.Ambience:
                group = ambienceMixerGroup;
                break;
            case BridgeAudioBus.Footsteps:
                group = footstepMixerGroup;
                break;
            case BridgeAudioBus.Vhs:
                group = vhsMixerGroup;
                break;
        }

        if (group != null)
        {
            source.outputAudioMixerGroup = group;
        }
    }
}
