// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using NaughtyAttributes;
using ServerData;
using ServerData.Design;
using TycoonBuilder.Configs.Design;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/Dialogues/DialogueCharacterConfig", fileName = "cfg_bundle_dialogueCharacters_Name")]
	public class CDialogueCharacterConfig : CScriptableResourceConfig<EDialogueCharacter>, IIHaveBundleLinks
	{
		[SerializeField] private EDialogueCharacterJob _characterJob;
		[SerializeField] private CFacialExpressionSpritePair[] _characterFacialExpressions;

		public EDialogueCharacterJob CharacterJob => _characterJob;
		public CFacialExpressionSpritePair[] CharacterFacialExpressions => _characterFacialExpressions;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			foreach (CFacialExpressionSpritePair expression in _characterFacialExpressions)
			{
				expression.Visual2d.SetBundleId((int)EBundleId.BaseGame);
				yield return expression.Visual2d;
			}
		}
	}
}