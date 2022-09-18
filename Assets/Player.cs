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

    public bool firing = false;
    public float firerate = 2f;
    public float nextFireTime = 0f;

    public Bullet bulletPrefab;
    public float bulletTravelSpeed = 10f;

    public bool invuln = false;

    // Dash info
    public float dashDur = 0.5f;
    public float dashSpeed = 3f;
    public float postDashSpeed = 2f;
    public bool dashing = false;

    public bool dashAvailable = true;
    public float dashCooldown = 3f;

    // Physics profiles for different states
    public PhysicsProfile regProfile;
    public PhysicsProfile dashProfile;

    public Afterimage afterimagePrefab;

    // Start is called before the first frame update
    void Awake()
    {
        phys = GetComponent<PhysicsObject>();
    }

    private void Start()
    {
        phys.profile = regProfile;
    }

    // Update is called once per frame
    void Update()
    {
        // Gonna if else chain this instead since it seems easier
        if (dashing)
        {
            // Don't do anything I guess
        }
        else
        {
            // Regular movement

            // Spin
            int hSign = (int)Mathf.Sign(heldXY.x);
            if (Mathf.Abs(heldXY.x) > 0.01f)
                phys.spinVelo = Mathf.Lerp(phys.spinVelo, phys.maxSpinVelo * hSign, spinSpeed * Time.deltaTime);

            // Velo
            phys.moveVelo += (Vector2)(transform.rotation * Vector3.up * Time.deltaTime * moveSpeed * heldXY.y);
        }

        // Firing gun
        if (firing && Time.time > nextFireTime)
        {
            // Instantiate bullet and send it forth
            Bullet b = Instantiate(bulletPrefab, transform.position, transform.rotation);
            b.velo = transform.rotation * Vector2.up * bulletTravelSpeed;

            nextFireTime = Time.time + (1f / firerate);
        }
    }

    public void onMoveCall(InputAction.CallbackContext context)
    {
        heldXY = context.ReadValue<Vector2>();
    }

    public void onFireCall(InputAction.CallbackContext context)
    {
        // Filter action type
        if (context.started)
            firing = true;
        if (context.canceled)
            firing = false;
    }

    public void onDashCall(InputAction.CallbackContext context)
    {
        if (!context.performed || !dashAvailable)
            return;

        Vector2 dir = transform.rotation * Vector2.up;
        StartCoroutine(dashFor(dashDur, dir, dashSpeed));
    }

    public void hit()
    {
        if (invuln)
            return;

        // Boom im dead
        Destroy(gameObject);

        GameManager.sing.onPlayerDeath();
    }

    public IEnumerator invulnFor(float dur)
    {
        invuln = true;
        yield return new WaitForSeconds(dur);
        invuln = false;
    }

    public IEnumerator dashFor(float dur, Vector2 dir, float speed)
    {
        dashing = true;
        phys.profile = dashProfile;
        phys.spinVelo = 0;

        // DI on dash entrance
        if (heldXY.magnitude > 0.01)
            dir = heldXY;
        phys.moveVelo = speed * dir.normalized;

        // Calculate afterimage
        float aiDur = 0.1f;
        int iterations = (int) (dur / aiDur);
        StartCoroutine(spawnAfterImages(aiDur, iterations));

        // Open a redirect window
        float redirWind = Mathf.Min(0.05f, dur);
        StartCoroutine(openRedirWindow(redirWind, speed));

        yield return new WaitForSeconds(dur);
        dashing = false;

        phys.profile = regProfile;
        phys.moveVelo *= postDashSpeed / phys.moveVelo.magnitude;

        // DI on dash exit
        phys.moveVelo += phys.moveVelo.normalized * heldXY.y * 1;

        // Start cooldown
        StartCoroutine(startDashCooldown(dashCooldown));
    }

    public IEnumerator openRedirWindow(float dur, float speed)
    {
        float timestamp = Time.time + dur;

        while (timestamp > Time.time)
        {
            // Check for redirect
            if (heldXY.magnitude > 0)
                phys.moveVelo = speed * heldXY.normalized;

            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator startDashCooldown(float dur)
    {
        dashAvailable = false;
        yield return new WaitForSeconds(dur);
        dashAvailable = true;
    }

    public IEnumerator spawnAfterImages(float dur, int iterations)
    {
        for (int i=0; i<iterations; i++)
        {
            // Spawn an afterimage
            Afterimage ai = Instantiate(afterimagePrefab, transform.position, transform.rotation);

            // Copy over sprite
            ai.rend.sprite = GetComponent<SpriteRenderer>().sprite;

            yield return new WaitForSeconds(dur);
        }
    }
}
