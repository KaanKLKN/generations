using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AgentManager : MonoBehaviour {

  public Map map;
  public int agentsPerGeneration = 50;
  public GameObject agentPrefab;

  int deadAgents;
  int finishedAgents;
  int generation;

  public Agent[] currentAgents;

  void Start () {

    generation = 0;
    currentAgents = new Agent[agentsPerGeneration];

    // Build the map
    map.Generate();

    // Build the agents
    Generate(null);
  }

  int previousDead;
  int previousFinished;
  
  void Generate(Agent[] previousAgents) {
    generation++;

    previousDead = deadAgents;
    previousFinished = finishedAgents;

    // Build agents if we don't have any.

    foreach (Transform child in transform) {
      Destroy(child.gameObject);
    }

    deadAgents = 0;
    finishedAgents = 0;

    for (int i=0; i < agentsPerGeneration; i++) {
      
        GameObject agentObject = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        agentObject.name = "Agent " + i;
        agentObject.transform.parent = this.transform;

        Agent agent = agentObject.GetComponent<Agent>();
        agent.currentTile = map.startTile;
        agent.manager = this;

        if (generation < 2) {
          agent.CreateRandom();
        }
        else {
          Agent[] parents = new Agent[2];
          parents[0] = previousAgents[Random.Range(0, previousAgents.Length)];
          parents[1] = previousAgents[Random.Range(0, previousAgents.Length)];
          agent.CreateFromParents(parents);
        }

        currentAgents[i] = agent;

    }

  }

  public void OnAgentFinish(Agent finishedAgent) {
    finishedAgents += 1;
    CheckGenerationComplete();
  }

  public void OnAgentDeath(Agent deadAgent) {
    deadAgents += 1;
    CheckGenerationComplete();
  }

  int LivingAgents() {
    return agentsPerGeneration - deadAgents - finishedAgents;
  }

  int PreviousLivingAgents() {
    return agentsPerGeneration - previousDead - previousFinished;
  }

  void CheckGenerationComplete() {
    if (LivingAgents() <= 0) {
      StartCoroutine(SelectFittestAndBeginNewGeneration());
    }
  }

  IEnumerator SelectFittestAndBeginNewGeneration() {

    List<Agent> list = new List<Agent>();
    list.AddRange(currentAgents);
    list = list.OrderBy(a => a.Fitness()).ToList();

    Agent[] fittest = list.GetRange(0, agentsPerGeneration / 3).ToArray();

    foreach (Agent agent in fittest) {
      agent.SetColor(Color.cyan);
    }

    yield return new WaitForSeconds(1);

    Generate(fittest);
  }

  void OnGUI(){
    GUILayout.BeginHorizontal ("box");
    GUILayout.Label("Generation " + generation);
    GUILayout.Label(" | Living: " + LivingAgents());
    GUILayout.Label(" | Finished: " + finishedAgents + " (" + DeltaString(finishedAgents - previousFinished) + ")");
    GUILayout.Label(" | Dead: " + deadAgents);
    GUILayout.EndHorizontal ();
  }

  System.String DeltaString(int delta) {
    if (delta > 0) {
      return "+" + delta;
    }
    return "" + delta;
  }
  
}
