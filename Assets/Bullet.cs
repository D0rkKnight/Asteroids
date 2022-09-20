using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class Bullet : MonoBehaviour, GhostCollidable
{
    public Vector2 velo;
    public bool destroyed = false; // Tag for same frame collisions

    public float life = 3f;

    // Start is called before the first frame update
    void Start()
    {
        SpriteResolver resolver = GetComponent<SpriteResolver>();

        int rand = Random.Range(0, resolver.GetCategory().Length);
        resolver.SetCategoryAndLabel(resolver.GetCategory(), rand + "");

        // Queue destruction
        Destroy(gameObject, life);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += (Vector3) velo * Time.deltaTime;
    }

    public void destroyObj()
    {
        destroyed = true;
        Destroy(gameObject);
    }

    public void OnGhostCollision(GameObject obj)
    {
        // If this thing is already dead skip (for same frame collisions)
        if (destroyed)
            return;

        // Asteroid check (might've hit a wrap ghost)
        Asteroid ast = obj.GetComponent<Asteroid>();

        if (ast != null)
        {
            ast.kill();
            destroyObj();
        }
    }
}
