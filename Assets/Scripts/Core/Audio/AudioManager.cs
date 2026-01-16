using System;
using System.Collections;
using System.Collections.Generic;
using Core.SystemModules;
using Core.SystemModules.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : PersistentSingleton<AudioManager>
{
    [SerializeField] private AudioSource bgmPlayer;
    [SerializeField] private AudioSource sfxPlayer;
    
    private const float MIN_PITCH = 0.9f;
    private const float MAX_PITCH = 1.1f;
    
    private float _bgmVolume;
    private float _sfxVolume;
    
    // 音频轨道管理，轨道对应的音频源对象
    private Dictionary<int, AudioSource> exitAudio = new Dictionary<int, AudioSource>();
    
    // 所有音频源对象
    private List<AudioSource> trackAudioObj = new List<AudioSource>();

    public void SetBGMVolume(float volume)
    {
        _bgmVolume = volume;
        bgmPlayer.volume = _bgmVolume;
    }

    public void SetSfxVolume(float volume)
    {
        _sfxVolume = volume;
        sfxPlayer.volume = _sfxVolume;
    }

    
    // TODO：考虑做防抖，避免频繁调用 考虑使用异步方式，避免阻塞主线程
    public void SetAudioVolume(float volume)
    {
        foreach (var audioSource in trackAudioObj)
        {
            audioSource.volume = volume;
        }
    }


    public void PlayAudioSequence(AudioData[] audioDataArray, Action onComplete = null)
    {
        StartCoroutine(PlayAudioSequenceCoroutine(audioDataArray, onComplete));
    }

    private IEnumerator PlayAudioSequenceCoroutine(AudioData[] audioDataArray, Action onComplete = null)
    {
        foreach (var audioData in audioDataArray)
        {
            PlayAudio(audioData);
            yield return new WaitForSeconds(audioData.clip.length);
        }

        if (onComplete != null)
        {
            onComplete.Invoke();
        }
    }

    /// <summary>
    /// 播放音频
    /// 音频跟随audioData的track属性，若track小于 0，则默认叠加播放
    /// 若track大于 0，则使用轨道进行管理 
    /// </summary>
    /// <param name="audioData"></param>
    /// <param name="onComplete"></param>
    public void PlayAudio(AudioData audioData, Action onComplete = null)
    {
        if (audioData.track < 0)
        {
            var audioGameObject = new GameObject("Audio_Float");
            audioGameObject.transform.SetParent(transform);
            var audioSource = audioGameObject.AddComponent<AudioSource>();
            trackAudioObj.Add(audioSource);
            audioSource.clip = audioData.clip;
            audioSource.volume = audioData.volume;
            
            // 注意：如果在轨道小于 0 且 loop 为 true 的情况下，那么这个音频对象将游离于 AudioManager 中
            if (audioData.loop)
            {
                audioSource.loop = audioData.loop;
            }
            else
            {
                // TODO:考虑使用对象池
                // 默认播放完成后销毁音频对象
                Destroy(audioGameObject, audioData.clip.length);
            }
            audioSource.Play();
        }
        else
        {
            if (exitAudio.ContainsKey(audioData.track) && audioData.force)
            {
                exitAudio[audioData.track].Stop();
                Destroy(exitAudio[audioData.track].gameObject);
            }
            var audioGameObject = new GameObject("Audio" + audioData.track);
            audioGameObject.transform.SetParent(transform);
            var audioSource = audioGameObject.AddComponent<AudioSource>();
            trackAudioObj.Add(audioSource);
            exitAudio.Add(audioData.track, audioSource);
            audioSource.clip = audioData.clip;
            audioSource.volume = audioData.volume;
            if (audioData.loop)
            {
                audioSource.loop = audioData.loop;
            }
            else
            {
                // TODO:考虑使用对象池
                // 默认播放完成后销毁音频对象
                Destroy(audioGameObject, audioData.clip.length);
                if (onComplete != null)
                {
                    Util.Instance.RunAfter(audioData.clip.length, onComplete);
                }
            }
            audioSource.Play();
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// 背景音乐默认循环播放
    /// </summary>
    /// <param name="audioData"></param>
    public void PlayBGM(AudioData audioData)
    {
        if (bgmPlayer == null)
        {
            return;
        }

        if (bgmPlayer.clip == audioData.clip)
        {
            return;
        }

        bgmPlayer.clip = audioData.clip;
    }

    /// <summary>
    /// 播放音效
    /// 音效默认不循环播放
    /// </summary>
    /// <param name="audioData"></param>
    public void PlaySfx(AudioData audioData)
    {
        if (sfxPlayer == null)
        {
            return;
        }

        sfxPlayer.PlayOneShot(audioData.clip, audioData.volume);
    }

    /// <summary>
    /// 播放随机音调的音效
    /// </summary>
    /// <param name="audioData"></param>
    public void PlayRanSfx(AudioData audioData)
    {
        if (sfxPlayer == null)
        {
            return;
        }

        sfxPlayer.PlayOneShot(audioData.clip, audioData.volume * Random.Range(MIN_PITCH, MAX_PITCH));
    }


    /// <summary>
    /// 随机播放音效组中的音效
    /// </summary>
    /// <param name="audioClips"></param>
    public void PlayRanSfx(AudioData[] audioClips)
    {
        PlayRanSfx(audioClips[Random.Range(0, audioClips.Length)]);
    }
}

[Serializable]
public class AudioData
{
    public AudioClip clip;
    public float volume = 1;
    public int track = 0;
    public bool loop = false;
    public bool force = true;
}