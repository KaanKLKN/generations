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
  public float lifespan;

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
    lifespan = Random.Range(1F, 10F);
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
    lifespan = RandomParent(parents).lifespan;

    if (Random.value < 0.05) {
      speed = Random.Range(0.1F, 1F);
    }
    if (Random.value < 0.05) {
      startingEnergy = Random.Range(0.1F, 1F);
    }
    if (Random.value < 0.05) {
      lifespan = Random.Range(1F, 10F);
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

    MapTile[] nearby = currentTile.PassableNeighboringTilesForAgent(this);
    if (nearby.Length == 1) {
      MoveToTile(nearby[0]);
      return;
    }

    // Make a decision based on hunger
    if (IsHungry()) {
      MapTile[] foodspots = currentTile.PassableNeighboringTilesOfTypeForAgent(MapTileType.Food, this);
      if (foodspots.Length > 0 && Random.value < hunger) {
        FoodTile foodTile = foodspots[0] as FoodTile;
        if (foodTile.CanConsumeFood(this)) {
          MoveToTile(foodspots[0]);
          return;
        }
      }
    }

    if (Random.value < freeWill) {
      // Make a crazy decision
      MapTile[] rejectedNearby = RejectPreviousTiles(nearby);
      MapTile randomTile = rejectedNearby[Random.Range(0, rejectedNearby.Length)];
      MoveToTile(randomTile);
    }
    else {
      // Fit in
      //MapTile[] inDirection = RejectPreviousTiles(currentTile.NeighboringTilesClosestTo(manager.map.endTile));
      //MapTile forwardTile = currentTile.NeighboringTileInDirection(CurrentDirection());
      //if (forwardTile != null)
       // MoveToTile(forwardTile);
      //else
        MoveToTile(nearby[0]);
    }

    // Decision cost
    //energy -= 0.05F;

  }

  CardinalDirection CurrentDirection() {
    if (previousTile == null)
      return CardinalDirection.North;
    return previousTile.point.DirectionToMapPoint(currentTile.point);
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

  bool IsHungry() {
    return energy < 0.75;
  }

  void EatFoodTile() {
    FoodTile food = currentTile as FoodTile;
    if (IsHungry()) {
      if (food.ConsumeFood(this)) {
        energy += food.foodEnergy;
        if (energy > 1)
          energy = 1;
      }
    }
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
    if (previousTile != null)
      previousTile.AgentDidExit(this);

    currentTile = tile;
    currentTile.AgentDidEnter(this);

    iTween.MoveTo(gameObject, iTween.Hash("position", tile.RandomTop(), "speed", NormalizedSpeed(), "easetype", "linear", "oncomplete", "MoveToTileComplete", "orienttopath", false));

    ReproduceIfPossible();
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

  float LifeRemaining() {
    float deathDate = birthTime + lifespan;
    return deathDate - Time.time;
  }

  float LifeRemainingPercent() {
    return LifeRemaining() / lifespan;
  }

  void Update() {

    if (!dead && !finished) {

      // Reduce energy for lifespan
      energy -= Time.deltaTime * EnergyUsageRate();

      SetColorToHealth();

      if (energy <= 0F) {
        Die();
      }
      else if (LifeRemaining() <= 0F) {
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
    currentTile.AgentDidExit(this);
    manager.OnAgentDeath(this);
    StartCoroutine(EraseBodySoon());
  }

  void Finish() {
    finished = true;
    iTween.Stop(gameObject);
    fitness = Fitness();
    SetColor(Color.magenta);
    currentTile.AgentDidExit(this);
    manager.OnAgentFinish(this);
    StartCoroutine(EraseBodySoon());
  }

  IEnumerator EraseBodySoon() {
    yield return new WaitForSeconds(2);
    //EraseBody();
    Vector3 newPos = transform.position + new Vector3(0, -2, 0);
    iTween.MoveTo(gameObject, iTween.Hash("position", newPos, "duration", 1, "easetype", "linear", "oncomplete", "EraseBody"));
  }

  void EraseBody() {
    renderer.enabled = false;
  }

  public float Fitness() {
    return 1 / Lifetime();
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

  // Reproduction

  float reproductionCost = 0.25F;

  public bool CanReproduce() {
    if (!manager.PopulationCeilingExceeded() && energy > reproductionCost && LifeRemainingPercent() < 0.8) {
      Agent[] otherAgents = currentTile.AgentsHereExcluding(this);
      if (otherAgents.Length > 0) {
        return true;
      }
    }
    return false;
  }

  public void ReproduceIfPossible() {
    if (CanReproduce()) {
      Agent[] otherAgents = currentTile.AgentsHereExcluding(this);
      ReproduceWith(otherAgents[0]);
    }
  }

  public Agent ReproduceWith(Agent otherParent) {

    energy -= reproductionCost;
    otherParent.energy -= reproductionCost;

    Agent child = manager.BirthAgent();

    child.currentTile = currentTile;

    Agent[] parents = new Agent[2];
    parents[0] = this;
    parents[1] = otherParent;
    child.CreateFromParents(parents);

    return child;
  }

}
