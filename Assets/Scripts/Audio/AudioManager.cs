using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : PersistentSingleton<AudioManager>
{
    [SerializeField] private AudioSource sfxPlayer;
    private float pitch = 1f;
    const float MIN_PITCH = 0.9f;
    const float MAX_PITCH = 1.1f;
    public void PlaySFX(AudioData audioData)
    {
        sfxPlayer.PlayOneShot(audioData.clip, audioData.volume*pitch);
    }
    
    public void PlayRandomSFX(AudioData audioData)
    {
        sfxPlayer.pitch = Random.Range(MIN_PITCH, MAX_PITCH);
        PlaySFX(audioData);
    }

    public void PlayRandomSFX(AudioData[] audioClips)
    {
        PlayRandomSFX(audioClips[Random.Range(0, audioClips.Length)]);
    }
    
}

[System.Serializable] public class AudioData
{
    public AudioClip clip;
    public float volume;
}