using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsObject))]
public class Asteroid : FlyingObject, GhostCollidable
{
    public float splitSpeed = 1f;
    public float splitSpin = 20f;

    // Update is called once per frame
    public override void onUpdate()
    {
        base.onUpdate();
    }

    public override FlyingObject[] onHit(FlyingObject src)
    {
        // Destroy this asteroid
        Destroy(gameObject);

        if (size == 0)
            return new Asteroid[0];

        // Blow up into multiple asteroids
        Asteroid[] ret = new Asteroid[2];

        for (int i = 0; i < 2; i++) {
            Asteroid child = GameManager.spawnAsteroid(size-1, transform.position);

            // Slight spawn shift
            child.transform.position += (Vector3) Random.insideUnitCircle.normalized * (size + 1) / 4;

            // Make this respect conservation of momentum and bullet weight
            child.phys.moveVelo = phys.moveVelo + Random.insideUnitCircle.normalized * splitSpeed;

            // Random spin
            child.phys.spinVelo = Random.Range(-splitSpin, splitSpin);

            ret[i] = child;
        }

        // Add score!
        GameManager.sing.score += (size + 1) * 100;

        return ret;
    }
}
