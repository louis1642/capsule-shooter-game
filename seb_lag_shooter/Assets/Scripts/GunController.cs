    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {

    public Transform weaponHold;
    public Gun startingGun;
    public Gun[] allGuns;

    Gun equippedGun;


    private void Start() {
        if (startingGun != null) {
            EquipGun(startingGun);
        }
    }

    public void OnTriggerHold() {
        if (equippedGun != null) {
            equippedGun.OnTriggerHold();
        }
    }

    public void OnTriggerRelease() {
        if (equippedGun != null) {
            equippedGun.OnTriggerRelease();
        }
    }

    public void EquipGun(Gun gunToEquip) {
        if (equippedGun != null) {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation, weaponHold) as Gun;
    }

    public void EquipGun(int weaponIndex) {
        EquipGun(allGuns[weaponIndex]);
    }

    public float GunHeight {
        get {
            return weaponHold.position.y;
        }
    }

    public void Aim(Vector3 point) {
        if (equippedGun != null) {
            equippedGun.Aim(point);
        }
    }

    public void Reload() {
        if (equippedGun != null) {
            equippedGun.Reload();
        }
    }
}
