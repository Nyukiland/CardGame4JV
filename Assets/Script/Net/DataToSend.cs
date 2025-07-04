using CardGame.Card;
using Unity.Netcode;
using UnityEngine;
using System;

namespace CardGame.Net
{
	[Serializable]
	public class DataToSend : INetworkSerializable, IEquatable<DataToSend>
	{
		// === DONNÉES POSITION ===
		public Vector2Int Position { get; private set; }

		// === DONNÉES TILE ===
		public int TileSettingsId { get; private set; }
		public int TileRotationCount { get; private set; }
		public ZoneData[] Zones { get; private set; } = new ZoneData[4];
		public bool HasFlag { get; private set; }
		public int PlayerOwner { get; private set; }

		// === CONSTRUCTEUR PRINCIPAL (construit à partir de TileData classique) ===
		public DataToSend(TileData tileData, Vector2Int position)
		{
			Position = position;
			TileSettingsId = tileData.TileSettings.IdCode;
			TileRotationCount = tileData.TileRotationCount;
			HasFlag = tileData.HasFlag;
			PlayerOwner = tileData.OwnerPlayerIndex;

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
			int x = Position.x;
			int y = Position.y;
			serializer.SerializeValue(ref x);
			serializer.SerializeValue(ref y);
			if (serializer.IsReader)
				Position = new Vector2Int(x, y);

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
				ENVIRONEMENT_TYPE env = default;
				bool isOpen = false;

				serializer.SerializeValue(ref env);
				serializer.SerializeValue(ref isOpen);

				if (serializer.IsReader)
				{
					Zones[i].environment = env;
					Zones[i].isOpen = isOpen;
				}
			}

			int playerIndex = PlayerOwner;
			serializer.SerializeValue(ref playerIndex);
			if (serializer.IsReader)
				PlayerOwner = playerIndex;

			bool hasFlag = HasFlag;
			serializer.SerializeValue(ref hasFlag);
			if (serializer.IsReader)
				HasFlag = hasFlag;
		}

		public bool Equals(DataToSend other)
		{
			if (!Position.Equals(other.Position)) return false;
			if (TileSettingsId != other.TileSettingsId || TileRotationCount != other.TileRotationCount)
				return false;

			if (HasFlag != other.HasFlag) return false;
			if (PlayerOwner != other.PlayerOwner) return false;

			for (int i = 0; i < 4; i++)
				if (!Zones[i].Equals(other.Zones[i]))
					return false;

			return true;
		}

		public override int GetHashCode()
		{
			int hash = HashCode.Combine(Position, TileSettingsId, TileRotationCount, HasFlag, PlayerOwner);
			foreach (var zone in Zones)
				hash = HashCode.Combine(hash, zone.environment, zone.isOpen);
			return hash;
		}
	}
}