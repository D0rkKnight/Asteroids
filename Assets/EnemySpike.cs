using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Generalize into enemy class
public class EnemySpike : FlyingObject
{
    public FlyingObject[] shrapnel;
    int shrapCnt = 5;
    float shrapSpeed = 10f;
    float shrapSpin = 100f;

    public override FlyingObject[] onHit(FlyingObject src)
    {
        Destroy(gameObject);
        GameManager.sing.score += 1000;

        // Spit out shrapnel
        for (int i=0; i<shrapCnt; i++)
        {
            FlyingObject pref = shrapnel[Random.Range(0, shrapnel.Length)];
            FlyingObject shrap = Instantiate(pref, transform.position, Quaternion.Euler(0, 0, Random.Range(-180, 180)));

            shrap.phys.moveVelo = Quaternion.Euler(0, 0, 360f * ((float)i / shrapCnt) + Random.Range(-20, 20)) * Vector2.up
                * shrapSpeed;
            shrap.phys.spinVelo = Random.Range(-shrapSpin, shrapSpin);


        }

        return new FlyingObject[0];
    }
}
