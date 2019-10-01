using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Abilities")]
public class BossPrimary : Ability
{
    public Transform primaryActive;
    public Transform primaryInActive;
    public GameObject PrimaryAttack;
    public GameObject PrimaryEffect;
    public Transform primarySpawnPosition;

    public override void Initialize()
    {
        //Pooling objects
        for (int i = 0; i < 5; i++)
        {
            GameObject proj = (GameObject)Object.Instantiate(PrimaryAttack, Vector3.zero, Quaternion.identity, primaryInActive);
            GameObject effect = (GameObject)Object.Instantiate(PrimaryEffect, Vector3.zero, Quaternion.identity, proj.transform);
            effect.GetComponent<ParticleSystem>().Stop();
            proj.SetActive(false);
        }

        
    }

    public override void TriggerAbility(Vector3 target)
    {
        if (primaryInActive.childCount > 0)
        {
            GameObject proj = primaryInActive.GetChild(0).gameObject;
            proj.transform.position = primarySpawnPosition.position;
            Vector3 direction = target - primarySpawnPosition.position;
            direction = direction.normalized;
            proj.GetComponent<PrimaryProjectile>().SetDirection(direction);
            proj.GetComponentInChildren<ParticleSystem>().Play();
            proj.transform.SetParent(primaryActive);
            proj.SetActive(true);
            proj.GetComponent<PrimaryProjectile>().Spawn();
        }
    }
}
