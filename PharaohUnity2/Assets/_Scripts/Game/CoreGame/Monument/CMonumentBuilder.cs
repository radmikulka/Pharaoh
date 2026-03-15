using System.Collections.Generic;
using System.Text;
using NaughtyAttributes;
using UnityEngine;

namespace Pharaoh
{
    public class CMonumentBuilder : MonoBehaviour
    {
        private const string Tag = "[MonumentBuilder]";

        [SerializeField] private GameObject _sourceAsset;
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] [Range(0f, 1f)] private float _buildProgress = 1f;

        // ───────── Entry points ─────────

        [Button("Generate Parts")]
        private void GenerateParts()
        {
            if (!ValidateSetup(out MeshFilter filter, out List<CMonumentPart> parts))
                return;

            Mesh sourceMesh = filter.sharedMesh;
            LogGenerateStart(sourceMesh, parts.Count);

            List<CMeshSlicer.Tri> remaining = CMeshSlicer.ExtractTriangles(filter);
            List<CMeshSlicer.Tri> sourceTris = new List<CMeshSlicer.Tri>(remaining);
            Material sourceMaterial = _sourceAsset.GetComponent<MeshRenderer>().sharedMaterial;
            Texture2D sourceTexture = ExtractReadableTexture(sourceMaterial);
            Material monumentMaterial = CreateMonumentMaterial(sourceMaterial);
            Debug.Log($"{Tag} Extracted {remaining.Count} triangles (world space)");
            LogBounds(remaining, "Source mesh");

            float snapEps = CMeshSlicer.ComputeSnapEpsilon(remaining);
            Debug.Log($"{Tag} Snap epsilon: {snapEps:E3}");

            List<CMeshSlicer.BoxPlanes> allPlanes = new List<CMeshSlicer.BoxPlanes>();

            // Pass 1: CutoutOnly parts (carve the mesh)
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                if (parts[i].Mode != CMonumentPart.EPartMode.CutoutOnly)
                    continue;
                remaining = ProcessPart(parts[i], i, remaining, snapEps, allPlanes, sourceTris,
                    monumentMaterial, sourceTexture);
            }

