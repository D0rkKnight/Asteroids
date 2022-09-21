using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RendererController : MonoBehaviour
{
    public Color baseCol;

    public Color flicker1;
    public Color flicker2;
    public float flickerInterval = 0.1f;

    private SpriteRenderer rend;

    // Start is called before the first frame update
    void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        rend.color = baseCol;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void flicker(float dur)
    {
        flicker(dur, flicker1, flicker2);
    }

    public void flicker(float dur, Color flicker1_, Color flicker2_)
    {
        StopCoroutine("flickerCycle");
        StartCoroutine(flickerCycle(dur, flickerInterval, new Color[] { flicker1_, flicker2_ }));
    }

    public IEnumerator flickerCycle(float dur, float flickerInterval_, Color[] colors)
    {
        float timestamp = 0f;
        int mode = 0;

        while (timestamp < dur)
        {
            timestamp += flickerInterval_;

            rend.color = colors[mode];

            mode++;
            mode %= colors.Length;

            yield return new WaitForSeconds(flickerInterval_);
        }

        // Return to base color
        rend.color = baseCol;
    }
}
