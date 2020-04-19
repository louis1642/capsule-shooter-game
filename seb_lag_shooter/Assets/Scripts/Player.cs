using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]      //  forzo la presenza di PlayerController
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity {
    public float moveSpeed = 5;

    public Crosshairs crosshairs;

    Camera viewCamera;
    PlayerController playerController;
    GunController gunController;

    private void Awake() {
        playerController = GetComponent<PlayerController>();      //  assumo che PlayerController sia attaccato allo stesso oggetto a cui è attaccato questo script
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    protected override void Start() {
        base.Start();
    }

    public void OnNewWave(int waveNum) {
        health = startingHealth;
        gunController.EquipGun(waveNum - 1);
    }

    void Update() {
        //  MOVEMENT INPUT
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        playerController.Move(moveVelocity);      //  questa funzione sarà nel controller

        //  LOOK INPUT
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance)) {
            Vector3 point = ray.GetPoint(rayDistance);
            //  Debug.DrawLine(ray.origin, point, Color.red);
            playerController.LookAt(point);
            crosshairs.transform.position = point;
            crosshairs.DetectTargets(ray);

            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 2) {
                gunController.Aim(point);
            }
        }

        //  DIE IF FALLING
        if (transform.position.y < -10) {       //  TODO: smooth damage
            TakeDamage(health);
        }


        //  WEAPON INPUT
        if (Input.GetMouseButton(0)) {
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0)) {
            gunController.OnTriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            gunController.Reload();
        }

    }

    public override void Die() {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }
}
