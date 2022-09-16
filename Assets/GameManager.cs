using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player;
    public Asteroid asteroidPrefab;
    public AsteroidData[] asteroidData;

    public static GameManager sing;

    // Start is called before the first frame update
    void Start()
    {
        if (sing != null)
            throw new System.Exception("GM Singleton broken");

        sing = this;

        spawnAsteroid(2, Vector2.zero);
    }

    // Update is called once per frame
    void Update()
    {
        // Loop the player if they leave the camera
        float padding = 1f;

        Vector2 pPos = player.transform.position;
        Vector2 bl = Camera.main.ViewportToWorldPoint(Vector2.zero) - new Vector3(padding, padding);
        Vector2 ur = Camera.main.ViewportToWorldPoint(new Vector2(1, 1)) + new Vector3(padding, padding);
        Vector2 dims = ur - bl;

        if (pPos.x < bl.x)
            pPos.x += dims.x;
        if (pPos.x > ur.x)
            pPos.x -= dims.x;
        if (pPos.y < bl.y)
            pPos.y += dims.y;
        if (pPos.y > ur.y)
            pPos.y -= dims.y;

        player.transform.position = pPos;
    }

    public static Asteroid spawnAsteroid(int size, Vector2 pos)
    {
        // Builds valid asteroid list on demand because I really don't want to mantain a cache
        List<AsteroidData> valid = new List<AsteroidData>();

        foreach (AsteroidData a in sing.asteroidData)
            if (a.size == size)
                valid.Add(a);

        int rand = Random.Range(0, valid.Count);
        AsteroidData data = valid[rand];

        Asteroid spawned = Instantiate(sing.asteroidPrefab, pos, Quaternion.identity);
        spawned.setData(data);

        return spawned;
    }
}
