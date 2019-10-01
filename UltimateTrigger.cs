using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateTrigger : MonoBehaviour {

    public GameObject caster;
    
    public bool canDoDamage;

	void Start () {
       
	}

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            if (canDoDamage)
            {
                ResourceManager health = other.GetComponentInParent<ResourceManager>();
                health.TakeDamage(caster.GetComponent<BossAbility>().bossUltimateAbility.aDamage);
                canDoDamage = false;
                BossBrain.instance.RecordDamage(caster.GetComponent<BossAbility>().bossUltimateAbility.aDamage);
            }
            
        }
    }
}
