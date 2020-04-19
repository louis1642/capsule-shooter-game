using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity {

    public enum State { Idle, Chasing, Attacking };
    State currentState;

    public ParticleSystem deathEffect;
    ParticleSystem.MainModule deathEffectMainM;

    public static event System.Action OnDeathStatic;

    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;
    Material material;
    Color originalColor;

    float attackDistanceTreshold = .5f;
    float timerBetweenAttack = 1;
    float nextAttackTime;
    float damage = 1;

    float myCollisionRadius, targetCollisionRadius;

    bool hasTarget;

    private void Awake() {
        pathfinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null) {
            target = GameObject.FindGameObjectWithTag("Player").transform;
            hasTarget = true;

            targetEntity = target.GetComponent<LivingEntity>();


            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }

    protected override void Start() {
        base.Start();                   //  questo serve a chiamare la funzione Start() di LivingEntity, che è stata sovrascritta da questa funzione

        if (hasTarget) {
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;
            StartCoroutine(UpdatePath());
        }
    }

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor) {
        pathfinder.speed = moveSpeed;

        if (hasTarget) {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }
        startingHealth = enemyHealth;

        deathEffectMainM = deathEffect.main;
        deathEffectMainM.startColor = new Color(skinColor.r, skinColor.g, skinColor.b, 1);
        material = GetComponent<Renderer>().material;     //  affetcs all instances of the material
        material.color = skinColor;
        originalColor = material.color;
    }


    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection) {

        AudioManager.instance.PlaySound("Impact", transform.position);
        if (damage >= health && !dead) {
            if (OnDeathStatic != null) {
                OnDeathStatic();
            }
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
#pragma warning disable CS0618 // Il tipo o il membro è obsoleto
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.startLifetime);
#pragma warning restore CS0618 // Il tipo o il membro è obsoleto
        }
        base.TakeHit(damage, hitPoint, hitDirection);

    }

    void OnTargetDeath() {
        hasTarget = false;
        currentState = State.Idle;
    }

    private void Update() {
        if (hasTarget) {
            if (Time.time > nextAttackTime) {
                float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;

                if (sqrDistanceToTarget < Mathf.Pow(attackDistanceTreshold + myCollisionRadius + targetCollisionRadius, 2)) {
                    nextAttackTime = Time.time + timerBetweenAttack;
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack() {
        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - directionToTarget * (myCollisionRadius);

        float percent = 0;
        float attackSpeed = 3;

        material.color = Color.red;
        bool hasAppliedDamage = false;

        while (percent <= 1) {

            if (percent >= .5f && !hasAppliedDamage) {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }
        currentState = State.Chasing;
        pathfinder.enabled = true;
        material.color = originalColor;
    }

    IEnumerator UpdatePath() {
        float refreshRate = .25f;

        while (hasTarget) {
            if (currentState == State.Chasing) {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceTreshold / 2);
                if (!dead) {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }

}
