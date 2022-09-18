using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Afterimage : MonoBehaviour
{

    public SpriteRenderer rend;
    public float fadeRate = 1f;

    // Start is called before the first frame update
    void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Color c = rend.color;
        c.a -= fadeRate * Time.deltaTime;
        c.a = Mathf.Max(0, c.a);

        // Already completely faded
        if (c.a == 0)
            Destroy(gameObject);

        rend.color = c;
    }
}
