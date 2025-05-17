using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Net;
using System;

namespace CardGame.Net
{
	public class LanSearchBeacon : MonoBehaviour
	{
		[SerializeField]
		private int _broadcastPort = 60120;
		[SerializeField]
		private float _broadcastInterval = 1.5f;

		private UdpClient _hostUdp;
		private IPEndPoint _hostEndPoint;
		private bool _isHosting;
		private CancellationTokenSource _hostTokenSource;

		private UdpClient _clientUdp;
		private IPEndPoint _clientEndPoint;
		private bool _isListening;

		private Dictionary<string, BeaconDataWithIP> _discoveredHosts = new();
		public Action<List<BeaconDataWithIP>> OnHostsUpdated;

		// --- Host Functions ---
		public void StartBroadcast(string gameName, int port, string joinCode)
		{
			if (_isHosting) return;

			_hostUdp = new UdpClient();
			_hostUdp.EnableBroadcast = true;
			_hostEndPoint = new IPEndPoint(IPAddress.Broadcast, _broadcastPort);
			_isHosting = true;

			_hostTokenSource = new CancellationTokenSource();

			BroadcastLoop(gameName, port, joinCode, _hostTokenSource.Token).Forget();
		}

		private async UniTask BroadcastLoop(string gameName, int port, string joinCode, CancellationToken token)
		{
			try
			{
				while (!token.IsCancellationRequested)
				{
					BeaconData data = new BeaconData
					{
						gameName = gameName,
						port = port,
						joinCode = joinCode
					};

					string json = JsonUtility.ToJson(data);
					byte[] bytes = Encoding.UTF8.GetBytes(json);

					_hostUdp.Send(bytes, bytes.Length, _hostEndPoint);

					await UniTask.Delay(TimeSpan.FromSeconds(_broadcastInterval), cancellationToken: token);
				}
			}
			catch (OperationCanceledException)
			{
				// Expected on stop
			}
			catch (Exception e)
			{
				Debug.LogError($"Broadcast error: {e.Message}");
			}
		}

		public void StopBroadcast()
		{
			if (!_isHosting) return;

			_isHosting = false;
			_hostTokenSource?.Cancel();
			_hostTokenSource?.Dispose();
			_hostUdp?.Close();
			_hostUdp = null;
		}

		// --- Client Functions ---
		public void StartListening()
		{
			if (_isListening) return;

			_discoveredHosts.Clear();
			_clientUdp = new UdpClient(_broadcastPort);
			_clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
			_isListening = true;
			_clientUdp.BeginReceive(OnReceive, null);
		}

		public void StopListening()
		{
			if (!_isListening) return;

			_isListening = false;
			_clientUdp?.Close();
			_clientUdp = null;
			_discoveredHosts.Clear();
			OnHostsUpdated?.Invoke(new List<BeaconDataWithIP>());
		}

		private void OnReceive(IAsyncResult result)
		{
			if (!_isListening || _clientUdp == null) return;

			byte[] data = _clientUdp.EndReceive(result, ref _clientEndPoint);
			string json = Encoding.UTF8.GetString(data);

			UniTask.Post(() =>
			{
				try
				{
					BeaconData beacon = JsonUtility.FromJson<BeaconData>(json);

					string key = beacon.joinCode;

					if (_discoveredHosts.TryGetValue(key, out var existing))
					{
						existing.gameName = beacon.gameName;
						existing.joinCode = beacon.joinCode;
					}
					else
					{
						_discoveredHosts.Add(key, new BeaconDataWithIP
						{
							ip = _clientEndPoint.Address.ToString(),
							port = beacon.port,
							gameName = beacon.gameName,
							joinCode = beacon.joinCode
						});
					}

					List<BeaconDataWithIP> beaconDatas = _discoveredHosts.Values.ToList();
					OnHostsUpdated?.Invoke(beaconDatas);
				}
				catch (Exception e)
				{
					UnityEngine.Debug.LogWarning("Invalid beacon: " + e.Message);
				}

				// Resume listening after handling
				if (_isListening && _clientUdp != null)
					_clientUdp.BeginReceive(OnReceive, null);
			});
		}


		private void OnDestroy()
		{
			StopBroadcast();
			StopListening();
		}

		[Serializable]
		public class BeaconData
		{
			public string gameName;
			public int port;
			public string joinCode;
		}

		// Class with IP included for easier UI display
		public class BeaconDataWithIP
		{
			public string ip;
			public int port;
			public string gameName;
			public string joinCode;
		}
	}
}