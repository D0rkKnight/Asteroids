using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeTracker : MonoBehaviour
{
    public List<Image> icons;
    public Image iconPrefab;

    // Start is called before the first frame update
    void Start()
    {
        icons = new List<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        while (icons.Count > GameManager.sing.lives)
        {
            Destroy(icons[icons.Count - 1].gameObject);
            icons.RemoveAt(icons.Count-1);
        }
        while (icons.Count < GameManager.sing.lives)
        {
            Image img = Instantiate(iconPrefab, transform, false);
            img.transform.localPosition = Vector2.right * icons.Count * 50;

            icons.Add(img);
        }
    }
}
