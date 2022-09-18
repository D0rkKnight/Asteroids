using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class Bullet : MonoBehaviour
{
    public Vector2 velo;
    public bool destroyed = false; // Tag for same frame collisions

    // Start is called before the first frame update
    void Start()
    {
        SpriteResolver resolver = GetComponent<SpriteResolver>();

        int rand = Random.Range(0, resolver.GetCategory().Length);
        resolver.SetCategoryAndLabel(resolver.GetCategory(), rand + "");
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

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // If this thing is already dead skip (for same frame collisions)
        if (destroyed)
            return;

        // Asteroid check (might've hit a wrap ghost)
        Asteroid ast = collision.GetComponentInParent<Asteroid>();

        if (ast != null)
        {
            ast.kill();
            destroyObj();
        }
    }
}
