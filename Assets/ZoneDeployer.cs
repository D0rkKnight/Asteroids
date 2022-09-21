using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ScreenWrapper))]
public class ZoneDeployer : MonoBehaviour
{
    float life = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        endCollision(life);
    }

    public IEnumerator endCollision(float dur)
    {
        yield return new WaitForSeconds(dur);

        GetComponent<ScreenWrapper>().onCollision.RemoveAllListeners();
    }
}
