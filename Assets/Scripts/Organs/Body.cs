using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Body : Organ {

  public NumericalTrait lifespan = new NumericalTrait(25, 35); 
  public NumericalTrait speed = new NumericalTrait(0.5F, 4.0F); 
  public NumericalTrait strength = new NumericalTrait(0, 1); 
  public NumericalTrait startingEnergy = new NumericalTrait(0.8F, 1); 
  public NumericalTrait hue = new NumericalTrait(0, 1); 
  public NumericalTrait hunger = new NumericalTrait(0, 1); 

}
