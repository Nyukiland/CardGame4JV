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

			return true;
		}

		public override int GetHashCode()
		{
			int hash = HashCode.Combine(Position, TileSettingsId, TileRotationCount, HasFlag, PlayerOwner);
			
			return hash;
		}
	}
}