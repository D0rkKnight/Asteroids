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

    // Bullets
    public bool firing = false;
    public float firerate = 2f;
    public float nextFireTime = 0f;

    public Bullet bulletPrefab;
    public float bulletTravelSpeed = 10f;
    public float recoil = 1f;

    // Dash info
    public float dashDur = 0.5f;
    public float dashSpeed = 3f;
    public float postDashSpeed = 2f;
    public bool dashing = false;

    public bool dashAvailable = true;
    public float dashCooldown = 3f;

    // Pulse info
    public float pulseRadius = 2f;
    public float pulseStrength = 1f;
    public float pulseCooldown = 5f;
    public bool pulseAvailable = true;
    public CircleVisualizer pulseAfterimagePrefab;

    // Physics profiles for different states
    public PhysicsProfile regProfile;
    public PhysicsProfile dashProfile;

    public bool invuln = false;

    public Afterimage afterimagePrefab;

    // Object links
    public Transform noseTrans;
    public TrailRenderer trail;

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
                phys.spinVelo = Mathf.Lerp(phys.spinVelo, phys.profile.maxSpinVelo * hSign, spinSpeed * Time.deltaTime);

            // Velo
            phys.moveVelo += (Vector2)(transform.rotation * Vector3.up * Time.deltaTime * moveSpeed * heldXY.y);
        }

        // Firing gun
        if (firing && Time.time > nextFireTime)
        {
            // Instantiate bullet and send it forth
            Bullet b = Instantiate(bulletPrefab, noseTrans.position, transform.rotation);

            // Bullet velocity also contains spin of the nose
            float tangentSpeed = 2 * (noseTrans.position - transform.position).magnitude * Mathf.Sin(phys.spinVelo / 2); // Signed
            Vector2 spin = transform.rotation * Vector2.right * tangentSpeed;

            b.velo = transform.rotation * Vector2.up * bulletTravelSpeed + (Vector3) spin;

            // Recoil
            phys.moveVelo -= (Vector2) (transform.rotation * Vector2.up * recoil);

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

    public void onPulseCall(InputAction.CallbackContext context)
    {
        if (!context.performed || !pulseAvailable)
            return;

        GameManager.pulseAt(transform.position, pulseRadius, pulseStrength);

        CircleVisualizer cViz = Instantiate(pulseAfterimagePrefab, transform.position, Quaternion.identity);
        cViz.radius = pulseRadius;
    }

    // TODO: Write custom trail renderer that isn't so unstable
    public void onSetupGhostCall(GameObject ghost)
    {
        // Copy over trail
        // Utilities.copyComponent(trail, ghost);
    }

    public void onWrapCall(Vector2 dir, GameObject ghost)
    {
        /*// Swap trail vertices from ghost
        TrailRenderer ghostTrail = ghost.GetComponent<TrailRenderer>();

        Vector3[] ghostVerts = new Vector3[ghostTrail.positionCount+1];
        Vector3[] baseVerts = new Vector3[trail.positionCount+1];

        if (ghostVerts.Length != baseVerts.Length)
            throw new System.Exception("Trail ghost desync between " + ghostVerts.Length + " and " + baseVerts.Length);

        ghostTrail.GetPositions(ghostVerts);
        trail.GetPositions(baseVerts);

        ghostTrail.Clear();
        trail.Clear();

        ghostTrail.AddPositions(baseVerts);
        trail.AddPositions(ghostVerts);*/
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
