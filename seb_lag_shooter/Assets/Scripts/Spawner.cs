using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public bool devMode;

    public Wave[] waves;
    public Enemy enemy;

    LivingEntity playerEntity;
    Transform playerT;

    Wave currentWave;
    int currentWaveNumber;

    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;

    MapGenerator map;

    float timeBetweenCampingChecks = 2;
    float nextCampCheckTime;
    float campTresholdDistance = 1.5f;
    Vector3 campPositionOld;
    bool isCamping;

    bool isDisabled;

    public event System.Action<int> OnNewWave;

    private void Start() {
        map = FindObjectOfType<MapGenerator>();
        NextWave();
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;
        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPositionOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;
    }

    private void Update() {
        if (!isDisabled) {
            if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime) {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                StartCoroutine("SpawnEnemy");
                CampingChecks();
            }
        }

        if (devMode) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                StopCoroutine("SpawnEnemy"); 
                foreach (Enemy e in FindObjectsOfType<Enemy>()) {
                    GameObject.Destroy(e.gameObject);
                }
                NextWave();
            }
        }
    }

    void CampingChecks() {
        if (Time.time > nextCampCheckTime) {
            nextCampCheckTime = Time.time + timeBetweenCampingChecks;

            isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campTresholdDistance);
            campPositionOld = playerT.position;
        }
    }

    IEnumerator SpawnEnemy() {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping) {
            spawnTile = map.GetTileFromPosition(campPositionOld);
        }
        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColor = Color.white;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay) {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor);
    }

    void OnPlayerDeath() {
        isDisabled = true;
    }

    void ResetPlayerPosition() { // to fix !!!!!
        // playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
        // print(playerEntity.name);
    }

    void OnEnemyDeath() {
        enemiesRemainingAlive--;

        if (enemiesRemainingAlive == 0) {
            NextWave();
        }
    }

    void NextWave() {
        if (currentWaveNumber > 0) {
            AudioManager.instance.Play2DSound("Level Completed");
        }

        currentWaveNumber++;
        if (currentWaveNumber - 1 < waves.Length) {
            currentWave = waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            if (OnNewWave != null) {
                OnNewWave(currentWaveNumber);
            }
            ResetPlayerPosition();

        }
    }

    [System.Serializable]
    public class Wave {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpawns;

        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
        public Color skinColor;
    }


}
