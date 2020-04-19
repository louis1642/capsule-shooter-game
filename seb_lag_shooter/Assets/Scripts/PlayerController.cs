using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PlayerController : MonoBehaviour {

    Rigidbody rb;
    Vector3 _velocity;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }


    public void LookAt(Vector3 point) {
        transform.LookAt(new Vector3(point.x, transform.position.y, point.z));
    }

    public void Move(Vector3 velocity) {
        _velocity = velocity;
    }

    void FixedUpdate() {
        rb.MovePosition(rb.position + _velocity * Time.fixedDeltaTime);
    }
}
