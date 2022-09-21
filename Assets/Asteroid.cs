using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsObject))]
public class Asteroid : MonoBehaviour, GhostCollidable
{
    public int size; // 0 is smallest
    public float splitSpeed = 1f;
    public float splitSpin = 20f;
    public float naturalSpeed = 1f;
    public float overcapSlowdownRate = 1f;

    public PhysicsObject phys;

    // Start is called before the first frame update
    void Awake()
    {
        phys = GetComponent<PhysicsObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (phys.moveVelo.magnitude > naturalSpeed)
        {
            phys.moveVelo = Vector2.Lerp(phys.moveVelo, phys.moveVelo.normalized * naturalSpeed, overcapSlowdownRate * Time.deltaTime);
        }

        // TODO: Might want the gamemanager handling this behavior
        // Activate looping on full screen entry
        SpriteRenderer sprRend = GetComponent<SpriteRenderer>();
        ScreenWrapper wrapper = sprRend.GetComponent<ScreenWrapper>();

        GameManager.getGameCorners(out Vector2 bl, out Vector2 ur);
        Bounds screenBounds = new Bounds((bl+ur)/2, (Vector3) (ur-bl) + new Vector3(0, 0, 1000));

        if (screenBounds.Contains(sprRend.bounds.min) && screenBounds.Contains(sprRend.bounds.max) &&
            !wrapper.loopable)
            wrapper.activateWrapper();

        // Destroy self if too far from edge (might struggle with large objects)
        if (screenBounds.SqrDistance(transform.position) > 10)
            Destroy(gameObject);
    }

    public void kill()
    {
        // Destroy this asteroid
        Destroy(gameObject);

        if (size == 0)
            return;

        // Blow up into multiple asteroids
        for (int i = 0; i < 2; i++) {
            Asteroid child = GameManager.spawnAsteroid(size-1, transform.position);

            // Slight spawn shift
            child.transform.position += (Vector3) Random.insideUnitCircle.normalized * (size + 1) / 4;

            // Make this respect conservation of momentum and bullet weight
            child.phys.moveVelo = phys.moveVelo + Random.insideUnitCircle.normalized * splitSpeed;

            // Random spin
            child.phys.spinVelo = Random.Range(-splitSpin, splitSpin);
        }

        // Add score!
        GameManager.sing.score += (size + 1) * 100;
    }

    public void OnGhostCollision(GameObject collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player != null && !player.destroyed)
            player.hit();
    }
}
