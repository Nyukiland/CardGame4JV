using CardGame.StateMachine;
using CardGame.UI;
using DG.Tweening;
using UnityEngine;

namespace CardGame.Turns
{
	public class TauntShakeTileAbility : Ability
	{
		[SerializeField]
		private LayerMask _layerTile;

		[SerializeField, Min(0)]
		private float _duration = 0.1f;

		[SerializeField, Min(0)]
		private float _specialRotDuration = 0.3f;

		[SerializeField]
		private Vector2 _shakeStrength = new(0.05f, 0.05f);

		[SerializeField, Min(0)]
		private int _shakeIntensity = 20;

		[SerializeField, Min(0)]
		private int _shakeIntensitySpecial = 50;

		[SerializeField]
		private Vector2 _scaleDown = new(-0.4f, -0.4f);

		[SerializeField, Range(0, 1)]
		private float _probaSpecial;

		private SendInfoAbility _sendInfo;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_sendInfo = owner.GetStateComponent<SendInfoAbility>();
		}

		public void ShakeTile(Vector2 position)
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(position), out RaycastHit hit, 100f, _layerTile))
			{
				if (hit.collider.GetComponentInParent<TileVisu>() is TileVisu visu)
				{
					bool special = Random.Range(0f, 1f) < _probaSpecial;

					_sendInfo.SendTauntShake(visu.PositionOnGrid, special);

					ShakeTileVisu(visu, special);
				}
			}
		}

		public void ShakeTileVisu(TileVisu tile, bool special)
		{
			Sequence seq = DOTween.Sequence();

			if (!special)
			{
				seq.Append(tile.transform.DOShakePosition(_duration, _shakeStrength, _shakeIntensity));
				seq.Join(tile.transform.DOPunchScale(_scaleDown, _duration));
			}
			else
			{
				seq.Append(tile.transform.DOPunchScale(_scaleDown, _duration));
				seq.Join(tile.transform.DOShakeRotation(_specialRotDuration, _shakeIntensitySpecial))
					.OnComplete(() => tile.transform.rotation = Quaternion.identity);
			}

			seq.Play();
		}
	}
}