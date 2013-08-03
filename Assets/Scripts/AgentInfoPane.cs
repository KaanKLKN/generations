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

    //DrawProgressBar(new Rect(0, 0, 200, 16), agent.energy / agent.body.MaxEnergy());

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

  public Texture2D progressBarFull;
  public Texture2D progressBarEmpty;

  void DrawProgressBar(Rect rect, float percentage) {

    Vector2 size = new Vector2(rect.width, rect.height);
    Vector2 pos = new Vector2(rect.x, rect.y);

    GUI.BeginGroup(new Rect (pos.x, pos.y, size.x, size.y));
        GUI.Box (new Rect(0,0, size.x, size.y), progressBarEmpty);
 
        // draw the filled-in part:
        GUI.BeginGroup (new Rect (0, 0, size.x * percentage, size.y));
            GUI.Box (new Rect (0,0, size.x, size.y), progressBarFull);
        GUI.EndGroup ();
 
    GUI.EndGroup();
 
  } 


}
