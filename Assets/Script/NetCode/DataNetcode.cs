using Unity.Netcode;

namespace CardGame.Net
{
	[System.Serializable]
	public class DataNetcode : INetworkSerializable
	{
		public DataNetcode() { }
		public DataNetcode(string text)
		{
			Text = text;
		}

		private string _text;

		public string Text { get => _text; private set => _text = value; }

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref _text);
		}
	}
}