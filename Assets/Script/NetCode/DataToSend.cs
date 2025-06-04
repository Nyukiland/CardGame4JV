using Unity.Netcode;
using UnityEngine;
using System;
using CardGame.Card;

namespace CardGame.Net
{
	[Serializable]
	public class DataToSend : INetworkSerializable, IEquatable<DataToSend>
	{
		// === DONNÉES POSITION ===
		public Vector2 Position { get; private set; }

		// === DONNÉES TILE ===
		public int TileSettingsId { get; private set; }
		public int TileRotationCount { get; private set; }
		public ZoneData[] Zones { get; private set; } = new ZoneData[4];

		// === CONSTRUCTEUR PRINCIPAL (construit à partir de TileData classique) ===
		public DataToSend(Vector2 position, TileData tileData)
		{
			Position = position;
			TileSettingsId = tileData.TileSettings.IdCode;
			TileRotationCount = tileData.TileRotationCount;

			for (int i = 0; i < 4; i++)
			{
				Zones[i] = tileData.Zones[i];
			}
		}

		// === CONSTRUCTEUR PAR DÉFAUT POUR LE RÉSEAU ===
		public DataToSend() { }

		// === SÉRIALISATION POUR UNITY NETCODE ===
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			float x = Position.x;
			float y = Position.y;
			serializer.SerializeValue(ref x);
			serializer.SerializeValue(ref y);
			if (serializer.IsReader)
				Position = new Vector2(x, y);

			int id = TileSettingsId;
			serializer.SerializeValue(ref id);
			if (serializer.IsReader)
				TileSettingsId = id;

			int rot = TileRotationCount;
			serializer.SerializeValue(ref rot);
			if (serializer.IsReader)
				TileRotationCount = rot;

			if (serializer.IsReader)
				Zones = new ZoneData[4];

			for (int i = 0; i < 4; i++)
			{
				var env = Zones[i].environment;
				var isOpen = Zones[i].isOpen;

				serializer.SerializeValue(ref env);
				serializer.SerializeValue(ref isOpen);

				if (serializer.IsReader)
				{
					Zones[i].environment = env;
					Zones[i].isOpen = isOpen;
				}
			}
		}

		public bool Equals(DataToSend other)
		{
			if (!Position.Equals(other.Position)) return false;
			if (TileSettingsId != other.TileSettingsId || TileRotationCount != other.TileRotationCount)
				return false;

			for (int i = 0; i < 4; i++)
				if (!Zones[i].Equals(other.Zones[i]))
					return false;

			return true;
		}

		public override int GetHashCode()
		{
			int hash = HashCode.Combine(Position, TileSettingsId, TileRotationCount);
			foreach (var zone in Zones)
				hash = HashCode.Combine(hash, zone.environment, zone.isOpen);
			return hash;
		}
	}
}