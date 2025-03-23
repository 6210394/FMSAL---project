using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CombatScript : MonoBehaviour
{
    public enum CombatActionType
    {
        LightMelee, HeavyMelee, Shoot, Slash, Parry
    }

    [Header ("Stats")]
    public int health = 3;
    public int attackDamage = 1;

    [Header("Attack Values")]

    public float meleeRange;
    public float meleeReach;
    public float meleeDuration;
    public float meleeStunDuration;
    public float targetDistanceOffset = 1f;

    public float punchRange = 3f; //Range withing which the melee hits
    public float punchReach = 4f; //Range within which the attack will tween
    public float punchDuration = 0.5f; //Duration of the attack
    public float punchStunDuration = 0.3f; //Duration of the stun
    public float punchTargetDistanceOffset = 1f;

    [Space]
    public float gunHipFireBulletAccuracyRange = 10f;
    public float gunAimAssistSize = 1f;
    public float gunStunDuration;
    public float gunRateOfFireTime = 140f; //in round per minute

    [Header ("States")]

    //Stun & Resistance
    public bool isStunned = false;
    public bool stunImmune = false;

    //Parrying & Animation Lock
    bool isParrying = false;
    bool inParryWindow = false;

    [Space]
    public int currentAnimationComboChain = 0;
    public bool attackIsAvailable = true;
    public bool isAttacking = false;
    float attackCooldown = 0.5f;
    float attackCooldownTimer = 0f;
    
    public bool meleeEquipped = false;
    public bool gunEquipped = false;
    public bool junkEquipped;
    
    [Header ("Object & Component References ")]
    [SerializeField] Vector3 reticleOffset;
    public DiogenicInventory diogenicInventory;
    [SerializeField] GameObject bulletVisualsPrefab;
    [SerializeField] Transform bulletSpawnOriginOffset;

    [Space]
    public Animator animator;

        private List<GameObject> currentHurtboxReferences; //attack type, hurtbox


    [Header ("Coroutines")]
    public Coroutine CombatActionCoroutine;
    public Coroutine stunCoroutine;

    [SerializeField] public bool ultimateCanAttack = false; //debug variable


    public void Start()
    {
        attackCooldownTimer = 0;
        diogenicInventory = GetComponent<DiogenicInventory>();
        //animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        AttackCooldownCountdown();
    }

    public void ProcessAttackList(Dictionary<CombatActionType, int> stringOfAttacks)
    {
        //
    }

    public struct HitEventArgs
    {
        public int damageReceived;
        public float stunDuration;
        public float attackRange;
        public Transform damageSource;
    }

    public HitEventArgs BuildAttack(int damageReceived, float stunDuration, float attackRange, Transform damageSource)
    {
        HitEventArgs hitEventArgs;

        hitEventArgs.damageReceived = damageReceived;
        hitEventArgs.stunDuration = stunDuration;
        hitEventArgs.attackRange = attackRange;
        hitEventArgs.damageSource = damageSource;

        return hitEventArgs;
    }

    public GameObject BuildHurtbox(Transform parent, HitEventArgs attackInformation, GameObject hurtBox, int hurtboxIndex)
    {
        if(diogenicInventory.currentHeldWeapon.listOfAttacks.Count - 1 <= currentAnimationComboChain)
        {
            GameObject hurtboxInstance = Instantiate(hurtBox, parent);
            hurtboxInstance.GetComponent<HurtboxScript>()._hitEventArgs = attackInformation;

            hurtboxInstance.transform.localPosition = diogenicInventory.currentHeldWeapon.listOfAttacks[currentAnimationComboChain].hurtboxes[hurtboxIndex].hurtboxOffset;
            hurtboxInstance.transform.localRotation = diogenicInventory.currentHeldWeapon.listOfAttacks[currentAnimationComboChain].hurtboxes[hurtboxIndex].hurtboxRotationOffset;

            return hurtboxInstance;
        }
        else
        {
            Debug.LogError("The Combo index is too high! It is bigger than the list of attacks!");
            return null;
        }
    }

    public void CreateHurtbox(int hurtboxIndex)
    {
        HitEventArgs hitEventArgs = BuildAttack(attackDamage, meleeStunDuration, meleeRange, transform);
        GameObject hurtBox = BuildHurtbox(diogenicInventory.handAnchor.transform, hitEventArgs, gameObject, hurtboxIndex);
        if(hurtBox != null)
        {
            currentHurtboxReferences.Add(hurtBox); //CHANGE THIS TO USE THE PROVIDED PARENT IN THE ATTACK DATA
        }
    }

    public void DestroyHurtbox(int index)
    {
        Destroy(currentHurtboxReferences[index]);
        currentHurtboxReferences[index] = null;
    }

    public void ClearHurtboxes()
    {
        foreach(GameObject gameObject in currentHurtboxReferences)
        {
            Destroy(gameObject);
        }
        currentHurtboxReferences = null;
    }

    public void SwitchWeapons(int slot)
    {
        if(slot == 1)
        {
            if(diogenicInventory.mainWeapon)
            {
                diogenicInventory.ShowItemInHands(diogenicInventory.mainWeapon);
                UpdateStatsBasedOnWeapon(diogenicInventory.mainWeapon);   
            }
            else if (diogenicInventory.sidearm)
            {
                diogenicInventory.ShowItemInHands(diogenicInventory.sidearm);
                UpdateStatsBasedOnWeapon(diogenicInventory.sidearm);
            }
            else
            {
                //maybe drop whatever the player is holding if it was a pickup
                diogenicInventory.HideItemInHands();
                UpdateStatsBasedOnWeapon(null);
            }
        }

        if(slot == 2)
        {
            if (diogenicInventory.sidearm)
            {
                diogenicInventory.ShowItemInHands(diogenicInventory.sidearm);
                UpdateStatsBasedOnWeapon(diogenicInventory.sidearm);
            }

            else if(diogenicInventory.mainWeapon)
            {
                diogenicInventory.ShowItemInHands(diogenicInventory.mainWeapon);
                UpdateStatsBasedOnWeapon(diogenicInventory.mainWeapon);   
            }
            else
            {
                //maybe drop whatever the player is holding if it was a pickup
                diogenicInventory.HideItemInHands();
                UpdateStatsBasedOnWeapon(null);
            }
        }

        /*
        if(gunEquipped)
        {
            enemyDetection.autoLockOnRange = 10f;
        }
        else
        {
            enemyDetection.autoLockOnRange = 5f;
        }
        */
    }

    public void UpdateStatsBasedOnWeapon(WeaponScript weapon) //THIS NEEDS TO BE A WEAPON AND MUST BE VERIFIED
    {
        if(weapon != null)
        {
            attackDamage = weapon.damage;

            switch(weapon.weaponType)
            {
                case WeaponScript.WeaponType.Melee:
                {
                    meleeEquipped = true;
                    gunEquipped = false;

                    meleeRange = weapon.weaponRange;
                    meleeDuration = weapon.swingTime;
                    meleeStunDuration = weapon.stunTime;
                    meleeReach = weapon.weaponReach;
                    targetDistanceOffset = weapon.weaponTargetOffset;

                    float animationSpeed = 1f / (meleeDuration * 1.2f);
                    animator.SetFloat("MeleeSpeed", animationSpeed);
                    break;
                }

                case WeaponScript.WeaponType.Gun:
                {
                    gunEquipped = true;
                    gunAimAssistSize = weapon.weaponAimAssistValue;
                    gunRateOfFireTime = 60f / weapon.rateOfFire;
                    meleeEquipped = false;

                    float animationSpeed = weapon.rateOfFire / 60f;
                    animator.SetFloat("ShootSpeed", animationSpeed);
                    break;
                }
            }
        }

        else
        {
            attackDamage = 1;
            meleeRange = punchRange;
            meleeDuration = punchDuration;
            meleeStunDuration = punchStunDuration;
            meleeReach = punchReach;
            targetDistanceOffset = punchTargetDistanceOffset;
            meleeEquipped = true;
            gunEquipped = false;
        }
    }

    public void Attack(CombatActionType attackType, float specificAttackCooldown)
    {
        if(!isStunned && ultimateCanAttack)
        {
            attackCooldown = specificAttackCooldown;
            Debug.Log(diogenicInventory.currentHeldWeapon.listOfAttacks.Count + " attacks available for this weapon.");

            string animationTriggerName; //default animation trigger name
            if(currentAnimationComboChain <= diogenicInventory.currentHeldWeapon.listOfAttacks.Count - 1)
            {
                animationTriggerName = diogenicInventory.currentHeldWeapon.listOfAttacks[currentAnimationComboChain].animationTriggerName;
            }
            else
            {
                Debug.Log("Combo restart");
                currentAnimationComboChain = 0;
                animationTriggerName = diogenicInventory.currentHeldWeapon.listOfAttacks[currentAnimationComboChain].animationTriggerName;
            }

            switch(attackType)
            {
                case CombatActionType.LightMelee:
                {
                    if(CombatActionCoroutine != null) //action override is allowed
                    {
                        StopCoroutine(CombatActionCoroutine);
                    }
                    CombatActionCoroutine = StartCoroutine(ILightMelee(animationTriggerName));
                    break;
                }
                case CombatActionType.Shoot:
                {
                    if(CombatActionCoroutine != null) //action override is allowed
                    {
                        StopCoroutine(CombatActionCoroutine);
                    }
                    CombatActionCoroutine = StartCoroutine(IShoot(animationTriggerName));
                    break;
                }
                case CombatActionType.Parry:
                {
                    if(CombatActionCoroutine == null) //action override is not allowed
                    {
                        StopCoroutine(CombatActionCoroutine);
                    }
                    CombatActionCoroutine = StartCoroutine(IParry(animationTriggerName));
                    break;
                }
            }

            animator.SetInteger("ComboValue", currentAnimationComboChain);
            //currentAnimationComboChain += 1;
            Debug.Log(currentAnimationComboChain);
        }
        else if (!ultimateCanAttack)
        {
            Debug.LogWarning("debugCanAttack is set to false!!");
        }
    }

    public IEnumerator ILightMelee(string animationName)
    {
        isAttacking = true;
        attackIsAvailable = false;

        animator.SetTrigger(animationName);
        yield return new WaitUntil(() => attackIsAvailable = true);
        
        isAttacking = false;
        yield return new WaitForSeconds(0.7f);
    }

    public IEnumerator IShoot(string animationName)
    {
        isAttacking = true;

        animator.SetTrigger(animationName);
        yield return new WaitUntil(() => attackIsAvailable = true);

        isAttacking = false;
    }

    public IEnumerator IParry(string animationName)
    {
        animator.SetTrigger(animationName);
        isParrying = true;
        yield return new WaitForEndOfFrame();

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitWhile(() => stateInfo.normalizedTime < 1.0f);
        isParrying = false;
    }

    public void AttackCancel()
    {
        isAttacking = false;
        attackIsAvailable = true;
        //give control back to the player/enemy that is trying to move
    }

    public void AttackCooldownCountdown()
    {
        if (isAttacking && attackCooldownTimer <= 0)
        {
            attackCooldownTimer = attackCooldown;
        }

        if (attackCooldownTimer > 0)
        {
            attackIsAvailable = false;
            attackCooldownTimer -= Time.deltaTime;
            
            if (attackCooldownTimer <= 0)
            {
                attackCooldownTimer = 0;
                attackIsAvailable = true;
                Debug.LogWarning("Attack Cooldown Over!");
            }
        }
    }

    public void Parry()
    {
        isParrying = !isParrying;
    }

    public void Stun(float time)
    {
        stunCoroutine = StartCoroutine(IStunned(time));
    }

    public IEnumerator IStunned(float time)
    {
        isStunned = true;
        yield return new WaitForSeconds(time);
        isStunned = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if(bulletSpawnOriginOffset)
        {
            Gizmos.DrawSphere(transform.position + transform.rotation * bulletSpawnOriginOffset.position, 0.1f);
        }
    }
}
