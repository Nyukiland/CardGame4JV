using System.Collections.Generic;
using CardGame.StateMachine;
using CardGame.UI;
using CardGame.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGame.Turns
{
	public class ZoneHolderResource : Resource
	{
		[SerializeField]
		private Canvas _canvas;

		[SerializeField]
		private RectTransform _visualHandArea;

		[SerializeField]
		private RectTransform _handZone;

		[SerializeField]
		private float _targetWidth3 = 500f;

		[SerializeField]
		private float _targetWidth10 = 1000f;

		[SerializeField]
		private float _sizeInHand = 1;

		[Disable]
		public List<GameObject> TileInHand;

		public int TileInHandCount => TileInHand.Count;

		private HUDResource _hudResource;

		private float _tileSize;
		private float _defaultCamSize;

		public bool IsHidden { private set; get; } = false;

		public override void Init(Controller owner)
		{
			_hudResource = owner.GetStateComponent<HUDResource>();
			_defaultCamSize = Camera.main.orthographicSize;
			_tileSize = _sizeInHand;
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
			{
				int numberOfTile = TileInHandCount;

				DrawPile drawPile = Storage.Instance.GetElement<DrawPile>();

				for (int i = numberOfTile - 1; i >= 0; i--)
				{
					TileVisu tile = TileInHand[i].GetComponent<TileVisu>();
					drawPile.DiscardTile(tile.TileData.TileSettings.IdCode);
					RemoveTileFromHand(tile.gameObject);
					GameObject.Destroy(tile.gameObject);
				}

				Owner.GetStateComponent<CreateHandAbility>().GenerateTiles(numberOfTile);
			}

			if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
			{
				Owner.GetStateComponent<CreateHandAbility>().GenerateTiles(1);
			}

			if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
			{
				if (TileInHandCount == 0)
					return;

				GameObject temp = TileInHand[0];
				RemoveTileFromHand(temp);
				Storage.Instance.GetElement<DrawPile>().DiscardTile(temp.GetComponent<TileVisu>().TileData.TileSettings.IdCode);
				GameObject.Destroy(temp.gameObject);
			}
		}

		public bool IsInHand(Vector2 position)
		{
			return RectTransformUtility.RectangleContainsScreenPoint(_handZone, position, Camera.main);
		}

		private int GetInsertionIndex(Vector3 droppedPosition)
		{
			for (int i = 0; i < TileInHand.Count; i++)
			{
				if (droppedPosition.x < TileInHand[i].transform.position.x)
					return i;
			}

			return TileInHand.Count; // add a la fin sinon
		}


		public void GiveTileToHand(GameObject tile)
		{
			if (TileInHand.Contains(tile))
			{
				UnityEngine.Debug.LogError($"[{nameof(ZoneHolderResource)}] Tile already in hand");
				return;
			}

			int insertIndex = GetInsertionIndex(tile.transform.position);
			tile.transform.parent = Camera.main.transform;
			TileInHand.Insert(insertIndex, tile);
			tile.GetComponent<TileVisu>().SetTileLayerGrid(LayerTile.InHand);
			tile.GetComponent<TileVisu>().SetWrongRotationFeedbackActive(false);

			UpdatePlacementInHand();
		}

		public void RemoveTileFromHand(GameObject tile)
		{
			if (!TileInHand.Contains(tile))
			{
				UnityEngine.Debug.LogError($"[{nameof(ZoneHolderResource)}] Tile not in hand");
				return;
			}

			TileInHand.Remove(tile);
			tile.transform.parent = null;
			tile.transform.DORotate(new(0, 0, 0), 0.2f, RotateMode.Fast);
			tile.transform.DOScale(Vector3.one, 0.2f);
			tile.GetComponent<TileVisu>().SetTileLayerGrid(LayerTile.InBetween);

			UpdatePlacementInHand();
		}

		public void UpdatePlacementInHand(bool force = false)
		{
			float canvasWidth = _canvas.GetComponent<RectTransform>().rect.width;
			float margin = (canvasWidth - Mathf.Lerp(_targetWidth3, _targetWidth10, Mathf.Clamp01((TileInHandCount-1)/10f))) / 2f;

			_visualHandArea.anchorMin = new Vector2(0, 0);
			_visualHandArea.anchorMax = new Vector2(1, 0);

			_visualHandArea.anchoredPosition = new Vector2(0, _visualHandArea.anchoredPosition.y);

			_visualHandArea.offsetMin = new Vector2(margin, _visualHandArea.offsetMin.y);
			_visualHandArea.offsetMax = new Vector2(-margin, _visualHandArea.offsetMax.y);

			Vector3[] worldCorners = new Vector3[4];
			_handZone.GetWorldCorners(worldCorners);

			Vector3 pos1 = (worldCorners[0] + worldCorners[1]) / 2;
			Vector3 pos2 = (worldCorners[2] + worldCorners[3]) / 2;

			int count = TileInHand.Count;

			for (int i = 0; i < count; i++)
			{
				//float t = (float)i / (float)count;
				// Faut faire count -1 : si 4 cartes, on avait 0, .25 .50 .75 et jamais 1, donc la on a 0 .33 .66 1
				// On evite juste la division par 0 s'il n'y a qu'une carte
				float t = count > 1 ? (float)i / (count - 1) : 0.5f;
				Vector3 pos = Vector3.Lerp(pos1, pos2, t);

				if (!force)
				{
					TileInHand[i].transform.DOMove(pos + (Camera.main.transform.forward * 2), 0.2f);
					TileInHand[i].transform.DORotate(Camera.main.transform.eulerAngles, 0.2f, RotateMode.Fast);
					TileInHand[i].transform.DOScale(Vector3.one * _tileSize, 0.2f);
				}
				else
				{
					TileInHand[i].transform.position = pos + (Camera.main.transform.forward * 2);
					TileInHand[i].transform.eulerAngles = Camera.main.transform.eulerAngles;
					TileInHand[i].transform.localScale = Vector3.one * _tileSize;
				}
			}
		}

		public void UpdateTileInHandSize()
		{
			_tileSize = _sizeInHand * (Camera.main.orthographicSize / _defaultCamSize);
		}

		public void HideMyHand(bool isHidden)
		{
			IsHidden = isHidden;

			foreach (GameObject t in TileInHand)
			{
				if (t != null) t.SetActive(!isHidden);
			}

			if (isHidden)
				_hudResource.CloseHud();
			else
				_hudResource.OpenHud();
		}

		public void EndDestroyTile()
		{
			for (int i = TileInHand.Count - 1; i >= 0; i--)
			{
				GameObject.Destroy(TileInHand[i]);
			}
		}
	}
}