using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PhysicsObject))]
public class Player : MonoBehaviour
{
    public float spinSpeed = 1f;
    public float moveSpeed = 1f;

    private PhysicsObject phys;

    public PlayerInput input;
    public Vector2 heldXY;

    public Bullet bulletPrefab;
    public float bulletTravelSpeed = 10f;

    // Start is called before the first frame update
    void Awake()
    {
        phys = GetComponent<PhysicsObject>();
    }

    // Update is called once per frame
    void Update()
    {
        // Spin
        int hSign = (int) Mathf.Sign(heldXY.x);
        if (Mathf.Abs(heldXY.x) > 0.01f)
            phys.spinVelo = Mathf.Lerp(phys.spinVelo, phys.maxSpinVelo * hSign, spinSpeed * Time.deltaTime);

        // Velo
        phys.moveVelo += (Vector2) (transform.rotation * Vector3.up * Time.deltaTime * moveSpeed * heldXY.y);
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

    public void kill()
    {
        // Boom im dead
        Destroy(gameObject);

        GameManager.sing.onPlayerDeath();
    }
}
