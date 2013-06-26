using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Agent : MonoBehaviour {

  public float speed;
  public float freeWill;
  public float hunger;
  public float vision;
  public float startingEnergy;

  public float energy;


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
    hunger = Random.Range(0F, 1F);
    vision = Random.Range(0, 10);

    FinishCreating();
  }

  public void CreateFromParents(Agent[] parents) {

    speed = RandomParent(parents).speed;
    startingEnergy = RandomParent(parents).startingEnergy;
    freeWill = RandomParent(parents).freeWill;
    hunger = RandomParent(parents).hunger;
    vision = RandomParent(parents).vision;

    if (Random.value < 0.05) {
      speed = Random.Range(0.1F, 1F);
    }
    if (Random.value < 0.05) {
      startingEnergy = Random.Range(0.1F, 1F);
    }
    if (Random.value < 0.05) {
      freeWill = Random.Range(0F, 1F);
    }
    if (Random.value < 0.05) {
      hunger = Random.Range(0F, 1F);
    }
    if (Random.value < 0.05) {
      vision = Random.Range(0, 10);
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

    // Make a decision based on hunger
    MapTile[] foodspots = RejectLockedOutTilesFromTiles(currentTile.NeighboringTilesOfType(MapTileType.Food));
    if (foodspots.Length > 0 && Random.value < hunger) {
      MoveToTile(foodspots[0]);
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
    //energy -= 0.05F;

  }

  void MoveToTileComplete() {
    if (dead || finished)
      return;

    if (currentTile.type == MapTileType.Food) {
      EatFoodTile();
    }
    else if (currentTile.type == MapTileType.End) {
      Finish();
    }

    UpdateAI();
  }

  void EatFoodTile() {
    if (!IsLockedOutOfTile(currentTile)) {
      energy += 0.5F;
      if (energy > 1)
        energy = 1;
    }
    LockOutTile(currentTile);
  }

  MapTile[] RejectPreviousTiles(MapTile[] tiles) {
    ArrayList neighborList = new ArrayList();
    foreach (MapTile tile in tiles) {
        if (tile != previousTile)
            neighborList.Add(tile);
    }
    return neighborList.ToArray( typeof( MapTile ) ) as MapTile[];
  }

  ArrayList lockedOutTiles;
  MapTile[] RejectLockedOutTilesFromTiles(MapTile[] tiles) {
    if (lockedOutTiles == null)
      return tiles;
    ArrayList neighborList = new ArrayList();
    foreach (MapTile tile in tiles) {
        if (!IsLockedOutOfTile(tile))
            neighborList.Add(tile);
    }
    return neighborList.ToArray( typeof( MapTile ) ) as MapTile[];
  }

  bool IsLockedOutOfTile(MapTile tile) {
    if (lockedOutTiles == null)
      return false;
    return lockedOutTiles.IndexOf(tile) != -1;
  }

  void LockOutTile(MapTile tile) {
    if (lockedOutTiles == null) {
      lockedOutTiles = new ArrayList();
    }
    lockedOutTiles.Add(tile);
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
    float rate = 0.02F; // Lifespan
    rate += NormalizedSpeed() / 20; // Faster guys use more energy
    return rate;
  }

  void Update() {

    if (!dead && !finished) {

      // Reduce energy for lifespan
      energy -= Time.deltaTime * EnergyUsageRate();

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
    float fitness = DistanceToGoal();
    if (finished)
      fitness = - 1000;
    return fitness;
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
