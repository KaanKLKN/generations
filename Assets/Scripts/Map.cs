using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {

  public int size = 64;

  public GameObject startTilePrefab;
  public GameObject endTilePrefab;
  public GameObject freeTilePrefab;
  public GameObject blockedTilePrefab;
  public GameObject foodTilePrefab;

  public MapTile startTile;
  public MapTile endTile;

  public void Generate () {
    BuildTiles(GenerateRandomMapTileTypes());
  }

  MapTileType[,] GenerateRandomMapTileTypes () {
    MapTileType[,] tileTypes = new MapTileType[size, size];
    for (int i=0; i < size; i++) {
       for (int j=0; j < size; j++){

        float val = Random.value;
        MapTileType type = MapTileType.Free;

        if (val < 0.3) {
          type = MapTileType.Food;
        }

        tileTypes[i,j] = type;

       }
    }

    // Add a start tile
    tileTypes[Random.Range(0, size / 4), Random.Range(0, size)] = MapTileType.Start;

    // Add an end tile
    //tileTypes[Random.Range(size - size / 4, size), Random.Range(0, size)] = MapTileType.End;

    return tileTypes;
  }

  MapTile[,] tiles;

  void BuildTiles(MapTileType[,] map) {
    tiles = new MapTile[size, size];

    double[,] tileHeightmap = new PlasmaFractalGenerator().Generate(size, size, 50);
    double[,] tileHuemap = new PlasmaFractalGenerator().Generate(size, size, 50);

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
           case MapTileType.Food:
              prefab = foodTilePrefab;
              break;
        }


        GameObject tileObject = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        tileObject.name = "Tile (" + i + ", " + j + ")";
        tileObject.transform.parent = this.transform;

        MapTile tile = tileObject.gameObject.GetComponent<MapTile>();
        tile.map = this;
        tile.type = type;
        tile.point = new MapPoint(i, j);
        tile.height = (float)tileHeightmap[i, j];
        tile.hue = (float)tileHuemap[i, j];

        tiles[i, j] = tile;

        if (type == MapTileType.Start) {
          startTile = tile;
        }
        else if (type == MapTileType.End) {
          endTile = tile;
        }

       }
    }
  }

  public MapTile RandomTile() {
    return tiles[Random.Range(0, size), Random.Range(0, size)];
  }

  public MapTile MapTileAtPoint(MapPoint point) {
    if (point.x < 0 || point.y < 0 || point.x >= size || point.y >= size)
      return null;
    return tiles[point.x, point.y];
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
      return 1;
  }

  public Bounds Bounds(){
      float absoluteWidth  = size  * TileSize();
      float absoluteHeight = size  * TileSize();
      return new Bounds(Center(), new Vector3(absoluteWidth, 1000, absoluteHeight));
  }

}
