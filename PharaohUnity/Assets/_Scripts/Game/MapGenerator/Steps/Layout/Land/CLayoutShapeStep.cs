using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Vypeče organické tvary terénu definované dětskými CLayoutShapeStamp objekty.
    /// Přidej child GameObject, přiřaď mu komponent CLayoutShapeStamp a umísti ho ve Scene view.
    /// </summary>
    public class CLayoutShapeStep : CMapGenerationStepBase
    {
        public override string StepName => "Layout Shapes";
        public override string StepDescription => "Vypeče organické tvary terénu (jezera, ostrovy) na pozicích dětských CLayoutShapeStamp objektů. Přidej child GO, přiřaď CLayoutShapeStamp, nastav tvar a umísti ve Scene view.";

        public override void Execute(CMapData mapData, int seed)
        {
            var stamps = GetComponentsInChildren<CLayoutShapeStamp>();
            if (stamps.Length == 0)
            {
                Debug.LogWarning("[CLayoutShapeStep] No CLayoutShapeStamp children found — skipping.");
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
                    Debug.LogWarning($"[CLayoutShapeStep] Stamp '{stamp.name}' at grid {gridCenter} is out of bounds — skipped.");
                    continue;
                }
                if (!stamp.IsCenterValid(mapData.Get(gridCenter.x, gridCenter.y).Type))
                {
                    Debug.LogWarning($"[CLayoutShapeStep] Stamp '{stamp.name}' at grid {gridCenter} lands on non-buildable tile — skipped.");
                    continue;
                }

                int placed = stamp.Bake(mapData, gridCenter);
                Debug.Log($"[CLayoutShapeStep] Stamp '{stamp.name}' → {placed} tiles at {gridCenter}.");
            }
        }
    }
}
