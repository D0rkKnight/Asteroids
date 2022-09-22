using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingObject : MonoBehaviour, GhostCollidable
{
    public int size; // 0 is smallest
    public float naturalSpeedCap = 1f;
    public float overcapSlowdownRate = 1f;

    public PhysicsObject phys;

    public Bullet laserPrefab;
    public float laserFirerate = 0.5f;
    public float laserSpeed = 4f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Natural slowdown
        if (phys.moveVelo.magnitude > naturalSpeedCap)
        {
            phys.moveVelo = Vector2.Lerp(phys.moveVelo, phys.moveVelo.normalized * naturalSpeedCap, overcapSlowdownRate * Time.deltaTime);
        }

        // TODO: Might want the gamemanager handling this behavior
        // Activate looping on full screen entry
        SpriteRenderer sprRend = GetComponent<SpriteRenderer>();
        ScreenWrapper wrapper = sprRend.GetComponent<ScreenWrapper>();

        GameManager.getGameCorners(out Vector2 bl, out Vector2 ur);
        Bounds screenBounds = new Bounds((bl + ur) / 2, (Vector3)(ur - bl) + new Vector3(0, 0, 1000));

        if (screenBounds.Contains(sprRend.bounds.min) && screenBounds.Contains(sprRend.bounds.max) &&
            !wrapper.loopable)
            wrapper.activateWrapper();

        // Destroy self if too far from edge (might struggle with large objects)
        if (screenBounds.SqrDistance(transform.position) > 10)
            Destroy(gameObject);
    }

    // Returns children generated from hit
    public virtual FlyingObject[] onHit()
    {
        return new FlyingObject[0];
    }
    public void OnGhostCollision(GameObject collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player != null && !player.destroyed)
            player.hit();
    }

    public IEnumerator laserCycle()
    {
        // Find target
        while (true) {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 0) {

                GameObject target = players[0];
                foreach (GameObject p in players)
                    if (Vector2.Distance(p.transform.position, transform.position) <
                        Vector2.Distance(target.transform.position, transform.position))
                        target = p;

                Vector2 dir = (target.transform.position - transform.position).normalized;
                Bullet laser = Instantiate(laserPrefab, transform.position,
                    Quaternion.LookRotation(dir, Vector3.forward));

                laser.phys.moveVelo = dir * laserSpeed;
            }

            yield return new WaitForSeconds(1.0f / laserFirerate);
        }
    }
}
