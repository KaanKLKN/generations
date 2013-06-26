using UnityEngine;
using System.Collections;

public class AgentGenerator : MonoBehaviour {

  public Map map;
  public int agentsPerGeneration = 50;
  public GameObject agentPrefab;

  void Start () {

    // Build the map
    map.Generate();

    // Build the agents
    Generate();
  }
  
  void Generate() {

    for (int i=0; i < agentsPerGeneration; i++) {

        GameObject agentObject = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        agentObject.name = "Agent " + i;
        //agentObject.transform.parent = this.transform;

        Agent agent = agentObject.GetComponent<Agent>();
        agent.currentTile = map.startTile;

    }


  }

}
