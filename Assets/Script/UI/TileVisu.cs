using CardGame.Card;
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

        [SerializeField]
        private Material _red;
        [SerializeField]
        private Material _green;
        [SerializeField]
        private Material _blue;
        [SerializeField]
        private Material _white;
        [SerializeField]
        private Material _grey; // default
        [SerializeField]
        private Material _black; // Not valid
        [SerializeField]
        private Material _yellow; // valid
        [SerializeField]
        private Material _purple; // flag

        [Space(10)]

        [SerializeField] private TMPro.TextMeshPro _ownerTextMeshPro;
        public TileData TileData { get; set; }

        public Vector2 PositionOnGrid { get; private set; }

        public bool IsLinked = true; // Sert pour la preview des placements possibles

        private void Start()
        {
            UpdateTile(TileData);
            _ownerTextMeshPro.text = "";
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

            _visuNorth.enabled = true;
            _visuSouth.enabled = true;
            _visuEast.enabled = true;
            _visuWest.enabled = true;
            _visuCenter.enabled = true;

            ZoneData[] zonesRotated = TileData.Zones;

            zones.Add(zonesRotated[0]); // North
            _visuNorth.material = GetMaterialForType(zonesRotated[0].environment);

            zones.Add(zonesRotated[1]); // East
            _visuEast.material = GetMaterialForType(zonesRotated[1].environment);

            zones.Add(zonesRotated[2]); // South
            _visuSouth.material = GetMaterialForType(zonesRotated[2].environment);

            zones.Add(zonesRotated[3]); // West
            _visuWest.material = GetMaterialForType(zonesRotated[3].environment);


            //temp
            _visuCenter.enabled = false;

            for (int i = 0; i < zones.Count - 1; i++)
            {
                if (!zones[i].isOpen)
                    continue;

                for (int j = i + 1; j < zones.Count; j++)
                {
                    if (!zones[j].isOpen)
                        continue;

                    if (zones[i].environment == zones[j].environment)
                    {
                        _visuCenter.enabled = true;
                        _visuCenter.material = GetMaterialForType(zones[i].environment);
                        return;
                    }
                }
            }
        }

        public void ChangeValidityVisual(bool isValid)
        {
            _visuValidity.material = isValid ? _yellow : _black;
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
                case ENVIRONEMENT_TYPE.Forest:
                    return _green;
                case ENVIRONEMENT_TYPE.Snow:
                    return _white;
                case ENVIRONEMENT_TYPE.Lava:
                    return _red;
                case ENVIRONEMENT_TYPE.River:
                    return _blue;
            }

            return null;
        }

        public void SetTileLayerGrid(bool isOnGrid)
        {
            if (isOnGrid) gameObject.layer = LayerMask.NameToLayer(_layerGrid);
            else gameObject.layer = LayerMask.NameToLayer(_layerHand);
        }

        public void SetTilePosOnGrid(Vector2 pos) => PositionOnGrid = pos;
    }
}