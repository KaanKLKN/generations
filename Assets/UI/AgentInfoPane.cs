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

  System.String _currentTabName = "Status";
  bool Tab(System.String name) {
    if (GUILayout.Button(name)) {
      _currentTabName = name;
      return true;
    }
    return false;
  }

  bool TabContent(System.String name){
    return _currentTabName == name;
  }

  void OnGUI() {
    if (!agent)
      return;

    GUILayout.BeginArea (new Rect(Screen.width - 200 - 10, 10, 200, Screen.height - 10));

    GUILayout.BeginVertical ("box");
    GUILayout.Label(agent.lastEventName);
    GUILayout.EndVertical ();

    GUILayout.BeginHorizontal();
    Tab("Status");
    Tab("Genetics");
    Tab("Family");
    GUILayout.EndHorizontal();


    GUILayout.BeginVertical ("box");

    if (TabContent("Status")) {
      GUILayout.Label("INFO");
      GUILayout.Label("Energy: " + SimpleFloat(agent.energy));
      GUILayout.Label("Age: " + SimpleFloat(agent.Age()) + " / " + SimpleFloat(agent.body.Lifespan()) + "s (" + SimplePercent(1 - agent.energy / agent.body.MaxEnergy()) + ")");
    }

    if (TabContent("Genetics")) {
      GUILayout.Label("INHERITED TRAITS");
      foreach (var pair in agent.Traits()) {
        NumericalTrait trait = pair.Value as NumericalTrait;
        if (trait != null) {
          GUILayout.Label(pair.Key + ": " + SimpleFloat(trait.floatValue));
        }
      }

      GUILayout.Label("CALCULATED TRAITS");

      GUILayout.Label("Speed: " + SimpleFloat(agent.body.Speed()));
      GUILayout.Label("Strength: " + SimpleFloat(agent.body.Strength()));
      GUILayout.Label("EnergyDrainPerSecond: " + SimpleFloat(agent.body.EnergyDrainPerSecond()));
      GUILayout.Label("MaxEnergy: " + SimpleFloat(agent.body.MaxEnergy()));
      GUILayout.Label("CamouflageFactor: " + agent.body.CamouflageFactor());
    }

    //DrawProgressBar(new Rect(0, 0, 200, 16), agent.energy / agent.body.MaxEnergy());

    if (TabContent("Family")) {
      GUILayout.Label("Generation: " + agent.reproductiveSystem.generation);

      GUILayout.Label("PARENTS");
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
      if (parents == null || parents.Length < 1) {
        GUILayout.Label("No parents");
      }

      GUILayout.Label("CHILDREN");
      // Agent[] children = agent.reproductiveSystem.children;
      // if (children == null || children.Length < 1) {
      //   GUILayout.Label("No children");
      // }

    }

    GUILayout.EndVertical ();
    GUILayout.EndArea ();

  }

  System.String SimplePercent(float percent) {
    return Mathf.RoundToInt(percent * 100) + "%";
  }

  System.String SimpleFloat(float amount) {
    return amount.ToString("F2");
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
