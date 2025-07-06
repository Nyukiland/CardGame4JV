using CardGame.StateMachine;
using FMODUnity;
using UnityEngine;

namespace CardGame.Turns
{
	public class SoundResource : Resource
	{
		[Header("Tile")]
		[SerializeField]
		private EventReference _tilePlaced;

		[SerializeField]
		private EventReference _tilePlacedOther;

		[Space(5)]
		[Header("Scoring")]

		[SerializeField]
		private EventReference _scoring;

		[SerializeField]
		private EventReference _scoringOther;

		public void PlayTilePlaced(bool isSelf = true)
		{
			if (isSelf) 
				FMODUnity.RuntimeManager.PlayOneShot(_tilePlaced);
			else
				FMODUnity.RuntimeManager.PlayOneShot(_tilePlacedOther);
		}

		public void PlayScoring(bool isSelf = true)
		{
			if (isSelf)
				FMODUnity.RuntimeManager.PlayOneShot(_scoring);
			else
				FMODUnity.RuntimeManager.PlayOneShot(_scoringOther);
		}
	}
}