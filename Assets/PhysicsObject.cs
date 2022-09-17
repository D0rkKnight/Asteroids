using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    public float linearDrag = 1f;
    public float angularSlowdown = 1f;

    public float spinVelo;
    public Vector2 moveVelo;
    public float maxSpinVelo = 100f;
    public float maxMoveVelo = 100f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Limiters
        if (moveVelo.magnitude > maxMoveVelo)
            moveVelo *= maxMoveVelo / moveVelo.magnitude;
        if (Mathf.Abs(spinVelo) > maxSpinVelo)
            spinVelo *= maxSpinVelo / Mathf.Abs(spinVelo);

        // Drag
        float drag = Time.deltaTime * linearDrag;
        drag = Mathf.Min(drag, Mathf.Abs(moveVelo.magnitude));
        moveVelo -= moveVelo.normalized * drag;

        spinVelo = Mathf.Lerp(spinVelo, 0, angularSlowdown * Time.deltaTime);

        // Apply velocities
        transform.Rotate(Vector3.back, Time.deltaTime * spinVelo);
        transform.position += (Vector3)moveVelo * Time.deltaTime;
    }
}
