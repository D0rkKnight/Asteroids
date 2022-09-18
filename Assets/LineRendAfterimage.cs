using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendAfterimage : MonoBehaviour
{
    LineRenderer rend;
    public float fadeRate = 2f;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Color col = rend.startColor;
        col.a -= fadeRate * Time.deltaTime;

        rend.startColor = col;
        rend.endColor = col;

        if (col.a < 0.01f)
            Destroy(gameObject);
    }
}
