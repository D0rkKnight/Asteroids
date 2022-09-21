using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrapGhost : MonoBehaviour
{
    public ScreenWrapper parent;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        parent.tryCollisionCheck(other);
    }
}
