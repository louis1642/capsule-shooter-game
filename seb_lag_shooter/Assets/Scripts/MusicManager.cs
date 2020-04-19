using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour {

    public AudioClip mainTheme, menuTheme;

    string activeSceneName;

    private void Start() {
        OnLevelWasLoaded(0);
    }

    void OnLevelWasLoaded(int index) {
        string newSceneName = SceneManager.GetActiveScene().name;
        if (newSceneName != activeSceneName) {
            activeSceneName = newSceneName;
            Invoke("PlayMusic", .2f);       //  il ritardo è per assicurarci che nella nuova scena un eventuale duplicato dell'AudioMGR si autodistrugga
        }
    }

    void PlayMusic() {
        AudioClip clipToPlay = null;
        if (activeSceneName == "Menu") {
            clipToPlay = menuTheme;
        } else if (activeSceneName == "Game") {
            clipToPlay = mainTheme;
        }

        if (clipToPlay != null) {
            AudioManager.instance.PlayMusic(clipToPlay, 2);
            Invoke("PlayMusic", clipToPlay.length);
        }
    }

}
