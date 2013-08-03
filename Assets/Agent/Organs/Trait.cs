using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Abstract base class.
public class Trait {
  public Trait() {}
  public virtual void Randomize() {}
  public virtual void Inherit(Trait trait1, Trait trait2) {}
}

public class NumericalTrait : Trait {

  public NumericalTrait() {}

  public float min;
  public float max;

  public float floatValue;
  public int intValue;

  public NumericalTrait(float _min, float _max) {
    min = _min;
    max = _max;
  }

  public void SetValue(float value) {
    floatValue = value;
    intValue = Mathf.RoundToInt(floatValue);
  }

  public override void Randomize() {
    base.Randomize();
    SetValue(Random.Range(min, max));
  }

  public override void Inherit(Trait trait1, Trait trait2) {
    NumericalTrait nt1 = trait1 as NumericalTrait;
    NumericalTrait nt2 = trait2 as NumericalTrait;
    SetValue(Random.Range(nt1.floatValue, nt2.floatValue));
  }

  public float LowerFactor() {
    if (floatValue < 0.5f)
      return (1 - floatValue) * 2;
    return 0;
  }

  public float UpperFactor() {
    if (floatValue > 0.5)
      return (floatValue - 0.5f) * 2;
    return 0;
  }

  public float InverseFloatValue() {
    return 1 - floatValue;
  }

}

public class BooleanTrait : Trait {

  public bool boolValue;
  public float floatValue;

  public void SetValue(bool v) {
    boolValue = v;
    floatValue = v ? 1 : 0;
  }

  public override void Randomize() {
    base.Randomize();
    SetValue(Random.value > 0.5F);
  }

  public override void Inherit(Trait trait1, Trait trait2) {
    BooleanTrait bt1 = trait1 as BooleanTrait;
    BooleanTrait bt2 = trait2 as BooleanTrait;
    if (bt1.boolValue == bt2.boolValue)
      SetValue(bt1.boolValue);
    else
      Randomize();
  }

}
