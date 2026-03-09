# GenerateVoxelMesh Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add `GenerateVoxelMesh()` to `CMonumentBuilder` that produces per-cell GameObjects (all `SetActive(false)`) — interior cells get culled cube meshes, boundary cells get Sutherland-Hodgman clipped surface meshes.

**Architecture:** All logic lives inside `#if UNITY_EDITOR` in `CMonumentBuilder.cs`. Uses pre-computed voxel arrays (`_cachedVoxels`, `_cachedBoundary`, `_cachedExcluded`) from `CMonumentPart`. Static helpers for: (1) S-H clipper, (2) culled cube faces, (3) asset triangle collection. One GO per cell named `cell_{x}_{y}_{z}`.

**Tech Stack:** Unity Editor C#, `UnityEngine.Mesh`, no extra packages.

---

## Key Concepts

**Interior cell** (`_cachedVoxels[idx] == true`, not excluded): generates a cube mesh with only the faces that border a non-interior cell (Minecraft face culling).

**Boundary cell** (`_cachedBoundary[idx] == true`, not excluded): clips every asset triangle against the cell's 6 AABB planes using Sutherland-Hodgman; surviving polygon is fan-triangulated.

**Sutherland-Hodgman single-plane clip:** iterate polygon edges (A→B); output A if inside, output intersection if edge crosses plane, skip if both outside.

---

## Task 1: Static utility helpers

**Files:**
- Modify: `Assets/_Scripts/Game/CoreGame/CMonumentBuilder.cs`

Add inside the `#if UNITY_EDITOR` block, after `CreateGeneratedGO()` and before `OnDrawGizmosSelected()`.

---

### Step 1 — Add `ClipPolyByPlane`

Clips a convex polygon list against one half-space (keeps vertices where `dot(normal, v−planePoint) ≥ 0`).

```csharp
static List<Vector3> ClipPolyByPlane(List<Vector3> poly, Vector3 normal, Vector3 planePoint)
{
    var result = new List<Vector3>(poly.Count + 1);
    int n = poly.Count;
    for (int i = 0; i < n; i++)
    {
        var   a  = poly[i];
        var   b  = poly[(i + 1) % n];
        float da = Vector3.Dot(normal, a - planePoint);
        float db = Vector3.Dot(normal, b - planePoint);
        if (da >= 0f) result.Add(a);
        if ((da < 0f) != (db < 0f))
        {
            float t = da / (da - db);
            result.Add(a + t * (b - a));
        }
    }
    return result;
}
```

---

### Step 2 — Add `ClipTriToCell`

Clips a triangle against an axis-aligned cell box using 6 successive half-space clips.
Returns the surviving convex polygon (≥3 vertices) or an empty list.

```csharp
static List<Vector3> ClipTriToCell(Vector3 v0, Vector3 v1, Vector3 v2,
                                   Vector3 cMin, Vector3 cMax)
{
    var poly = new List<Vector3>(8) { v0, v1, v2 };
    poly = ClipPolyByPlane(poly, Vector3.right,   new Vector3(cMin.x, 0f,     0f    )); // x ≥ cMin.x
    if (poly.Count == 0) return poly;
    poly = ClipPolyByPlane(poly, Vector3.left,    new Vector3(cMax.x, 0f,     0f    )); // x ≤ cMax.x
    if (poly.Count == 0) return poly;
    poly = ClipPolyByPlane(poly, Vector3.up,      new Vector3(0f,     cMin.y, 0f    )); // y ≥ cMin.y
    if (poly.Count == 0) return poly;
    poly = ClipPolyByPlane(poly, Vector3.down,    new Vector3(0f,     cMax.y, 0f    )); // y ≤ cMax.y
    if (poly.Count == 0) return poly;
    poly = ClipPolyByPlane(poly, Vector3.forward, new Vector3(0f,     0f,     cMin.z)); // z ≥ cMin.z
    if (poly.Count == 0) return poly;
    poly = ClipPolyByPlane(poly, Vector3.back,    new Vector3(0f,     0f,     cMax.z)); // z ≤ cMax.z
    return poly;
}
```

