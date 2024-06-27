using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Settings : MonoBehaviour
{
    public Dropdown resolutionDropdown;
    public Dropdown qualityDropdown;

    [Header("�����")]
    public AudioMixer mixer;    
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider musicSlider;

    Resolution[] resolutions;

    void Start()
    {
        //�����
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height + " " + resolutions[i].refreshRate + "Hz";
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
        LoadSettings(currentResolutionIndex);

        //�����
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            //���������� ������ ��������� ������� �� ������ ����������� �������� �������������
            mixer.SetFloat("MasterVolume", PlayerPrefs.GetFloat("MasterVolume"));
            mixer.SetFloat("SFXVolume", PlayerPrefs.GetFloat("SFXVolume"));
            mixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume"));
            SetSliders();
        }
        else
        {
            SetSliders();
        }
    }

    public void SetFullscreen(bool isfullscreen)
    {
        Screen.fullScreen = isfullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    //�����
    void SetSliders()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
    }

    //���������� ����� ���������
    public void UpdateMasterVolume()
    {
        mixer.SetFloat("MasterVolume", masterSlider.value);
        PlayerPrefs.SetFloat("MasterVolume", masterSlider.value);
    }

    //���������� ��������� �������
    public void UpdateSFXVolume()
    {
        mixer.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
    }
    
    //���������� ��������� ������
    public void UpdateMusicVolume()
    {
        mixer.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
    }

    //���������� ��������
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("QualitySettingsPreference", qualityDropdown.value);
        PlayerPrefs.SetInt("ResolutionPreference", resolutionDropdown.value);
        PlayerPrefs.SetInt("FullscreenPreference", System.Convert.ToInt32(Screen.fullScreen));
    }

    //�������� ���������� ��������
    public void LoadSettings(int currentResolutionIndex)
    {
        //�������� �������� �������
        if (PlayerPrefs.HasKey("QualitySettingsPreference"))
        {
            qualityDropdown.value = PlayerPrefs.GetInt("QualitySettingsPreference");
        }
        else
            qualityDropdown.value = 2;

        //�������� �������� ���������� ������
        if (PlayerPrefs.HasKey("ResolutionPreference"))
        {
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionPreference");
        }
        else
            resolutionDropdown.value = currentResolutionIndex;

        //�������� �������� �������� ������
        if (PlayerPrefs.HasKey("FullscreenPreference"))
        {
            Screen.fullScreen = System.Convert.ToBoolean(PlayerPrefs.GetInt("FullscreenPreference"));
        }
        else
            Screen.fullScreen = true;
    }
}
