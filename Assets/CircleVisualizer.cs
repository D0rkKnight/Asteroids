using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleVisualizer : MonoBehaviour
{
    public float radius = 1f;
    public int pointDensity = 3; // Points per unity unit of circumference
    public LineRenderer lRend;

    // Start is called before the first frame update
    void Start()
    {
        int pointCnt = (int) Mathf.Ceil(2 * radius * Mathf.PI * pointDensity) + 2;


        Vector3[] points = new Vector3[pointCnt];
        for (int i=0; i<pointCnt; i++)
        {
            float rad = (float)i / (pointCnt-2) * 2 * Mathf.PI;
            Vector3 delta = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
            points[i] = transform.position + delta;
        }

        lRend = GetComponent<LineRenderer>();
        lRend.positionCount = pointCnt;
        lRend.SetPositions(points);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