**Plane normals used:**
- `Vector3.right` = (+1,0,0) → keeps x ≥ cMin.x
- `Vector3.left`  = (−1,0,0) → keeps x ≤ cMax.x
- `Vector3.up`    = (0,+1,0) → keeps y ≥ cMin.y
- `Vector3.down`  = (0,−1,0) → keeps y ≤ cMax.y
- `Vector3.forward` = (0,0,+1) → keeps z ≥ cMin.z
- `Vector3.back`  = (0,0,−1) → keeps z ≤ cMax.z

---

### Step 3 — Add `TriAABBOverlap` (quick reject)

```csharp
static bool TriAABBOverlap(Vector3 v0, Vector3 v1, Vector3 v2,
                            Vector3 aMin, Vector3 aMax)
{
    var tMin = Vector3.Min(Vector3.Min(v0, v1), v2);
    var tMax = Vector3.Max(Vector3.Max(v0, v1), v2);
    return tMin.x <= aMax.x && tMax.x >= aMin.x
        && tMin.y <= aMax.y && tMax.y >= aMin.y
        && tMin.z <= aMax.z && tMax.z >= aMin.z;
}
```

---

### Step 4 — Add `IsVoxelAt`

Returns true only if the cell at (x,y,z) is a filled voxel AND not excluded (excluded cells belong to a child part and are treated as empty for face-culling purposes).

```csharp
static bool IsVoxelAt(bool[] voxels, bool[] excluded,
                      int nx, int ny, int nz, int x, int y, int z)
{
    if (x < 0 || x >= nx || y < 0 || y >= ny || z < 0 || z >= nz) return false;
    int idx = x + y * nx + z * nx * ny;
    return voxels[idx] && (excluded == null || !excluded[idx]);
}
```

---

### Step 5 — Add `AppendQuad` and `AppendCulledCube`

`AppendQuad` adds a CCW quad (two triangles) with 4 unique vertices.

`AppendCulledCube` adds only the cube faces that border a non-voxel neighbour.
It is static and takes a `Transform` to convert world corners to builder-local space.

```csharp
static void AppendQuad(List<Vector3> v, List<int> t,
                       Vector3 a, Vector3 b, Vector3 c, Vector3 d)
{
    int b0 = v.Count;
    v.Add(a); v.Add(b); v.Add(c); v.Add(d);
    t.Add(b0); t.Add(b0 + 1); t.Add(b0 + 2);
    t.Add(b0); t.Add(b0 + 2); t.Add(b0 + 3);
}

static void AppendCulledCube(List<Vector3> verts, List<int> tris,
    Vector3 worldCenter, Vector3 halfSize,
    bool[] voxels, bool[] excluded, int nx, int ny, int nz,
    int ix, int iy, int iz, Transform builderTransform)
{
    float hx = halfSize.x, hy = halfSize.y, hz = halfSize.z;

    // Convert a world offset to builder-local space
    Vector3 L(float dx, float dy, float dz) =>
        builderTransform.InverseTransformPoint(worldCenter + new Vector3(dx, dy, dz));

    if (!IsVoxelAt(voxels, excluded, nx, ny, nz, ix,     iy,     iz + 1))
        AppendQuad(verts, tris, L(-hx,-hy,+hz), L(+hx,-hy,+hz), L(+hx,+hy,+hz), L(-hx,+hy,+hz)); // +Z
    if (!IsVoxelAt(voxels, excluded, nx, ny, nz, ix,     iy,     iz - 1))
        AppendQuad(verts, tris, L(+hx,-hy,-hz), L(-hx,-hy,-hz), L(-hx,+hy,-hz), L(+hx,+hy,-hz)); // -Z
    if (!IsVoxelAt(voxels, excluded, nx, ny, nz, ix - 1, iy,     iz    ))
        AppendQuad(verts, tris, L(-hx,-hy,-hz), L(-hx,-hy,+hz), L(-hx,+hy,+hz), L(-hx,+hy,-hz)); // -X
    if (!IsVoxelAt(voxels, excluded, nx, ny, nz, ix + 1, iy,     iz    ))
        AppendQuad(verts, tris, L(+hx,-hy,+hz), L(+hx,-hy,-hz), L(+hx,+hy,-hz), L(+hx,+hy,+hz)); // +X
    if (!IsVoxelAt(voxels, excluded, nx, ny, nz, ix,     iy + 1, iz    ))
        AppendQuad(verts, tris, L(-hx,+hy,+hz), L(+hx,+hy,+hz), L(+hx,+hy,-hz), L(-hx,+hy,-hz)); // +Y
    if (!IsVoxelAt(voxels, excluded, nx, ny, nz, ix,     iy - 1, iz    ))
        AppendQuad(verts, tris, L(-hx,-hy,-hz), L(+hx,-hy,-hz), L(+hx,-hy,+hz), L(-hx,-hy,+hz)); // -Y
}
```