            // Pass 2: Default parts (generate geometry from carved mesh)
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                if (parts[i].Mode == CMonumentPart.EPartMode.CutoutOnly)
                    continue;
                remaining = ProcessPart(parts[i], i, remaining, snapEps, allPlanes, sourceTris,
                    monumentMaterial, sourceTexture);
            }

            Debug.Log($"{Tag} Remaining tris after all parts: {remaining.Count} (discarded)");
            Debug.Log($"{Tag} ═══ GENERATE PARTS COMPLETE ═══");

            _sourceAsset.SetActive(false);
        }

        [Button("Generate All")]
        private void GenerateAll()
        {
            if (!ValidateSetup(out MeshFilter filter, out List<CMonumentPart> parts))
                return;

            // Step 1: Generate parts
            Debug.Log($"{Tag} ═══ GENERATE ALL: STEP 1 — PARTS ═══");

            Mesh sourceMesh = filter.sharedMesh;
            LogGenerateStart(sourceMesh, parts.Count);

            List<CMeshSlicer.Tri> remaining = CMeshSlicer.ExtractTriangles(filter);
            List<CMeshSlicer.Tri> sourceTris = new List<CMeshSlicer.Tri>(remaining);
            Material sourceMaterial = _sourceAsset.GetComponent<MeshRenderer>().sharedMaterial;
            Texture2D sourceTexture = ExtractReadableTexture(sourceMaterial);
            Material monumentMaterial = CreateMonumentMaterial(sourceMaterial);
            Debug.Log($"{Tag} Extracted {remaining.Count} triangles (world space)");
            LogBounds(remaining, "Source mesh");

            float snapEps = CMeshSlicer.ComputeSnapEpsilon(remaining);
            Debug.Log($"{Tag} Snap epsilon: {snapEps:E3}");

            List<CMeshSlicer.BoxPlanes> allPlanes = new List<CMeshSlicer.BoxPlanes>();

            // Pass 1: CutoutOnly parts (carve the mesh)
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                if (parts[i].Mode != CMonumentPart.EPartMode.CutoutOnly)
                    continue;
                remaining = ProcessPart(parts[i], i, remaining, snapEps, allPlanes, sourceTris,
                    monumentMaterial, sourceTexture);
            }

            // Pass 2: Default parts (generate geometry from carved mesh)
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                if (parts[i].Mode == CMonumentPart.EPartMode.CutoutOnly)
                    continue;
                remaining = ProcessPart(parts[i], i, remaining, snapEps, allPlanes, sourceTris,
                    monumentMaterial, sourceTexture);
            }

            Debug.Log($"{Tag} Remaining tris after all parts: {remaining.Count} (discarded)");
            Debug.Log($"{Tag} ═══ STEP 1 COMPLETE ═══");

            // Step 2: Grid subdivision
            Debug.Log($"{Tag} ═══ GENERATE ALL: STEP 2 — GRID SUBDIVISION ═══");

            int subdividedCount = 0;
            int skippedCount = 0;

            foreach (CMonumentPart part in parts)
            {
                if (part.Mode == CMonumentPart.EPartMode.CutoutOnly)
                {
                    Debug.Log($"{Tag}   Part '{part.name}': CutoutOnly → skipping subdivision");
                    skippedCount++;
                    continue;
                }

                if (part.CellSize == Vector3.zero)
                {
                    Debug.Log($"{Tag}   Part '{part.name}': CellSize is zero → skipping subdivision");
                    skippedCount++;
                    continue;
                }

                if (part.GeneratedMesh == null)
                {
                    Debug.Log($"{Tag}   Part '{part.name}': no generated mesh → skipping subdivision");
                    skippedCount++;
                    continue;
                }

                Debug.Log($"{Tag}   Part '{part.name}': CellSize={part.CellSize} → subdividing...");

                Transform partTransform = part.transform;
                Transform generatedChild = null;
                for (int i = 0; i < partTransform.childCount; i++)
                {
                    if (partTransform.GetChild(i).name == "_Generated")
                    {
                        generatedChild = partTransform.GetChild(i);
                        break;
                    }
                }

                if (generatedChild == null)
                {
                    Debug.LogWarning($"{Tag}   Part '{part.name}': _Generated child not found → skipping");
                    skippedCount++;
                    continue;
                }

                MeshFilter partFilter = generatedChild.GetComponent<MeshFilter>();
                List<CMeshSlicer.Tri> partTris = CMeshSlicer.ExtractTriangles(partFilter);
                Debug.Log($"{Tag}   Part '{part.name}': extracted {partTris.Count} tris from generated mesh");

                SubdivideIntoGrid(partTris, part, snapEps, monumentMaterial, sourceTexture);
                subdividedCount++;
            }

            Debug.Log($"{Tag} ═══ GENERATE ALL COMPLETE: {subdividedCount} subdivided, {skippedCount} skipped ═══");

            _sourceAsset.SetActive(false);
        }

        [Button]
        private void Clean()
        {
            List<CMonumentPart> parts = CollectParts();
            foreach (CMonumentPart part in parts)
            {
                part.GeneratedMesh = null;
                part.Cells = null;
                CleanPartChild(part);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(part);
#endif
            }

            if (_sourceAsset != null)
            {
                _sourceAsset.SetActive(true);
            }

            Debug.Log($"{Tag} Cleaned {parts.Count} parts, source asset re-enabled.");
        }

        // ───────── Progress preview ─────────

        private void OnValidate()
        {
            List<CMonumentPart> parts = CollectParts();
            var allCells = new List<GameObject>();

            foreach (CMonumentPart part in parts)
            {
                if (part.Cells == null)
                    continue;
                allCells.AddRange(part.Cells);
            }

            if (allCells.Count == 0)
                return;

            // Cross-part merge sort: Y (primary), X (secondary), Z (tertiary)
            allCells.Sort((a, b) =>
            {
                int cmp = a.transform.position.y.CompareTo(b.transform.position.y);
                if (cmp != 0)
                    return cmp;

                cmp = a.transform.position.x.CompareTo(b.transform.position.x);
                if (cmp != 0)
                    return cmp;

                return a.transform.position.z.CompareTo(b.transform.position.z);
            });

            int visibleCount = Mathf.RoundToInt(_buildProgress * allCells.Count);

            for (int i = 0; i < allCells.Count; i++)
            {
                allCells[i].SetActive(i < visibleCount);
            }
        }

        // ───────── Per-part processing ─────────

        private List<CMeshSlicer.Tri> ProcessPart(
            CMonumentPart part, int index,
            List<CMeshSlicer.Tri> remaining, float snapEps,
            List<CMeshSlicer.BoxPlanes> allPlanes,
            List<CMeshSlicer.Tri> sourceTris,
            Material sourceMaterial,
            Texture2D sourceTexture)
        {
            string partName = part.name;
            Debug.Log($"{Tag} ─── Part [{index}] '{partName}' ───");

            BoxCollider box = part.GetComponent<BoxCollider>();

            LogBoxCollider(box, partName);

            CMeshSlicer.BoxPlanes planes = CMeshSlicer.GetBoxPlanes(box);
            LogBoxPlanes(planes, partName);

            if (part.Mode == CMonumentPart.EPartMode.CutoutOnly)
            {
                remaining = CMeshSlicer.SubtractBox(remaining, planes);
                Debug.Log($"{Tag}   CutoutOnly: subtracted box, remaining={remaining.Count}");
                allPlanes.Add(planes);
                part.GeneratedMesh = null;
                part.Cells = null;
                CleanPartChild(part);
                return remaining;
            }

            int remainingBefore = remaining.Count;
            List<CMeshSlicer.Tri> clipped = CMeshSlicer.ClipToBox(remaining, planes);
            Debug.Log($"{Tag}   ClipToBox: {remainingBefore} input tris → {clipped.Count} clipped tris");

            if (clipped.Count == 0)
            {
                // No surface geometry inside the box.
                // Check if the box is inside the source mesh (interior part).
                Vector3 boxCenter = box.transform.TransformPoint(box.center);
                bool isInside = CMeshSlicer.IsPointInsideMesh(boxCenter, remaining);
                Debug.Log($"{Tag}   Box center {boxCenter} is {(isInside ? "INSIDE" : "OUTSIDE")} the mesh " +
                          $"(ray-cast test on {remaining.Count} tris)");

                if (!isInside)
                {
                    Debug.LogWarning($"{Tag}   Part '{partName}' got 0 clipped tris and box is outside mesh → no mesh generated");
                    part.GeneratedMesh = null;
                    part.Cells = null;
                    allPlanes.Add(planes);
                    return remaining;
                }

                // Interior box: generate box faces as geometry
                Debug.Log($"{Tag}   Part '{partName}': box is INSIDE mesh → generating box face geometry");

                List<CMeshSlicer.Tri> boxTris = CMeshSlicer.GenerateBoxTris(box, false, sourceTris, sourceTexture);
                Debug.Log($"{Tag}   Generated {boxTris.Count} box face tris (outward normals) for part");

                Mesh mesh = CMeshSlicer.BuildMesh(boxTris, part.transform);
                Debug.Log($"{Tag}   Mesh built: verts={mesh.vertexCount}, " +
                          $"tris={mesh.triangles.Length / 3}, bounds={mesh.bounds}");
                ApplyMesh(part, mesh, sourceMaterial);
                part.Cells = null;

                List<CMeshSlicer.Tri> flippedTris = CMeshSlicer.GenerateBoxTris(box, true, sourceTris, sourceTexture);
                remaining.AddRange(flippedTris);
                Debug.Log($"{Tag}   Added {flippedTris.Count} inverted box tris to remaining " +
                          $"(hole walls for outer parts). Remaining now: {remaining.Count}");

                // Skip SubtractBox — no surface geometry is inside the box,
                // so SubtractBox would only split tris at box planes without removing anything.
                allPlanes.Add(planes);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(part);
#endif

                return remaining;
            }

            remaining = CMeshSlicer.SubtractBox(remaining, planes);
            Debug.Log($"{Tag}   SubtractBox: {remainingBefore} input tris → {remaining.Count} remaining tris");

            LogBounds(clipped, $"Part '{partName}' clipped");

            List<CMeshSlicer.Tri> caps = CMeshSlicer.BuildCaps(clipped, snapEps, allPlanes, planes, partName,
                sourceTris, sourceTexture);
            allPlanes.Add(planes);
            Debug.Log($"{Tag}   Caps: {caps.Count} cap tris generated");
            clipped.AddRange(caps);

            int geometryCount = clipped.Count - caps.Count;
            Debug.Log($"{Tag}   Total tris for '{partName}': {clipped.Count} " +
                      $"(geometry={geometryCount} + caps={caps.Count})");

            Mesh mesh2 = CMeshSlicer.BuildMesh(clipped, part.transform);
            Debug.Log($"{Tag}   Mesh built: verts={mesh2.vertexCount}, " +
                      $"tris={mesh2.triangles.Length / 3}, bounds={mesh2.bounds}");
            ApplyMesh(part, mesh2, sourceMaterial);
            part.Cells = null;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(part);
#endif

            return remaining;
        }

        // ───────── Grid subdivision ─────────

        private void SubdivideIntoGrid(
            List<CMeshSlicer.Tri> partTris, CMonumentPart part, float snapEps,
            Material sourceMaterial, Texture2D sourceTexture = null)
        {
            BoxCollider box = part.GetComponent<BoxCollider>();
            Vector3 boxMin = box.center - box.size * 0.5f;
            Vector3 boxMax = box.center + box.size * 0.5f;
            Vector3 cellSize = part.CellSize;
            Transform t = part.transform;

            int countX = cellSize.x > 0f ? Mathf.CeilToInt((boxMax.x - boxMin.x) / cellSize.x) : 1;
            int countY = cellSize.y > 0f ? Mathf.CeilToInt((boxMax.y - boxMin.y) / cellSize.y) : 1;
            int countZ = cellSize.z > 0f ? Mathf.CeilToInt((boxMax.z - boxMin.z) / cellSize.z) : 1;

            float meshMinY = float.MaxValue;
            foreach (var tri in partTris)
            {
                if (tri.A.y < meshMinY) { meshMinY = tri.A.y; }
                if (tri.B.y < meshMinY) { meshMinY = tri.B.y; }
                if (tri.C.y < meshMinY) { meshMinY = tri.C.y; }
            }

            Debug.Log($"{Tag}   Grid subdivision: {countX}x{countY}x{countZ} = {countX * countY * countZ} cells");

            CleanPartChild(part);
            int cellCount = 0;
            int interiorCount = 0;
            int emptyCells = 0;

            for (int ix = 0; ix < countX; ix++)
            {
                for (int iy = 0; iy < countY; iy++)
                {
                    for (int iz = 0; iz < countZ; iz++)
                    {
                        Vector3 cellMin = new Vector3(
                            boxMin.x + ix * (cellSize.x > 0f ? cellSize.x : boxMax.x - boxMin.x),
                            boxMin.y + iy * (cellSize.y > 0f ? cellSize.y : boxMax.y - boxMin.y),
                            boxMin.z + iz * (cellSize.z > 0f ? cellSize.z : boxMax.z - boxMin.z));

                        Vector3 cellMax = new Vector3(
                            Mathf.Min(cellMin.x + (cellSize.x > 0f ? cellSize.x : boxMax.x - boxMin.x), boxMax.x),
                            Mathf.Min(cellMin.y + (cellSize.y > 0f ? cellSize.y : boxMax.y - boxMin.y), boxMax.y),
                            Mathf.Min(cellMin.z + (cellSize.z > 0f ? cellSize.z : boxMax.z - boxMin.z), boxMax.z));

                        string cellName = $"_Cell_{ix}_{iy}_{iz}";
                        CMeshSlicer.BoxPlanes cellPlanes = CMeshSlicer.GetCellPlanes(t, cellMin, cellMax);
                        List<CMeshSlicer.Tri> cellTris = CMeshSlicer.ClipToBox(partTris, cellPlanes);
                        CMeshSlicer.SnapVerticesToPlanes(cellTris, cellPlanes, snapEps);

                        if (cellTris.Count == 0)
                        {
                            Vector3 cellCenter = t.TransformPoint((cellMin + cellMax) / 2f);
                            if (cellCenter.y >= meshMinY && CMeshSlicer.IsPointInsideMesh(cellCenter, partTris))
                            {
                                cellTris = CMeshSlicer.GenerateBoxTris(t, cellMin, cellMax, false, partTris, sourceTexture);
                                Debug.Log($"{Tag}     {cellName}: INTERIOR cell, generated {cellTris.Count} box face tris");
                                interiorCount++;
                            }
                            else
                            {
                                emptyCells++;
                                continue;
                            }
                        }
                        else
                        {
                            Debug.Log($"{Tag}     {cellName}: localMin={cellMin}, localMax={cellMax}, " +
                                      $"clipped={cellTris.Count} tris");

                            List<CMeshSlicer.Tri> caps = CMeshSlicer.BuildCellCaps(
                                cellTris, snapEps, cellPlanes, t, cellMin, cellMax, partTris, cellName,
                                sourceTexture);
                            cellTris.AddRange(caps);

                            Debug.Log($"{Tag}     {cellName}: +{caps.Count} cap tris → total={cellTris.Count}");
                        }

                        Mesh mesh = CMeshSlicer.BuildMesh(cellTris, t, 60f);

                        // Center pivot: move GO to mesh center, offset vertices
                        Vector3 meshCenter = mesh.bounds.center;
                        Vector3[] verts = mesh.vertices;
                        for (int v = 0; v < verts.Length; v++)
                        {
                            verts[v] -= meshCenter;
                        }
                        mesh.vertices = verts;
                        mesh.RecalculateBounds();

                        GameObject child = new GameObject(cellName);
                        child.transform.SetParent(t, false);
                        child.transform.localPosition = meshCenter;

                        MeshFilter filter = child.AddComponent<MeshFilter>();
                        filter.sharedMesh = mesh;

                        MeshRenderer renderer = child.AddComponent<MeshRenderer>();
                        renderer.sharedMaterial = sourceMaterial;

                        cellCount++;
                    }
                }
            }

            // Collect and sort cells: Y (primary), X (secondary), Z (tertiary)
            List<GameObject> sortedCells = new List<GameObject>();
            for (int c = 0; c < t.childCount; c++)
            {
                Transform child = t.GetChild(c);
                if (child.name.StartsWith("_Cell_"))
                {
                    sortedCells.Add(child.gameObject);
                }
            }

            sortedCells.Sort((a, b) =>
            {
                int cmp = a.transform.position.y.CompareTo(b.transform.position.y);
                if (cmp != 0)
                    return cmp;
                cmp = a.transform.position.x.CompareTo(b.transform.position.x);
                if (cmp != 0)
                    return cmp;
                return a.transform.position.z.CompareTo(b.transform.position.z);
            });

            part.Cells = sortedCells;

            Debug.Log($"{Tag}   Grid subdivision complete: {cellCount} cells ({interiorCount} interior), {emptyCells} empty cells skipped");
        }

        // ───────── Validation ─────────

        private bool ValidateSetup(out MeshFilter filter, out List<CMonumentPart> parts)
        {
            filter = null;
            parts = null;

            if (_sourceAsset == null)
            {
                Debug.LogWarning($"{Tag} No source asset assigned.");
                return false;
            }

            filter = _sourceAsset.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
            {
                Debug.LogWarning($"{Tag} Source asset '{_sourceAsset.name}' has no MeshFilter or mesh is null.");
                return false;
            }

            parts = CollectParts();
            if (parts.Count == 0)
            {
                Debug.LogWarning($"{Tag} No CMonumentPart children found.");
                return false;
            }

            return true;
        }

        // ───────── Part collection ─────────

        private List<CMonumentPart> CollectParts()
        {
            List<CMonumentPart> parts = new List<CMonumentPart>();
            for (int i = 0; i < transform.childCount; i++)
            {
                CMonumentPart part = transform.GetChild(i).GetComponent<CMonumentPart>();
                if (part != null)
                {
                    parts.Add(part);
                }
            }
            return parts;
        }

        // ───────── Texture / material helpers ─────────

        private static Texture2D ExtractReadableTexture(Material material)
        {
            Texture2D texture = material.mainTexture as Texture2D;
            if (texture != null && !texture.isReadable)
            {
                Debug.LogWarning($"{Tag} Source texture is not readable. Enable Read/Write in import settings.");
                texture = null;
            }

            return texture;
        }

        private static Material CreateMonumentMaterial(Material sourceMaterial)
        {
            Material material = new Material(sourceMaterial);
            material.EnableKeyword("_VERTEX_COLOR");
            return material;
        }

        // ───────── Mesh application ─────────

        private static void ApplyMesh(CMonumentPart part, Mesh mesh, Material material)
        {
            part.GeneratedMesh = mesh;
            CleanPartChild(part);

            // Center pivot: move GO to mesh center, offset vertices
            Vector3 meshCenter = mesh.bounds.center;
            Vector3[] verts = mesh.vertices;
            for (int v = 0; v < verts.Length; v++)
            {
                verts[v] -= meshCenter;
            }
            mesh.vertices = verts;
            mesh.RecalculateBounds();

            GameObject child = new GameObject("_Generated");
            child.transform.SetParent(part.transform, false);
            child.transform.localPosition = meshCenter;

            MeshFilter filter = child.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        private static void CleanPartChild(CMonumentPart part)
        {
            for (int i = part.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = part.transform.GetChild(i);
                if (child.name == "_Generated" || child.name.StartsWith("_Cell_"))
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        // ───────── Logging ─────────

        private void LogGenerateStart(Mesh sourceMesh, int partCount)
        {
            Debug.Log($"{Tag} ═══ GENERATE START ═══");
            Debug.Log($"{Tag} Source: '{_sourceAsset.name}', mesh='{sourceMesh.name}', " +
                      $"verts={sourceMesh.vertexCount}, tris={sourceMesh.triangles.Length / 3}");
            Debug.Log($"{Tag} Parts count: {partCount}");
        }

        private static void LogBounds(List<CMeshSlicer.Tri> tris, string label)
        {
            if (tris.Count == 0)
                return;

            (Vector3 min, Vector3 max) = CMeshSlicer.ComputeBounds(tris);
            Debug.Log($"{Tag}   {label} bounds: min={min}, max={max}, size={max - min}");
        }

        private static void LogBoxCollider(BoxCollider box, string partName)
        {
            Transform t = box.transform;
            Vector3 worldCenter = t.TransformPoint(box.center);
            Vector3 worldSize = Vector3.Scale(box.size, t.lossyScale);

            Debug.Log($"{Tag}   BoxCollider '{partName}': " +
                      $"localCenter={box.center}, localSize={box.size}, " +
                      $"worldCenter={worldCenter}, worldSize≈{worldSize}, " +
                      $"pos={t.position}, rot={t.rotation.eulerAngles}, scale={t.lossyScale}");
        }

        private static void LogBoxPlanes(CMeshSlicer.BoxPlanes planes, string partName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Tag}   Box planes for '{partName}':");
            string[] faceNames = { "+X", "-X", "+Y", "-Y", "+Z", "-Z" };

            for (int p = 0; p < 6; p++)
            {
                sb.Append($"\n{Tag}     [{faceNames[p]}] normal={planes.Normals[p]:F4}, dist={planes.Dists[p]:F4}");
            }

            Debug.Log(sb.ToString());
        }

        // ───────── Gizmos ─────────

        private static readonly Color[] Palette =
        {
            Color.red, Color.green, Color.blue,
            Color.yellow, Color.cyan, Color.magenta
        };

        private void OnDrawGizmos()
        {
            if (!_drawGizmos)
                return;

            OnDrawGizmosSelected();
        }

        private void OnDrawGizmosSelected()
        {
            if (_sourceAsset == null)
                return;

            MeshFilter filter = _sourceAsset.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
                return;

            List<CMonumentPart> parts = CollectParts();
            List<CMeshSlicer.Tri> sourceTris = CMeshSlicer.ExtractTriangles(filter);

            for (int p = 0; p < parts.Count; p++)
            {
                BoxCollider box = parts[p].GetComponent<BoxCollider>();
                if (box == null)
                    continue;

                Gizmos.color = Palette[p % Palette.Length];
                CMeshSlicer.BoxPlanes planes = CMeshSlicer.GetBoxPlanes(box);
                DrawCutEdges(sourceTris, planes);

                Vector3 cellSize = parts[p].CellSize;
                if (cellSize != Vector3.zero)
                {
                    Color gridColor = Palette[p % Palette.Length];
                    gridColor.a = 0.3f;
                    Gizmos.color = gridColor;
                    DrawGridCutEdges(sourceTris, box, cellSize);
                }
            }
        }

        /// <summary>
        /// For each face of the box, finds where source triangles cross the plane
        /// and draws only the intersection segments that lie within the box face.
        /// </summary>
        private static void DrawCutEdges(List<CMeshSlicer.Tri> tris, CMeshSlicer.BoxPlanes planes)
        {
            for (int pi = 0; pi < 6; pi++)
            {
                Vector3 normal = planes.Normals[pi];
                float dist = planes.Dists[pi];

                foreach (CMeshSlicer.Tri tri in tris)
                {
                    float dA = Vector3.Dot(normal, tri.A) - dist;
                    float dB = Vector3.Dot(normal, tri.B) - dist;
                    float dC = Vector3.Dot(normal, tri.C) - dist;

                    bool aIn = dA <= 0f;
                    bool bIn = dB <= 0f;
                    bool cIn = dC <= 0f;

                    if (aIn == bIn && bIn == cIn)
                        continue;

                    // Find the two edge-plane intersection points
                    Vector3 i0 = Vector3.zero, i1 = Vector3.zero;
                    int count = 0;

                    if (aIn != bIn)
                    {
                        float k = dA / (dA - dB);
                        if (count == 0)
                            i0 = tri.A + k * (tri.B - tri.A);
                        else
                            i1 = tri.A + k * (tri.B - tri.A);
                        count++;
                    }

                    if (bIn != cIn)
                    {
                        float k = dB / (dB - dC);
                        if (count == 0)
                            i0 = tri.B + k * (tri.C - tri.B);
                        else
                            i1 = tri.B + k * (tri.C - tri.B);
                        count++;
                    }

                    if (count < 2 && cIn != aIn)
                    {
                        float k = dC / (dC - dA);
                        i1 = tri.C + k * (tri.A - tri.C);
                        count++;
                    }

                    if (count < 2)
                        continue;

                    // Only draw if both points are inside the box
                    if (IsInsideBox(i0, planes) && IsInsideBox(i1, planes))
                    {
                        Gizmos.DrawLine(i0, i1);
                    }
                }
            }
        }

        private static void DrawGridCutEdges(
            List<CMeshSlicer.Tri> tris, BoxCollider box, Vector3 cellSize)
        {
            Vector3 boxMin = box.center - box.size * 0.5f;
            Vector3 boxMax = box.center + box.size * 0.5f;
            Transform t = box.transform;
            CMeshSlicer.BoxPlanes outerPlanes = CMeshSlicer.GetBoxPlanes(box);

            for (int axis = 0; axis < 3; axis++)
            {
                if (cellSize[axis] <= 0f)
                    continue;

                int count = Mathf.CeilToInt((boxMax[axis] - boxMin[axis]) / cellSize[axis]);

                for (int i = 1; i < count; i++)
                {
                    float offset = boxMin[axis] + i * cellSize[axis];
                    if (offset >= boxMax[axis])
                        break;

                    var localN = Vector3.zero;
                    localN[axis] = 1f;
                    var worldN = t.TransformDirection(localN).normalized;

                    var pointOnPlane = Vector3.zero;
                    pointOnPlane[axis] = offset;
                    float dist = Vector3.Dot(worldN, t.TransformPoint(pointOnPlane));

                    // Draw intersections with the single slice plane, clipped to box
                    foreach (CMeshSlicer.Tri tri in tris)
                    {
                        float dA = Vector3.Dot(worldN, tri.A) - dist;
                        float dB = Vector3.Dot(worldN, tri.B) - dist;
                        float dC = Vector3.Dot(worldN, tri.C) - dist;

                        bool aIn = dA <= 0f;
                        bool bIn = dB <= 0f;
                        bool cIn = dC <= 0f;

                        if (aIn == bIn && bIn == cIn)
                            continue;

                        Vector3 i0 = Vector3.zero, i1 = Vector3.zero;
                        int cnt = 0;

                        if (aIn != bIn)
                        {
                            float k = dA / (dA - dB);
                            if (cnt == 0)
                                i0 = tri.A + k * (tri.B - tri.A);
                            else
                                i1 = tri.A + k * (tri.B - tri.A);
                            cnt++;
                        }

                        if (bIn != cIn)
                        {
                            float k = dB / (dB - dC);
                            if (cnt == 0)
                                i0 = tri.B + k * (tri.C - tri.B);
                            else
                                i1 = tri.B + k * (tri.C - tri.B);
                            cnt++;
                        }

                        if (cnt < 2 && cIn != aIn)
                        {
                            float k = dC / (dC - dA);
                            i1 = tri.C + k * (tri.A - tri.C);
                            cnt++;
                        }

                        if (cnt < 2)
                            continue;

                        if (IsInsideBox(i0, outerPlanes) && IsInsideBox(i1, outerPlanes))
                        {
                            Gizmos.DrawLine(i0, i1);
                        }
                    }
                }
            }
        }

        private static bool IsInsideBox(Vector3 point, CMeshSlicer.BoxPlanes planes)
        {
            const float tolerance = 0.001f;
            for (int i = 0; i < 6; i++)
            {
                if (Vector3.Dot(planes.Normals[i], point) - planes.Dists[i] > tolerance)
                    return false;
            }
            return true;
        }
    }
}
