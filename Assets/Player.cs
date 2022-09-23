using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PhysicsObject))]
public class Player : FlyingObject
{
    public float spinAccel = 1f;
    public float moveAccel = 1f;

    public float spinBreakFactor = 1.2f;
    public float moveBreakFactor = 1.5f;

    public float floatingSpinInput = 0f;
    public float spinAccelRampFactor = 3f;

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

    // Pulse info
    public float pulseRadius = 2f;
    public float pulseStrength = 1f;
    public float pulseCooldown = 5f;
    public bool pulseAvailable = true;

    // Parries
    public float parryRadius = 0.5f;
    public float dashingParryRadius = 1.5f;

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

    public Afterimage afterimagePrefab;

    // Object links
    public Transform noseTrans;
    public TrailRenderer trail;
    public RendererController rendCtrl;

    // Start is called before the first frame update
    public override void onAwake()
    {
        base.onAwake();

        rendCtrl = GetComponent<RendererController>();
    }

    public override void onStart()
    {
        phys.profile = regProfile;
    }

    // Update is called once per frame
    public override void onUpdate()
    {
        base.onUpdate();

        // Gonna if else chain this instead since it seems easier
        if (dashing)
        {
            // Don't do anything I guess
        }
        else
        {
            // Regular movement

            // Spin
            floatingSpinInput = Mathf.Lerp(floatingSpinInput, Mathf.Abs(heldXY.x), Time.deltaTime * spinAccelRampFactor);
            if (Mathf.Abs(heldXY.x) < 0.01f)
                floatingSpinInput = 0; // Reset accel on taps

            int hSign = (int)Mathf.Sign(heldXY.x);
            if (Mathf.Abs(heldXY.x) > 0.01f)
            {
                phys.spinVelo = Mathf.Lerp(phys.spinVelo, phys.profile.maxSpinVelo * hSign, 
                    floatingSpinInput * spinAccel * Time.deltaTime);
            }

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

            b.phys.moveVelo = transform.rotation * Vector2.up * bulletTravelSpeed + (Vector3) spin;

            // Recoil
            phys.moveVelo -= (Vector2) (transform.rotation * Vector2.up * recoil);

            nextFireTime = Time.time + (1f / firerate);
        }

        setColor();
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
        StartCoroutine(dashFor(dashDur, dir, dashSpeed, dashCooldown));
    }

    public void onPulseCall(InputAction.CallbackContext context)
    {
        if (!context.performed || !pulseAvailable)
            return;

        GameManager.pulseAt(transform.position, pulseRadius, pulseStrength, new GameObject[] { gameObject });

        float floatingParryRadius = parryRadius;
        if (hyperAble)
            floatingParryRadius = dashingParryRadius;
        GameManager.parryAt(gameObject, transform.position, floatingParryRadius, 10f, new GameObject[] { gameObject });

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
            floatingPulseCD = hyperCooldown;

            // End hyper window
            hyperAble = false;

            // Reset dash cooldown
            StartCoroutine(startDashCooldown(hyperCooldown));
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

    public override FlyingObject[] onHit(FlyingObject src)
    {
        if (invuln)
            return new FlyingObject[0];

        // Boom im dead
        Destroy(gameObject);
        destroyed = true;

        GameManager.sing.onPlayerDeath(this);
        return new FlyingObject[0];
    }

    public IEnumerator invulnFor(float dur)
    {
        // Begin flickering
        rendCtrl.flicker(dur);

        invuln = true;
        yield return new WaitForSeconds(dur);
        invuln = false;
    }

    public IEnumerator dashFor(float dur, Vector2 dir, float speed, float cooldown)
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
        StartCoroutine(startDashCooldown(cooldown));
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
        // Clear existing cooldowns
        StopCoroutine("startDashCooldown");

        dashAvailable = false;
        yield return new WaitForSeconds(dur);
        dashAvailable = true;
    }

    public IEnumerator startPulseCooldown(float dur)
    {
        // Clear existing cooldowns
        StopCoroutine("startDashCooldown");

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

    // Use a centralized color switch for this, it's easier
    public void setColor()
    {
        Color col = Color.clear;

        if (hyperAble)
            col = Color.magenta;

        rendCtrl.overtone = col;
        rendCtrl.compileColor();
    }
}
