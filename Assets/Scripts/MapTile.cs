using UnityEngine;
using System.Collections;

public enum MapTileType {
  Start,
  End,
  Free,
  Blocked
}

public class MapTile : MonoBehaviour {

  public MapTileType type;
  public MapPoint point;
  public Map map;

  public Vector3 Origin() {
    return new Vector3(point.x * map.TileSize(), 0, point.y * map.TileSize());
  }

  public Vector3 Center() {
    return Origin() + new Vector3(map.TileSize() / 2, 0, map.TileSize() / 2);
  }

}
