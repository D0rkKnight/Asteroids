using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ScreenWrapper))]
public class ZoneDeployer : MonoBehaviour
{
    public float life = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        endCollision(life);
    }

    public IEnumerator endCollision(float dur)
    {
        yield return new WaitForSeconds(dur);

        // Remove all colliders on the object
        foreach (Collider2D c in GetComponents<Collider2D>())
            Destroy(c);
    }
}
