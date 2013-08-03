using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum AIDecisionType {
  ConsumeTurn,
  ShareTurn,
  NoDecision
}

// Call this game Mammaljam

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

  public Body body = new Body();
  public ReproductiveSystem reproductiveSystem = new ReproductiveSystem();
  public HungerCenter hungerCenter = new HungerCenter();

  public Organ[] Organs() {
    return new Organ[] {reproductiveSystem, body, hungerCenter};
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
    hungerCenter.InheritTraitsFromParents(mom.hungerCenter, dad.hungerCenter, mutationChance);

    FinishCreating();
  }

  void FinishCreating() {
    dead = false;
    finished = false;
    energy = body.MaxEnergy();
    SetColor(new HSBColor(body.hue.floatValue, 1, 1).ToColor());
    transform.position = currentTile.CenterTop();
    transform.localScale = transform.localScale + new Vector3(0, Random.Range(-0.1F, 0.1F), 0);
    birthTime = Time.time;
    TriggerLifeEvent("Born");
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
    if (manager.showNotifications)
      GetComponent<AgentNotifier>().Notify(type);
  }

  public System.String lastEventName;
  public void TriggerLifeEvent(System.String eventName) {
    manager.IncrementCounter(eventName, 1);
    lastEventName = eventName;
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
    if (hungerCenter.MakeHungerDecision() == AIDecisionType.ConsumeTurn) {
      return;
    }

    if (Random.value < 0.5) {
      reproductiveSystem.ReproduceIfPossible();
    }

    MapTile[] rejectedNearby = RejectPreviousTiles(nearby);
    if (rejectedNearby.Length > 0) {
      MapTile randomTile = rejectedNearby[Random.Range(0, rejectedNearby.Length)];
      MoveToTile(randomTile);      
    }

  }

  CardinalDirection CurrentDirection() {
    if (previousTile == null)
      return CardinalDirection.North;
    return previousTile.point.DirectionToMapPoint(currentTile.point);
  }

  void MoveToTileComplete() {
    if (dead || finished)
      return;
    UpdateAI();
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

  public void MoveToTile(MapTile tile) {

    previousTile = currentTile;
    if (previousTile != null)
      previousTile.AgentDidExit(this);

    currentTile = tile;
    currentTile.AgentDidEnter(this);

    iTween.MoveTo(gameObject, iTween.Hash("position", tile.RandomTop(), "speed", NormalizedSpeed(), "easetype", "linear", "oncomplete", "MoveToTileComplete", "orienttopath", false));
  }

  void SetColorToHealth() {
    HSBColor currentColor = new HSBColor(recoloredMaterial.color);
    currentColor.s = energy;
    SetColor(currentColor.ToColor());
  }

  void Update() {

    if (!dead && !finished) {

      // Reduce energy for lifespan
      energy -= Time.deltaTime * body.EnergyDrainPerSecond();

      //SetColorToHealth();

      if (manager == null)
        return;

      if (energy <= 0F) {
        Starve();
      }
      else if (Age() >= body.Lifespan()) {
        DieOfOldAge();
      }
      else {
        reproductiveSystem.Update();
      }

    }

  }

  public float fitness;

  void Starve() {
    Notify(AgentNotificationType.Death);
    TriggerLifeEvent("Died");
    TriggerLifeEvent("Died of Starvation");
    Die();
  }

  void DieOfOldAge() {
    Notify(AgentNotificationType.Death);
    TriggerLifeEvent("Died");
    TriggerLifeEvent("Died of Old Age");
    Die();
  }

  public void BeMurdered() {
    Notify(AgentNotificationType.Murder);
    TriggerLifeEvent("Died");
    TriggerLifeEvent("Killed and Eaten");
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

  public float Age() {
    if (dead) {
      return Lifetime();
    }
    return Time.time - birthTime;
  }

  public float DistanceToGoal() {
    return currentTile.point.DistanceFromMapPoint(manager.map.endTile.point);
  }

  float NormalizedSpeed() {
    return body.Speed() * timeScaleFactor;
  }

}
