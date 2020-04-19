using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    
    public LayerMask collisionMask;
    public Color trailColor;

    float projectileSpeed = 10;
    float damage = 1;
    float lifetime = 3;
    float skinWidth = .1f;

    private void Start() {
        Destroy(gameObject, lifetime);

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);       //  se il proiettile spawna all'interno di un collider non è in grado di detectarlo con un raycast

        if (initialCollisions.Length > 0) {
            OnHitObject(initialCollisions[0], transform.position);
        }

        GetComponent<Renderer>().material.SetColor("_TintColor", trailColor);
    }

    public void SetSpeed(float newSpeed) {
        projectileSpeed = newSpeed;
    }

    void Update() {
        float moveDistance = projectileSpeed * Time.deltaTime;
        CheckCollision(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    void CheckCollision(float distance) {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance + skinWidth, collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitObject(hit.collider, hit.point);
        }
    }



    void OnHitObject(Collider c, Vector3 hitPoint) {
        IDamageable damageableObj = c.GetComponent<IDamageable>();
        if (damageableObj != null) {
            damageableObj.TakeHit(damage, hitPoint, transform.forward);
        }
        GameObject.Destroy(gameObject);
    }

}

