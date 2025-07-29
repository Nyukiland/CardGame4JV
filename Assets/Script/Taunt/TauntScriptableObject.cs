using FMODUnity;
using UnityEngine;

namespace CardGame
{
	[CreateAssetMenu(fileName = "Taunt", menuName = "Scriptable Objects/TauntScriptableObject")]
	public class TauntScriptableObject : ScriptableObject
	{
		public string Text;
		public EventReference FmodEvent;

		public float WaitTime = 1f;
		public Sprite[] Anim;
	}
}
