using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ColliderLink : MonoBehaviour
{
    public UnityEvent<Collider2D> onColl;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        onColl.Invoke(collision);
    }
}
