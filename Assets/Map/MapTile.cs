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
  public float hue;

  public virtual void Start() {
    Setup();
  }

  public void Setup() {
    if (map != null)
      transform.position = Center();

    transform.localScale = new Vector3(1F, height * map.maxElevation, 1F);
    transform.localPosition = transform.localPosition + new Vector3(0, height * map.maxElevation / 2, 0);
    SetColorForHeight();
  }

  public Color TileColor() {
    return new HSBColor(hue, 0.5F, 0.5F + height / 2).ToColor();
  }

  void SetColorForHeight() {
    SetColor(TileColor());
  }

  public Vector3 Origin() {
    float tileSizeWithSqueeze = map.tileSize + map.tilePadding;
    return map.transform.position + new Vector3(point.x * tileSizeWithSqueeze, 0, point.y * tileSizeWithSqueeze);
  }

  public Vector3 Center() {
    return Origin() + new Vector3(Radius(), 0, Radius());
  }

  public Vector3 CenterTop() {
    return Center() + new Vector3(0, transform.localScale.y, 0);
  }

  public float Radius() {
    return map.tileSize / 2;
  }

  public Vector3 RandomTop() {
    return CenterTop() + new Vector3(Random.Range(-Radius(), Radius()), 0, Random.Range(-Radius(), Radius()));
  }

  public MapTile NeighboringTileInDirection(CardinalDirection direction) {
    return map.MapTileAtPoint(point.NearestMapPointInDirection(direction));
  }

  public MapTile[] NeighboringTilesClosestTo(MapTile otherTile) {
    List<MapTile> list = new List<MapTile>();
    list.AddRange(NeighboringTiles());
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

  public MapTile[] PassableNeighboringTilesForAgent(Agent agent) {
    ArrayList neighborList = new ArrayList();
    foreach (MapTile tile in NeighboringTiles()) {
        if (tile != null && PassableFromTileForAgent(tile, agent))
            neighborList.Add(tile);
    }
    return neighborList.ToArray( typeof( MapTile ) ) as MapTile[];
  }

  public MapTile[] PassableNeighboringTilesOfTypeForAgent(MapTileType type, Agent agent) {
    ArrayList neighborList = new ArrayList();
    foreach (MapTile tile in PassableNeighboringTilesForAgent(agent)) {
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
    if (here == null)
        here = new ArrayList();
    foreach (Agent otherAgent in here) {
        if (otherAgent != excludedAgent)
            others.Add(otherAgent);
    }
    return others.ToArray( typeof( Agent ) ) as Agent[];
  }

  // Passability

  public bool PassableFromTileForAgent(MapTile destinationTile, Agent agent) {
    float heightDifference = Mathf.Abs(height - destinationTile.height);
    return heightDifference < 0.25;
  }

  public void DrawSelectedGizmo() {
    Gizmos.DrawWireCube (transform.position, transform.localScale);
  }

  void OnDrawGizmosSelected() {
    if (map != null) {
      Gizmos.color = Color.red;
      DrawSelectedGizmo();
      Gizmos.color = Color.green;
      foreach (MapTile passableNeighbor in PassableNeighboringTilesOfTypeForAgent(MapTileType.Food, null)) {
        passableNeighbor.DrawSelectedGizmo();
      }      
    }
  }

  // Appearance

  Material recoloredMaterial;
  public void SetColor(Color color) {
      if (recoloredMaterial == null) {
        recoloredMaterial = new Material(renderer.material);
        renderer.material = recoloredMaterial;
      }
    recoloredMaterial.color = color;
  }

}
