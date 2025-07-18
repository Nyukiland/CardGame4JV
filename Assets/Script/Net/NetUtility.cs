using System.Collections.Generic;
using CardGame.Card;
using System;

namespace CardGame.Net
{
	public static class NetUtility
	{
		private const string Digits = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
		private static int Length => Digits.Length;

		//byte shift
		//je comprend bof � chaque fois mais trust the process
		public static string GetJoinCode(string ip, ushort port)
		{
			string[] parts = ip.Split('.');
			if (parts.Length != 4) throw new Exception("Invalid IP");

			uint ipNum = (uint)(
				(int.Parse(parts[0]) << 24) |
				(int.Parse(parts[1]) << 16) |
				(int.Parse(parts[2]) << 8) |
				int.Parse(parts[3])
			);

			return Encode(((ulong)ipNum << 16) | port);
		}

		public static void DecodeJoinCode(string code, out string ip, out ushort port)
		{
			ulong data = Decode(code);

			uint ipNum = (uint)(data >> 16);
			port = (ushort)(data & 0xFFFF);

			ip = string.Join(".",
				(ipNum >> 24) & 0xFF,
				(ipNum >> 16) & 0xFF,
				(ipNum >> 8) & 0xFF,
				ipNum & 0xFF
			);
		}

		//use N symbol (letter + number)
		//transform the number into a code

		private static string Encode(ulong value)
		{
			string result = "";

			if (value == 0)
				return Digits[0].ToString();

			while (value > 0)
			{
				result = Digits[(int)(value % (ulong)Length)] + result;
				value /= (ulong)Length;
			}

			return result;
		}

		private static ulong Decode(string input)
		{
			ulong result = 0;

			foreach (char c in input.ToUpperInvariant())
			{
				int index = Digits.IndexOf(c);
				if (index == -1)
					throw new Exception($"Invalid character in code: {c}");

				result = result * (ulong)Length + (ulong)index;
			}

			return result;
		}

		public static TileData FromDataToTile(DataToSend data, List<TileSettings> allTileSettings)
		{
			// Find TileSettings by IdCode
			TileSettings matchingSettings = allTileSettings.Find(ts => ts.IdCode == data.TileSettingsId);
			if (matchingSettings == null)
			{
				UnityEngine.Debug.LogError($"[{nameof(NetCommunication)}] No TileSettings found with IdCode {data.TileSettingsId}");
				return null;
			}

			// Create and initialize TileData
			TileData tile = new ();
			tile.InitTile(matchingSettings);
			tile.HasFlag = data.HasFlag;
			tile.OwnerPlayerIndex = data.PlayerOwner;

			// Rotate to match received rotation count
			for (int i = 0; i < data.TileRotationCount; i++)
			{
				tile.RotateTile();
			}

			return tile;
		}
	}
}