---

### Step 6 — Add `BuildCellMesh` and `CollectAssetWorldTris`

```csharp
static Mesh BuildCellMesh(string meshName, List<Vector3> verts, List<int> tris)
{
    var mesh = new Mesh
    {
        name        = meshName,
        indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
    };
    mesh.vertices = verts.ToArray();
    mesh.SetTriangles(tris, 0);
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();
    return mesh;
}

List<(Vector3 v0, Vector3 v1, Vector3 v2)> CollectAssetWorldTris()
{
    var result = new List<(Vector3, Vector3, Vector3)>();
    foreach (var mf in _asset.GetComponentsInChildren<MeshFilter>())
    {
        var mesh = mf.sharedMesh;
        if (mesh == null) continue;
        var raw = mesh.vertices;
        var ltw = mf.transform.localToWorldMatrix;
        var wv  = new Vector3[raw.Length];
        for (int i = 0; i < raw.Length; i++) wv[i] = ltw.MultiplyPoint3x4(raw[i]);
        for (int sub = 0; sub < mesh.subMeshCount; sub++)
        {
            var st = mesh.GetTriangles(sub);
            for (int t = 0; t < st.Length; t += 3)
                result.Add((wv[st[t]], wv[st[t + 1]], wv[st[t + 2]]));
        }
    }
    return result;
}
```

---

### Step 7 — Commit

```bash
git add Assets/_Scripts/Game/CoreGame/CMonumentBuilder.cs
git commit -m "feat: add voxel mesh utility helpers (SH clipper, culled cube, asset tri collector)"
```

---

## Task 2: GenerateVoxelMesh() and BuildVoxelPart()

**Files:**
- Modify: `Assets/_Scripts/Game/CoreGame/CMonumentBuilder.cs`

Add after the utility helpers from Task 1, still before `OnDrawGizmosSelected()`.

---

### Step 1 — Add `GenerateVoxelMesh()`

```csharp
internal void GenerateVoxelMesh()
{
    EnsureAssetCached();
    if (_asset == null)
    {
        Debug.LogWarning("[CMonumentBuilder] No asset assigned.", this);
        return;
    }

    var existing = transform.Find(_generatedRootName);
    if (existing != null)
        DestroyImmediate(existing.gameObject);

    var genRoot = new GameObject(_generatedRootName);
    genRoot.transform.SetParent(transform, false);

    var assetTris = CollectAssetWorldTris();
    foreach (var root in GetRootParts())
        BuildVoxelPart(root, genRoot.transform, assetTris);

    SceneView.RepaintAll();
}
```

---

### Step 2 — Add `BuildVoxelPart()`

