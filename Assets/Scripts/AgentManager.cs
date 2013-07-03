using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct TraitStatistic {
   public string name;
   public float floatValue;
   public Trait trait;
   public TraitStatistic(string n, float value, Trait t) 
   {
      floatValue = value;
      name = n;
      trait = t;
   }
}

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
        agent.CreateRandom();
    }

  }

  public bool PopulationCeilingExceeded() {
    return livingAgents >= populationCeiling;
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
    DidUpdateTraits();

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
    DidUpdateTraits();
  }

  int LivingAgents() {
    return livingAgents;
  }

  int PreviousLivingAgents() {
    return agentsPerGeneration - previousDead - previousFinished;
  }

  void CheckGenerationComplete() {
    if (LivingAgents() <= 0) {
      Generate(null);
      //StartCoroutine(SelectFittestAndBeginNewGeneration());
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

  ArrayList traitAverages = new ArrayList();

  bool _needsTraitUpdate = false;
  float _lastTraitUpdate = 0;
  public void DidUpdateTraits() {
    _needsTraitUpdate = true;
  }

  public void CalculateTraitAverages() {
    ArrayList _currentAgents = currentAgents;

      var traitSums = new Dictionary<string, TraitStatistic>();
      foreach (Agent agent in _currentAgents) {
        foreach (var pair in agent.Traits()) {
          NumericalTrait trait = pair.Value as NumericalTrait;
          if (trait != null) {
            TraitStatistic statistic;
            if (!traitSums.TryGetValue(pair.Key, out statistic)) {
              statistic = new TraitStatistic(pair.Key, 0, trait);
            }
            traitSums[pair.Key] = new TraitStatistic(pair.Key, statistic.floatValue + trait.floatValue, trait);
          }
        }
      }

      ArrayList newTraits = new ArrayList();

      foreach (KeyValuePair<string, TraitStatistic>pair in traitSums) {
        TraitStatistic statistic = pair.Value;
        statistic.floatValue = statistic.floatValue / _currentAgents.Count;
        newTraits.Add(statistic);
      }

      traitAverages = newTraits;
      _needsTraitUpdate = false;
      _lastTraitUpdate = Time.time;

  }

  void FixedUpdate() {
    if (_needsTraitUpdate && _lastTraitUpdate < Time.time - 0.5)
      CalculateTraitAverages();
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
    GUILayout.Label("INHERITED TRAITS");
    foreach(TraitStatistic statistic in traitAverages) {
      GUILayout.Label(statistic.name + ": " + statistic.floatValue);
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
