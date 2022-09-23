using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUFO : FlyingObject
{
    public Bullet laserPrefab;
    public float laserFirerate = 0.5f;
    public float laserSpeed = 4f;

    // Start is called before the first frame update
    public override void onStart() {
        StartCoroutine(laserCycle());
    }

    public IEnumerator laserCycle()
    {
        // Find target
        while (true)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 0)
            {

                GameObject target = players[0];
                foreach (GameObject p in players)
                    if (Vector2.Distance(p.transform.position, transform.position) <
                        Vector2.Distance(target.transform.position, transform.position))
                        target = p;

                Vector2 dir = (target.transform.position - transform.position).normalized;
                float deg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                Bullet laser = Instantiate(laserPrefab, transform.position,
                    Quaternion.Euler(0, 0, deg + 90));

                laser.phys.moveVelo = dir * laserSpeed;
                laser.GetComponent<Allegiance>().alignment = Allegiance.ALIGN.ENEMY;
            }

            yield return new WaitForSeconds(1.0f / laserFirerate);
        }
    }

    public override FlyingObject[] onHit(FlyingObject src)
    {
        Destroy(gameObject);
        GameManager.sing.score += 1000;

        return new FlyingObject[0];
    }
}
