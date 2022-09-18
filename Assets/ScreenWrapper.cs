using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

// Creates clones that screen wrap item and propagate collision messages upstream
public class ScreenWrapper : MonoBehaviour
{
    public GameObject[,] ghosts;

    // Start is called before the first frame update
    void Start()
    {
        ghosts = new GameObject[3, 3];

        GameManager.getGameCorners(out Vector2 bl, out Vector2 ur);
        Vector2 dims = ur - bl;

        for (int x = 0; x < 3; x++) for (int y = 0; y < 3; y++)
            {
                if (x == 1 && y == 1)
                    continue;

                ghosts[x, y] = genGhost((x-1) * dims.x, (y-1) * dims.y, "Ghost_"+x+y);
            }
    }

    private GameObject genGhost(float x, float y, string name)
    {
        GameObject ghost = new GameObject(name);
        ghost.transform.parent = transform;

        // Build components
        SpriteRenderer sprRend = GetComponent<SpriteRenderer>(); 
        if (sprRend != null)
            ghost.AddComponent<SpriteRenderer>();

        FieldInfo[] test = typeof(SpriteRenderer).GetFields();

        // Copy colliders as well
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            System.Type type = col.GetType();
            Collider2D newCol = (Collider2D) ghost.AddComponent(type);

            // Copy over fields
            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo f in fields)
                f.SetValue(newCol, f.GetValue(col));

            // Copy over properties too (this feels really janky...)
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo p in props)
                if (p.SetMethod != null && p.GetMethod != null)
                    p.SetValue(newCol, p.GetValue(col));
        }

        // Set tag
        ghost.tag = "Untagged";

        updateGhost(x, y, ghost);

        return ghost;
    }

    private void updateGhost(float x, float y, GameObject ghost)
    {
        ghost.transform.position = transform.position + new Vector3(x, y);
        ghost.transform.rotation = transform.rotation;

        SpriteRenderer sprRend = GetComponent<SpriteRenderer>();
        if (sprRend != null)
        {
            // Manually write in the data since we'll need to do it in runtime later
            SpriteRenderer gRend = ghost.GetComponent<SpriteRenderer>();
            gRend.sprite = sprRend.sprite;
            gRend.color = sprRend.color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Reposition ghosts
        GameManager.getGameCorners(out Vector2 bl, out Vector2 ur);
        Vector2 dims = ur - bl;

        for (int x = 0; x < 3; x++) for (int y = 0; y < 3; y++) if (ghosts[x, y] != null)
                    updateGhost((x - 1) * dims.x, (y - 1) * dims.y, ghosts[x, y]);
    }
}
