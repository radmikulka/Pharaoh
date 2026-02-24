using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Carves rivers defined by child CRiverStamp GameObjects.
    /// Add a child GameObject, assign a CRiverStamp component, orient transform.forward in the
    /// flow direction, then position it in the Scene view to set the cross-axis intercept.
    /// Multiple stamps produce multiple rivers.
    /// </summary>
    public class CRiverGenerationStep : CMapGenerationStepBase
    {
        public override string StepName => "Rivers";
        public override string StepDescription => "Vyřezává řeky na pozicích dětských CRiverStamp objektů. Přidej child GO, přiřaď CRiverStamp, nastav orientaci a šířku, umísti ve Scene view.";

        public override void Execute(CMapData mapData, int seed)
        {
            var stamps = GetComponentsInChildren<CRiverStamp>();
            if (stamps.Length == 0)
            {
                Debug.LogWarning("[Rivers] No CRiverStamp children found — skipping.");
                return;
            }

            var generator = GetComponentInParent<CMapGenerator>();

            foreach (var stamp in stamps)
            {
                Vector3 localPos = generator != null
                    ? generator.transform.InverseTransformPoint(stamp.transform.position)
                    : stamp.transform.localPosition;

                var stampGrid = new Vector2Int(
                    Mathf.RoundToInt(localPos.x),
                    Mathf.RoundToInt(localPos.z));

                if (!mapData.IsValid(stampGrid.x, stampGrid.y))
                {
                    Debug.LogWarning($"[Rivers] Stamp '{stamp.name}' at grid {stampGrid} is out of bounds — skipped.");
                    continue;
                }

                int placed = stamp.Bake(mapData, stampGrid, seed);
                Debug.Log($"[Rivers] Stamp '{stamp.name}' → {placed} tiles.");
            }
        }
    }
}
