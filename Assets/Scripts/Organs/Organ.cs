using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Organ {

  public Agent agent;

  public Dictionary<string, object> TraitValues() {
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
    foreach (var entry in TraitValues()) {
      Trait trait = entry.Value as Trait;
      trait.Randomize();
    }
  }

  public void InheritTraitsFromParents(Organ mom, Organ dad) {
    Dictionary<string, object> myTraits   = TraitValues();
    Dictionary<string, object> momTraits  = mom.TraitValues();
    Dictionary<string, object> dadTraits  = dad.TraitValues();
    foreach (var entry in myTraits) {
      Trait myTrait = entry.Value as Trait;
      myTrait.Inherit(momTraits[entry.Key] as Trait, dadTraits[entry.Key] as Trait);
    }
  }

}
