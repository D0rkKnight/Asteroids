using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Generalize into enemy class
public class EnemySpike : FlyingObject
{
    public FlyingObject[] shrapnel;
    public int shrapCnt = 5;
    public float shrapSpeed = 10f;
    public float shrapSpin = 100f;

    public float explodeRad = 2f;
    public float explodeStr = 1f;

    public override FlyingObject[] onHit(FlyingObject src)
    {
        Destroy(gameObject);
        GameManager.sing.score += 1000;

        GameObject[] bans = new GameObject[shrapCnt];

        // Spit out shrapnel
        for (int i=0; i<shrapCnt; i++)
        {
            FlyingObject pref = shrapnel[Random.Range(0, shrapnel.Length)];
            FlyingObject shrap = Instantiate(pref, transform.position, Quaternion.Euler(0, 0, Random.Range(-180, 180)));

            shrap.phys.moveVelo = Quaternion.Euler(0, 0, 360f * ((float)i / shrapCnt) + Random.Range(-20, 20)) * Vector2.up
                * shrapSpeed;
            shrap.phys.spinVelo = Random.Range(-shrapSpin, shrapSpin);

            bans[i] = shrap.gameObject;
        }

        // Boom
        GameManager.pulseAt(gameObject, transform.position, explodeRad, explodeStr, bans);

        return new FlyingObject[0];
    }
}
