using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AgentManager : MonoBehaviour {

  public Map map;
  public int agentsPerGeneration = 50;
  public GameObject agentPrefab;

  public bool placeInGroup = false;

  public int populationCeiling = 500;

  int livingAgents;
  int deadAgents;
  int finishedAgents;
  int generation;

  public ArrayList currentAgents;

  void Start () {

    generation = 0;
    currentAgents = new ArrayList();

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
    livingAgents = 0;

    // Build agents if we don't have any.

    foreach (Transform child in transform) {
      Destroy(child.gameObject);
    }

    currentAgents = new ArrayList();
    ResetCounters();
    
    deadAgents = 0;
    finishedAgents = 0;

    for (int i=0; i < agentsPerGeneration; i++) {
      
        Agent agent = BirthAgent();
        if (generation < 2) {
          agent.CreateRandom();
        }
        else {
          Agent[] parents = new Agent[2];
          parents[0] = previousAgents[Random.Range(0, previousAgents.Length)];
          parents[1] = previousAgents[Random.Range(0, previousAgents.Length)];
          agent.CreateFromParents(parents);
        }
    }

    CalculateAverages();
  }

  public bool PopulationCeilingExceeded() {
    return livingAgents > populationCeiling;
  }

  public Agent BirthAgent() {
    GameObject agentObject = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity) as GameObject;
    agentObject.transform.parent = this.transform;

    Agent agent = agentObject.GetComponent<Agent>();
    agent.InitializeAgent();
    agent.manager = this;
    if (placeInGroup) {
      agent.currentTile = map.startTile;//map.RandomTile();
    }
    else {
      agent.currentTile = map.RandomTile();
    }

    currentAgents.Add(agent);
    livingAgents++;
    CalculateTraitAverages();

    return agent;
  }

  public void OnAgentFinish(Agent finishedAgent) {
    finishedAgents += 1;
    CheckGenerationComplete();
  }

  public void OnAgentDeath(Agent deadAgent) {
    deadAgents += 1;
    livingAgents -= 1;
    CheckGenerationComplete();
    currentAgents.Remove(deadAgent);
    CalculateTraitAverages();
  }

  int LivingAgents() {
    return livingAgents;
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
    foreach (Agent agent in currentAgents) {
      list.Add(agent);
    }
    list = list.OrderBy(a => a.Fitness()).ToList();

    Agent[] fittest = list.GetRange(0, agentsPerGeneration / 3).ToArray();

    foreach (Agent agent in fittest) {
      agent.SetColor(Color.cyan);
    }

    yield return new WaitForSeconds(1);

    Generate(fittest);
  }

  Dictionary<string, int> counters = new Dictionary<string, int>();

  public void ResetCounters() {
    counters = new Dictionary<string, int>();
  }

  public void IncrementCounter(string name, int incrementAmount) {

    int value;
    if (!counters.TryGetValue(name, out value)) {
      value = 0;
    }

    counters[name] = value + incrementAmount;
  }

  Dictionary<string, float> traitAverages = new Dictionary<string, float>();

  public void CalculateTraitAverages() {
    Dictionary<string, float> traitSums = new Dictionary<string, float>();
    foreach (Agent agent in currentAgents) {
      foreach (var pair in agent.Traits()) {
        NumericalTrait trait = pair.Value as NumericalTrait;
        if (trait != null) {
          float value;
          if (!traitSums.TryGetValue(pair.Key, out value)) {
            value = 0;
          }
          traitSums[pair.Key] = value + trait.floatValue;
        }
      }
    }

    foreach (var pair in traitSums) {
      traitAverages[pair.Key] = pair.Value / currentAgents.Count;
    }
  }

  float previousSpeed;
  float currentSpeed;

  float previousHunger;
  float currentHunger;

  void CalculateAverages() {
    previousSpeed = currentSpeed;
    currentSpeed = 0;
    previousHunger = currentHunger;
    currentHunger = 0;
    foreach(Agent agent in currentAgents) {
      currentSpeed += agent.speed;
      currentHunger += agent.hunger;
    }
    currentSpeed = currentSpeed / agentsPerGeneration;
    currentHunger = currentHunger / agentsPerGeneration;
  }

  void OnGUI(){
    if (GUILayout.Button("New Map")) {
      Application.LoadLevel ("main");
    }

    GUILayout.BeginHorizontal ("box");
    GUILayout.Label("Spawn generation " + generation);
    GUILayout.EndHorizontal ();

    GUILayout.BeginVertical ("box");
    GUILayout.Label("Living: " + LivingAgents());
    foreach(KeyValuePair<string, int> counter in counters) {
      GUILayout.Label(counter.Key + ": " + counter.Value);
    }
    GUILayout.EndVertical ();

    GUILayout.BeginVertical ("box");
    GUILayout.Label("Averages:");
    foreach(var average in traitAverages) {
      GUILayout.Label(average.Key + ": " + average.Value);
    }
    GUILayout.EndVertical ();

  }

    System.String DeltaString(float delta) {
    if (delta > 0) {
      return "+" + delta;
    }
    return "" + delta;
  }

  System.String DeltaString(int delta) {
    if (delta > 0) {
      return "+" + delta;
    }
    return "" + delta;
  }
  
}
