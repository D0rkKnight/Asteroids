using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarField : MonoBehaviour
{
    public float distBetween = 2f;
    public float posVariance = 0.5f;

    public List<BGStar> stars;
    public BGStar starPrefab;

    // Start is called before the first frame update
    void Start()
    {
        stars = new List<BGStar>();

        GameManager.getGameCorners(out Vector2 bl, out Vector2 ur);
        Vector2 pointer = bl;

        int shiftPar = 0;
        while (pointer.y < ur.y)
        {
            pointer.x = bl.x - (shiftPar * distBetween/2);
            shiftPar = (shiftPar + 1) % 2;
            while (pointer.x < ur.x)
            {
                // Instantiate star
                BGStar s = Instantiate(starPrefab, transform);
                s.transform.position = pointer + Random.insideUnitCircle * posVariance;
                stars.Add(s);

                pointer.x += distBetween;
            }
            pointer.y += distBetween;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
