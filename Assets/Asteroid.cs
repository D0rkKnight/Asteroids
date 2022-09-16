using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public AsteroidData data;

    public Vector2 velo;
    public float splitSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += (Vector3)velo * Time.deltaTime;
    }

    public void setData(AsteroidData data_)
    {
        data = data_;

        GetComponent<SpriteRenderer>().sprite = data_.spr;
    }

    public void kill()
    {
        // Destroy this asteroid
        Destroy(gameObject);

        if (data.size == 0)
            return;

        // Blow up into multiple asteroids
        for (int i = 0; i < 2; i++) {
            Asteroid child = GameManager.spawnAsteroid(data.size-1, transform.position);

            // Make this respect conservation of momentum and bullet weight
            child.velo = Random.insideUnitCircle.normalized * splitSpeed;
        }
    }
}
