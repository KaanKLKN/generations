using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Agent : MonoBehaviour {

  public int timesReproduced;

  public float energy;

  [HideInInspector]
  public MapTile currentTile;
  [HideInInspector]
  public MapTile previousTile;

  [HideInInspector]
  public AgentManager manager;

  public bool dead;
  public bool finished;
  public float birthTime;
  public float deathTime;

  public static float timeScaleFactor = 1;

  private static float mutationChance = 0.05F; // 0-1

  public ReproductiveSystem reproductiveSystem = new ReproductiveSystem();
  public Body body = new Body();
  public int timesEaten = 0;

  public Organ[] Organs() {
    return new Organ[] {reproductiveSystem, body};
  }

  public Dictionary<string, Trait> Traits() {
    Dictionary<string, Trait> traitValues = new Dictionary<string, Trait>();
    foreach (Organ organ in Organs()) {
      foreach (var entry in organ.TraitValues()) {
        traitValues[entry.Key] = entry.Value as Trait;
      }
    }
    return traitValues;
  }

  public void InitializeAgent() {
    foreach (Organ organ in Organs()) {
      organ.agent = this;
    }
  }

  public void CreateRandom() {
    foreach (Organ organ in Organs()) {
      organ.RandomizeTraits();
    }
    FinishCreating();
  }

  public void CreateFromParents(Agent[] parents) {

    Agent mom = parents[0];
    Agent dad = parents[1];

    body.InheritTraitsFromParents(mom.body, dad.body, mutationChance);
    reproductiveSystem.InheritTraitsFromParents(mom.reproductiveSystem, dad.reproductiveSystem, mutationChance);

    FinishCreating();
  }

  void FinishCreating() {
    dead = false;
    finished = false;
    energy = body.startingEnergy.floatValue;
    SetColor(new HSBColor(body.hue.floatValue, 1, 1).ToColor());
    transform.position = currentTile.CenterTop();
    transform.localScale = transform.localScale + new Vector3(0, Random.Range(-0.1F, 0.1F), 0);
    birthTime = Time.time;
    manager.IncrementCounter("Born", 1);
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

  public void Notify(AgentNotificationType type) {
    GetComponent<AgentNotifier>().Notify(type);
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
      if (foodspots.Length > 0 && Random.value < body.hunger.floatValue) {
        FoodTile foodTile = foodspots[0] as FoodTile;
        if (foodTile.CanConsumeFood(this)) {
          MoveToTile(foodspots[0]);
          return;
        }
      }
    }

    MapTile[] rejectedNearby = RejectPreviousTiles(nearby);
    MapTile randomTile = rejectedNearby[Random.Range(0, rejectedNearby.Length)];
    MoveToTile(randomTile);

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
        manager.IncrementCounter("Ate", 1);
        Notify(AgentNotificationType.Ate);
        timesEaten++;
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

    if (Random.value < 0.5) {
      reproductiveSystem.ReproduceIfPossible();
    } else {
      EatAgentIfPossible();
    }
  }

  void SetColorToHealth() {
    HSBColor currentColor = new HSBColor(recoloredMaterial.color);
    currentColor.s = energy;
    SetColor(currentColor.ToColor());
  }

  float EnergyUsageRate() {
    float rate = 0.01F * timeScaleFactor; // Lifespan
    //rate += speed / 100; // Faster guys use more energy
    return rate;
  }

  float LifeRemaining() {
    float deathDate = birthTime + (body.lifespan.floatValue / timeScaleFactor);
    return (deathDate - Time.time);
  }

  float LifeRemainingPercent() {
    return LifeRemaining() / (body.lifespan.floatValue / timeScaleFactor);
  }

  float TimeToDieOfHunger() {
    return (body.lifespan.floatValue / timeScaleFactor) * 0.5F;
  }

  void Update() {

    if (!dead && !finished) {

      // Reduce energy for lifespan
      energy -= Time.deltaTime * (1 / TimeToDieOfHunger());

      //SetColorToHealth();

      if (manager ==null)
        return;

      if (energy <= 0F) {
        Starve();
      }
      else if (LifeRemaining() <= 0F) {
        DieOfOldAge();
      }
    }

  }

  public float fitness;

  void Starve() {
    Notify(AgentNotificationType.Death);
    manager.IncrementCounter("Died", 1);
    manager.IncrementCounter("Died of Starvation", 1);
    Die();
  }

  void DieOfOldAge() {
    Notify(AgentNotificationType.Death);
    manager.IncrementCounter("Died", 1);
    manager.IncrementCounter("Died of Old Age", 1);
    Die();
  }

  void BeMurdered() {
    Notify(AgentNotificationType.Murder);
    manager.IncrementCounter("Died", 1);
    manager.IncrementCounter("Eaten", 1);
    Die();
  }

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
    return body.speed.floatValue * timeScaleFactor;
  }

  // Carnivore

  public void EatAgentIfPossible() {
    Agent[] otherAgents = currentTile.AgentsHereExcluding(this);
    if (otherAgents.Length > 0) {
      Agent[] weakerAgents = SelectWeakerAgents(otherAgents);
      if (weakerAgents.Length > 0) {
        EatAgent(weakerAgents[Random.Range(0, weakerAgents.Length)]);          
      }
    }
  }

  public void EatAgent(Agent prey) {

    energy += prey.energy;
    if (energy > 1)
      energy = 1;

    manager.IncrementCounter("Ate", 1);

    timesEaten++;

    prey.BeMurdered();

  }

  public Agent[] SelectWeakerAgents(Agent[] agents) {
    ArrayList fertile = new ArrayList();
    foreach (Agent agent in agents) {
        if (agent.body.strength.floatValue < body.strength.floatValue)
            fertile.Add(agent);
    }
    return fertile.ToArray( typeof( Agent ) ) as Agent[];
  }

}
