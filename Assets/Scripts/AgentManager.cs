using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AgentManager : MonoBehaviour {

  public Map map;
  public int agentsPerGeneration = 50;
  public GameObject agentPrefab;

  int livingAgents;
  int generation;

  public Agent[] currentAgents;

  void Start () {

    generation = 0;
    livingAgents = 0;

    // Build the map
    map.Generate();

    // Build the agents
    Generate(null);
  }
  
  void Generate(Agent[] previousAgents) {
    generation++;

    foreach (Transform child in transform) {
      Destroy(child.gameObject);
    }

    currentAgents = new Agent[agentsPerGeneration];

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

    livingAgents = agentsPerGeneration;

  }

  public void OnAgentDeath(Agent deadAgent) {
    livingAgents -= 1;

    if (livingAgents <= 0) {
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
    GUILayout.Label("Generation: " + generation);
    GUILayout.Label("Living agents: " + livingAgents);
  }
  
}
