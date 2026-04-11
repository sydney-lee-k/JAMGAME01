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
    JUMP,
    LANDING,
    COLLECT,
    DEATH,
    LEVELCHANGE
}

public enum StageTrack
{
    STAGE_1,
    STAGE_2,
    STAGE_3,
    STAGE_4,
    STAGE_5,
    STAGE_6,
}

//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioMixer mainAudioMixer;
    //[SerializeField] private AudioMixerGroup AudioMixerSound;
    //[SerializeField] private AudioMixerGroup AudioMixerMusic;

    [SerializeField] private SoundList[] soundList;
    [SerializeField] private TrackList[] trackList;

    [SerializeField] private AudioSource audioSourceSFX;
    [SerializeField] private AudioSource audioSourceMusic_A;
    [SerializeField] private AudioSource audioSourceMusic_B;

    [SerializeField][Range(0, 1f)] private float soundVar;

    //private bool isPlaying = false;

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
        //PlayPauseMusic();
    }

    /*
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
    */

    //Called at the start of each stage
    public static void StartStageMusic(StageTrack stage)
    {
        //Make sure neither music audioSource is playing
        instance.audioSourceMusic_A.Stop();
        instance.audioSourceMusic_B.Stop();

        AudioClip[] clips = instance.trackList[(int)stage].Tracks;  //List of songs for the current stage
        AudioClip sideA = clips[0];                                 //The song playing before all keys are collected
        AudioClip sideB = clips[1];                                 //The song that starts the moment the last key is collected

        //Set clips to their audioSources
        instance.audioSourceMusic_A.clip = sideA;                                                   
        instance.audioSourceMusic_B.clip = sideB;

        //Begin playing sideA
        instance.audioSourceMusic_A.Play();
    }

    //Called when the last key is collected
    public static void ChangeMusic()
    {
        instance.audioSourceMusic_A.Stop();
        instance.audioSourceMusic_B.Play();
    }

    public static void StopAllMusic()
    {
        instance.audioSourceMusic_A.Stop();
        instance.audioSourceMusic_B.Stop();
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        float effectMultiplier = PlayerPrefs.HasKey("EffectVolume") ? PlayerPrefs.GetFloat("EffectVolume") : 1f;
        float masterMultiplier = PlayerPrefs.HasKey("MasterVolume") ? PlayerPrefs.GetFloat("MasterVolume") : 1f;
        
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;                                            //List of available clips in given SoundType
        AudioClip clip = clips[Random.Range(0, clips.Length)];                                                //Randomize clip
        instance.audioSourceSFX.pitch = Random.Range(1 - instance.soundVar, 1 + instance.soundVar);           //Randomize pitch
        instance.audioSourceSFX.PlayOneShot(clip, volume * effectMultiplier * masterMultiplier);              //Play clip
    }

    public static void PlayFootSteps()
    {

        //instance.audioSourceSFX
    }
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [SerializeField] private string name;
    [SerializeField] private AudioClip[] sounds;
}
[Serializable]
public struct TrackList
{
    public AudioClip[] Tracks { get => tracks; }
    [SerializeField] private string name;
    [SerializeField] private AudioClip[] tracks;
}
