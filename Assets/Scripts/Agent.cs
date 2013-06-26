using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour {

  public float speed;
  public float energy;
  public float startingEnergy;

  public MapTile currentTile;
  public MapTile previousTile;

  public AgentManager manager;

  public bool dead;
  public float birthTime;
  public float deathTime;

  public void CreateRandom() {
    speed = Random.Range(0.1F, 1F);
    energy = Random.Range(0.1F, 1F);
    dead = false;

    FinishCreating();
  }

  public void CreateFromParents(Agent[] parents) {

    speed = RandomParent(parents).speed;
    energy = RandomParent(parents).startingEnergy;

    if (Random.value < 0.05) {
      speed = Random.Range(0.1F, 1F);
    }
    if (Random.value < 0.05) {
      energy = Random.Range(0.1F, 1F);
    }

    FinishCreating();
  }

  Agent RandomParent(Agent[] parents) {
    return parents[Random.Range(0, parents.Length)];
  }

  void FinishCreating() {
    startingEnergy = energy;
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
    if (dead)
      return;

    // Move to a random available tile
    MapTile[] nearby = currentTile.PassableNeighboringTiles();
    if (nearby.Length == 1) {
      MoveToTile(nearby[0]);
    }
    else if (nearby.Length > 1) {
      MapTile[] rejectedNearby = RejectPreviousTiles(nearby);
      MapTile randomTile = rejectedNearby[Random.Range(0, rejectedNearby.Length)];
      MoveToTile(randomTile);
    }
  }

  void MoveToTileComplete() {
    if (dead)
      return;

    if (currentTile.type == MapTileType.Food) {
      energy += 0.5F;
      if (energy > 1)
        energy = 1;
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
    rate += NormalizedSpeed() / 10; // Faster guys use more energy
    return rate;
  }

  void Update() {

    if (!dead) {
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
