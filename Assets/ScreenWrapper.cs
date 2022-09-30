using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;

// Creates clones that screen wrap item and propagate collision messages upstream
public class ScreenWrapper : MonoBehaviour
{
    public GameObject[,] ghosts;
    public UnityEvent<GameObject> onSetupObject;
    public UnityEvent<Vector2, GameObject> onWrap;

    // Collision delegate (could be eventually separated to another component)
    public UnityEvent<GameObject> onCollision; // Handles both ghost and core collisions
    public List<GameObject> colBanList;

    public bool loopable = true; // Toggles whether the screen wrapper is wrapping items

    // Start is called before the first frame update
    void Start()
    {
        ghosts = new GameObject[3, 3];

        // Collect collision events from elsewhere
        foreach (GhostCollidable collTarget in GetComponents<GhostCollidable>())
            onCollision.AddListener(collTarget.OnGhostCollision);

        // Redundancy check inside prevents double init from on-awake forced init
        if (loopable)
            initWrapper();
    }

    private GameObject genGhost(float x, float y, string name)
    {
        GameObject ghost = new GameObject(name);
        ghost.transform.parent = GameManager.sing.transform.Find("Ghosts");

        // Build components
        SpriteRenderer sprRend = GetComponent<SpriteRenderer>();
        if (sprRend != null)
            ghost.AddComponent<SpriteRenderer>();

        LineRenderer lRend = GetComponent<LineRenderer>();
        if (lRend != null)
            ghost.AddComponent<LineRenderer>();

        WrapGhost wr = ghost.AddComponent<WrapGhost>();
        wr.parent = this;

        // Copy colliders as well
        foreach (Collider2D col in GetComponents<Collider2D>())
            Utilities.copyComponent(col, ghost);

        // Set tag
        ghost.tag = "Untagged";
        updateGhost(x, y, ghost, true);

        onSetupObject.Invoke(ghost);

        return ghost;
    }

    private void updateGhost(float x, float y, GameObject ghost, bool forceAdopt = false)
    {
        ghost.transform.position = transform.position + new Vector3(x, y);
        ghost.transform.rotation = transform.rotation;
        ghost.transform.localScale = transform.localScale;

        // Collect all renderers for bounds checking
        Renderer[] rends = ghost.GetComponents<Renderer>();
        bool visible = false;
        if (rends.Length > 0 && !forceAdopt)
        {
            Bounds b1 = rends[0].bounds;
            foreach (Renderer rend in rends)
            {
                Bounds bComp = rend.bounds;
                b1.SetMinMax(Vector2.Min(b1.min, bComp.min), Vector2.Max(bComp.max, bComp.max));
                b1.extents = new Vector3(b1.extents.x, b1.extents.y, 1000f);
            }

            GameManager.getGameCorners(out Vector2 bl, out Vector2 ur);

            Bounds b2 = new Bounds();
            b2.SetMinMax(bl, ur);
            b2.extents = new Vector3(b2.extents.x, b2.extents.y, 1000f);

            visible = b1.Intersects(b2);
        }

        bool renderToScreen = visible || forceAdopt;
        ghost.SetActive(renderToScreen);
        if (!renderToScreen)
            return;

        SpriteRenderer sprRend = GetComponent<SpriteRenderer>();
        if (sprRend != null)
            Utilities.copyComponent(sprRend, ghost);

        LineRenderer lRend = GetComponent<LineRenderer>();
        if (lRend != null)
        {
            Utilities.copyComponent(lRend, ghost);

            // Write in points manually
            LineRenderer gRend = ghost.GetComponent<LineRenderer>();
            gRend.positionCount = lRend.positionCount;

            Vector3[] positions = new Vector3[gRend.positionCount];
            lRend.GetPositions(positions);

            // Translate to ghost's space
            for (int i=0; i<positions.Length; i++)
                positions[i] += ghost.transform.position - transform.position;

            gRend.SetPositions(positions);
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

        // Screen wrap
        if (loopable)
        {
            Vector2Int dir = loopObject(transform);
            if (dir.magnitude > 0)
                onWrap.Invoke(dir, ghosts[dir.x + 1, dir.y + 1]);
        }
    }

    public static Vector2Int loopObject(Transform obj)
    {
        // Loop the target if they leave the camera

        Vector2 pPos = obj.position;
        GameManager.getGameCorners(out Vector2 bl, out Vector2 ur);
        Vector2 dims = ur - bl;

        Vector2Int dirWrapped = Vector2Int.zero;

        if (pPos.x < bl.x)
        {
            pPos.x += dims.x;
            dirWrapped.x++;
        }
        if (pPos.x > ur.x)
        {
            pPos.x -= dims.x;
            dirWrapped.x--;
        }
        if (pPos.y < bl.y)
        {
            pPos.y += dims.y;
            dirWrapped.y++;
        }
        if (pPos.y > ur.y)
        {
            pPos.y -= dims.y;
            dirWrapped.y--;
        }

        obj.position = pPos;

        return dirWrapped;
    }

    private void OnDestroy()
    {
        foreach (GameObject g in ghosts) if (g != null)
            Destroy(g);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        tryCollisionCheck(collision);
    }

    public void tryCollisionCheck(Collider2D collision)
    {
        GameObject trueObj = tryGetTrueObject(collision.gameObject);
        if (colBanList.Contains(trueObj))
            return;

        // Check true object's col ban list too, if it has one
        ScreenWrapper oppWrap = trueObj.GetComponent<ScreenWrapper>();
        if (oppWrap != null && oppWrap.colBanList.Contains(gameObject))
            return;

        onCollision.Invoke(trueObj);
    }

    public static GameObject tryGetTrueObject(GameObject obj)
    {
        WrapGhost ghost = obj.GetComponent<WrapGhost>();
        if (ghost != null)
            return ghost.parent.gameObject;

        return obj;
    }

    // 1 way switch
    public void activateWrapper()
    {
        if (!loopable)
        {
            loopable = true;
            initWrapper();
        }
    }

    public void initWrapper()
    {
        // Might be getting tripped before the start call
        if (ghosts == null)
            ghosts = new GameObject[3, 3];

        GameManager.getGameCorners(out Vector2 bl, out Vector2 ur);
        Vector2 dims = ur - bl;

        for (int x = 0; x < 3; x++) for (int y = 0; y < 3; y++)
            {
                if (x == 1 && y == 1)
                    continue;

                ghosts[x, y] = genGhost((x - 1) * dims.x, (y - 1) * dims.y, "Ghost_" + x + y);
            }
    }
}
