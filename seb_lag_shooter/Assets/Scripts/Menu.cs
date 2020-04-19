using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

    public GameObject mainMenuHold, optMenuHold;

    public Slider[] volumeSliders;
    public Toggle[] resToggles;
    public Toggle fullscreenToggle;
    public int[] screenWidths;

    int activeScreenRes;

    private void Start() {
        activeScreenRes = PlayerPrefs.GetInt("screenResIndex");
        bool fullscreenByPlayerPref = (PlayerPrefs.GetInt("fullscreen", 0) == 1) ? true : false;

        volumeSliders[0].value = AudioManager.instance.masterVolumePerc;
        volumeSliders[1].value = AudioManager.instance.musicVolumePerc;
        volumeSliders[2].value = AudioManager.instance.sfxVolumePerc;

        for (int i = 0; i < resToggles.Length; i++) {
            resToggles[i].isOn = (i == activeScreenRes);
        }

        fullscreenToggle.isOn = fullscreenByPlayerPref;
    }


    public void Play() {
        SceneManager.LoadScene("Game");
    }

    public void Quit() {
        Application.Quit();
    }

    public void OptionsMenu() {
        mainMenuHold.SetActive(false);
        optMenuHold.SetActive(true);
    }

    public void MainMenu() {
        mainMenuHold.SetActive(true);
        optMenuHold.SetActive(false);
    }

    public void SetScreenResolution(int i) {
        if (resToggles[i].isOn) {
            float aspectRatio = 16 / 9f;
            Screen.SetResolution(screenWidths[i], (int)(screenWidths[i] / aspectRatio), false);
            activeScreenRes = i;
            PlayerPrefs.SetInt("screenResIndex", activeScreenRes);
            PlayerPrefs.Save();

        }
    }

    public void SetFullscreen(bool fullscreen) {
        foreach (Toggle t in resToggles) {
            t.interactable = !fullscreen;
        }

        if (fullscreen) {
            Resolution[] allRes = Screen.resolutions;
            Resolution maxRes = allRes[allRes.Length - 1];
            Screen.SetResolution(maxRes.width, maxRes.height, true);
        }
        else {
            SetScreenResolution(activeScreenRes);
        }

        PlayerPrefs.SetInt("isFullscreen", ((fullscreen) ? 1 : 0));
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float v) {
        AudioManager.instance.SetVolume(v, AudioManager.AudioChannel.Master);
    }

    public void SetMusicVolume(float v) {
        AudioManager.instance.SetVolume(v, AudioManager.AudioChannel.Music);

    }

    public void SetSfxVolume(float v) {
        AudioManager.instance.SetVolume(v, AudioManager.AudioChannel.Sfx);

    }


}
