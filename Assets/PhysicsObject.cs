using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    public PhysicsProfile profile;

    public float spinVelo;
    public Vector2 moveVelo;

    // Start is called before the first frame update
    void Start()
    {
        if (profile == null)
            profile = ScriptableObject.CreateInstance<PhysicsProfile>();
    }

    // Update is called once per frame
    void Update()
    {
        // Limiters
        if (moveVelo.magnitude > profile.maxMoveVelo)
            moveVelo *= profile.maxMoveVelo / moveVelo.magnitude;
        if (Mathf.Abs(spinVelo) > profile.maxSpinVelo)
            spinVelo *= profile.maxSpinVelo / Mathf.Abs(spinVelo);

        // Drag
        float drag = Time.deltaTime * profile.linearDrag;
        drag = Mathf.Min(drag, Mathf.Abs(moveVelo.magnitude));
        moveVelo -= moveVelo.normalized * drag;

        spinVelo = Mathf.Lerp(spinVelo, 0, profile.angularSlowdown * Time.deltaTime);

        // Apply velocities
        transform.Rotate(Vector3.back, Time.deltaTime * spinVelo);
        transform.position += (Vector3)moveVelo * Time.deltaTime;
    }
}
