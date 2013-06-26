using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {

  public int size = 64;

  public GameObject startTilePrefab;
  public GameObject endTilePrefab;
  public GameObject freeTilePrefab;
  public GameObject blockedTilePrefab;

  public MapTile startTile;
  public MapTile endTile;

  public void Generate () {
    BuildTiles(GenerateRandomMapTileTypes());
  }

  MapTileType[,] GenerateRandomMapTileTypes () {
    MapTileType[,] tiles = new MapTileType[size, size];
    bool startTileUsed = false;
    bool endTileUsed = false;
    for (int i=0; i < size; i++) {
       for (int j=0; j < size; j++){

        int random = Random.Range(0, 4);
        MapTileType type = MapTileType.Free;
        switch (random) {
           case 0:
              type = MapTileType.Free;
              break;
           case 1:
              type = MapTileType.Blocked;
              break;
           case 2:
              if (!startTileUsed) {
                type = MapTileType.Start;
                startTileUsed = true;
              }
              break;
           case 3:
              if (!endTileUsed) {
                type = MapTileType.End;
                endTileUsed = true;
              }
              break;
        }

        tiles[i,j] = type;

       }
    }
    return tiles;
  }

  Vector3 PositionForTileAtIndices(int i, int j) {
    return new Vector3(i * TileSize(), 0, j * TileSize());
  }

  void BuildTiles(MapTileType[,] map) {
    for (int i=0; i < size; i++) {
       for (int j=0; j < size; j++){

        MapTileType type = map[i,j];
        GameObject prefab = freeTilePrefab;
        switch (type) {
           case MapTileType.Free:
              prefab = freeTilePrefab;
              break;
           case MapTileType.Blocked:
              prefab = blockedTilePrefab;
              break;
           case MapTileType.End:
              prefab = endTilePrefab;
              break;
           case MapTileType.Start:
              prefab = startTilePrefab;
              break;
        }


        GameObject tileObject = Instantiate(prefab, PositionForTileAtIndices(i, j), Quaternion.identity) as GameObject;
        tileObject.name = "Tile (" + i + ", " + j + ")";
        tileObject.transform.parent = this.transform;

        MapTile tile = tileObject.GetComponent<MapTile>();
        tile.map = this;
        tile.type = type;
        tile.point = new MapPoint(i, j);

        if (type == MapTileType.Start) {
          startTile = tile;
        }
        else if (type == MapTileType.End) {
          endTile = tile;
        }

       }
    }
  }

  public Vector3 Origin () {
      return transform.position;
  }
  
  public Vector3 Center() {
      float absoluteX  = size  * TileSize();
      float absoluteZ = size   * TileSize();
      return Origin() + new Vector3(absoluteX / 2, 0, absoluteZ / 2);
  }
  
  public float TileSize () {
      return 10;
  }

  public Bounds Bounds(){
      float absoluteWidth  = size  * TileSize();
      float absoluteHeight = size  * TileSize();
      return new Bounds(Center(), new Vector3(absoluteWidth, 1000, absoluteHeight));
  }
  
  // Update is called once per frame
  void Update () {
  
  }

}
