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
    return 1 + metabolism.floatValue - size.floatValue;
  }

  public float Strength() {
    return 1 + size.floatValue;
  }

  public float EnergyDrainPerSecond() {
    return 0.5f + metabolism.floatValue * 3;
  }

  public float MaxEnergy() {
    return 15 + 45 * metabolism.InverseFloatValue();
  }

  public float Lifespan() {
    return MaxEnergy() / EnergyDrainPerSecond();
  }

  public float CamouflageFactor() {
    return Mathf.Abs(hue.floatValue - agent.currentTile.hue);
  }

}
