using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BossDefense : MonoBehaviour {

    public enum DefenseState {None, Run, Ability, Strafe};
    public DefenseState currentState = DefenseState.None;
    public bool canBeChanged;
    public Transform player;
    NavMeshAgent bossAgent;
    Animator bossAnimController;
    bool playerAbilityDetected;
    ResourceManager bossResources;

    //ability
    float defenceAbilityCD;
    int defenceManaCost;
    int defenceDuration;
    public bool defenceAbilityAvailable;
    public bool defenceAbilityActive;
    public GameObject bossAbilityEffect;
    int UseDefenseWeight;

    //playerStats
    int abilityDamageRank;
    float distance;
    int abilityID;

    public Text state;
    public Text previousState;
    public bool defenseMode;
    void Start () {
        canBeChanged = true;
        playerAbilityDetected = false;

        bossAgent = GetComponent<NavMeshAgent>();
        bossAnimController = GetComponent<Animator>();
        bossResources = GetComponentInParent<ResourceManager>();
        playerAbilityDetected = false;
       

        //ability
        defenceAbilityCD = 10;
        defenceManaCost = 25;
        defenceDuration = 5;
        UseDefenseWeight = 0;
        defenceAbilityActive = false;
        defenceAbilityAvailable = true;
        defenseMode = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!defenseMode) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine("Strafe");          
        }

        if (playerAbilityDetected)
        {
            if (abilityID == 2)
            {
                return;
            }

            canBeChanged = false;
            UseDefenseWeight = 0;

            if(abilityID == 1)
            {
                UseDefenseWeight += 10;
                if(distance >= 10)
                {
                    UseDefenseWeight += 10;
                }
                else if (BossBrain.instance.bossAbility.ultimateAbilityActivated)
                {
                    if (!BossBrain.instance.hasUltimateAvail)
                    {
                        UseDefenseWeight += 10;
                    }
                 
                    if ( abilityDamageRank > bossResources.GetHealth())
                    {
                        UseDefenseWeight += 20;
                    }
                }
                int rand = Random.Range(0, 101);
                
                if (rand >= bossResources.GetHealthPercentage())
                {
                    UseDefenseWeight += 20;
                }
            }
            else if(abilityID == 3)
            {
                UseDefenseWeight += 20;

                if (distance >= 15)
                {
                    UseDefenseWeight += 5;

                    if (abilityDamageRank > bossResources.GetHealth())
                    {
                        if (Random.Range(0, 100) > bossResources.GetHealthPercentage())
                        {
                            UseDefenseWeight += 10;
                        }

                    }

                }
                else
                {
                    UseDefenseWeight += 15;
                
                }

               
            }
          
            if(UseDefenseWeight >= 30 && defenceAbilityAvailable)
            {
                bossAnimController.SetBool("isWalking", false);
                bossAnimController.SetBool("isDefend", true);
                currentState = DefenseState.Ability;
               
                
                if (!state.text.Equals(previousState.text))
                {
                    previousState.text = state.text;                  
                }
                state.text = "Defense";
            }
            else if(UseDefenseWeight <=30 && !defenceAbilityActive )
            {


                currentState = DefenseState.Strafe;
                    StartCoroutine("Strafe");

              
                if (!state.text.Equals(previousState.text))
                {
                    previousState.text = state.text;                 
                }
                state.text = "Strafe";

            }

            playerAbilityDetected = false;
        }
        else
        {

            if (currentState == DefenseState.None)
            {
                currentState = DefenseState.Run;
                bossAnimController.SetBool("isWalking", true);
                StartCoroutine("RunAway");
                if (!state.text.Equals(state.text))
                {
                    previousState.text = state.text;
                    state.text = "RunAway";
                }
            }
        }

    }
    IEnumerator Strafe()
    {
        int rnd = Random.Range(1, 3);
        int strafeAmount = 0;
        bossAnimController.SetBool("isWalking", true);
        Vector3 direction = transform.position;
        bossAgent.isStopped = false;

        if (abilityID == 1)
        {
            strafeAmount = 4;
        }
        else
        {
            strafeAmount = 8;
        }
      
        if(abilityID == 1 && distance > 9)
        {
            direction = direction + player.transform.forward * 3;
            bossAgent.SetDestination(direction);
        }
        else
        {
            if (rnd == 1)
            {

                direction = direction + player.transform.right * strafeAmount;
                bossAgent.SetDestination(direction);

            }
            else if (rnd == 2)
            {

                direction = direction + (-player.transform.right) * strafeAmount;
                bossAgent.SetDestination(direction);
            }

        }

        
        while (bossAgent.remainingDistance >= bossAgent.stoppingDistance)
        {
            yield return null;
        }

        bossAgent.isStopped = true;
        bossAnimController.SetBool("isWalking", false);
        currentState = DefenseState.None;
        canBeChanged = true;

    }
    IEnumerator RunAway()
    {
        while(currentState == DefenseState.Run)
        {
            yield return new WaitForSeconds(1f);

            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.position - player.position, out hit, 5))
            {
                bossAgent.isStopped = false;
                
                bossAgent.SetDestination(hit.point);
                
            }
            else
            {
                bossAgent.isStopped = false;
                Vector3 direction = transform.position - player.position;
                direction = direction.normalized;
                bossAgent.SetDestination(transform.position + direction * 5);             
            }
        }
    }

    public void CastDefence()
    {
        bossAbilityEffect.SetActive(true);
        bossResources.canBeDamaged = false;
        defenceAbilityAvailable = false;
        defenceAbilityActive = true;
        StartCoroutine("DefenceRuntime");
        
    }
    public void EndDefenceAnim()
    {
        currentState = DefenseState.None;
        bossAnimController.SetBool("isDefend", false);
        canBeChanged = true;
    }

    IEnumerator DefenceRuntime()
    {
        yield return new WaitForSeconds(defenceDuration);
        bossAbilityEffect.SetActive(false);
        bossResources.canBeDamaged = true;
        defenceAbilityActive = false;
        Debug.Log("End duration");
        yield return new WaitForSeconds(defenceAbilityCD);
        defenceAbilityAvailable = true;


    }
    
    public void UpgradeDefense()
    {
        defenceAbilityCD -= (defenceAbilityCD * 15) / 100;
        defenceManaCost -= (defenceManaCost * 5) / 100;
        defenceDuration++;
    }

    public void SetPlayerAttackDetected(bool value)
    {
        playerAbilityDetected = value;
    }
    public bool ChangeState()
    {
        if (canBeChanged)
        {
            StopCoroutine("RunAway");
            currentState = DefenseState.None;
            bossAnimController.SetBool("isWalking", false);
            bossAgent.isStopped = true;
       
            return canBeChanged;
        }
        return false;
    }

    public void AbilityDetected(bool value, int id, int damage, float d)
    {
        playerAbilityDetected = true;
        abilityID = id;
        abilityDamageRank = damage;
        distance = d;
       
    }
}
