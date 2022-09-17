using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGStar : MonoBehaviour
{
    public Sprite[] sprs;
    public float blinkInterval = 1f;
    public float angleOffMax = 40f;
    public float posVariance = 0.5f;
    public Vector2 anchor;

    // Start is called before the first frame update
    void Start()
    {
        anchor = transform.position;
        StartCoroutine(loop());
    }

    public IEnumerator loop()
    {
        while (true)
        {
            int rand = Random.Range(0, sprs.Length);
            GetComponent<SpriteRenderer>().sprite = sprs[rand];
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(-angleOffMax, angleOffMax));
            transform.position = anchor + Random.insideUnitCircle * posVariance;

            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
