using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Vypeče obsah (dekorace nebo překážky) definovaný dětskými CContentStamp objekty.
    /// Přidej child GameObject, přiřaď mu komponent CContentStamp a umísti ho ve Scene view.
    /// Zařaď za CDecorationPlacementStep a CObstaclePlacementStep, aby přepsal obecné generování.
    /// </summary>
    public class CContentStep : CMapGenerationStepBase
    {
        public override string StepName => "Content Stamps";
        public override string StepDescription => "Vypeče obsah (dekorace nebo překážky) na pozicích dětských CContentStamp objektů. Přidej child GO, přiřaď CContentStamp, nastav režim a umísti ve Scene view.";

        public override void Execute(CMapData mapData, int seed)
        {
            var stamps = GetComponentsInChildren<CContentStamp>();
            if (stamps.Length == 0)
            {
                Debug.LogWarning("[CContentStep] No CContentStamp children found — skipping.");
                return;
            }

            var generator = GetComponentInParent<CMapGenerator>();

            foreach (var stamp in stamps)
            {
                Vector3 localPos = generator != null
                    ? generator.transform.InverseTransformPoint(stamp.transform.position)
                    : stamp.transform.localPosition;

                var gridCenter = new Vector2Int(
                    Mathf.RoundToInt(localPos.x),
                    Mathf.RoundToInt(localPos.z));

                if (!mapData.IsValid(gridCenter.x, gridCenter.y))
                {
                    Debug.LogWarning($"[CContentStep] Stamp '{stamp.name}' at grid {gridCenter} is out of bounds — skipped.");
                    continue;
                }
                if (!mapData.Get(gridCenter.x, gridCenter.y).Type.IsBuildable())
                {
                    Debug.LogWarning($"[CContentStep] Stamp '{stamp.name}' at grid {gridCenter} lands on non-buildable tile — skipped.");
                    continue;
                }

                int placed = stamp.Bake(mapData, gridCenter, seed);
                Debug.Log($"[CContentStep] Stamp '{stamp.name}' → {placed} tiles at {gridCenter}.");
            }
        }
    }
}
