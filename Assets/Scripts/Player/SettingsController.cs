using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private Slider fovSlider;
    [SerializeField] private TMP_Text fovNumber;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider effectVolumeSlider;
    
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
        
        masterVolumeSlider.onValueChanged.AddListener(SaveMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SaveMusicVolume);
        effectVolumeSlider.onValueChanged.AddListener(SaveEffectVolume);
        
        resetButton.onClick.AddListener(Reset);
        initializeInteractables();
    }

    private void initializeInteractables()
    {
        if (PlayerPrefs.HasKey("FOV"))
        {
            fovSlider.value = PlayerPrefs.GetFloat("FOV");
            fovNumber.text = PlayerPrefs.GetFloat("FOV").ToString();
        }
        if (PlayerPrefs.HasKey("MouseSensitivity")) { mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity"); }
        if(PlayerPrefs.HasKey("masterVolume")) { masterVolumeSlider.value = PlayerPrefs.GetFloat("masterVolume"); }
        if(PlayerPrefs.HasKey("MusicVolume")) { musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume"); }
        if(PlayerPrefs.HasKey("EffectVolume")) { effectVolumeSlider.value = PlayerPrefs.GetFloat("EffectVolume"); }
        
    }

    private void SaveMasterVolume(float value)
    {
        AudioManager.instance.UpdateVolume();
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    private void SaveMusicVolume(float value)
    {
        AudioManager.instance.UpdateVolume();
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    private void SaveEffectVolume(float value)
    {
        PlayerPrefs.SetFloat("EffectVolume", value);
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
        effectVolumeSlider.SetValueWithoutNotify( 1f);
        musicVolumeSlider.SetValueWithoutNotify( 1f);
        
        AudioManager.instance.UpdateVolume();
    }
}
