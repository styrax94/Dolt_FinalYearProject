using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour {

    Vector3 targetPosition;
    Vector3 lookAtTarget;
    Vector3 direction;
    Quaternion playerRot;
    float rotSpeed = 5f;
    float speed = 4f;
    bool rightClick;
    bool leftClick;
    bool moving;
    Animator anim;
    NavMeshAgent playerAgent;
    ResourceManager myResource;
    float basicAttackRange;
    bool enemyTargeted;
    bool isAttacking;
    Transform target;

    public Transform GateKeeper;
    public Text distanceText;

    [HideInInspector]
    public bool canMove;

    #region Abilities
    //Abilities
    bool abilityActive; 

    bool primaryAbilityActive;
    public GameObject abilityCanvas;
    public GameObject ActivatePrimaryAbility;
    public Image primaryAbility;
    bool primaryAbilityAvailable;
    float primaryAbilityCD;
    public Image primary;
    int primaryManaCost;
    public GameObject PrimaryTrigger;

    bool isDashing;
    bool dashAbilityActive;
    public GameObject dashRangeAbility;
    float dashTimer;
    bool dashAbilityAvailable;
    float dashAbilityCD;
    public Image secondary;
    int secondaryManaCost;

    bool ultimateAbilityActive;
    public GameObject UltimateAbilityIndicator;
    float ultimateTimer;
    public Transform activeAbility;
    public Transform inActiveAbility;
    bool ultimateAbilityAvailable;
    float ultimateAbilityCD;
    public Image ultimate;
    int ultimateManaCost;
    int ultimateDamage;
    public GameObject UltimateSkillSlot;

    public Transform fountain;
    #endregion

    #region ScreenUI
    public Text pManaCost;
    public Text sManaCost;
    public Text uManaCost;
    #endregion

    void Start () {

        myResource = GetComponentInParent<ResourceManager>();

        rightClick = false;
        leftClick = false;
       
        anim = GetComponent<Animator>();
        moving = false;
        basicAttackRange = 2.4f;
        playerAgent = GetComponent<NavMeshAgent>();
        isAttacking = false;
        primaryAbilityActive = false;
        abilityActive = false;
        canMove = false;
        primaryAbilityAvailable = true;
        dashAbilityAvailable = true;
        ultimateAbilityAvailable = true;
        primaryAbilityCD = 5f;
        dashAbilityCD = 4f;
        ultimateAbilityCD = 12f;
        primaryManaCost = 30;
        secondaryManaCost = 15;
        ultimateManaCost = 60;
        ultimateDamage = 50;

        //screenUI
        pManaCost.text = primaryManaCost.ToString();
        sManaCost.text = secondaryManaCost.ToString();
        uManaCost.text = ultimateManaCost.ToString();
    }
	/*
       Boss Ultimate effect 51 
       23
    */
	// Update is called once per frame
	void Update () {

        if (!canMove)
        {
            if (!playerAgent.isStopped)
            {
                playerAgent.isStopped = true;
                anim.SetBool("isMoving", false);              
            }
            return;
         }
       
        //Movement
        if (Input.GetMouseButton(1) && !rightClick && !abilityActive)
        {
            SetPosition();
           
            rightClick = true;
        }
        else rightClick = false;

        //Activate ability
        if (Input.GetMouseButton(0) && !abilityActive)
        {
            
           
            if (primaryAbilityActive)
            {
                //disables attacking and moving conditions
                PrepareForAbility();

                //enables/disables conditions for primary ability
                anim.SetBool("isPrimaryAttack", true);             
                ActivatePrimaryAbility.GetComponent<Image>().enabled = false;
                primaryAbilityActive = false;               
                primaryAbilityAvailable = false;
                primary.fillAmount = 1;
                myResource.UseMana(primaryManaCost);
            }
            else if (dashAbilityActive)
            {
                //disables attacking and moving conditions
                PrepareForAbility();

                //enables/disables conditions for dash ability
                anim.SetBool("isDashing", true);
                dashRangeAbility.GetComponent<Image>().enabled = false;
                dashAbilityActive = false; 
                dashAbilityAvailable = false;
                secondary.fillAmount = 1;
                myResource.UseMana(secondaryManaCost);

                StartCoroutine("Dash");
            }
            else if (ultimateAbilityActive)
            {
                //disables attacking and moving conditions
                PrepareForAbility();

                //enables/disables conditions for primary ability
                anim.SetBool("isUltimateAttack", true);
                UltimateAbilityIndicator.GetComponent<Image>().enabled = false;
                ultimateAbilityActive = false;
                ultimateAbilityAvailable = false;
                ultimate.fillAmount = 1;
                myResource.UseMana(ultimateManaCost);

                FireUltimateAbility();
            }

            if (abilityActive) leftClick = true;
        }

        //Ability indicators
        if (primaryAbilityAvailable && myResource.GetMana() >= primaryManaCost)
        {
            if (abilityActive) { }
            else if (Input.GetButton("PrimaryAbility") && !primaryAbilityActive && !dashAbilityActive && !ultimateAbilityActive)
            {
                abilityCanvas.GetComponent<RotateIndicator>().SetActiveAbility(true);
                ActivatePrimaryAbility.GetComponent<Image>().enabled = true;
                primaryAbilityActive = true;
            }
            else if (Input.GetButtonUp("PrimaryAbility") && primaryAbilityActive)
            {
                abilityCanvas.GetComponent<RotateIndicator>().SetActiveAbility(false);
                ActivatePrimaryAbility.GetComponent<Image>().enabled = false;
                primaryAbilityActive = false;
            }
        }
        if (dashAbilityAvailable && myResource.GetMana() >= secondaryManaCost)
        {
            if (abilityActive) { }

            else if (Input.GetButton("SecondaryAbility") && !dashAbilityActive && !primaryAbilityActive && !ultimateAbilityActive)
            {
                abilityCanvas.GetComponent<RotateIndicator>().SetActiveAbility(true);
                dashRangeAbility.GetComponent<Image>().enabled = true;
                dashAbilityActive = true;
            }
            else if (Input.GetButtonUp("SecondaryAbility") && dashAbilityActive)
            {
                abilityCanvas.GetComponent<RotateIndicator>().SetActiveAbility(false);
                dashRangeAbility.GetComponent<Image>().enabled = false;
                dashAbilityActive = false;
            }
        }
        if (ultimateAbilityAvailable && myResource.GetMana() >= ultimateManaCost)
        {
            if (abilityActive) { }
            else if (Input.GetButton("UltimateAbility") && !ultimateAbilityActive && !primaryAbilityActive && !dashAbilityActive)
            {
                abilityCanvas.GetComponent<RotateIndicator>().SetActiveAbility(true);
                UltimateAbilityIndicator.GetComponent<Image>().enabled = true;
                ultimateAbilityActive = true;
            }
            else if (Input.GetButtonUp("UltimateAbility") && ultimateAbilityActive)
            {
                abilityCanvas.GetComponent<RotateIndicator>().SetActiveAbility(false);
                UltimateAbilityIndicator.GetComponent<Image>().enabled = false;
                ultimateAbilityActive = false;
            }
        }
        

        //UpdateScreenUI
        if(myResource.GetMana() < primaryManaCost)
        {
            pManaCost.color = Color.red;
        }
        else
        {
            if(pManaCost.color != Color.white)
            {
                pManaCost.color = Color.white;
            }
        }
        if (myResource.GetMana() < secondaryManaCost)
        {
            sManaCost.color = Color.red;
        }
        else
        {
            if (sManaCost.color != Color.white)
            {
                sManaCost.color = Color.white;
            }
        }
        if (myResource.GetMana() < ultimateManaCost)
        {
            uManaCost.color = Color.red;
        }
        else
        {
            if (uManaCost.color != Color.white)
            {
                uManaCost.color = Color.white;
            }
        }

        distanceText.text = "Distance: " + Vector3.Distance(GateKeeper.position, transform.position);

       
        
    }

    private void FixedUpdate()
    {
        if (!canMove) return;
        if (moving)
        {
            Move();
        }
        if (enemyTargeted && Vector3.Distance(GateKeeper.position, transform.position) > basicAttackRange)
        {
            moving = true;
            anim.SetBool("isMoving", true);
        }
        else if(enemyTargeted && !isAttacking)
        {
            Attack();
        }
    }

    void SetPosition()
    {
        //Cast a ray from screen to game world.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray,out hit, 100))
        {
            enemyTargeted = false;
            targetPosition = hit.point;
            lookAtTarget = new Vector3(targetPosition.x - transform.position.x, transform.position.y, targetPosition.z - transform.position.z);

            
            if(hit.collider.tag == "Enemy")
            {
                enemyTargeted = true;
                target = hit.collider.gameObject.transform;
                //if ray hits an enemy, and the distance is less than player basic attack range then instead of moving
                //attack the enemy.

                if (Vector3.Distance(target.position, transform.position)<= basicAttackRange)
                {
                    if (!isAttacking)
                    {
                        playerRot = Quaternion.LookRotation(lookAtTarget);
                        transform.rotation = playerRot;
                        Attack();
                    }                 
                    return;
                }
              

            }
            //Using navigation agent from unity, set the destination of the agent to the ray hit point
            playerAgent.isStopped = false;
            playerAgent.SetDestination(targetPosition);

            moving = true;
            anim.SetBool("isMoving", true);

            if (!enemyTargeted)
            {
                AttackFinished();
                SwordDamage sword = GetComponentInChildren<SwordDamage>();
                sword.SetCanDealDamage(false);
            }
        }
    }

    void Move()
    {
        if (isAttacking) return;

        if (enemyTargeted)
        {
            //Stop moving, rotate to face the target and attack
            if (Vector3.Distance(target.position, transform.position) <= basicAttackRange && enemyTargeted)
            {
                if (!playerAgent.isStopped)
                {
                    playerAgent.isStopped = true;
                }

                transform.rotation = Quaternion.LookRotation(new Vector3(target.position.x - transform.position.x,
                transform.position.y, target.position.z - transform.position.z));
                Attack();
                return;
            }
            else
            {
                //follow the target 
                if (playerAgent.isStopped)
                {
                    playerAgent.isStopped = false;
                }
                playerAgent.SetDestination(target.position);
            }       
        }

   
        if (playerAgent.remainingDistance <= playerAgent.stoppingDistance)
        {
            moving = false;
            anim.SetBool("isMoving", false);            
        }         
    }

    void Attack()
    {
        isAttacking = true;
        anim.SetBool("isAttacking", true);

        moving = false;
        anim.SetBool("isMoving", false);
        SwordDamage sword = GetComponentInChildren<SwordDamage>();
        sword.SetCanDealDamage(true);      
    }

    void AttackFinished()
    {
         anim.SetBool("isAttacking", false);
        isAttacking = false;      
    }

    public void PrimaryAttackEnabled()
    {
       
        ActivatePrimaryAbility.GetComponent<BoxCollider>().enabled = true;
        ActivatePrimaryAbility.transform.GetChild(0).gameObject.SetActive(true);
    }
    public void PrimaryAttackFinished()
    {
        BossBrain.instance.SetAttackDetected(false);
        abilityActive = false;
        anim.SetBool("isPrimaryAttack", false);
        ActivatePrimaryAbility.transform.GetChild(0).gameObject.SetActive(false);
        leftClick = false;
        ActivatePrimaryAbility.GetComponent<BoxCollider>().enabled = false;
        StartCoroutine(PrimaryCoolDown(primaryAbilityCD));
    }

    IEnumerator Dash()
    {
        Vector3 startMarker = transform.position;
        Vector3 endMarker = transform.position;

        RaycastHit hit;
        endMarker = abilityCanvas.transform.position;
        endMarker.y += 0.5f;

        if(Physics.Raycast(endMarker,abilityCanvas.transform.forward,out hit, 5))
        {
            endMarker = hit.point;
            endMarker.y = transform.position.y;
        }
        else
        {
            endMarker = transform.position + transform.forward * 5;
        }

        float journeyLength = Vector3.Distance(startMarker, endMarker);
        float speed = 15.0f;
        float startTime = Time.time;
        float distCovered = (Time.time - startTime) * speed;
        float fracJourney = distCovered / journeyLength;

        while (fracJourney <= 1)
        {
            if (hit.collider != null)
            {
                if (hit.collider.tag == "Enemy")
                {
                    if (Vector3.Distance(transform.position, endMarker) <= 0.2f)
                    {
                        fracJourney = 2;
                        
                    }
                }

            }
            yield return null;
            distCovered = (Time.time - startTime) * speed;
            fracJourney = distCovered / journeyLength;
         
            //GetComponentInParent<Rigidbody>().MovePosition(Vector3.Lerp(startMarker, endMarker, fracJourney));
            transform.position = Vector3.Lerp(startMarker, endMarker, fracJourney);      
        }

        EndDash();
    }

    public void EndDash()
    {
        StopCoroutine("Slide");
        dashTimer = Time.time;
        abilityActive = false;
        anim.SetBool("isDashing", false);
        leftClick = false;
        StartCoroutine(SecondaryCoolDown(dashAbilityCD));
    }

    public void FireUltimateAbility()
    {
        //anim.SetBool("isUltimateAttack",false);
        
        UltimateAbilityIndicator.transform.GetChild(0).gameObject.SetActive(true);
        UltimateAbilityIndicator.transform.GetChild(0).gameObject.GetComponent<DelayObjectMake>().Fire();
        StartCoroutine("FireUltimate");

    }
    IEnumerator FireUltimate()
    {
        yield return new WaitForSeconds(1.2f);
        anim.SetBool("ActivateSlash", true);
       

    }

    public void UltimateSlash()
    {
        if (inActiveAbility.childCount != 0)
        {
            GameObject proj = inActiveAbility.GetChild(0).gameObject;
            proj.SetActive(true);
            proj.transform.position = UltimateAbilityIndicator.transform.GetChild(0).transform.position;
            Quaternion rot = UltimateAbilityIndicator.transform.GetChild(0).transform.rotation;
            proj.transform.rotation = new Quaternion(rot.x, rot.y, rot.z, rot.w);
            proj.transform.SetParent(activeAbility);
            proj.GetComponent<TranslateMove>().Spawn();
            proj.GetComponent<AbilityTrigger>().damage = ultimateDamage;
            proj.GetComponent<CapsuleCollider>().enabled = true;
        }
    }

    public void EndAnimForSlash()
    {
        anim.SetBool("isUltimateAttack", false);
        anim.SetBool("ActivateSlash", false);
        abilityActive = false;
        leftClick = false;
        UltimateAbilityIndicator.transform.GetChild(0).gameObject.SetActive(false);
        StartCoroutine(UltimateCoolDown(ultimateAbilityCD));
    }

    IEnumerator PrimaryCoolDown(float value)
    {
        float timer = value;
        //float currentTime = Time.time - timer;

        while(timer > 0)
        {
            timer -= Time.deltaTime;
            primary.fillAmount = timer / value;
            yield return null;
           // currentTime = Time.time - timer;
        }
             
        primaryAbilityAvailable = true;
    }

    IEnumerator SecondaryCoolDown(float value)
    {
        float timer = value;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            secondary.fillAmount = timer / value;
            yield return null;
        }

        dashAbilityAvailable = true;
    }
    IEnumerator UltimateCoolDown(float value)
    {
        float timer = value;
        bool abilityDetected = false;
        while (timer > 0)
        {
            timer -= Time.deltaTime;

            if (!abilityDetected)
            {
                if (value - timer >= 3)
                {
                    BossBrain.instance.SetAttackDetected(false);
                    abilityDetected = true;
                }
            }
           
            ultimate.fillAmount = timer / value;
            yield return null;
        }

        ultimateAbilityAvailable = true;
    }

    private void PrepareForAbility()
    {
        transform.rotation = abilityCanvas.transform.rotation;
        abilityActive = true;
        moving = false;
        anim.SetBool("isMoving", false);
        playerAgent.isStopped = true;
        abilityCanvas.GetComponent<RotateIndicator>().SetActiveAbility(false);
        anim.SetBool("isAttacking", false);
        isAttacking = false;
        SwordDamage sword = GetComponentInChildren<SwordDamage>();
        sword.SetCanDealDamage(false);
        enemyTargeted = false;
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public void UltimateAwaken()
    {
        UltimateSkillSlot.SetActive(true);
        ultimateAbilityAvailable = true;
    }

    public void UpgradePrimaryAbility()
    {
        primaryAbilityCD -= (primaryAbilityCD * 15) / 100;
        primaryManaCost -= (primaryManaCost * 10) / 100;
        int damage = ActivatePrimaryAbility.GetComponent<AbilityTrigger>().damage;
        ActivatePrimaryAbility.GetComponent<AbilityTrigger>().damage += damage * 20 / 100;
        pManaCost.text = primaryManaCost.ToString();
        BossBrain.instance.UpgradeAbility(false);
    }
    public void UpgradeSecondaryAbility()
    {
        dashAbilityCD -= (dashAbilityCD * 18) / 100;
        secondaryManaCost -= (secondaryManaCost * 25) / 100;
        sManaCost.text = secondaryManaCost.ToString();
        BossBrain.instance.UpgradeAbility(false);

    }
    public void UpgradeUltimateAbility()
    {
        ultimateAbilityCD -= (ultimateAbilityCD * 20) / 100;
        ultimateManaCost -= (ultimateManaCost * 18) / 100;
        ultimateDamage += ultimateDamage * 15 / 100;
        uManaCost.text = ultimateManaCost.ToString();
        BossBrain.instance.UpgradeAbility(true);
    }
    public void ResetPosition()
    {
        transform.position = fountain.position;
        enemyTargeted = false;
    }

    public void RecordAbility(int id)
    {
        BossBrain.instance.RecordAbility(id);
    }
}
