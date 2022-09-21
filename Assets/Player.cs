using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PhysicsObject))]
public class Player : MonoBehaviour
{
    public float spinAccel = 1f;
    public float moveAccel = 1f;

    public float spinBreakFactor = 1.2f;
    public float moveBreakFactor = 1.5f;

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
    public bool dashSlowdownAndEDI = true;
    public float floatingDashCD;

    // Pulse info
    public float pulseRadius = 2f;
    public float pulseStrength = 1f;
    public float pulseCooldown = 5f;
    public bool pulseAvailable = true;
    public CircleVisualizer pulseAfterimagePrefab;

    // Hyperdashes
    public float hyperSpeed = 15f;
    public float hyperPrewindow = 0.1f;
    public float hyperPostwindow = 0.02f;
    public bool hyperAble = false;
    public float hyperCooldown = 0.5f;

    // Physics profiles for different states
    public PhysicsProfile regProfile;
    public PhysicsProfile dashProfile;

    public bool invuln = false;
    public bool destroyed = false;

    public Afterimage afterimagePrefab;

    // Object links
    public Transform noseTrans;
    public TrailRenderer trail;
    public RendererController rendCtrl;

    // Start is called before the first frame update
    void Awake()
    {
        phys = GetComponent<PhysicsObject>();
        rendCtrl = GetComponent<RendererController>();
    }

    private void Start()
    {
        phys.profile = regProfile;
        floatingDashCD = dashCooldown;
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
                phys.spinVelo = Mathf.Lerp(phys.spinVelo, phys.profile.maxSpinVelo * hSign, spinAccel * Time.deltaTime);

            // Velo
            Vector2 accelApplied = transform.rotation * Vector3.up * moveAccel * heldXY.y;

            // Account for breaking if existing motion
            if (phys.moveVelo.magnitude > 0.01f)
            {
                float parAxisMag = Vector2.Dot(accelApplied, phys.moveVelo.normalized);
                Vector2 parallel = parAxisMag * phys.moveVelo.normalized;
                Vector2 perp = accelApplied - parallel;

                if (parAxisMag < 0)
                    phys.moveVelo += parallel * moveBreakFactor * Time.deltaTime;
                else
                    phys.moveVelo += parallel * Time.deltaTime;
                phys.moveVelo += perp * Time.deltaTime;
            }

            // For moving from rest
            else
                phys.moveVelo += accelApplied * Time.deltaTime;
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

        float floatingPulseCD = pulseCooldown;

        // Perform a hyper if available
        if (hyperAble)
        {
            Vector2 hyperSum = (phys.moveVelo.normalized + heldXY * 1.2f);
            Vector2 hyperDir = hyperSum;
            if (hyperDir.magnitude > 1)
                hyperDir = hyperDir.normalized; // Constrain to unit circle

            phys.moveVelo = hyperSpeed * hyperDir;
            dashSlowdownAndEDI = false;

            // Change cooldown times
            floatingDashCD = hyperCooldown;
            floatingPulseCD = hyperCooldown;
        }

        StartCoroutine(startPulseCooldown(floatingPulseCD));
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
        destroyed = true;

        GameManager.sing.onPlayerDeath();
    }

    public IEnumerator invulnFor(float dur)
    {
        // Begin flickering
        rendCtrl.flicker(dur);

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

        yield return new WaitForSeconds(dur - hyperPrewindow);

        StartCoroutine(openHyperWindow(hyperPrewindow + hyperPostwindow));

        yield return new WaitForSeconds(hyperPrewindow);

        // Swap profile
        phys.profile = regProfile;

        if (dashSlowdownAndEDI)
        {
            phys.moveVelo *= postDashSpeed / phys.moveVelo.magnitude;

            // DI on dash exit
            phys.moveVelo += phys.moveVelo.normalized * heldXY.y * 1;
        }
        dashSlowdownAndEDI = true; // Default to having this

        // Exit dash
        dashing = false;
        StartCoroutine(startDashCooldown(floatingDashCD));
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

    public IEnumerator openHyperWindow(float dur)
    {
        hyperAble = true;
        yield return new WaitForSeconds(dur);
        hyperAble = false;
    }

    public IEnumerator startDashCooldown(float dur)
    {
        dashAvailable = false;
        yield return new WaitForSeconds(dur);
        dashAvailable = true;

        floatingDashCD = dashCooldown; // Return floating value to normal
    }

    public IEnumerator startPulseCooldown(float dur)
    {
        pulseAvailable = false;
        yield return new WaitForSeconds(dur);
        pulseAvailable = true;
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
