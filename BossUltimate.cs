using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UltimateOne")]
public class BossUltimate : Ability
{

    public GameObject[] ultimateAttack;
    public Transform ultimateActive;
    public Transform ultimateInActive;
    public GameObject owner;
    Vector3 pointOfActivation;
    public float interval;
    public int makeCount;

    public TidalWave tidalWave;
   
    public float delay;
    float m_Time;
    float time;
    int count;
    public float scaleValue;

    public int totalWaves;
    int currentWave;

    public override void Initialize()
    {
       
        //Pooling objects
        for (int i = 0; i < 30; i++)
        {
            foreach(GameObject prefabs in ultimateAttack)
            {
                GameObject proj = (GameObject)Object.Instantiate(prefabs, Vector3.zero, Quaternion.identity, ultimateInActive);
                // GameObject effect = (GameObject)Object.Instantiate(PrimaryEffect, Vector3.zero, Quaternion.identity, proj.transform);
                //effect.GetComponent<ParticleSystem>().Stop();
                proj.transform.localScale *= scaleValue;
                proj.SetActive(false);

                if (proj.GetComponent<UltimateTrigger>())
                proj.GetComponent<UltimateTrigger>().caster = owner;
            }
          
        }


    }

    public override void TriggerAbility(Vector3 target)
    {
            pointOfActivation = target;
            time = Time.time;
            currentWave = 0;
            interval = 3;
            makeCount = 6;
            delay = 0f;
            count = 0;
            
            tidalWave.StartCoroutine(MakeWaveOne());
            
    }

    IEnumerator MakeWaveOne()
    {
        //yield return null;
        //Debug.Log("Doesnt Crash");
        while (currentWave < totalWaves)
        {
            yield return null;
            while (count < makeCount)
            {
                yield return null;
                if (Time.time < time + delay)
                {
                    
                }
                else if (count < makeCount)
                {
                    float Angle = 2.0f * Mathf.PI / makeCount * count;
                    float pos_X = Mathf.Cos(Angle) * interval;
                    float pos_Z = Mathf.Sin(Angle) * interval;


                    for (int i = 0; i < ultimateAttack.Length; i++)
                    {
                        GameObject proj = ultimateInActive.GetChild(0).gameObject;
                        proj.transform.position = pointOfActivation + new Vector3(pos_X, 0, pos_Z);
                        proj.transform.rotation = Quaternion.LookRotation(new Vector3(pos_X, 0, pos_Z)) * ultimateAttack[i].transform.rotation;
                        //GameObject m_obj = Instantiate(ultimateAttack[i], owner.position + new Vector3(pos_X, 0, pos_Z),
                        proj.transform.parent = ultimateActive;
                        proj.SetActive(true);
                        UltimateTrigger trigger = proj.GetComponent<UltimateTrigger>();
                        if (trigger)
                        {
                            trigger.canDoDamage = true;
                        }
                    }
                    count++;
                }
            }

            currentWave++;
            interval += 2;
            makeCount += 4;
            delay += .20f;
            time = Time.time;
            count = 0;
        }
        tidalWave.StartCoroutine(SetInActive());
    }

    IEnumerator SetInActive()
    {
       
        int count = ultimateActive.childCount;
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < count; i++)
        {
            GameObject proj = ultimateActive.GetChild(0).gameObject;
            proj.SetActive(false);
            proj.transform.SetParent(ultimateInActive);

        }
      
    }

    public void GetInfo(Transform activePool, Transform inActivePool, GameObject o, TidalWave tidal)
    {
        ultimateActive = activePool;
        ultimateInActive = inActivePool;
        owner = o;
        tidalWave = tidal;
    }
}
