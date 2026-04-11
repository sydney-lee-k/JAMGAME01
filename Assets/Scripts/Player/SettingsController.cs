using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private Slider fovSlider;
    [SerializeField] private TMP_Text fovNumber;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundVolumeSlider;

    [SerializeField] private AudioMixer audioMixer;
    
    [SerializeField] private Button resetButton;

    private Camera cam;
    private void Start()
    {
        cam = Camera.main;
        if (PlayerPrefs.HasKey("FOV"))
        {
            cam.fieldOfView = PlayerPrefs.GetFloat("FOV");
        }
        fovSlider.onValueChanged.AddListener(SaveFov);
        mouseSensitivitySlider.onValueChanged.AddListener(SaveMouseSensitivity);

        //masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        //musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        //soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);

        resetButton.onClick.AddListener(Reset);
        InitializeInteractables();
        LoadVolume();
    }

    private void InitializeInteractables()
    {
        if (PlayerPrefs.HasKey("FOV"))
        {
            fovSlider.value = PlayerPrefs.GetFloat("FOV");
            fovNumber.text = PlayerPrefs.GetFloat("FOV").ToString();
        }
        if (PlayerPrefs.HasKey("MouseSensitivity")) { mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity"); }
        if(PlayerPrefs.HasKey("MasterVolume")) { masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume"); }
        if(PlayerPrefs.HasKey("MusicVolume")) { musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume"); }
        if(PlayerPrefs.HasKey("SoundVolume")) { soundVolumeSlider.value = PlayerPrefs.GetFloat("SoundVolume"); }
        
    }

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSoundVolume(float value)
    {
        audioMixer.SetFloat("SoundVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SoundVolume", value);
    }

    private void LoadVolume()
    {
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        SetMasterVolume(masterVolumeSlider.value);

        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        SetMusicVolume(musicVolumeSlider.value);

        soundVolumeSlider.value = PlayerPrefs.GetFloat("SoundVolume");
        SetSoundVolume(soundVolumeSlider.value);
    }

    private void SaveFov(float value)
    {
        PlayerPrefs.SetFloat("FOV", value);
        fovNumber.text = value.ToString();
        cam.fieldOfView = value;
    }

    private void SaveMouseSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }

    private void Reset()
    {
        PlayerPrefs.DeleteAll();
        fovSlider.SetValueWithoutNotify( 90f);
        fovNumber.text = "90";
        cam.fieldOfView = 90;
        mouseSensitivitySlider.SetValueWithoutNotify(0.25f);
        masterVolumeSlider.SetValueWithoutNotify( 1f);
        soundVolumeSlider.SetValueWithoutNotify( 1f);
        musicVolumeSlider.SetValueWithoutNotify( 1f);      
    }
}
