using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

[RequireComponent(typeof(PhysicsObject))]
public class Bullet : FlyingObject, GhostCollidable
{
    public float life = 3f;
    public int piercing = 1;

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
    void onUpdate()
    {
        base.onUpdate();
    }

    public void destroyObj()
    {
        destroyed = true;
        Destroy(gameObject);
    }

    public override FlyingObject[] onHit(FlyingObject src)
    {
        piercing--;
        if (piercing <= 0)
            destroyObj();

        return new FlyingObject[0];
    }
}
