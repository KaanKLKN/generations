using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ReproductiveSystem : Organ {

  public NumericalTrait fertility = new NumericalTrait(0F, 1F); 

  int timesReproduced = 0;
  static int _maximumChildren = 10;

  // Reproduction

  static float reproductionCost = 0.1F;
  static float reproductionThreshold = 0.5F;
  static int maxChildren = 4;

  public bool CanReproduce() {
    if (!agent.manager.PopulationCeilingExceeded()
        && agent.timesEaten > 1 
        && agent.energy > reproductionThreshold 
        && timesReproduced < maxChildren) {
      return true;
    }
    return false;
  }

  public void ReproduceIfPossible() {
    if (CanReproduce()) {
      // Find other agents
      Agent[] otherAgents = agent.currentTile.AgentsHereExcluding(agent);
      if (otherAgents.Length > 0) {
        Agent[] fertileAgents = SelectFertileAgents(otherAgents);
        if (fertileAgents.Length > 0) {
          ReproduceWith(fertileAgents[Random.Range(0, fertileAgents.Length)]);          
        }
      }
    }
  }

  public Agent[] SelectFertileAgents(Agent[] agents) {
    ArrayList fertile = new ArrayList();
    foreach (Agent a in agents) {
        if (a.reproductiveSystem.CanReproduce())
            fertile.Add(a);
    }
    return fertile.ToArray( typeof( Agent ) ) as Agent[];
  }

  public Agent[] ReproduceWith(Agent otherParent) {

    if (Random.value <= fertility.floatValue 
        && Random.value <= otherParent.reproductiveSystem.fertility.floatValue) {
      return new Agent[0];
    }

    int kidsToHave = Mathf.RoundToInt(
        Random.Range(_maximumChildren * fertility.floatValue,
                     _maximumChildren * otherParent.reproductiveSystem.fertility.floatValue)
        );

    Agent[] children = new Agent[kidsToHave];

    for (int i=0; i < kidsToHave; i++) {
      if (CanReproduce() && otherParent.reproductiveSystem.CanReproduce()) {
        Agent child = agent.manager.BirthAgent();

        agent.energy -= reproductionCost;
        timesReproduced++;
        otherParent.energy -= reproductionCost;
        otherParent.reproductiveSystem.timesReproduced++;

        child.currentTile = agent.currentTile;

        Agent[] parents = new Agent[2];
        parents[0] = agent;
        parents[1] = otherParent;
        child.CreateFromParents(parents);

        child.energy = reproductionThreshold;

        agent.manager.IncrementCounter("Reproduced", 1);
        agent.Notify(AgentNotificationType.Sex);
        otherParent.Notify(AgentNotificationType.Sex);

        child.Notify(AgentNotificationType.Birth);


        children[i] = child;
      }
    }

    return children;
  }

}
