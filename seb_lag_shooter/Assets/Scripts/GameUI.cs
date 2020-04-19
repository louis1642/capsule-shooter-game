using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    public Image fadePlane;
    public GameObject gameOverUI;

    public RectTransform newWaveBanner;
    public Text title, enemycount;
    public Text scoreUI;
    public Text gameOverScoreUI;

    public RectTransform healthBar;
    Player player;

    Spawner spawner;
    private void Awake() {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    private void Start() {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;   
    }

    private void Update() {
        scoreUI.text = ScoreKeeper.score.ToString("D6");        //  D6 per averla nel formato 000000
        float healthPerc = 0;
        if (player != null) {
            healthPerc = player.health / player.startingHealth;
        }
        healthBar.localScale = new Vector3(healthPerc, 1, 1);

    }

    void OnNewWave(int waveN) {
        title.text = "- Wave " + waveN.ToString() + " -";
        if (!spawner.waves[waveN - 1].infinite) {
            enemycount.text = "Enemies: " + spawner.waves[waveN - 1].enemyCount;
        } else {
            enemycount.text = "Survive!";
        }
        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    IEnumerator AnimateNewWaveBanner() {
        float animPercent = 0;
        float speed = 3f;
        float delayTime = 1.5f;
        float endDelayTime = Time.time + 1 / speed + delayTime;
        int dir = 1;

        while (animPercent >= 0) {
            animPercent += Time.deltaTime * speed * dir;

            if (animPercent >= 1) {
                animPercent = 1;
                if (Time.time > endDelayTime) {
                    dir = -1;
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-435, -120, animPercent);
            yield return null;
        }
    }

    void OnGameOver() {
        StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, .9f), 1));
        scoreUI.gameObject.SetActive(false);
        healthBar.transform.parent.gameObject.SetActive(false);
        gameOverScoreUI.text = scoreUI.text;
        gameOverUI.SetActive(true);
        Cursor.visible = true;
    }

    IEnumerator Fade(Color from, Color to, float time) {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1) {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }


    // UI Input
    public void StartNewGame() {
        SceneManager.LoadScene("Game");
    }

    public void ReturnToMenu() {
        SceneManager.LoadScene("Menu"); 
    }

}
