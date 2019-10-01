using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BossMovement : MonoBehaviour {

    NavMeshAgent gateKeeperAgent;
    public Transform player;

    Animator bossAnimController;
    float distance;

    public float basicAttackRange;
    BossFist[] bossFists;

    public Text distanceText;

	void Start () {
        gateKeeperAgent = GetComponent<NavMeshAgent>();
        bossAnimController = GetComponent<Animator>();
        bossFists = GetComponentsInChildren<BossFist>();
       
	}
	

    IEnumerator BasicAttack()
    {
        yield return null;
        distance = Vector3.Distance(player.position, transform.position);
        distanceText.text = "Distance: " + distance;

        if (distance <= basicAttackRange && !bossAnimController.GetBool("isAttacking"))
        {
            bossAnimController.SetBool("isAttacking", true);
            bossAnimController.SetBool("isWalking", false);
            gateKeeperAgent.isStopped = true;
        }
        else if (distance > basicAttackRange)
        {
            // StopCoroutine("Attack");
            if (!bossAnimController.GetBool("isWalking"))
            {
                bossAnimController.SetBool("isWalking", true);
                bossAnimController.SetBool("isAttacking", false);
                gateKeeperAgent.isStopped = false;
            }
            gateKeeperAgent.SetDestination(player.position);

        }


    }
    private void EnableFistDamage()
    {
        
        foreach(BossFist fist in bossFists)
        {
            if (bossAnimController.GetBool("rightHook"))
            {
                if (!fist.rightHook)
                {
                    fist.SetCanDealDamage(true);
                    Debug.Log("LeftHook");
                }
            }
            else
            {
                if (fist.rightHook)
                {
                    fist.SetCanDealDamage(true);
                    Debug.Log("RightHook");
                }
            }

        }
    }
    private void AlternateHooks()
    {
        //if (bossAnimController.GetBool("rightHook"))
        //{
        //    bossAnimController.SetBool("rightHook", false);
        //}
        //else bossAnimController.SetBool("rightHook", true);

    }

    public void ChangedState()
    {
        gateKeeperAgent.isStopped = true;
        bossAnimController.SetBool("isWalking", false);
        bossAnimController.SetBool("isAttacking", false);
        StopCoroutine("BasicAttack");
    }
}
