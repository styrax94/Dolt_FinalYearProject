using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject
 {

    public string aName = "New Ability";
    public float aBaseCoolDown = 5f;
    public int aDamage = 10;
    public int aManaCost = 50;
    public float aRange = 10f;
    public int damageRank = 10;
    public abstract void Initialize();
    public abstract void TriggerAbility(Vector3 direction);

}
