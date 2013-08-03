using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ReproductiveSystem : Organ {

  public NumericalTrait fertility = new NumericalTrait(0F, 1F); 

  int timesReproduced = 0;

  // Reproduction

  static int _maxTotalChildren = 9;
  static int _maxLitterSize = 3;

  public bool CanReproduce() {
    if (!agent.manager.PopulationCeilingExceeded()
        && agent.body.OlderThan(PrepubescencyLength())
        && !_isPregnant
        && timesReproduced < _maxTotalChildren) {
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
          GetPregnantWithParent(fertileAgents[Random.Range(0, fertileAgents.Length)]);          
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

  public float PregnancyLength() {
    return agent.body.Lifespan() * 0.3f;
  }

  public float PrepubescencyLength() {
    return agent.body.Lifespan() * 0.3f;
  }

  Agent _currentPartner = null;
  bool _isPregnant = false;
  float _birthDate = 0;
  public void GetPregnantWithParent(Agent otherParent) {
    agent.TriggerLifeEvent("Got Pregnant");
    _currentPartner = otherParent;
    _isPregnant = true;
    _birthDate = Time.time + PregnancyLength();
    agent.Notify(AgentNotificationType.Pregnant);
    otherParent.Notify(AgentNotificationType.Sex);
  }

  public void Update() {
    if (_isPregnant && Time.time >= _birthDate) {
      GiveBirth(_currentPartner);
      _isPregnant = false;
      _currentPartner = null;
    }
  }

  public Agent[] parents;
  public int generation = 0;

  public Agent[] GiveBirth(Agent otherParent) {

    /*if (Random.value <= fertility.floatValue 
        || Random.value <= otherParent.reproductiveSystem.fertility.floatValue) {
      return new Agent[0];
    }
    */
    int kidsToHave = Mathf.CeilToInt(
        Random.Range(_maxLitterSize * fertility.floatValue,
                     _maxLitterSize * otherParent.reproductiveSystem.fertility.floatValue)
        );
    if (kidsToHave < 1) {
      kidsToHave = 1;
    }

    Agent[] children = new Agent[kidsToHave];

    for (int i=0; i < kidsToHave; i++) {
      if (!agent.manager.PopulationCeilingExceeded()) {
        Agent child = agent.manager.BirthAgent();

        timesReproduced++;
        otherParent.reproductiveSystem.timesReproduced++;

        child.currentTile = agent.currentTile;

        Agent[] theParents = new Agent[2];
        theParents[0] = agent;
        theParents[1] = otherParent;
        child.CreateFromParents(theParents);

        int highestParentGeneration = theParents[0].reproductiveSystem.generation;
        if (theParents[1].reproductiveSystem.generation > highestParentGeneration)
          highestParentGeneration = theParents[1].reproductiveSystem.generation;

        child.reproductiveSystem.parents = theParents;
        child.reproductiveSystem.generation = highestParentGeneration + 1;

        agent.TriggerLifeEvent("Gave Birth");

        child.Notify(AgentNotificationType.Birth);


        children[i] = child;
      }
    }

    return children;
  }


}
