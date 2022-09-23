using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsObject))]
public class FlyingObject : MonoBehaviour, GhostCollidable
{
    public int size = 1; // 0 is smallest
    public float naturalSpeedCap = 1f;
    public float overcapSlowdownRate = 1f;

    public PhysicsObject phys;

    public bool destroyOnOOB = true;
    public bool loopOnFullEntry = true;

    public bool destroyed = false; // Tag for same frame collisions

    // Horrible OOP jank
    public void Awake()
    {
        phys = GetComponent<PhysicsObject>();

        onAwake();
    }

    public virtual void onAwake()
    {

    }

    public void Start()
    {
        onStart();
    }

    public virtual void onStart()
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

        GameManager.getGameCorners(out Vector2 bl, out Vector2 ur);
        Bounds screenBounds = new Bounds((bl + ur) / 2, (Vector3)(ur - bl) + new Vector3(0, 0, 1000));

        if (loopOnFullEntry)
        {
            // TODO: Might want the gamemanager handling this behavior
            // Activate looping on full screen entry
            SpriteRenderer sprRend = GetComponent<SpriteRenderer>();
            ScreenWrapper wrapper = sprRend.GetComponent<ScreenWrapper>();

            if (screenBounds.Contains(sprRend.bounds.min) && screenBounds.Contains(sprRend.bounds.max) &&
                !wrapper.loopable)
                wrapper.activateWrapper();
        }

        if (destroyOnOOB)
        {
            // Destroy self if too far from edge (might struggle with large objects)
            if (screenBounds.SqrDistance(transform.position) > 10)
                Destroy(gameObject);
        }

        onUpdate();
    }

    public virtual void onUpdate()
    {

    }

    // Returns children generated from hit
    public virtual FlyingObject[] onHit(FlyingObject src)
    {
        return new FlyingObject[0];
    }

    public void hitIfOpposing(GameObject collision)
    {
        FlyingObject fObj = collision.GetComponent<FlyingObject>();

        bool allegPassed = true;
        Allegiance thisAlleg = GetComponent<Allegiance>();
        Allegiance collAlleg = collision.GetComponent<Allegiance>();
        if (thisAlleg != null && collAlleg != null)
            allegPassed = thisAlleg.isOpponent(collAlleg);

        if (fObj != null && !fObj.destroyed && allegPassed)
            fObj.onHit(this);
    }

    // Body to body collisions handled here
    public void OnGhostCollision(GameObject collision)
    {
        if (destroyed)
            return;

        hitIfOpposing(collision);
    }
}
