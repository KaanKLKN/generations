using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum MapTileType {
  Start,
  End,
  Free,
  Food,
  Blocked
}

public enum CardinalDirection {
  North,
  South,
  East,
  West,
  NorthEast,
  NorthWest,
  SouthEast,
  SouthWest
}

public class MapTile : MonoBehaviour {

  public MapTileType type;
  public MapPoint point;
  public Map map;
  public float height;

  public virtual void Start() {
    if (map != null)
      transform.position = Center();

    transform.localScale = new Vector3(1F, height * 3, 1F);
  }

  public Vector3 Origin() {
    return map.transform.position + new Vector3(point.x * map.TileSize(), 0, point.y * map.TileSize());
  }

  public Vector3 Center() {
    return Origin() + new Vector3(Radius(), 0, Radius());
  }

  public Vector3 CenterTop() {
    float height = ((BoxCollider)(collider)).size.y * transform.localScale.y;
    return Center() + new Vector3(0, height / 2, 0);
  }

  public float Radius() {
    return map.TileSize() / 2;
  }

  public Vector3 RandomTop() {
    return CenterTop() + new Vector3(Random.Range(-Radius(), Radius()), 0, Random.Range(-Radius(), Radius()));
  }

  public MapTile NeighboringTileInDirection(CardinalDirection direction) {
    return map.MapTileAtPoint(point.NearestMapPointInDirection(direction));
  }

  public MapTile[] NeighboringTilesClosestTo(MapTile otherTile) {
    List<MapTile> list = new List<MapTile>();
    list.AddRange(PassableNeighboringTiles());
    return list.OrderBy(a => a.point.DistanceFromMapPoint(otherTile.point)).ToArray();
  }

  public MapTile[] NeighboringTiles() {
    ArrayList neighborList = new ArrayList();
    foreach (CardinalDirection direction in new ArrayList{
        CardinalDirection.North, 
        CardinalDirection.South, 
        CardinalDirection.West, 
        CardinalDirection.East,
        CardinalDirection.NorthWest, 
        CardinalDirection.SouthWest, 
        CardinalDirection.SouthEast, 
        CardinalDirection.NorthEast
      }) {
      MapTile tile = NeighboringTileInDirection(direction);
      if (tile)
        neighborList.Add(tile);
    }
    return neighborList.ToArray( typeof( MapTile ) ) as MapTile[];
  }

  public MapTile[] PassableNeighboringTiles() {
    ArrayList neighborList = new ArrayList();
    foreach (MapTile tile in NeighboringTiles()) {
        if (tile.type != MapTileType.Blocked)
            neighborList.Add(tile);
    }
    return neighborList.ToArray( typeof( MapTile ) ) as MapTile[];
  }

  public MapTile[] NeighboringTilesOfType(MapTileType type) {
    ArrayList neighborList = new ArrayList();
    foreach (MapTile tile in NeighboringTiles()) {
        if (tile.type == type)
            neighborList.Add(tile);
    }
    return neighborList.ToArray( typeof( MapTile ) ) as MapTile[];
  }

  // Presence

  ArrayList here;

  public void AgentDidEnter(Agent agent) {
    if (here == null)
        here = new ArrayList();
    here.Add(agent);
  }

  public void AgentDidExit(Agent agent) {
    if (here == null)
        here = new ArrayList();
    here.Remove(agent);
  }

  public Agent[] AgentsHereExcluding(Agent excludedAgent) {
    ArrayList others = new ArrayList();
    foreach (Agent otherAgent in here) {
        if (otherAgent != excludedAgent)
            others.Add(otherAgent);
    }
    return others.ToArray( typeof( Agent ) ) as Agent[];
  }

}
