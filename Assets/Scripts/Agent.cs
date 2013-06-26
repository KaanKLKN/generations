using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour {

  public float speed;
  public float energy;

  public MapTile currentTile;
  public MapTile previousTile;

	// Use this for initialization
	void Start () {
    SetupProperties();
    transform.position = currentTile.CenterTop();
    UpdateAI();
	}

  void SetupProperties() {
    speed = Random.Range(0F, 1F);
    energy = Random.Range(0F, 1F);
  }

  void UpdateAI() {
    // Move to a random available tile
    MapTile[] nearby = currentTile.NeighboringTilesOfType(MapTileType.Free);
    if (nearby.Length == 1) {
      MoveToTile(nearby[0]);
    }
    else if (nearby.Length > 1) {
      MapTile[] rejectedNearby = RejectPreviousTiles(nearby);
      MapTile randomTile = rejectedNearby[Random.Range(0, rejectedNearby.Length)];
      MoveToTile(randomTile);
    }
  }

  public MapTile[] RejectPreviousTiles(MapTile[] tiles) {
    ArrayList neighborList = new ArrayList();
    foreach (MapTile tile in tiles) {
        if (tile != previousTile)
            neighborList.Add(tile);
    }
    return neighborList.ToArray( typeof( MapTile ) ) as MapTile[];
  }


  void MoveToTile(MapTile tile) {
    previousTile = currentTile;
    currentTile = tile;
    iTween.MoveTo(gameObject, iTween.Hash("position", tile.RandomTop(), "speed", NormalizedSpeed(), "easetype", "linear", "oncomplete", "MoveToTileComplete"));
  }

  float NormalizedSpeed() {
    return speed * 2;
  }

  void MoveToTileComplete() {
    UpdateAI();
  }

}
