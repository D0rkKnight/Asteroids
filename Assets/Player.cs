using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float spinSpeed = 1f;
    public float moveSpeed = 1f;
    public float linearDrag = 1f;
    public float angularSlowdown = 1f;

    public float spinVelo;
    public Vector2 moveVelo;

    public float maxSpinVelo = 100f;
    public float maxMoveVelo = 100f;

    public PlayerInput input;
    public Vector2 heldXY;

    public Bullet bulletPrefab;
    public float bulletTravelSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Spin
        int hSign = (int) Mathf.Sign(heldXY.x);
        if (Mathf.Abs(heldXY.x) > 0.01f)
        {
            spinVelo = Mathf.Lerp(spinVelo, maxSpinVelo * hSign, spinSpeed * Time.deltaTime);
        } else
        {
            spinVelo = Mathf.Lerp(spinVelo, 0, angularSlowdown * Time.deltaTime);
        }

        // Velo
        moveVelo += (Vector2) (transform.rotation * Vector3.up * Time.deltaTime * moveSpeed * heldXY.y);
        if (moveVelo.magnitude > maxMoveVelo)
            moveVelo *= maxMoveVelo / moveVelo.magnitude;

        // Drag
        float drag = Time.deltaTime * linearDrag;
        drag = Mathf.Min(drag, Mathf.Abs(moveVelo.magnitude));
        moveVelo -= moveVelo.normalized * drag;

        // Apply velocities
        transform.Rotate(Vector3.back, Time.deltaTime * spinVelo);
        transform.position += (Vector3) moveVelo * Time.deltaTime;
    }

    public void onMoveCall(InputAction.CallbackContext context)
    {
        heldXY = context.ReadValue<Vector2>();
    }

    public void onFireCall(InputAction.CallbackContext context)
    {
        // Filter action type
        if (!context.performed) return;

        // Instantiate bullet and send it forth
        Bullet b = Instantiate(bulletPrefab, transform.position, transform.rotation);

        b.velo = transform.rotation * Vector2.up * bulletTravelSpeed;
    }
}
