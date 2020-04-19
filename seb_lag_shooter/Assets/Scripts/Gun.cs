using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

    public enum FireMode { Auto, Burst, Single };

    [Header("Shooting Options")]
    public FireMode firemode;

    public Transform[] projectileSpawn;
    public Projectile bullet;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;
    public int burstCount = 4;

    public int projPerMag = 20;
    public float reloadTime = .3f;
    public float maxReloadAngle = 30;
    int projRemainingInMag;
    bool isReloading;

    [Header("Effects")]
    public Transform shell;
    public Transform shellEject;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;

    MuzzleFlash muzzleFlash;

    float nextShotTime;
    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;

    [Header("Recoil")]
    public Vector2 kickMinMax = new Vector2(.05f, .2f);
    public Vector2 recoilAngleMinMax = new Vector2(7, 10);
    public float recoilMoveSettleTime = .1f;
    public float recoilRotSettleTime = .1f;
    Vector3 recoilSmoothDampVelocity;
    float recoilRotationSmoothDampVel;
    float recoilAngle;


    private void Start() {
        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        projRemainingInMag = projPerMag;
    }

    private void LateUpdate() {         //  LateUpdate viene dopo Aim(), così l'animazione di recoil viene processata dopo la mira
        //  animate recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotationSmoothDampVel, recoilRotSettleTime);
        transform.localEulerAngles += Vector3.left * recoilAngle;

        if(!isReloading && projRemainingInMag == 0) {
            Reload();
        }
    }

    void Shoot() {
        if (!isReloading && Time.time > nextShotTime && projRemainingInMag > 0) {
            if (firemode == FireMode.Burst) {
                if (shotsRemainingInBurst == 0) {
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if (firemode == FireMode.Single) {
                if (!triggerReleasedSinceLastShot) {
                    return;
                }
            }

            foreach (Transform p in projectileSpawn) {
                if (projRemainingInMag == 0) {
                    break;
                }
                projRemainingInMag--;
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newBullet = Instantiate(bullet, p.position, p.rotation) as Projectile;
                newBullet.SetSpeed(muzzleVelocity);
            }

            Instantiate(shell, shellEject.position, shellEject.rotation);
            muzzleFlash.Activate();
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
    }

    public void Reload() {
        if (!isReloading && projRemainingInMag != projPerMag) {
            StartCoroutine(ReloadAnim());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);

        }
    }

    IEnumerator ReloadAnim() {
        isReloading = true;
        yield return new WaitForSeconds(.2f);

        float reloadSpeed = 1 / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        // float maxReloadAngle = 30;

        while (percent < 1) {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        projRemainingInMag = projPerMag;
    }

    public void Aim(Vector3 point) {
        if (!isReloading) {
            transform.LookAt(point);
        }
    }

    public void OnTriggerHold() {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease() {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;

    }

}

