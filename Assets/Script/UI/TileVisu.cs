using CardGame.Card;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame.UI
{
    public class TileVisu : MonoBehaviour
    {
        [SerializeField]
        private string _layerGrid;
        [SerializeField]
        private string _layerHand;

        [Space(10)]

        [SerializeField]
        private MeshRenderer _visuValidity;
        [SerializeField]
        private MeshRenderer _visuNorth;
        [SerializeField]
        private MeshRenderer _visuSouth;
        [SerializeField]
        private MeshRenderer _visuEast;
        [SerializeField]
        private MeshRenderer _visuWest;
        [SerializeField]
        private MeshRenderer _visuCenter;

        [Space(10)]

		//Enviro
		[SerializeField] private Material _neutral;
        [SerializeField] private Material _grass;
        [SerializeField] private Material _water;
        [SerializeField] private Material _fields;
		[SerializeField] private Material _terrain;


		[SerializeField] private Material _grey; // default
        [SerializeField] private Material _black; // Not valid
        [SerializeField] private Material _red; // valid
        [SerializeField] private Material _purple; // flag

        [Space(10)]

        [SerializeField] private TMPro.TextMeshPro _ownerTextMeshPro;

		[SerializeField] private List<GameObject> _meshesPresetList = new();
		private GameObject _currentVisualMesh;

		public TileData TileData { get; set; }

        public Vector2 PositionOnGrid { get; private set; }

        public bool IsLinked = true; // Sert pour la preview des placements possibles

        private void Start()
        {
            UpdateTile(TileData);
            _ownerTextMeshPro.text = "";
        }

		private void OnDestroy()
		{
			if (TileData != null) TileData.OnTileRotated -= RotateTileVisual;
		}

		public void UpdateTile(TileData data)
        {
            TileData = data;
            UpdateVisu();
		}

        public void ChangeParent(Transform parent)
        {
            gameObject.transform.SetParent(parent);
        }

        private void UpdateVisu()
        {

            List<ZoneData> zones = new();

            if (TileData == null)
            {
                _visuNorth.enabled = false;
                _visuSouth.enabled = false;
                _visuEast.enabled = false;
                _visuWest.enabled = false;
                _visuCenter.enabled = false;

                return;
            }

            _ownerTextMeshPro.text = TileData.OwnerPlayerIndex >= 0 ? $"P{TileData.OwnerPlayerIndex}" : "";

			_visuValidity.material = TileData.HasFlag ? _purple : _grey;

			SetTileMesh(TileData.TileSettings.tilePreset); // Y a une sécu dedans pour eviter de tout reconfig
        }

        public void ChangeValidityVisual(bool isValid)
        {
            _visuValidity.material = isValid ? _red : _black;
        }

        public void ResetValidityVisual()
        {
            _visuValidity.material = _grey;
        }

        //public void SetNewOwner()
        //{
        //    Debug.Log($"Second show, Played tile by player {GameManager.Instance.PlayerIndex}");
        //    Debug.Log($"{GameManager.Instance.GetInfo()}");
        //}

        private Material GetMaterialForType(ENVIRONEMENT_TYPE type)
        {
            switch (type)
            {
                case ENVIRONEMENT_TYPE.Neutral:
                    return _neutral;
                case ENVIRONEMENT_TYPE.Grass:
                    return _grass;
                case ENVIRONEMENT_TYPE.Fields:
                    return _fields;
                case ENVIRONEMENT_TYPE.Water:
                    return _water;
				case ENVIRONEMENT_TYPE.Terrain:
					return _terrain;
			}

            return null;
        }

        public void SetTileLayerGrid(bool isOnGrid)
        {
            if (isOnGrid) gameObject.layer = LayerMask.NameToLayer(_layerGrid);
            else gameObject.layer = LayerMask.NameToLayer(_layerHand);
        }

        public void SetTilePosOnGrid(Vector2 pos) => PositionOnGrid = pos;

		private void RotateTileVisual()
		{
			if (!_currentVisualMesh) return;

			Transform transform = _currentVisualMesh.transform;

			float angle = (TileData.TileRotationCount % 4) * -90f;
			transform.localRotation = Quaternion.Euler(0f, 0f, angle);
		}

		public void SetTileMesh(TilePreset preset)
		{
            if (_currentVisualMesh != null)
                return;

			TileData.OnTileRotated += RotateTileVisual; // Pas besoin de securiser, on peut arriver ici qu'une fois grace au return

			GameObject prefab = _meshesPresetList[(int)preset]; // Recup le bon prefab

			_currentVisualMesh = Instantiate(prefab, transform); // Spawn en enfant
			var renderers = _currentVisualMesh.GetComponentsInChildren<MeshRenderer>(); // On recup les renderer, pour set materials

			ZoneData[] zones = TileData.Zones;

			switch (preset) // Ici je me base sur les 5 presets de tiles : ils ont tous un nbr de mesh different, je dois donc adapter au cas par cas
			{
				case TilePreset.FourDifferentClosed: // 4 tiles dif
					for (int i = 0; i < 4 && i < renderers.Length; i++)
					    renderers[i].material = GetMaterialForType(zones[i].environment); // Classic
					break;

				case TilePreset.ThreeSame: 
					renderers[0].material = GetMaterialForType(zones[0].environment); // North
					renderers[1].material = GetMaterialForType(zones[1].environment); // East + South + West
					break;

				case TilePreset.DiagonalOpenFull:
					renderers[0].material = GetMaterialForType(zones[0].environment); // North + Est
					renderers[1].material = GetMaterialForType(zones[2].environment); // South + West
					break;

				case TilePreset.DiagonalOpenHalf:
					renderers[0].material = GetMaterialForType(zones[0].environment); // North
					renderers[1].material = GetMaterialForType(zones[2].environment); // East
					renderers[2].material = GetMaterialForType(zones[3].environment); // South + West
					break;

				case TilePreset.Path:
					renderers[0].material = GetMaterialForType(zones[0].environment); // North + South
					renderers[1].material = GetMaterialForType(zones[1].environment); // East
					renderers[2].material = GetMaterialForType(zones[3].environment); // West
					break;
			}
		}

	}
}