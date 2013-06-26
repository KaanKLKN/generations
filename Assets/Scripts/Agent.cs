using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Agent : MonoBehaviour {

  public float speed;
  public float freeWill;
  public float energy;
  public float startingEnergy;

  public MapTile currentTile;
  public MapTile previousTile;

  public AgentManager manager;

  public bool dead;
  public bool finished;
  public float birthTime;
  public float deathTime;

  public void CreateRandom() {
    speed = Random.Range(0.1F, 1F);
    startingEnergy = Random.Range(0.5F, 1F);
    freeWill = Random.Range(0F, 1F);

    FinishCreating();
  }

  public void CreateFromParents(Agent[] parents) {

    speed = RandomParent(parents).speed;
    startingEnergy = RandomParent(parents).startingEnergy;
    freeWill = RandomParent(parents).freeWill;

    if (Random.value < 0.05) {
      speed = Random.Range(0.1F, 1F);
    }
    if (Random.value < 0.05) {
      startingEnergy = Random.Range(0.1F, 1F);
    }
    if (Random.value < 0.05) {
      freeWill = Random.Range(0F, 1F);
    }

    FinishCreating();
  }

  Agent RandomParent(Agent[] parents) {
    return parents[Random.Range(0, parents.Length)];
  }

  void FinishCreating() {
    dead = false;
    finished = false;
    energy = startingEnergy;
    SetColor(renderer.material.color);
    transform.position = currentTile.CenterTop();
    transform.localScale = transform.localScale + new Vector3(0, Random.Range(-0.1F, 0.1F), 0);
    birthTime = Time.time;
    UpdateAI();
  }
    
  Material recoloredMaterial;
  public void SetColor(Color color) {
      if (recoloredMaterial == null) {
        recoloredMaterial = new Material(renderer.material);
        renderer.material = recoloredMaterial;
      }
    recoloredMaterial.color = color;
  }
    
  void UpdateAI() {
    if (dead || finished)
      return;

    // Move to a random available tile

    MapTile[] nearby = currentTile.PassableNeighboringTiles();
    if (nearby.Length == 1) {
      MoveToTile(nearby[0]);
      return;
    }

    MapTile[] exits = currentTile.NeighboringTilesOfType(MapTileType.End);
    if (exits.Length > 0) {
      MoveToTile(exits[0]);
      return;
    }

    if (Random.value < freeWill) {
      // Make a crazy decision
      MapTile[] rejectedNearby = RejectPreviousTiles(nearby);
      MapTile randomTile = rejectedNearby[Random.Range(0, rejectedNearby.Length)];
      MoveToTile(randomTile);
    }
    else {
      // Fit in
      MapTile[] inDirection = RejectPreviousTiles(currentTile.NeighboringTilesClosestTo(manager.map.endTile));
      MoveToTile(inDirection[0]);
    }

    // Decision cost
    energy -= 0.05F;

  }

  void MoveToTileComplete() {
    if (dead || finished)
      return;

    if (currentTile.type == MapTileType.Food) {
      energy += 0.5F;
      if (energy > 1)
        energy = 1;
    }
    else if (currentTile.type == MapTileType.End) {
      Finish();
    }

    UpdateAI();
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
    iTween.MoveTo(gameObject, iTween.Hash("position", tile.RandomTop(), "speed", NormalizedSpeed(), "easetype", "linear", "oncomplete", "MoveToTileComplete", "orienttopath", false));
  }

  void SetColorToHealth() {
    HSBColor currentColor = new HSBColor(recoloredMaterial.color);
    currentColor.s = energy;// / startingEnergy;
    SetColor(currentColor.ToColor());
  }

  float EnergyUsageRate() {
    float rate = 0.01F; // Lifespan
    rate += NormalizedSpeed() / 20; // Faster guys use more energy
    return rate;
  }

  void Update() {

    if (!dead && !finished) {

      // Reduce energy for lifespan
      energy -= Time.deltaTime * 0.02F;

      SetColorToHealth();

      if (energy <= 0F) {
        Die();
      }
    }

  }

  public float fitness;

  void Die() {
    deathTime = Time.time;
    dead = true;
    iTween.Stop(gameObject);
    SetColor(Color.black);
    fitness = Fitness();
    manager.OnAgentDeath(this);
  }

  void Finish() {
    finished = true;
    iTween.Stop(gameObject);
    fitness = Fitness();
    SetColor(Color.magenta);
    manager.OnAgentFinish(this);
  }

  public float Fitness() {
    return DistanceToGoal();
  }

  public float Lifetime() {
    return deathTime - birthTime;
  }

  public float DistanceToGoal() {
    return currentTile.point.DistanceFromMapPoint(manager.map.endTile.point);
  }

  float NormalizedSpeed() {
    return speed * 16;
  }

}
