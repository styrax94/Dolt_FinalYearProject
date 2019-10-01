using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class BossBrain : MonoBehaviour
{

    public enum BossState { None, Basic, Ability, Defense, PriorityTarget };
    public BossState stateBoss = BossState.None;

    public static BossBrain instance;

    BossMovement basicAttack;
    public BossAbility bossAbility;
    ResourceManager bossResources;
    public BossDefense bossDefense;
    bool disablingScripts;
    //Player stats
    public Transform player;
    public int p_MaxHp;
    public int p_CurrentHP;
    int p_EstimateHP;
    bool playerIsAlive;
    public bool hasPrimaryAvail;
    public bool hasDefensiveAvail;
    public bool hasUltimateAvail = false;
    List<PlayerAbilities> p_AbilityList = new List<PlayerAbilities>();
    PlayerAbilities lastAbilitySeen;

    public Text mainWeight;

    NavMeshAgent bossAgent;
    Animator bossAnim;

    public bool changingToAttack = false;
    public bool changingToDefence = false;

    bool attackDetected;

    public Text _pHealth;
    public Text abilityOne;
    float timerOne;
    float cdOne;
    public Text abilityTwo;
    float timerTwo;
    float cdTwo;
    public Text abilityThree;
    float timerThree;
    float cdThree;
    int choice;

    public Text dmgOne;
    public Text dmgTwo;
    public Text dmgThree;
    public GameObject firstAbility;
    public GameObject secondAbility;
    public GameObject thirdAbility;

    int primaryUpgradeCount;
    int defensiveUpgradeCount;

 
    void Start()
    {
        instance = this;
        basicAttack = GetComponent<BossMovement>();
        bossAbility = GetComponent<BossAbility>();
        bossDefense = GetComponent<BossDefense>();
        bossResources = GetComponentInParent<ResourceManager>();
        bossAgent = GetComponent<NavMeshAgent>();
        disablingScripts = false;
        bossAnim = GetComponent<Animator>();

        p_MaxHp = 100;
        p_CurrentHP = p_MaxHp;
        _pHealth.text = "p_Health: " + p_CurrentHP;
        p_EstimateHP = 0;
        playerIsAlive = true;

        cdOne = 0;
        cdTwo = 0;
        cdThree = 0;
        primaryUpgradeCount = 0;
        defensiveUpgradeCount = 0;
        choice = 0;

        attackDetected = false;
        this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {


        if (disablingScripts) return;


        if (CalculateWeights(bossAbility.attackAvailable,
            (bossResources.GetHealth() - p_CurrentHP),
            bossResources.GetTotalTimeTillMaxMana(), attackDetected) >= 30)
        {
            if (!changingToAttack)
            {
                if (stateBoss != BossState.Ability)
                {
                    StopCoroutine("ChangeState");
                    StartCoroutine("ChangeState");
                    changingToAttack = true;
                }

            }
        }
        else
        {

            if (!changingToDefence)
            {
                if (stateBoss != BossState.Defense)
                {
                    StopCoroutine("ChangeState");
                    StartCoroutine("ChangeState");
                    changingToDefence = true;
                }

            }
            if (attackDetected)
            {

                if (bossDefense.defenseMode)
                {

                    bossDefense.AbilityDetected(true, lastAbilitySeen.id, lastAbilitySeen.damage, Vector3.Distance(player.position, transform.position));
                    attackDetected = false;

                }
            }
        }


    }

    void EnableScripts(int id)
    {
        if (id == 0)
        {
            if (!basicAttack.isActiveAndEnabled)
            {
                basicAttack.enabled = true;
            }
        }
        if (id == 1)
        {
            if (!bossAbility.isActiveAndEnabled)
            {
                bossAbility.enabled = true;
                bossAbility.UsePrimary();
            }
        }
    }

    void DisableScripts()
    {
        bossAbility.enabled = false;

    }

    float CalculateWeights(bool attackAvailable, float healthDiff,
        float manaConservation, bool attackDetected)
    {
        int totalWeight = 0;
        mainWeight.text = "StateWeight: ";
        if (attackAvailable || bossDefense.defenceAbilityAvailable)
        {
            totalWeight += 10;
            mainWeight.text += "10A, ";

        }
        else
        {

            totalWeight -= 10;
            mainWeight.text += "-10A, ";

        }
        if (bossResources.GetHealthPercentage() >= 75)
        {
            totalWeight += 20;
            mainWeight.text += "20H, ";
        }
        else if (bossResources.GetHealthPercentage() < 75)
        {
            if (healthDiff > 35)
            {
                totalWeight += 20;
                mainWeight.text += "15H, ";
            }
            else if (healthDiff <= 35 && healthDiff >= 10)
            {
                totalWeight += 15;
                mainWeight.text += "15H, ";
            }
            else if (healthDiff < 10)
            {
                totalWeight += 10;
                mainWeight.text += "10H, ";
            }

        }

        if (manaConservation >= 25)
        {
            totalWeight += 10;
            mainWeight.text += "10M, ";
        }
        else if (manaConservation < 25 && manaConservation >= 10)
        {
            totalWeight += 15;
            mainWeight.text += "15M, ";
        }
        else if (manaConservation < 10)
        {
            totalWeight += 20;
            mainWeight.text += "20M, ";
        }
        if (attackDetected)
        {
            totalWeight -= 40;
            mainWeight.text += "-4D";
        }

        if (bossDefense.defenceAbilityActive)
        {
            totalWeight += 20;
        }
        return totalWeight;
    }

    IEnumerator ChangeState()
    {
        bool changed = false;

        while (!changed)
        {
            yield return null;

            if (changingToAttack)
            {
                if (bossDefense.ChangeState())
                {
                    bossAbility.attackMode = true;
                    bossDefense.defenseMode = false;
                    changed = true;
                    stateBoss = BossState.Ability;
                    changingToAttack = false;

                }
            }
            else if (changingToDefence)
            {
                if (bossAbility.ChangeState())
                {
                    bossDefense.defenseMode = true;
                    bossAbility.attackMode = false;
                    changed = true;
                    changingToDefence = false;
                    stateBoss = BossState.Defense;
                }
            }
        }
        bossAgent.isStopped = true;

    }
    IEnumerator DisableState()
    {
        disablingScripts = true;
        StopCoroutine("ChangeState");
        bool changeOne = false;
        bool changeTwo = false;


        while (!changeOne && !changeTwo)
        {
            yield return null;


            if (bossDefense.ChangeState() && bossDefense.defenseMode)
            {
                bossDefense.currentState = BossDefense.DefenseState.None;
                bossDefense.defenseMode = false;
                changeOne = true;
                stateBoss = BossState.None;
            }



            if (bossAbility.ChangeState() && bossAbility.attackMode)
            {
                bossAbility.currentState = BossAbility.AbilityState.none;
                bossAbility.attackMode = false; 
                changeTwo = true;
                stateBoss = BossState.None;
            }

        }

      
        ResetPlayerStats();
       
        Debug.Log("Disabled boss");
        disablingScripts = false;
        this.enabled = false;
       
    }

    public void SetAttackDetected(bool value)
    {
        attackDetected = value;
    }

    public void RecordDamage(int damage)
    {
        p_CurrentHP -= damage;

        if (p_CurrentHP < 0) p_CurrentHP = 25;

        p_EstimateHP += damage;
        _pHealth.text = "p_Health: " + p_CurrentHP;

    }
    public void SetPlayerIsAlive(bool value)
    {
        playerIsAlive = value;
    }
    private void ResetPlayerStats()
    {
        if (playerIsAlive)
        {
            if (p_EstimateHP > p_MaxHp)
            {
                p_MaxHp = p_EstimateHP;
            }
        }
        else
        {
            if (p_EstimateHP != p_MaxHp)
            {
                p_MaxHp = p_EstimateHP;
            }
        }

        p_CurrentHP = p_MaxHp;
        p_EstimateHP = 0;
        playerIsAlive = true;
        _pHealth.text = "p_Health: " + p_CurrentHP;
    }

    public void RecordAbility(int i)
    {
        if (!this.isActiveAndEnabled) return;
        attackDetected = true;
        PlayerAbilities temp = new PlayerAbilities(i, 0);
        bool abilityRecorded = false;
        if (p_AbilityList.Count != 0)
        {
            foreach (PlayerAbilities abil in p_AbilityList)
            {
                if (abil.id == i)
                {
                    abilityRecorded = true;

                    lastAbilitySeen = abil;

                    if (i == 1)
                    {
                        StopCoroutine(TimerOne(cdOne));
                        abilityOne.text = "Unavailable";
                        hasPrimaryAvail = false;

                        float tempCD = Time.time - timerOne;

                        if (tempCD < cdOne || cdOne == 0)
                        {
                            cdOne = tempCD;
                        }
                        timerOne = Time.time;


                        StartCoroutine(TimerOne(cdOne));

                    }
                    else if (i == 2)
                    {

                        abilityTwo.text = "Unavailable";
                        hasDefensiveAvail = false;
                        float tempCD = Time.time - timerTwo;

                        if (tempCD < cdTwo || cdTwo == 0)
                        {
                            cdTwo = tempCD;
                        }
                        timerTwo = Time.time;
                        StopCoroutine(TimerTwo(cdTwo));
                        StartCoroutine(TimerTwo(cdTwo));

                    }
                    else if (i == 3)
                    {
                        abilityThree.text = "Unavailable";
                        hasUltimateAvail = false;
                        float tempCD = Time.time - timerThree;

                        if (tempCD < cdThree || cdThree == 0)
                        {
                            cdThree = tempCD;
                        }
                        timerThree = Time.time;
                        StopCoroutine(TimerThree(cdThree));
                        StartCoroutine(TimerThree(cdThree));

                    }

                    return;
                }
            }
        }
        if (!abilityRecorded)
        {
            p_AbilityList.Add(temp);
            lastAbilitySeen = temp;


            if (i == 1)
            {
                firstAbility.SetActive(true);
                timerOne = Time.time;
                hasPrimaryAvail = false;
            }
            else if (i == 2)
            {
                secondAbility.SetActive(true);
                timerTwo = Time.time;
                hasDefensiveAvail = false;
            }
            else if (i == 3)
            {
                thirdAbility.SetActive(true);
                timerThree = Time.time;
                hasUltimateAvail = false;
            }
        }
    }

    public void RecordAbilityDamage(int i, int d)
    {
        foreach (PlayerAbilities abil in p_AbilityList)
        {
            if (abil.id == i)
            {
                if (abil.damage != d)
                {
                    abil.SetDamage(d);

                    if (i == 1)
                    {
                        dmgOne.text = abil.damage.ToString();
                    }
                    else if (i == 2)
                    {
                        dmgTwo.text = abil.damage.ToString();
                    }
                    else if (i == 3)
                    {
                        dmgThree.text = abil.damage.ToString();
                    }
                }

            }
        }

    }

    IEnumerator TimerOne(float cd)
    {

        yield return new WaitForSeconds(cd);
        hasPrimaryAvail = true;
        abilityOne.text = "available";

    }
    IEnumerator TimerTwo(float cd)
    {

        yield return new WaitForSeconds(cd);
        hasDefensiveAvail = true;
        abilityTwo.text = "available";

    }
    IEnumerator TimerThree(float cd)
    {

        yield return new WaitForSeconds(cd);
        hasUltimateAvail = true;
        abilityThree.text = "available";
    }


    public void UpgradeAbility(bool ultimate)
    {
        if (ultimate)
        {
            bossAbility.UpgradeUltimate();
            Debug.Log("Ulty Upgrade");
        }
        else
        {
            Random.Range(0, 2);
            if (primaryUpgradeCount == 0 && defensiveUpgradeCount == 0)
            {
                choice = Random.Range(0, 2);
                if (choice == 0)
                {
                    bossAbility.UpgradePrimary();
                    primaryUpgradeCount++;
                    Debug.Log("Primary Upgrade");
                }
                else
                {
                    bossDefense.UpgradeDefense();
                    defensiveUpgradeCount++;
                    Debug.Log("Defense Upgrade");
                }
            }
            else if (primaryUpgradeCount == 0)
            {
                bossAbility.UpgradePrimary();
                primaryUpgradeCount++;
                Debug.Log("Primary Upgrade");
            }
            else if (defensiveUpgradeCount == 0)
            {
                bossDefense.UpgradeDefense();
                defensiveUpgradeCount++;
                Debug.Log("Defense Upgrade");

            }
            else
            {
                if (choice == 0 && primaryUpgradeCount < 3 || defensiveUpgradeCount >= 3)
                {
                    bossAbility.UpgradePrimary();
                    primaryUpgradeCount++;
                    Debug.Log("Primary Upgrade");
                }
                else if (defensiveUpgradeCount < 3)
                {
                    bossDefense.UpgradeDefense();
                    defensiveUpgradeCount++;
                    Debug.Log("Defense Upgrade");

                }
            }
        }
    }
}



struct PlayerAbilities
{
    public int id;
    public int damage;

    public PlayerAbilities(int i, int d)
    {
        id = i;
        damage = d;
    }

    public void SetDamage(int d)
    {
        damage = d;
    }

}