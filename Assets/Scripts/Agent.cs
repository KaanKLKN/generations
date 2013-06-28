using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Agent : MonoBehaviour {

  public float speed;
  public float strength;
  public float hunger;
  public float vision;
  public float startingEnergy;
  public float lifespan;
  public float fertility;
  public float hue;
  public int timesReproduced;

  public float energy;


  public MapTile currentTile;
  public MapTile previousTile;

  public AgentManager manager;

  public bool dead;
  public bool finished;
  public float birthTime;
  public float deathTime;

  public static float timeScaleFactor = 1;

  private static float mutationChance = 0.05F; // 0-1

  private static float _minLifespan = 25; // seconds
  private static float _maxLifespan = 35; // seconds

  private static float _minEnergy = 0.8F;
  private static float _maxEnergy = 1.0F;

  private static float _minSpeed = 0.5F;
  private static float _maxSpeed = 4.0F;

  private static float _minStrength = 0F;
  private static float _maxStrength = 1F;

  public ReproductiveSystem reproductiveSystem = new ReproductiveSystem();
  public int timesEaten = 0;

  public Organ[] Organs() {
    return new Organ[] {reproductiveSystem};
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
    //foreach (var entry in reproductiveSystem.InheritableTraitValues())
        //Debug.Log(entry.Key + " : " + entry.Value); 

    speed = Random.Range(_minSpeed, _maxSpeed);
    startingEnergy = Random.Range(_minEnergy, _maxEnergy);
    lifespan = Random.Range(_minLifespan, _maxLifespan);
    strength = Random.Range(_minStrength, _maxStrength);

    hue = Random.Range(0F, 1F);
    hunger = Random.Range(0F, 1F);
    vision = Random.Range(0, 10);

    FinishCreating();
  }

  public void CreateFromParents(Agent[] parents) {

    Agent mom = parents[0];
    Agent dad = parents[1];

    reproductiveSystem.InheritTraitsFromParents(mom.reproductiveSystem, dad.reproductiveSystem);

    hue = Random.Range(parents[0].hue, parents[1].hue);
    //fertility = Random.Range(parents[0].fertility, parents[1].fertility);

    speed = RandomParent(parents).speed;
    startingEnergy = RandomParent(parents).startingEnergy;
    strength = RandomParent(parents).strength;
    hunger = RandomParent(parents).hunger;
    vision = RandomParent(parents).vision;
    lifespan = RandomParent(parents).lifespan;

    if (Random.value < mutationChance) {
      speed = Random.Range(_minSpeed, _maxSpeed);
    }
    if (Random.value < mutationChance) {
      startingEnergy = Random.Range(_minEnergy, _maxEnergy);
    }
    if (Random.value < mutationChance) {
      lifespan = Random.Range(_minLifespan, _maxLifespan);
    }
    if (Random.value < mutationChance) {
      strength = Random.Range(_minStrength, _maxStrength);
    }
    if (Random.value < mutationChance) {
      hue = Random.Range(0F, 1F);
    }
    if (Random.value < mutationChance) {
      hunger = Random.Range(0F, 1F);
    }
    if (Random.value < mutationChance) {
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
    SetColor(new HSBColor(hue, 1, 1).ToColor());
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
      if (foodspots.Length > 0 && Random.value < hunger) {
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
    currentColor.s = energy;// / startingEnergy;
    SetColor(currentColor.ToColor());
  }

  float EnergyUsageRate() {
    float rate = 0.01F * timeScaleFactor; // Lifespan
    //rate += speed / 100; // Faster guys use more energy
    return rate;
  }

  float LifeRemaining() {
    float deathDate = birthTime + (lifespan / timeScaleFactor);
    return (deathDate - Time.time);
  }

  float LifeRemainingPercent() {
    return LifeRemaining() / (lifespan / timeScaleFactor);
  }

  float TimeToDieOfHunger() {
    return (lifespan / timeScaleFactor) * 0.5F;
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
    return speed * timeScaleFactor;
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
        if (agent.strength < strength)
            fertile.Add(agent);
    }
    return fertile.ToArray( typeof( Agent ) ) as Agent[];
  }

}
