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

		[SerializeField]
		private Vector2 _shakeStrength = new(0.05f, 0.05f);

		[SerializeField, Min(0)]
		private int _shakeIntensity = 20;

		[SerializeField]
		private Vector2 _scaleDown = new(-0.4f, -0.4f);

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
					_sendInfo.SendTauntShake(visu.PositionOnGrid);

					ShakeTileVisu(visu);
				}
			}
		}

		public void ShakeTileVisu(TileVisu tile)
		{
			tile.transform.DOShakePosition(_duration, _shakeStrength, _shakeIntensity);
			tile.transform.DOPunchScale(_scaleDown, _duration);
		}
	}
}