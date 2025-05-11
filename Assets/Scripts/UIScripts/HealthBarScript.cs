using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
	public Slider slider;
	public Gradient gradient;
	public Image fill;
    public Animator animator;

    HealthScript playerHealthScript;

    void Start()
    {
        playerHealthScript = FindFirstObjectByType<PlayerCombatController>().GetComponent<HealthScript>();
        SetMaxHealth(playerHealthScript.maxHealth);
        playerHealthScript.OnTakeDamage.AddListener(TakeDamage);
    }

    public void SetMaxHealth(int health)
	{
		slider.maxValue = health;
		slider.value = health;

		fill.color = gradient.Evaluate(1f);
	}

    public void TakeDamage(CombatScript.HitEventArgs hitEventArgs)
	{
		slider.value -= hitEventArgs.damageReceived;

		fill.color = gradient.Evaluate(slider.normalizedValue);
        animator.SetTrigger("TakeDamage");

        if(playerHealthScript.currentHealth == 1)
        {
            animator.SetBool("CloseToDeath", true);
        }
	}

}