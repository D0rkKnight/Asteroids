using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class RocketTrail : MonoBehaviour
{
    TrailRenderer tRend;

    // Start is called before the first frame update
    void Start()
    {
        tRend = GetComponent<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
