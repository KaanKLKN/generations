using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HungerCenter : Organ {

  public NumericalTrait hunger  = new NumericalTrait(0, 1); 
  public BooleanTrait carnivore = new BooleanTrait(); 
  public BooleanTrait herbivore = new BooleanTrait(); 

  public int timesEaten = 0;

  // Generic Methods

  bool IsHungry() {
    return agent.energy < 0.75;
  }

  public void EatFoodAmount(float energyAmount) {
    agent.energy += energyAmount;
    if (agent.energy > 1)
      agent.energy = 1;
    agent.manager.IncrementCounter("Ate", 1);
    agent.Notify(AgentNotificationType.Ate);
    timesEaten++;
  }

  // AI Hooks

  // Return true to prevent any other action by the AI this turn.
  public AIDecisionType MakeHungerDecision() {
    if (IsHungry()) {
      if (herbivore.boolValue && agent.currentTile.type == MapTileType.Food) {
        EatFoodTile();
        return AIDecisionType.ShareTurn;
      }
      else if (herbivore.boolValue && HeadForFoodTile()) {
        return AIDecisionType.ConsumeTurn;
      }
      else if (carnivore.boolValue && EatAgentIfPossible()) {
        return AIDecisionType.ShareTurn;
      }
    }
    return AIDecisionType.NoDecision;
  }

  // Food Tiles / Herbivores

  bool HeadForFoodTile() {
    FoodTile[] foodTiles = agent.currentTile.PassableNeighboringTilesOfTypeForAgent(MapTileType.Food, agent) as FoodTile[];
    if (foodTiles != null && foodTiles.Length > 0) {
      FoodTile foodTile = foodTiles[Random.Range(0, foodTiles.Length)] as FoodTile;
      if (foodTile.CanConsumeFood(agent)) {
        agent.MoveToTile(foodTile);
        return true;
      }
    }
    return false;
  }

  public void EatFoodTile() {
    FoodTile foodTile = agent.currentTile as FoodTile;
    if (IsHungry() && foodTile.ConsumeFood(agent)) {
      EatFoodAmount(foodTile.foodEnergy);
      agent.manager.IncrementCounter("Ate Hay", 1);
    }
  }

  // Carnivore

  public bool EatAgentIfPossible() {
    Agent[] otherAgents = agent.currentTile.AgentsHereExcluding(agent);
    if (otherAgents.Length > 0) {
      Agent[] weakerAgents = SelectWeakerAgents(otherAgents);
      if (weakerAgents.Length > 0) {
        if (Random.value < agent.body.CamouflageFactor()) {
          EatAgent(weakerAgents[Random.Range(0, weakerAgents.Length)]);
          return true;          
        }
      }
    }
    return false;
  }

  public void EatAgent(Agent prey) {
    EatFoodAmount(prey.energy);
    agent.manager.IncrementCounter("Ate Meat", 1);
    prey.BeMurdered();
  }

  public Agent[] SelectWeakerAgents(Agent[] agentList) {
    ArrayList weaker = new ArrayList();
    foreach (Agent testAgent in agentList) {
        if (testAgent.body.Strength() < agent.body.Strength())
            weaker.Add(testAgent);
    }
    return weaker.ToArray( typeof( Agent ) ) as Agent[];
  }

}
