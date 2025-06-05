using System.Collections.Generic;
using Unity.Netcode;
using System;

namespace CardGame.Net
{
	[Serializable]
	public class DataToSendList : INetworkSerializable, IEquatable<DataToSendList>
	{
		public List<DataToSend> DataList { get; private set; } = new();

		public DataToSendList() { }

		public DataToSendList(List<DataToSend> dataList)
		{
			DataList = dataList;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			int count = DataList.Count;
			serializer.SerializeValue(ref count);

			if (serializer.IsReader)
			{
				DataList = new List<DataToSend>(count);
				for (int i = 0; i < count; i++)
				{
					var data = new DataToSend();
					data.NetworkSerialize(serializer);
					DataList.Add(data);
				}
			}
			else
			{
				for (int i = 0; i < count; i++)
				{
					DataList[i].NetworkSerialize(serializer);
				}
			}
		}

		public bool Equals(DataToSendList other)
		{
			if (DataList.Count != other.DataList.Count)
				return false;

			for (int i = 0; i < DataList.Count; i++)
				if (!DataList[i].Equals(other.DataList[i]))
					return false;

			return true;
		}

		public override int GetHashCode()
		{
			int hash = 17;
			foreach (var item in DataList)
				hash = hash * 31 + item.GetHashCode();
			return hash;
		}
	}
}