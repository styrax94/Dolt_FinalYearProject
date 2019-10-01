using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour {

    public float maxHealth;
    public float maxMana;
    float currentHealth;
    float currentMana;
    public float manaRegen;
    bool regenMana;

    public RectTransform healthBar;
    public RectTransform manaBar;
    float barWidth;
    public UnityEvent OnDeath;
    //ScreenUI
    public bool hasScreenUI;
    public Text currentHPText;
    public Text currentManaText;
    public Image screenHPBar;
    public Image screenManaBar;

    public bool canBeDamaged;

    void Start ()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        regenMana = false;
        barWidth = healthBar.sizeDelta.x;

        if (hasScreenUI)
        {
            currentHPText.text = maxHealth.ToString();
            currentManaText.text = maxMana.ToString();
        }

       
	}
	
	// Update is called once per frame
	IEnumerator Regen()
    {
        regenMana = true;
       while( currentMana != maxMana)
        {
            yield return new WaitForSeconds(1f);
            currentMana += manaRegen;
            if (currentMana > maxMana) currentMana = maxMana;
            manaBar.sizeDelta = new Vector2((currentMana * barWidth) / maxMana, manaBar.sizeDelta.y);

            if (hasScreenUI)
            {
                screenManaBar.fillAmount = (float)currentMana / maxMana;
                currentManaText.text = currentMana.ToString();
            }
            
        }
        regenMana = false;	
	}
    
    public void TakeDamage(int amount)
    {
        if (!canBeDamaged) return;
        currentHealth -= amount;

        if(currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath.Invoke();
        }

        healthBar.sizeDelta = new Vector2((currentHealth * barWidth)/maxHealth, healthBar.sizeDelta.y);

        if (hasScreenUI)
        {
            screenHPBar.fillAmount = (float)currentHealth / maxHealth;
            currentHPText.text = currentHealth.ToString();
        }
        
    }

    public void UseMana(int amount)
    {
        currentMana -= amount;

        if(currentMana <= 0)
        {
            currentMana = 0;
        }

        manaBar.sizeDelta = new Vector2((currentMana * barWidth) / maxMana, manaBar.sizeDelta.y);
        if (hasScreenUI)
        {
            screenManaBar.fillAmount = (float)currentMana / maxMana;
            currentManaText.text = currentMana.ToString();
        }

        if (!regenMana)
        StartCoroutine("Regen");      
    }

    public float GetMana()
    {
        return currentMana; 
    }

    public float GetHealth()
    {
        return currentHealth;
    }
    public float GetManaPercentage()
    {
        return currentMana / maxMana * 100;

    }
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth * 100;

    }

    public float GetTotalTimeTillMaxMana()
    {
        return (maxMana - currentMana) / manaRegen;
    }
    
    public void Recover()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        healthBar.sizeDelta = new Vector2((currentHealth * barWidth) / maxHealth, healthBar.sizeDelta.y);
        manaBar.sizeDelta = new Vector2((currentMana * barWidth) / maxMana, manaBar.sizeDelta.y);

        if (hasScreenUI)
        {
            screenManaBar.fillAmount = (float)currentMana / maxMana;
            currentManaText.text = currentMana.ToString();          
            screenHPBar.fillAmount = (float)currentHealth / maxHealth;
            currentHPText.text = currentHealth.ToString();         
        }
    }
}
