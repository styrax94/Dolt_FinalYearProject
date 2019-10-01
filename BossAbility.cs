using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BossAbility : MonoBehaviour
{
    public enum AbilityState {none,basic,primary, ultimate};
    public AbilityState currentState = AbilityState.none;
    public Transform player;
  
    public bool attackAvailable;
    ResourceManager bossResources;
    public bool canBeChanged;
    bool inRangeForAbility;

    //BasicAttack
    NavMeshAgent gateKeeperAgent;
    Animator bossAnimController;
    float distance;
    public float basicAttackRange;
    BossFist[] bossFists;
    bool isAttacking;

    public Text distanceText;

    //PrimaryAbility
    public Transform primaryActive;
    public Transform primaryInActive;
    //public GameObject PrimaryAttack;
   // public GameObject PrimaryEffect;
    public Transform primarySpawnPosition;
    private float primaryCoolDown;
    bool primaryAbilityAvailable;
   // int primaryManaCost;
   // float primaryAbilityRange;
    public BossPrimary bossPrimaryAbility;

    //SecondaryAbility
    private float secondaryCoolDown;
    bool secondaryAbilityAvailable;
    int secondaryManaCost;
    float secondaryAbilityRange;

    //UltimateAbility
    [HideInInspector]
    public bool ultimateAbilityActivated;
    private float ultimateCoolDown;
    bool ultimateAbilityAvailable;
    int ultimateManaCost;
    float ultimateAbilityRange;
    public BossUltimate bossUltimateAbility;
    public Transform ultimateActivePool;
    public Transform ultimateInActivePool;
    //scenarios
    List<Scenario> scenarioList = new List<Scenario>();
    int wDist;
    int wMana;
    public bool attackMode;
    public Text attackWeight;

    public Text state;
    public Text previousState;

    void Start()
    {
        ////Pooling objects
        //for (int i = 0; i < 5; i++)
        //{
        //    GameObject proj = (GameObject)Object.Instantiate(PrimaryAttack, Vector3.zero, Quaternion.identity, primaryInActive);
        //    GameObject effect = (GameObject)Object.Instantiate(PrimaryEffect, Vector3.zero, Quaternion.identity, proj.transform);
        //    effect.GetComponent<ParticleSystem>().Stop();
        //    proj.SetActive(false);
        //}

        bossResources = GetComponentInParent<ResourceManager>();
        bossAnimController = GetComponent<Animator>();

        attackAvailable = true;
        //PrimaryAbility
        bossPrimaryAbility.primaryActive = primaryActive;
        bossPrimaryAbility.primaryInActive = primaryInActive;
        bossPrimaryAbility.primarySpawnPosition = primarySpawnPosition;
        bossPrimaryAbility.Initialize();      
        primaryAbilityAvailable = true;
        bossPrimaryAbility.aBaseCoolDown = 8;
        bossPrimaryAbility.aDamage = 22;
        bossPrimaryAbility.aManaCost = 30;

       

        //SecondaryAbility
        secondaryCoolDown = 10f;
        secondaryAbilityAvailable = true;
        secondaryManaCost = 40;
        secondaryAbilityRange = 0;

        //ultimateAbility
        ultimateCoolDown = 35f;
        ultimateAbilityAvailable = true;
        ultimateManaCost = 75;
        ultimateAbilityRange = 3.5f;
        bossUltimateAbility.aBaseCoolDown = 25;
        bossUltimateAbility.aManaCost = 40;
        bossUltimateAbility.aDamage = 20;
        bossUltimateAbility.GetInfo(ultimateActivePool, ultimateInActivePool, this.gameObject, GetComponent<TidalWave>());
        bossUltimateAbility.Initialize();
        ultimateAbilityActivated = true;


        //basicAttack
        gateKeeperAgent = GetComponent<NavMeshAgent>();
        bossFists = GetComponentsInChildren<BossFist>();
        isAttacking = false;

        //for(int i =0; i < 3; i++)
        //{
        //    int dis, mana;
        //    dis = 10;
        //    mana = 10;
        //    List<Bability> abilList = new List<Bability>();

        //    if (distance == 10)
        //    {
        //        abilList.Add(new Bability(1, 50, 100));
        //        abilList.Add(new Bability(3, 50, 100));
        //    }
        //    else if (distance == 15)
        //    {
        //        abilList.Add(new Bability(1, 50, 100));
        //        abilList.Add(new Bability(3, 50, 100));
        //    }
        //    else if (distance == 20)
        //    {
        //        abilList.Add(new Bability(1, 50, 100));
        //    }


        //    scenarioList.Add(new Scenario(dis,mana,abilList));

        //}
        attackMode = false;
        //this.enabled = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(player.position, transform.position);
        distanceText.text = "Distance: " + distance;
        if (!attackMode) return;
        if (distance <= bossPrimaryAbility.aRange)
        {
            inRangeForAbility = true;
        }
        else inRangeForAbility = false;


        if (bossResources.GetMana() > bossPrimaryAbility.aManaCost && primaryAbilityAvailable)
        {
            attackAvailable = true;
        }
      
        else if (bossResources.GetMana() > bossUltimateAbility.aManaCost && ultimateAbilityAvailable && ultimateAbilityActivated)
        {
            attackAvailable = true;
        }
        else attackAvailable = false;

        //if (Input.GetKeyDown(KeyCode.Space) && ultimateAbilityAvailable)
        //{
        //    Debug.Log("UseUltimate");
        //    UseUltimate();
        //    ultimateAbilityAvailable = false;

        //}
        //if (Input.GetKeyUp(KeyCode.Space) && !ultimateAbilityAvailable)
        //{
        //    ultimateAbilityAvailable = true;
        //}
        if (inRangeForAbility && attackAvailable)
        {

            if (distance <= basicAttackRange)
            {
                wDist = 10;
            }
            else if (distance <= bossUltimateAbility.aRange)
            {
                wDist = 15;
            }
            else if (distance <= bossPrimaryAbility.aRange)
            {
                wDist = 20;
            }

            if (bossResources.GetMana() >= bossPrimaryAbility.aManaCost + bossUltimateAbility.aManaCost)
            {
                wMana = 20;
            }
            else if (bossResources.GetMana() >= bossUltimateAbility.aManaCost)
            {
                wMana = 15;
            }
            else if (bossResources.GetMana() >= bossPrimaryAbility.aManaCost)
            {
                wMana = 10;
            }

            Scenario currentScenario = RetrieveScenario(wDist, wMana);

            if (currentScenario.w_distance == 0)
            {
                List<B_Ability> abilList = RetrieveAbilityList(wDist, wMana);
                currentScenario = new Scenario(wDist, wMana, abilList);
                scenarioList.Add(currentScenario);
                
            }

            B_Ability toUse = new B_Ability(0, 0, 0);

            foreach (B_Ability ability in currentScenario.abilityList)
            {
                int weight = 0;

                if (ability.id == 1 && primaryAbilityAvailable)
                {
                    if (bossPrimaryAbility.aDamage > BossBrain.instance.p_CurrentHP)
                    {
                        weight += 20;
                    }
                    weight += bossPrimaryAbility.damageRank;

                    if (BossBrain.instance.hasDefensiveAvail)
                    {
                        weight -= 10;
                    }
                    weight += (int)((ability.successRate * 20) / 100);
                                    
                }
                else if (ability.id == 3 && ultimateAbilityAvailable && ultimateAbilityActivated)
                {
                    if (bossUltimateAbility.aDamage > BossBrain.instance.p_CurrentHP)
                    {
                        weight += 20;
                    }
                    weight += bossUltimateAbility.damageRank;

                    if (BossBrain.instance.hasDefensiveAvail)
                    {
                        if(Random.Range(0,100) <= (BossBrain.instance.p_CurrentHP/BossBrain.instance.p_MaxHp)*100)
                        weight -= 10;
                    }
                    weight += (int)((ability.successRate * 20) / 100);                
                }

                ability.SetWeight(weight);

                if (toUse.weight < weight)
                {
                    toUse = ability;
                }
            }
            if (toUse.weight > 7)
            {
                isAttacking = false;
                gateKeeperAgent.isStopped = true;
                StopCoroutine("BasicAttack");
                currentState = AbilityState.primary;
                bossAnimController.SetBool("isWalking", false);
                bossAnimController.SetBool("isAttacking", false);
                canBeChanged = false;

                if (toUse.id == 1)
                {
                    UsePrimary();
                    primaryAbilityAvailable = false;

                  
                   
                    if (!state.text.Equals(previousState.text))
                    {
                        previousState.text =state.text;
                       
                    }
                    state.text = "Primary";
                }

                else if (toUse.id == 3)
                {
                    UseUltimate();
                    ultimateAbilityAvailable = false;

                    
                    if (!state.text.Equals(previousState.text))
                    {
                        previousState.text = state.text;
                        
                    }
                    state.text = "Ultimate";
                }

            }
            else
            {
                if (currentState == AbilityState.none)
                {
                    currentState = AbilityState.basic;
                    if (!isAttacking)
                    {
                        StartCoroutine("BasicAttack");

                       
                        if (!state.text.Equals(previousState.text))
                        {
                            previousState.text = state.text;

                        }
                        state.text = "Basic";
                    }
                }
            }
        }
        else
        {
            if (currentState == AbilityState.none)
            {
                currentState = AbilityState.basic;
                if (!isAttacking)
                {
                    StartCoroutine("BasicAttack");
                    state.text = "Basic";
                    if (!state.text.Equals(previousState.text))
                    {
                        previousState.text = state.text;

                    }
                }
            }
           
        }
    }
    
    //PrimaryAttack
    public void UsePrimary()
    {     
        transform.rotation = Quaternion.LookRotation(new Vector3(player.position.x - transform.position.x,
           transform.position.y, player.position.z - transform.position.z));
        if (bossAnimController)
            bossAnimController.SetBool("isPrimaryAbility", true); 
    }
    void Fire()
    {
        bossPrimaryAbility.TriggerAbility(player.position);
        bossAnimController.SetBool("isPrimaryAbility", false);
        
        bossResources.UseMana(bossPrimaryAbility.aManaCost);
      
    }
    void EndPrimaryAnimation()
    {
        canBeChanged = true;
        currentState = AbilityState.none;
        StartCoroutine("PrimaryCoolDown");
    }

    public void UseUltimate()
    {
        bossAnimController.SetBool("isUltimate", true);
    }
    public void ActivateUltimate()
    {
        bossUltimateAbility.TriggerAbility(transform.position);
        bossResources.UseMana(bossUltimateAbility.aManaCost);
        currentState = AbilityState.none;
    }
    public void EndUltimateAnim()
    {
        StartCoroutine("UltimateCoolDown");
        bossAnimController.SetBool("isUltimate", false);
        canBeChanged = true;
        
    }


    //BasicAttack
    IEnumerator BasicAttack()
    {
        isAttacking = true;

        while (isAttacking)
        {
            yield return null;
            if (distance <= basicAttackRange && !bossAnimController.GetBool("isAttacking"))
            {
                bossAnimController.SetBool("isAttacking", true);
                bossAnimController.SetBool("isWalking", false);
                gateKeeperAgent.isStopped = true;
                
            }
            else if (distance > basicAttackRange)
            {
                // StopCoroutine("Attack");
                canBeChanged = true;
                if (!bossAnimController.GetBool("isWalking"))
                {
                    bossAnimController.SetBool("isWalking", true);
                    bossAnimController.SetBool("isAttacking", false);
                    gateKeeperAgent.isStopped = false;
                }
                gateKeeperAgent.SetDestination(player.position);

            }

        }
       
    }
    private void EnableFistDamage()
    {

        foreach (BossFist fist in bossFists)
        {
            if (bossAnimController.GetBool("rightHook"))
            {
                if (!fist.rightHook)
                {
                    fist.SetCanDealDamage(true);
                   
                }
            }
            else
            {
                if (fist.rightHook)
                {
                    fist.SetCanDealDamage(true);
                    
                }
            }


        }
    }

    public bool ChangeState()
    {
        if (canBeChanged)
        {
            StopCoroutine("BasicAttack");
            bossAnimController.SetBool("isWalking", false);
            bossAnimController.SetBool("isAttacking", false);
            currentState = AbilityState.none;
            gateKeeperAgent.isStopped = true;
            isAttacking = false;
            return canBeChanged;
        }
        return false;
    }

    //
    public Scenario RetrieveScenario(int d, int m)
    {
        Scenario empty = new Scenario(0,0,null);

       if(scenarioList.Count > 0)
        foreach (Scenario s in scenarioList)
        {
            
            if (s.w_distance == d && s.mana == m) return s;
              
        }

        return empty;
    }

    public List<B_Ability> RetrieveAbilityList(int d, int m)
    {
        List<B_Ability> temp = new List<B_Ability>();

        if (d == 10 && m == 10)
        {
            temp.Add(new B_Ability(1,50,100));
            
        }
        else if (d == 10 && m == 15)
        {
            temp.Add(new B_Ability(1, 50, 100));
            temp.Add(new B_Ability(3, 50, 100));
        }
        else if (d== 10 && m == 20)
        {
            temp.Add(new B_Ability(1, 50, 100));
            temp.Add(new B_Ability(3, 50, 100));
        }
        else if (d == 15 && m == 10)
        {
            temp.Add(new B_Ability(1, 50, 100));
            
        }
        else if (d == 15 && m == 15)
        {
            temp.Add(new B_Ability(1, 50, 100));
            temp.Add(new B_Ability(3, 50, 100));
        }
        else if (d == 15 && m == 20)
        {
            temp.Add(new B_Ability(1, 50, 100));
            temp.Add(new B_Ability(3, 50, 100));
        }
        else if (d == 20 && m == 10)
        {
            temp.Add(new B_Ability(1, 50, 100));
        }
        else if (d == 20 && m == 15)
        {
            temp.Add(new B_Ability(1, 50, 100));
        }
        else if (d == 20 && m == 20)
        {
            temp.Add(new B_Ability(1, 50, 100));
        }
        else
        {
            Debug.Log("error getting Alist");
        }

        return temp;
    }
    
    IEnumerator PrimaryCoolDown()
    {
        yield return new WaitForSeconds(bossPrimaryAbility.aBaseCoolDown);
        primaryAbilityAvailable = true;
        //Debug.Log("PrimaryActive");
    }
    IEnumerator UltimateCoolDown()
    {
        yield return new WaitForSeconds(bossUltimateAbility.aBaseCoolDown);
        ultimateAbilityAvailable = true;
       // Debug.Log("UltimateActive");
    }

    public void UpgradePrimary()
    {
        bossPrimaryAbility.aBaseCoolDown -= (bossPrimaryAbility.aBaseCoolDown * 20) / 100;
        bossPrimaryAbility.aManaCost -= (bossPrimaryAbility.aManaCost * 15) / 100;
        bossPrimaryAbility.aDamage += (bossPrimaryAbility.aDamage * 18) / 100;
    }
    public void UpgradeUltimate()
    {
        bossUltimateAbility.aBaseCoolDown -= (bossUltimateAbility.aBaseCoolDown * 22) / 100;
        bossUltimateAbility.aManaCost -= (bossUltimateAbility.aManaCost * 10) / 100;
        bossUltimateAbility.aDamage += 10;
    }
}

public struct Scenario
{
   public int w_distance;
   public int mana;
   public List<B_Ability> abilityList;
   public Scenario(int d, int m, List<B_Ability>list)
    {
        w_distance = d;
        mana = m;
        abilityList = list;
    }

}

public struct B_Ability
{
    
    public int id;
    public int weight;
    public float successRate;


    public B_Ability(int ID, int w, float s)
    {
        id = ID;
        weight = w;
        successRate = s;       
    }

    public void SetWeight(int w)
    {
        weight = w;
    }
}
