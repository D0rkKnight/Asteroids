using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    public PhysicsProfile profile;

    public float spinVelo;
    public Vector2 moveVelo;
    public bool pushRotable = false; // Some items are spun around when force is exerted upon them

    // Start is called before the first frame update
    void Start()
    {
        if (profile == null)
            profile = ScriptableObject.CreateInstance<PhysicsProfile>();
    }

    // Update is called once per frame
    void Update()
    {
        // Hard Limiters
        if (moveVelo.magnitude > profile.maxMoveVelo)
            moveVelo *= profile.maxMoveVelo / moveVelo.magnitude;
        if (Mathf.Abs(spinVelo) > profile.maxSpinVelo)
            spinVelo *= profile.maxSpinVelo / Mathf.Abs(spinVelo);

        // Soft limiters
        float effLinDrag = profile.linearDrag;
        float angLinDrag = profile.angularSlowdown;

        if (moveVelo.magnitude > profile.softMaxMoveVelo)
            effLinDrag = profile.overcapLinearDrag;

        // Drag
        float drag = Time.deltaTime * effLinDrag;
        drag = Mathf.Min(drag, Mathf.Abs(moveVelo.magnitude));
        moveVelo -= moveVelo.normalized * drag;

        spinVelo = Mathf.Lerp(spinVelo, 0, angLinDrag * Time.deltaTime);

        // Apply velocities
        transform.Rotate(Vector3.back, Time.deltaTime * spinVelo);
        transform.position += (Vector3)moveVelo * Time.deltaTime;
    }
}
