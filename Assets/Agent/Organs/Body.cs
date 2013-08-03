using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Body : Organ {

  public NumericalTrait hue = new NumericalTrait(0, 1); 

  public NumericalTrait metabolism = new NumericalTrait(0, 1); 
  public NumericalTrait size = new NumericalTrait(0, 1); 

  // Secondary Traits

  public float Speed() {
    return (1 + metabolism.floatValue - size.floatValue) * agent.reproductiveSystem.ChildScaleFactor(0.75f);
  }

  public float Strength() {
    return (1 + size.floatValue) * agent.reproductiveSystem.ChildScaleFactor(0.5f);
  }

  public float EnergyDrainPerSecond() {
    return 0.5f + metabolism.floatValue * 3;
  }

  public float MaxEnergy() {
    return 20 + 20 * size.floatValue;
  }

  public float TimeUntilStarvation() {
    return agent.energy / EnergyDrainPerSecond();
  }

  public float Lifespan() {
    return 40 + 60 * metabolism.InverseFloatValue();
  }

  public bool OlderThan(float seconds) {
    return agent.Age() > seconds;
  }

  public float CamouflageFactor() {
    return Mathf.Abs(hue.floatValue - agent.currentTile.hue);
  }

}
