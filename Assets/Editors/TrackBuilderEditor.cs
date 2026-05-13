using UnityEditor;
using UnityEngine;

/// <summary>
/// GameObject/Build Track  — buduje jeden odcinek toru z cube'ów.
/// GameObject/Build Track (append) — dokłada kolejny odcinek do istniejącego,
///   startując dokładnie tam gdzie poprzedni się skończył.
/// </summary>
public static class TrackBuilderEditor
{
    // Wymiary kafli
    private const float TileSize   = 4f;
    private const float TileHeight = 0.5f;
    private const float WallHeight = 2.5f;
    private const float WallWidth  = 1f;

    // Trasa: (kierunek_względny, długość_w_kaflach)
    // Kierunek względny: 0=prosto, 1=skręt w prawo, 3=skręt w lewo
    // Układ: długa prosta → łuk w prawo → długa prosta → łuk w lewo → prosta do mety
    private static readonly (int dir, int len)[] Path = new[]
    {
        (0, 15),  // długa prosta
        (1,  6),  // skręt w prawo
        (0, 10),  // prosta
        (3,  6),  // skręt w lewo (wracamy do pierwotnego kierunku)
        (0, 15),  // długa prosta do mety
    };

    // Zapamiętana pozycja i kierunek końca ostatnio zbudowanego odcinka
    private static Vector3 _nextStart  = Vector3.zero;
    private static int     _nextFacing = 0;
    private static int     _segmentCount = 0;

    // ── Menu ──────────────────────────────────────────────────────────────────

    [MenuItem("GameObject/Build Track")]
    public static void BuildTrackFresh()
    {
        _nextStart  = Vector3.zero;
        _nextFacing = 0;
        _segmentCount = 0;

        // Usuń stare odcinki
        for (int i = 0; i < 100; i++)
        {
            var old = GameObject.Find($"Track_{i}");
            if (old != null) Undo.DestroyObjectImmediate(old);
            else break;
        }
        var oldSingle = GameObject.Find("Track");
        if (oldSingle != null) Undo.DestroyObjectImmediate(oldSingle);

        BuildSegment();
    }

    [MenuItem("GameObject/Build Track (append)")]
    public static void BuildTrackAppend()
    {
        BuildSegment();
    }

    // ── Budowanie segmentu ────────────────────────────────────────────────────

    private static void BuildSegment()
    {
        string name = $"Track_{_segmentCount}";
        var root = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(root, "Build Track Segment");

        var floorMat = GetOrCreateMaterial("TrackFloor", new Color(0.25f, 0.25f, 0.25f));
        var wallMat  = GetOrCreateMaterial("TrackWall",  new Color(0.85f, 0.15f, 0.15f));
        var startMat = GetOrCreateMaterial("TrackStart", new Color(0.15f, 0.85f, 0.15f));
        var endMat   = GetOrCreateMaterial("TrackEnd",   new Color(0.15f, 0.15f, 0.85f));

        Vector3[] dirs = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

        Vector3 cursor  = _nextStart;
        int     facing  = _nextFacing;
        bool    isFirst = true;
        Vector3 lastTile = cursor;

        foreach (var (segDir, segLen) in Path)
        {
            facing = (facing + segDir) % 4;
            Vector3 step = dirs[facing] * TileSize;

            for (int i = 0; i < segLen; i++)
            {
                PlaceFloor(cursor, facing, root.transform, isFirst ? startMat : floorMat);
                PlaceWalls(cursor, facing, root.transform, wallMat);
                lastTile = cursor;
                cursor  += step;
                isFirst  = false;
            }
        }

        OverrideFloorMaterial(root.transform, lastTile, endMat);

        // Zapisz punkt startu dla następnego segmentu
        _nextStart  = cursor;
        _nextFacing = facing;
        _segmentCount++;

        Debug.Log($"[TrackBuilder] Segment {name} gotowy. " +
                  $"Start: {_nextStart - dirs[facing] * TileSize * Path[^1].len}, " +
                  $"Koniec (niebieski): {lastTile}. " +
                  $"Następny segment zacznie od: {_nextStart}");

        Selection.activeGameObject = root;
    }

    // ── Kafel podłogi ─────────────────────────────────────────────────────────

    private static void PlaceFloor(Vector3 center, int facing, Transform parent, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Floor";
        go.transform.SetParent(parent, false);
        go.transform.position   = center + Vector3.up * (TileHeight * 0.5f);
        go.transform.localScale = new Vector3(TileSize, TileHeight, TileSize);
        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    // ── Ściany ────────────────────────────────────────────────────────────────

    private static void PlaceWalls(Vector3 center, int facing, Transform parent, Material mat)
    {
        Vector3[] dirs  = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
        Vector3 forward = dirs[facing];
        Vector3 right   = dirs[(facing + 1) % 4];
        PlaceWall(center,  right, forward, parent, mat);
        PlaceWall(center, -right, forward, parent, mat);
    }

    private static void PlaceWall(Vector3 tileCenter, Vector3 side, Vector3 forward,
                                   Transform parent, Material mat)
    {
        float halfTile = TileSize * 0.5f;
        float wallY    = TileHeight + WallHeight * 0.5f;

        Vector3 pos = tileCenter
                    + side * (halfTile + WallWidth * 0.5f)
                    + Vector3.up * wallY;

        Vector3 scale = Mathf.Abs(forward.x) > 0.5f
            ? new Vector3(TileSize, WallHeight, WallWidth)
            : new Vector3(WallWidth, WallHeight, TileSize);

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Wall";
        go.transform.SetParent(parent, false);
        go.transform.position   = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void OverrideFloorMaterial(Transform root, Vector3 pos, Material mat)
    {
        float tolerance = 0.1f;
        foreach (Transform child in root)
        {
            if (child.name == "Floor" &&
                Vector3.Distance(child.position, pos + Vector3.up * (TileHeight * 0.5f)) < tolerance)
            {
                child.GetComponent<Renderer>().sharedMaterial = mat;
                return;
            }
        }
    }

    // ── Materiały ─────────────────────────────────────────────────────────────

    private static Material GetOrCreateMaterial(string name, Color color)
    {
        string path = $"Assets/Materials/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }
}
