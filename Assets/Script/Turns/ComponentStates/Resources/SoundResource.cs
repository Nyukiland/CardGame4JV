using CardGame.Card;
using CardGame.StateMachine;
using FMOD.Studio;
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

		[SerializeField]
		private EventReference _tileSelect;

		[SerializeField]
		private EventReference _tileRotate;

		[SerializeField]
		private EventReference _shakeTile;

		[Space(5)]
		[Header("Scoring")]

		[SerializeField]
		private EventReference _scoring;

		[SerializeField]
		private EventReference _scoringOther;

		[Space(5)]
		[Header("Ambiance")]

		[SerializeField]
		private EventReference _desert;

		[SerializeField]
		private EventReference _forest;

		[SerializeField]
		private EventReference _field;

		[SerializeField]
		private EventReference _sea;

		private EventInstance _desertInstance;
		private EventInstance _forestInstance;
		private EventInstance _fieldInstance;
		private EventInstance _seaInstance;

		[Space(5)]
		[Header("Music")]

		[SerializeField]
		private EventReference _music;

		private EventInstance _musicInstance;

		[Space(5)]
		[Header("Turn")]

		[SerializeField]
		private EventReference _myTurnSound;

		[Space(5)]
		[Header("UI")]

		[SerializeField]
		private EventReference _openMenu;

		[SerializeField]
		private EventReference _closeMenu;

		[SerializeField]
		private EventReference _clickButton;

		public override void Init(Controller owner)
		{
			base.Init(owner);

			_desertInstance = FMODUnity.RuntimeManager.CreateInstance(_desert);
			_forestInstance = FMODUnity.RuntimeManager.CreateInstance(_forest);
			_fieldInstance = FMODUnity.RuntimeManager.CreateInstance(_field);
			_seaInstance = FMODUnity.RuntimeManager.CreateInstance(_sea);
			_musicInstance = FMODUnity.RuntimeManager.CreateInstance(_music);

			_musicInstance.start();
		}

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

		public void PlayTileSelect() => FMODUnity.RuntimeManager.PlayOneShot(_tileSelect);
		public void PlayTileRotate() => FMODUnity.RuntimeManager.PlayOneShot(_tileRotate);
		public void PlayShakeTile() => FMODUnity.RuntimeManager.PlayOneShot(_shakeTile);
		public void PlayMyTurn() => FMODUnity.RuntimeManager.PlayOneShot(_myTurnSound);
		public void PlayClickButton() => FMODUnity.RuntimeManager.PlayOneShot(_clickButton);
		public void PlayOpenMenu() => FMODUnity.RuntimeManager.PlayOneShot(_openMenu);
		public void PlayCloseMenu() => FMODUnity.RuntimeManager.PlayOneShot(_closeMenu);

		public void PlayZoneAmbiance(ENVIRONEMENT_TYPE environment)
		{
			EventInstance instance = new();

			switch (environment)
			{
				case ENVIRONEMENT_TYPE.Water:
					instance = _seaInstance;
					break;
				case ENVIRONEMENT_TYPE.Grass:
					instance = _forestInstance;
					break;
				case ENVIRONEMENT_TYPE.Terrain:
					instance = _fieldInstance;
					break;
				case ENVIRONEMENT_TYPE.Fields:
					instance = _desertInstance;
					break;
			}

			if (IsPlaying(_seaInstance) && instance.handle != _seaInstance.handle)
				_seaInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

			if (IsPlaying(_desertInstance) && instance.handle != _desertInstance.handle)
				_desertInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

			if (IsPlaying(_fieldInstance) && instance.handle != _fieldInstance.handle)
				_fieldInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

			if (IsPlaying(_forestInstance) && instance.handle != _forestInstance.handle)
				_forestInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

			if (IsPlaying(instance))
				instance.start();
		}

		private bool IsPlaying(EventInstance instance)
		{
			PLAYBACK_STATE state;
			instance.getPlaybackState(out state);
			return state == PLAYBACK_STATE.PLAYING;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			_musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_forestInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_fieldInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_desertInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_seaInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
	}
}