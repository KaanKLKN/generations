using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Organ {

  public Agent agent;

  public virtual string[] InheritableTraits() {
    return new string[]{};
  }

  public Dictionary<string, object> InheritableTraitValues() {
    Dictionary<string, object> traitValues = new Dictionary<string, object>();
    System.Reflection.FieldInfo[] fields = this.GetType().GetFields();
    foreach(System.Reflection.FieldInfo field in fields) {
      object value = field.GetValue(this);
      Trait trait = value as Trait; 
      if (trait != null) {
        traitValues[field.Name] = trait;
      }
    }
    return traitValues;
  }

  public void RandomizeTraits() {
    foreach (var entry in InheritableTraitValues()) {
      Trait trait = entry.Value as Trait;
      trait.Randomize();
    }
  }

  public void InheritTraitsFromParents(Organ mom, Organ dad) {
    Dictionary<string, object> myTraits   = InheritableTraitValues();
    Dictionary<string, object> momTraits  = mom.InheritableTraitValues();
    Dictionary<string, object> dadTraits  = dad.InheritableTraitValues();
    foreach (var entry in myTraits) {
      Trait myTrait = entry.Value as Trait;
      myTrait.Inherit(momTraits[entry.Key] as Trait, dadTraits[entry.Key] as Trait);
    }
  }

}
