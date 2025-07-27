using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame
{
	[CreateAssetMenu(fileName = "Taunt", menuName = "Scriptable Objects/TauntScriptableObject")]
	public class TauntScriptableObject : ScriptableObject
	{
		public string Text;
		public EventReference FmodEvent;

		public float WaitTime = 1f;
		public List<Image> Anim = new();
	}
}
