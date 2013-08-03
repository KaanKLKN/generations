using UnityEngine;
using System.Collections;

public class AgentInfoPane : MonoBehaviour {


  Agent agent;
  public void DisplayAgent(Agent thisAgent) {
    agent = thisAgent;
  }

  public AgentManager Manager() {
    return GetComponent<AgentManager>();
  }

  void OnGUI() {
    if (!agent)
      return;

    GUILayout.BeginArea (new Rect(Screen.width - 200 - 10, 10, 200, Screen.height - 10));
    GUILayout.BeginVertical ("box");

    GUILayout.Label(agent.lastEventName);

    GUILayout.Label("INHERITED TRAITS");
    foreach (var pair in agent.Traits()) {
      NumericalTrait trait = pair.Value as NumericalTrait;
      if (trait != null) {
        GUILayout.Label(pair.Key + ": " + trait.floatValue);
      }
    }

    GUILayout.Label("CALCULATED TRAITS");

    GUILayout.Label("Speed: " + agent.body.Speed());
    GUILayout.Label("Strength: " + agent.body.Strength());
    GUILayout.Label("EnergyDrainPerSecond: " + agent.body.EnergyDrainPerSecond());
    GUILayout.Label("MaxEnergy: " + agent.body.MaxEnergy());
    GUILayout.Label("CamouflageFactor: " + agent.body.CamouflageFactor());

    GUILayout.Label("INFO");
    GUILayout.Label("Lifespan: " + agent.body.Lifespan() + "s");
    GUILayout.Label("Generation: " + agent.reproductiveSystem.generation);

    Agent[] parents = agent.reproductiveSystem.parents;
    if (parents != null) {
      int i = 1;
      foreach (Agent parent in parents) {
        if (GUILayout.Button ("Select Parent " + i + " (Gen " + parent.reproductiveSystem.generation + ")")) {
          Manager().SelectAgent(parent);
        }
        i++;
      }
    }

    GUILayout.EndVertical ();
    GUILayout.EndArea ();

  }

}
