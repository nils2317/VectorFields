using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorFieldParticle : MonoBehaviour
{
    public Vector3 acceleration;
    public Vector3 velocity;
    public Vector3 position;

    // Start is called before the first frame update
    void Start()
    {
        acceleration = Vector3.zero;
        velocity = Vector3.zero;
        position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        velocity += acceleration * Time.deltaTime;
        position += velocity * Time.deltaTime;

        transform.position = position;
    }

    public void ApplyAcceleration(Vector3 newAcceleration)
    {
        acceleration = newAcceleration;
    }
}
