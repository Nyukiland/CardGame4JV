using CardGame.Card;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class TileVisualSorter : MonoBehaviour
{
	[SerializeField]
	private MeshSpawnPoint _visuNorth;
	[SerializeField]
	private MeshSpawnPoint _visuSouth;
	[SerializeField]
	private MeshSpawnPoint _visuEast;
	[SerializeField]
	private MeshSpawnPoint _visuWest;

	[Space(10)]

	[SerializeField]
	List<GameObject> _fish = new();

	[SerializeField]
	List<GameObject> _flower = new();

	[SerializeField]
	List<GameObject> _yellowThing = new();

	[SerializeField]
	List<GameObject> _grass = new();

	public MeshRenderer VisuNorth => _visuNorth.Visu;
	public MeshRenderer VisuSouth => _visuSouth.Visu;
	public MeshRenderer VisuEast => _visuEast.Visu;
	public MeshRenderer VisuWest => _visuWest.Visu;

	private List<MeshSpawnPoint> _meshSpawnPoints = new();

	private void Start()
	{
		_meshSpawnPoints.Add(_visuNorth);
		_meshSpawnPoints.Add(_visuSouth);
		_meshSpawnPoints.Add(_visuEast);
		_meshSpawnPoints.Add(_visuWest);
	}

	public void AddElementToArea(ENVIRONEMENT_TYPE environment, MeshRenderer toSpawnOn)
	{
		for (int i = 0; i < _meshSpawnPoints.Count; i++)
		{
			if (_meshSpawnPoints[i].Visu == toSpawnOn && !_meshSpawnPoints[i].AlreadySpawned)
			{
				_meshSpawnPoints[i].AlreadySpawned = true;

				Transform t = _meshSpawnPoints[i].SpawnPoint;

				GameObject toSpawn = null;

				if (0.85f < Random.Range(0f, 1f))
					return;

				switch (environment)
				{
					case ENVIRONEMENT_TYPE.Terrain:
						toSpawn = _flower[Random.Range(0, _flower.Count)];
						break;
					case ENVIRONEMENT_TYPE.Grass:
						toSpawn = _grass[Random.Range(0, _grass.Count)];
						break;
					case ENVIRONEMENT_TYPE.Fields:
						toSpawn = _yellowThing[Random.Range(0, _yellowThing.Count)];
						break;
					case ENVIRONEMENT_TYPE.Water:
						toSpawn = _fish[Random.Range(0, _fish.Count)];
						break;
				}

				toSpawn = Instantiate(toSpawn, t);
				toSpawn.transform.SetParent(t);
				toSpawn.transform.DOLocalMoveY(0.35f, 0.3f);

				return;
			}
		}
	}

	[System.Serializable]
	public class MeshSpawnPoint
	{
		public MeshRenderer Visu;
		public Transform SpawnPoint;
		[Disable]
		public bool AlreadySpawned;
	}
}