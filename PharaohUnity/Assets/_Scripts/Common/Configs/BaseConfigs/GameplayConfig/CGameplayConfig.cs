// =========================================
// DATE:   02.03.2026
// =========================================

using UnityEngine;

namespace Pharaoh
{
	[CreateAssetMenu(menuName = "____Pharaoh/Configs/GameplayConfig")]
	public class CGameplayConfig : ScriptableObject
	{
		[SerializeField] private int _maxKnowledgePoints = 10;
		[SerializeField] private int _knowledgePointsRegenIntervalMinutes = 10;

		public int MaxKnowledgePoints => _maxKnowledgePoints;
		public int KnowledgePointsRegenIntervalMinutes => _knowledgePointsRegenIntervalMinutes;
	}
}
