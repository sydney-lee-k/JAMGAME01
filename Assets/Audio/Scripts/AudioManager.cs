using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using static Unity.VisualScripting.Member;
using Random = UnityEngine.Random;


public enum SoundType
{
    CLICK,
    FOOTSTEP,
    MASK,
    DEATH,
    PISTOL,
    SHOTGUN,
    RIFLE
}
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    [SerializeField] private SoundList[] soundList;
    [SerializeField] private AudioSource audioSourceSFX;
    [SerializeField] private AudioSource audioSourceMusic;

    [SerializeField] private AudioMixerGroup sfxMixer;

    [SerializeField][Range(0, 1f)] private float soundVar;

    private bool isPlaying = false;

    void Awake()
    {

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        PlayPauseMusic();
    }

    public static void PlayPauseMusic()
    {
        if (instance.isPlaying == true)
        {
            instance.audioSourceMusic.Stop();
            instance.isPlaying = false;
        }
        else
        {
            instance.audioSourceMusic.Play();
            instance.isPlaying = true;
        }
    }

    //Plays sound by SoundType
    public static void PlaySound(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;                                  //List of available clips in given SoundType
        AudioClip clip = clips[Random.Range(0, clips.Length)];                                      //Randomize clip
        instance.audioSourceSFX.pitch = Random.Range(1 - instance.soundVar, 1 + instance.soundVar);    //Randomize pitch
        instance.audioSourceSFX.PlayOneShot(clip, volume);                                             //Play clip
    }
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [SerializeField] private string name;
    [SerializeField] private AudioClip[] sounds;
}
