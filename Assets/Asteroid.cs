using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsObject))]
public class Asteroid : MonoBehaviour
{
    public int size; // 0 is smallest
    public float splitSpeed = 1f;
    public float splitSpin = 20f;

    public PhysicsObject phys;

    // Start is called before the first frame update
    void Awake()
    {
        phys = GetComponent<PhysicsObject>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += (Vector3)phys.moveVelo * Time.deltaTime;

        GameManager.loopObject(transform);
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

            // Make this respect conservation of momentum and bullet weight
            child.phys.moveVelo = Random.insideUnitCircle.normalized * splitSpeed;

            // Random spin
            child.phys.spinVelo = Random.Range(-splitSpin, splitSpin);
        }

        // Add score!
        GameManager.sing.score += (size + 1) * 100;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player != null)
            player.hit();
    }
}
