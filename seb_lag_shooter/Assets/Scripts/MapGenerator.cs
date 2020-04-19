using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform mapFloor;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;
    public Vector2 maxMapSize;

    public float tileSize = 1;

    [Range(0, 1)]
    public float outlinePercent;


    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledOpenTileCoords;
    Transform[,] tileMap;

    Map currentMap;

    [System.Obsolete]
    private void Awake() {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber) {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    public void GenerateMap() {
        currentMap = maps[mapIndex];
        System.Random prng = new System.Random(currentMap.seed);
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];

        //  Generating coords
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++) {
            for (int y = 0; y < currentMap.mapSize.y; y++) {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        //  Create map holder obj
        string holderName = "Generated map";
        if (transform.Find(holderName)) {          //  il metodo FindChild() è obsoleto
            DestroyImmediate(transform.Find(holderName).gameObject);       //  usiamo DestroyImmediate() perchè chiameremo il metodo dall'editor
        }
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //  Spawning tiles
        for (int x = 0; x < currentMap.mapSize.x; x++) {
            for (int y = 0; y < currentMap.mapSize.y; y++) {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90), transform) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;
            }
        }

        //  Spawning obstacles
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
        int currentObstacleCount = 0;
        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);

        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);

        for (int i = 0; i < obstacleCount; i++) {
            Coord randomCord = GetRandomCoord();

            obstacleMap[randomCord.x, randomCord.y] = true;
            currentObstacleCount++;

            if (randomCord.x != currentMap.mapCenter.x && randomCord.y != currentMap.mapCenter.y && MapIsFullyAccessible(obstacleMap, currentObstacleCount)) {
                Vector3 obstaclePosition = CoordToPosition(randomCord.x, randomCord.y);
                float obstacleHeight = Mathf.Lerp(currentMap.minObstHeight, currentMap.maxObstHeight, (float)prng.NextDouble());
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = randomCord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroudColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCord);
            }
            else {
                obstacleMap[randomCord.x, randomCord.y] = false;
                currentObstacleCount--;
            }
        }

        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));


        //  creating navmesh mask 
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity, mapHolder) as Transform;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, (currentMap.mapSize.y)) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity, mapHolder) as Transform;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, (currentMap.mapSize.y)) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity, mapHolder) as Transform;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskBack = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity, mapHolder) as Transform;
        maskBack.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        mapFloor.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);


    }


    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount) {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accessibleTilesCount = 1;

        while (queue.Count > 0) {
            Coord tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0) {           //  escludo le diagonali
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1)) {         //  controllo di non guardare tile fuori dalla mappa
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY]) {        //  se non è una tile già controllata, se non è un ostacolo
                                mapFlags[neighbourX, neighbourY] = true;        //  abbiamo controllato questa tile
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessibleTilesCount++;
                            }
                        }
                    }
                }
            }
        }       //  fine while

        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y) - currentObstacleCount;

        return targetAccessibleTileCount == accessibleTilesCount;

    }


    public Coord GetRandomCoord() {
        Coord randomCord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCord);
        return randomCord;
    }

    public Transform GetRandomOpenTile() {
        Coord randomCord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCord);
        return tileMap[randomCord.x, randomCord.y];
    }

    Vector3 CoordToPosition(int x, int y) {
        return new Vector3(-currentMap.mapSize.x / 2f + .5f + x, 0, -currentMap.mapSize.y / 2f + .5f + y) * tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position) {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2);
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
        return tileMap[x, y];
    }

    [System.Serializable]
#pragma warning disable CS0660 // Il tipo definisce l'operatore == o l'operatore != ma non esegue l'override di Object.Equals(object o)
#pragma warning disable CS0661 // Il tipo definisce l'operatore == o l'operatore != ma non esegue l'override di Object.GetHashCode()
    public struct Coord {
#pragma warning restore CS0661 // Il tipo definisce l'operatore == o l'operatore != ma non esegue l'override di Object.GetHashCode()
#pragma warning restore CS0660 // Il tipo definisce l'operatore == o l'operatore != ma non esegue l'override di Object.Equals(object o)
        public int x;
        public int y;

        public Coord(int _x, int _y) {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2) {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2) {
            return !(c1 == c2);
        }
    }


    [System.Serializable]
    public class Map {

        public Coord mapSize;
        [Range(0, 1)]
        public float obstaclePercent;
        public int seed;
        public float minObstHeight;
        public float maxObstHeight;
        public Color foregroudColor, backgroundColor;
        public Coord mapCenter {
            get {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}
