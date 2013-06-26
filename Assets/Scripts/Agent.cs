using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour {

  public float speed;
  public float energy;

  public MapTile currentTile;

	// Use this for initialization
	void Start () {
    SetupProperties();
	}

  void SetupProperties() {
    speed = Random.Range(0F, 1F);
    energy = Random.Range(0F, 1F);
  }
	
	// Update is called once per frame
	void Update () {
	
	}
}