```csharp
void BuildVoxelPart(CMonumentPart part, Transform parent,
                    List<(Vector3 v0, Vector3 v1, Vector3 v2)> assetTris)
{
    part.EnsureRecalculated();
    if (part._cachedVoxels == null) return;

    int nx       = part._nx;
    int ny       = part._ny;
    int nz       = part._nz;
    var cellSize = part._cachedCellSize;
    var bmin     = part._cachedWorldBounds.min;

    Material mat     = null;
    var      firstMR = _asset.GetComponentInChildren<MeshRenderer>();
    if (firstMR != null && firstMR.sharedMaterials.Length > 0)
        mat = firstMR.sharedMaterials[0];

    var partGO = new GameObject(part.name);
    partGO.transform.SetParent(parent, false);

    for (int z = 0; z < nz; z++)
    for (int y = 0; y < ny; y++)
    for (int x = 0; x < nx; x++)
    {
        int idx = x + y * nx + z * nx * ny;
        if (part._cachedExcluded != null && part._cachedExcluded[idx]) continue;

        var worldCenter = bmin + new Vector3(
            (x + 0.5f) * cellSize.x,
            (y + 0.5f) * cellSize.y,
            (z + 0.5f) * cellSize.z);

        Mesh cellMesh = null;

        if (part._cachedVoxels[idx])
        {
            // Interior: culled cube (only faces bordering non-voxel cells)
            var verts = new List<Vector3>();
            var tris  = new List<int>();
            AppendCulledCube(verts, tris, worldCenter, cellSize * 0.5f,
                part._cachedVoxels, part._cachedExcluded, nx, ny, nz, x, y, z,
                transform);
            if (verts.Count > 0)
                cellMesh = BuildCellMesh($"cell_{x}_{y}_{z}", verts, tris);
        }
        else if (part._cachedBoundary != null && part._cachedBoundary[idx])
        {
            // Boundary: Sutherland-Hodgman clip of all asset triangles
            var cellMin = bmin + new Vector3(x * cellSize.x, y * cellSize.y, z * cellSize.z);
            var cellMax = cellMin + cellSize;
            var verts   = new List<Vector3>();
            var tris    = new List<int>();

            foreach (var (v0, v1, v2) in assetTris)
            {
                if (!TriAABBOverlap(v0, v1, v2, cellMin, cellMax)) continue;
                var poly = ClipTriToCell(v0, v1, v2, cellMin, cellMax);
                if (poly.Count < 3) continue;

                // Fan triangulate clipped convex polygon
                for (int i = 1; i < poly.Count - 1; i++)
                {
                    tris.Add(verts.Count);
                    tris.Add(verts.Count + 1);
                    tris.Add(verts.Count + 2);
                    verts.Add(transform.InverseTransformPoint(poly[0]));
                    verts.Add(transform.InverseTransformPoint(poly[i]));
                    verts.Add(transform.InverseTransformPoint(poly[i + 1]));
                }
            }
            if (verts.Count > 0)
                cellMesh = BuildCellMesh($"cell_{x}_{y}_{z}", verts, tris);
        }

        if (cellMesh == null) continue;

        var go = new GameObject($"cell_{x}_{y}_{z}");
        go.transform.SetParent(partGO.transform, false);
        go.AddComponent<MeshFilter>().sharedMesh = cellMesh;
        go.AddComponent<MeshRenderer>().sharedMaterials = new[] { mat };
        go.SetActive(false);
    }

    foreach (var child in part.GetDirectChildParts())
        BuildVoxelPart(child, partGO.transform, assetTris);
}
```

---

### Step 3 — Commit

```bash
git add Assets/_Scripts/Game/CoreGame/CMonumentBuilder.cs
git commit -m "feat: add GenerateVoxelMesh() with SH boundary clipping and culled interior cubes"
```

---

## Task 3: Editor button

**Files:**
- Modify: `Assets/_Scripts/Game/CoreGame/CMonumentBuilder.cs`

In `CMonumentBuilderEditor.OnInspectorGUI()`, after the existing "Generate Mesh Parts" block:

```csharp
EditorGUILayout.Space();

if (GUILayout.Button("Generate Voxel Mesh"))
{
    builder.GenerateVoxelMesh();
    EditorUtility.SetDirty(builder.gameObject);
}
```

### Step 1 — Commit

```bash
git add Assets/_Scripts/Game/CoreGame/CMonumentBuilder.cs
git commit -m "feat: add Generate Voxel Mesh editor button"
```

---

## Manual Verification

1. Scene: `Builder` + sphere as `_asset`, one `CMonumentPart` child with BoxCollider, `Cell Size = (1,1,1)`
2. Select CMonumentPart → click **Recalculate** → gizmos show voxels (Inside > 0)
3. Select Builder → click **Generate Voxel Mesh**
4. Hierarchy: `GeneratedMeshes / PartName / cell_X_Y_Z` — all `SetActive(false)`
5. Enable a few interior cells → cubes with only exterior faces visible
6. Enable a few boundary cells → clipped sphere surface patches
7. Enable all cells → overall shape approximates the sphere's Minecraft voxelization
