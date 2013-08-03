using UnityEngine;
using System.Collections;

public class FoodTile : MapTile {

  public int startingFood = 1;
  public float foodEnergy = 1F;
  public float replenishTime = 2;

  public GameObject foodObjectPrefab;

  public int availableFood;

  public bool CanConsumeFood(Agent agent) {
    return availableFood > 0;
  }

  public bool ConsumeFood(Agent agent) {
    if (CanConsumeFood(agent)) {
      availableFood --;
      StatusDidChange();
      if (availableFood < 1) {
        OutOfFood();
      }
      return true;
    }
    return false;
  }

  bool respawning;

  public override void Start() {
    base.Start();
    respawning = false;
    availableFood = startingFood;
    StatusDidChange();
  }

  void StatusDidChange() {
    if (availableFood > 0) {
      SetColor(Color.green);
    }
    else {
      SetColor(Color.red);
    }
    DisplayFoodObjects();
  }

  void OutOfFood() {
    if (!respawning) {
      respawning = true;
      Invoke("ReplenishFood", replenishTime / (float)Agent.timeScaleFactor);
    }
  }

  void ReplenishFood() {
    availableFood = startingFood;
    StatusDidChange();
    respawning = false;
  }

  ArrayList _foodObjects = new ArrayList();
  void DisplayFoodObjects() {
    int spawnedFoods = _foodObjects.Count - availableFood;
    Vector3 scaleFactor = new Vector3(0.5F, 0.5F, 0.5F);
    if (spawnedFoods < 0) {
      for (int i = 0; i < Mathf.Abs(spawnedFoods) ; i++) {
        GameObject foodObject = Instantiate(foodObjectPrefab, CenterTop(), Quaternion.identity) as GameObject;
        _foodObjects.Add(foodObject);
        foodObject.transform.localScale = scaleFactor;
        foodObject.transform.parent = transform;
      }
    }
    foreach (GameObject obj in _foodObjects) {
      obj.renderer.enabled = false;
    }
    for (int i = 0; i < availableFood ; i++) {
      GameObject obj = _foodObjects[i] as GameObject;
      obj.transform.position = CenterTop() + new Vector3(0, scaleFactor.y / 2 + scaleFactor.y * i, 0);
      obj.renderer.enabled = true;
    }
  }

}
