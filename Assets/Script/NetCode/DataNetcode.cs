using Unity.Netcode;
using System;

namespace CardGame.Net
{
	[System.Serializable]
	public class DataNetcode : INetworkSerializable, IEquatable<DataNetcode>
	{
		public DataNetcode() { }
		public DataNetcode(string text)
		{
			Text = text;
		}

		private string _text = "";

		public string Text { get => _text; private set => _text = value; }

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref _text);
		}

		public bool Equals(DataNetcode other)
		{
			return Text == other.Text;
		}

		public override bool Equals(object obj)
		{
			return obj is DataNetcode other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Text != null ? Text.GetHashCode() : 0;
		}
	}
}