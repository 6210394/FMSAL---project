using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class CombatScript : MonoBehaviour
{
    public enum CombatActionType
    {
        LightMelee, HeavyMelee, Shoot, Slash, Parry
    }

    [Header ("Stats")]
    public int attackDamage = 1;

    [Header("Attack Values")]

    public float meleeReach;
    public float meleeDuration;
    public float meleeStunDuration;
    public float targetDistanceOffset = 1f;

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
    //bool inParryWindow = false;
    [Space]
    public int currentAnimationComboChain = 0;
    public int upcomingAnimationComboChain = 0;
    [Space]
    public bool attackIsAvailable = true;
    public bool isAttacking = false;
    float attackCooldown = 0.5f;
    float attackCooldownTimer = 0f;
    
    public bool meleeEquipped = false;
    public bool gunEquipped = false;
    public bool junkEquipped;
    
    [Header ("Object & Component References ")]
    public HealthScript healthScript;
    public MovementScript movementScript;
    public DiogenicInventory diogenicInventory;

    [SerializeField] Vector3 reticleOffset;
    [SerializeField] GameObject bulletVisualsPrefab;
    [SerializeField] Transform bulletSpawnOriginOffset;

    [Space]
    public Animator animator;

    public List<GameObject> currentHurtboxReferences;

    public UnityEvent OnAttackCompleted;

    [Header ("Coroutines")]
    public Coroutine CombatActionCoroutine;
    public Coroutine StunCoroutine;

    [SerializeField] public bool ultimateCanAttack = false; //debug variable

    void Awake()
    {
        diogenicInventory = GetComponent<DiogenicInventory>();
        healthScript = GetComponent<HealthScript>();
    }

    public void Start()
    {
        attackCooldownTimer = 0;
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
        public float cameraShakeAmplitude;
        public Transform damageSource;
    }

    public HitEventArgs BuildAttack(int damageReceived, float stunDuration, float cameraShakeAmplitude, Transform damageSource)
    {
        HitEventArgs hitEventArgs;

        hitEventArgs.damageReceived = damageReceived;
        hitEventArgs.stunDuration = stunDuration;
        hitEventArgs.cameraShakeAmplitude = cameraShakeAmplitude;
        hitEventArgs.damageSource = damageSource;

        return hitEventArgs;
    }

    public GameObject BuildHurtbox(Transform parent, HitEventArgs attackInformation, GameObject hurtBox, int hurtboxIndex)
    {
        if(currentAnimationComboChain <= diogenicInventory.currentHeldWeapon.listOfAttacks.Count - 1)
        {
            GameObject hurtboxInstance = Instantiate(hurtBox, parent);
            hurtboxInstance.GetComponent<HurtboxScript>()._hitEventArgs = attackInformation;
            hurtboxInstance.GetComponent<HurtboxScript>().particleEffect = diogenicInventory.currentHeldWeapon.listOfAttacks[currentAnimationComboChain].particleEffect;

            hurtboxInstance.transform.localPosition = diogenicInventory.currentHeldWeapon.listOfAttacks[currentAnimationComboChain].hurtboxes[hurtboxIndex].hurtboxOffset;
            hurtboxInstance.transform.localRotation = diogenicInventory.currentHeldWeapon.listOfAttacks[currentAnimationComboChain].hurtboxes[hurtboxIndex].hurtboxRotationOffset;
            hurtboxInstance.transform.localScale = diogenicInventory.currentHeldWeapon.listOfAttacks[currentAnimationComboChain].hurtboxes[hurtboxIndex].hurtboxScale;

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
        HitEventArgs hitEventArgs = BuildAttack(attackDamage, meleeStunDuration,diogenicInventory.currentHeldWeapon.listOfAttacks[currentAnimationComboChain].hitImpactCameraShakeAmplitude , transform);
        
        GameObject hurtBox = BuildHurtbox(diogenicInventory.handAnchor.transform, hitEventArgs, diogenicInventory.currentHeldWeapon.listOfAttacks[currentAnimationComboChain].hurtboxes[hurtboxIndex].hurtboxGameobject, hurtboxIndex);

        //CHANGE THIS TO USE THE PROVIDED PARENT IN THE ATTACK DATA
        if(hurtBox != null)
        {
            currentHurtboxReferences.Add(hurtBox);
        }
        else
        {
            Debug.Log("Hurtbox reference is null!");
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
        currentHurtboxReferences.Clear();
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

                    meleeDuration = weapon.swingTime;
                    meleeStunDuration = weapon.stunTime;
                    meleeReach = weapon.weaponReach;
                    targetDistanceOffset = weapon.weaponTargetOffset;

                    float animationSpeed = 1f / (meleeDuration * 1.2f);
                    animator.SetFloat("MeleeSpeedMod", animationSpeed);
                    break;
                }

                case WeaponScript.WeaponType.Gun:
                {
                    gunEquipped = true;
                    gunAimAssistSize = weapon.weaponAimAssistValue;
                    gunRateOfFireTime = 60f / weapon.rateOfFire;
                    meleeEquipped = false;

                    float animationSpeed = weapon.rateOfFire / 60f;
                    animator.SetFloat("ShootSpeedMod", animationSpeed);
                    break;
                }
            }
        }

        else
        {
            attackDamage = 1;
            meleeDuration = punchDuration;
            meleeStunDuration = punchStunDuration;
            meleeReach = punchReach;
            targetDistanceOffset = punchTargetDistanceOffset;
            meleeEquipped = true;
            gunEquipped = false;
        }
    }

    public void Attack(CombatActionType attackType)
    {
        List<AttackAnimationData> animationDataList = diogenicInventory.currentHeldWeapon.listOfAttacks;

        if(!isStunned && attackIsAvailable && ultimateCanAttack)
        {
            
            currentAnimationComboChain = upcomingAnimationComboChain;

            string animationTriggerName; //default animation trigger name
            if(currentAnimationComboChain <= diogenicInventory.currentHeldWeapon.listOfAttacks.Count - 1)
            {
                attackCooldown = animationDataList[currentAnimationComboChain].animationEndCooldown;
                animationTriggerName = animationDataList[currentAnimationComboChain].animationTriggerName;
            }
            else
            {
                currentAnimationComboChain = 0;
                upcomingAnimationComboChain = 0;
                animationTriggerName = animationDataList[currentAnimationComboChain].animationTriggerName;
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

        animator.SetInteger("ComboIndex", currentAnimationComboChain);
        animator.SetTrigger(animationName);
        yield return new WaitUntil(() => attackIsAvailable = true); //wait for the attack cooldown to be over

        upcomingAnimationComboChain += 1;
        
        yield return new WaitForSeconds(attackCooldown);
        currentAnimationComboChain = 0;
        upcomingAnimationComboChain = 0;
        animator.SetInteger("ComboIndex", currentAnimationComboChain);
        isAttacking = false;
        OnAttackCompleted.Invoke();
    }

    public IEnumerator IShoot(string animationName)
    {
        isAttacking = true;

        animator.SetTrigger(animationName);
        yield return new WaitUntil(() => attackIsAvailable = true);

        isAttacking = false;
        OnAttackCompleted.Invoke();
    }

    public void HitScan(HealthScript healthScript)
    {
        
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
            }
        }
    }

    public float GetAttackCooldown(CombatActionType attackType)
    {
        switch (attackType)
        {
            case CombatActionType.LightMelee:
                return meleeDuration;
            case CombatActionType.Shoot:
                return gunRateOfFireTime;
            case CombatActionType.Parry:
                return 0; // Adjust as needed
            default:
                return 0.5f; // Default cooldown
        }
    }

    public void Parry()
    {
        isParrying = !isParrying;
    }

    public void Stun(float time)
    {
        StunCoroutine = StartCoroutine(IStunned(time));
    }

    public IEnumerator IStunned(float time)
    {
        isStunned = true;
        
        ClearHurtboxes();
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
