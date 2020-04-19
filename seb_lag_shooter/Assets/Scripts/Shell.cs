using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour {

    public Vector2 forceMinMax;

    Rigidbody rb;
    float lifetime = 4;
    float fadetime = 2;

    void Start() {
        float force = Random.Range(forceMinMax.x, forceMinMax.y);
        rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.right * force);
        rb.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
    }

    IEnumerator Fade() {
        yield return new WaitForSeconds(lifetime);

        float percent = 0;
        float fadespeed = 1 / fadetime;
        Material mat = GetComponent<Renderer>().material;
        Color initial = mat.color;

        while (percent < 1) {
            percent += Time.deltaTime * fadespeed;
            mat.color = Color.Lerp(initial, Color.clear, percent);
            yield return null;
        }

        Destroy(gameObject);
    }
}
