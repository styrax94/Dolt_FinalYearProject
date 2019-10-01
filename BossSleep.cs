using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSleep : MonoBehaviour {

    BossBrain bossBrain;
    Vector3 startPosition;
    Quaternion startRotation;
    ResourceManager myResources;
    Animator bossAnim;

    public GameObject body;

	void Start () {
        bossBrain = GetComponentInChildren<BossBrain>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        myResources = GetComponent<ResourceManager>();
        //bossAnim.GetComponentInChildren<Animator>();

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void InitiateBossFight()
    {
        bossBrain.enabled = true;
        myResources.canBeDamaged = true;
    }

    public void DisActivateBossFight()
    {
        myResources.canBeDamaged = false;
        myResources.Recover();
        bossBrain.StartCoroutine("DisableState");
       
        transform.position = startPosition;
        transform.rotation = startRotation;
        body.transform.position = startPosition;
        body.transform.rotation = startRotation;
    }
}
