using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public enum AudioChannel { Master, Sfx, Music };

    public float masterVolumePerc { get; private set; }
    public float sfxVolumePerc { get; private set; }
    public float musicVolumePerc { get; private set; }

    AudioSource sfx2DSource;
    AudioSource[] musicSources;
    int activeMusicSourceIndex;

    public static AudioManager instance;

    Transform audioListener;
    Transform playerT;

    SoundLibrary library;

    private void Awake() {

        if (instance != null) {     //  allora esiste già, e questo è un duplicato
            Destroy(gameObject);
        }
        else {

            instance = this;        //  non un'ottima prassi, ma supponiamo che l'audio manager sia unico
            DontDestroyOnLoad(gameObject);      //  altrimenti cambiando scena si stopperebbe la musica

            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++) {
                GameObject newMusicSource = new GameObject("Music Source " + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                musicSources[i].transform.parent = transform;
            }

            GameObject newSfx2DSource = new GameObject("2D Sfx Source");
            sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();
            newSfx2DSource.transform.parent = transform;

            audioListener = FindObjectOfType<AudioListener>().transform;
            if (FindObjectOfType<Player>() != null) {
                playerT = FindObjectOfType<Player>().transform;
            }
            library = GetComponent<SoundLibrary>();

            masterVolumePerc = PlayerPrefs.GetFloat("MasterVol", .75f);
            sfxVolumePerc = PlayerPrefs.GetFloat("SfxVol", .75f);
            musicVolumePerc = PlayerPrefs.GetFloat("MusicVol", .75f);
        }
    }

    private void Update() {
        if (playerT != null) {
            audioListener.position = playerT.position;
        }
    }

    public void SetVolume(float volume, AudioChannel channel) {
        switch (channel) {
            case AudioChannel.Master:
                masterVolumePerc = volume;
                break;
            case AudioChannel.Music:
                musicVolumePerc = volume;
                break;
            case AudioChannel.Sfx:
                sfxVolumePerc = volume;
                break;
        }

        musicSources[0].volume = musicVolumePerc * masterVolumePerc;
        musicSources[1].volume = musicVolumePerc * masterVolumePerc;

        PlayerPrefs.SetFloat("MasterVol", masterVolumePerc);
        PlayerPrefs.SetFloat("SfxVol", sfxVolumePerc);
        PlayerPrefs.SetFloat("MusicVol", musicVolumePerc);
        PlayerPrefs.Save();

    }

    public void PlayMusic(AudioClip music, float fadeDuration = 1) {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = music;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossfade(fadeDuration));
    }

    IEnumerator AnimateMusicCrossfade(float duration) {
        float percent = 0;

        while (percent < 1) {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePerc * masterVolumePerc, percent);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePerc * masterVolumePerc, 0, percent);
            yield return null;
        }
    }

    public void PlaySound(AudioClip clip, Vector3 position) {
        if (clip != null) {
            AudioSource.PlayClipAtPoint(clip, position, sfxVolumePerc * masterVolumePerc);
        }
    }

    public void PlaySound(string clipName, Vector3 position) {
        PlaySound(library.GetClipFromName(clipName), position);
    }

    public void Play2DSound(string name) {
        sfx2DSource.PlayOneShot(library.GetClipFromName(name), sfxVolumePerc * masterVolumePerc);
    }

    void OnLevelWasLoaded(int index) {
        if (playerT == null) {
            if (FindObjectOfType<Player>() != null) {
                playerT = FindObjectOfType<Player>().transform;
            }
        }
    }

}
