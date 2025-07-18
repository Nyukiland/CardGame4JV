using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace CardGame.Net
{
	public class DistantNetController : NetControllerParent
	{
		[SerializeField] private string _sceneName;
		
		private string _lobbyId;
		private bool _publicSearchOn;
		private CancellationTokenSource _heartbeatCancellationToken;
		private CancellationTokenSource _publicSearchCancellationToken;

		protected override void Start()
		{
			base.Start();
			_networkUI.TogglePublicEvent += TogglePublicSearch;
			_networkUI.StartHostEvent += StartHost;
			_networkUI.JoinGameEvent += JoinGame;
			_networkUI.UnhostEvent += StopHosting;
			_networkUI.CopyCodeEvent += CopyJoinCode;
			_networkUI.QuitGameEvent += DisconnectFromGame;
			_networkUI.PlayGameEvent += LaunchGame;
			_networkUI.ToggleDistantEvent += ToggleDistant;

			//should be called by a button later
			Launch();
		}

		private void OnDestroy()
		{
			_networkUI.TogglePublicEvent -= TogglePublicSearch;
			_networkUI.StartHostEvent -= StartHost;
			_networkUI.JoinGameEvent -= JoinGame;
			_networkUI.UnhostEvent -= StopHosting;
			_networkUI.CopyCodeEvent -= CopyJoinCode;
			_networkUI.QuitGameEvent -= DisconnectFromGame;
			_networkUI.PlayGameEvent -= LaunchGame;
			_networkUI.ToggleDistantEvent -= ToggleDistant;
		}

		protected override void Launch()
		{
			base.Launch();
			InitUnityServices().Forget();
		}

		//call when go online 
		public async UniTask InitUnityServices()
		{
			await UnityServices.InitializeAsync();
			if (!AuthenticationService.Instance.IsSignedIn)
				await AuthenticationService.Instance.SignInAnonymouslyAsync();

			AnalyticsService.Instance.StartDataCollection();
		}

		protected override void StopHosting()
		{
			if (!_isDistant) return;
			
			base.StopHosting();

			StopHostingAsync().Forget();
		}

		private async UniTask StopHostingAsync()
		{
			if (!_isDistant) return;
			
			if (_heartbeatCancellationToken != null)
			{
				_heartbeatCancellationToken.Cancel();
				_heartbeatCancellationToken.Dispose();
				_heartbeatCancellationToken = null;
			}
			
			NetworkManager.Singleton.OnConnectionEvent -= CallOnConnect;
			NetworkManager.Singleton.OnClientDisconnectCallback -= CallOnDisconnect;

			if (!string.IsNullOrEmpty(_lobbyId))
			{
				try
				{
					await LobbyService.Instance.DeleteLobbyAsync(_lobbyId);
				}
				catch (LobbyServiceException e)
				{
					Debug.LogWarning($"[{nameof(DistantNetController)}] Failed to delete lobby: {e.Message}");
				}
				_lobbyId = null;
			}
		}

		protected override void StartHost()
		{
			if (!_isDistant) return;	
			
			base.StartHost();

			StartHostAsync().Forget();
		}

		private async UniTask StartHostAsync()
		{
			if (!_isDistant) return;
			
			if (string.IsNullOrEmpty(_networkUI.SessionName)) return;
			_networkUI.Password = _networkUI.Password;

			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_maxPlayer - 1);
			_networkUI.Code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

			if (_networkUI.IsPublicShown)
			{
				CreateLobbyOptions createOptions = new()
				{
					IsPrivate = false,
					Data = new Dictionary<string, DataObject>
					{
						{ "relayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, _networkUI.Code) },
						{ "password", new DataObject(DataObject.VisibilityOptions.Private, _networkUI.Password) }
					}
				};

				Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(_networkUI.SessionName, _maxPlayer, createOptions);
				_lobbyId = lobby.Id;

				_heartbeatCancellationToken = new CancellationTokenSource();
				_ = HeartbeatLobby(_heartbeatCancellationToken.Token);
			}

			NetworkManager.Singleton.OnConnectionEvent += CallOnConnect;
			NetworkManager.Singleton.OnClientDisconnectCallback += CallOnDisconnect;
			
			_transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
			NetworkManager.Singleton.ConnectionApprovalCallback = ApproveConnection;
			NetworkManager.Singleton.StartHost();

			await GetNetComForThisClientAsync();
			
			_networkUI.UpdateCodeAfterHost();
		}

		private void JoinGame()
		{
			if (!_isDistant) return;
			
			JoinGame(null);
		}
		
		public override void JoinGame(string code)
		{
			if (!_isDistant) return;
			
			_networkUI.ToggleInputBlock(true);
			
			base.JoinGame(code);
			JoinGameAsync(code).Forget();
		}

		private async UniTask JoinGameAsync(string code = null)
		{
			if (!_isDistant) return;
			
			if (string.IsNullOrEmpty(code)) code = _networkUI.Code;
			if (string.IsNullOrEmpty(code)) return;
			
			await _networkUI.OpenAfterClient();

			try
			{
				JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(code);
				_transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

				FastBufferWriter writer = new(32, Allocator.Temp);
				writer.WriteValueSafe(new FixedString32Bytes(_networkUI.Password));
				NetworkManager.Singleton.NetworkConfig.ConnectionData = writer.ToArray();

				SafeConnectAsync().Forget();
			}
			catch (RelayServiceException e)
			{
				Debug.LogError($"Failed to join relay: {e.Message}");
				await _networkUI.OpenBeforeClient();
				_networkUI.SpawnPopUp("No game has been found", 2f).Forget();
			}
			
			_networkUI.ToggleInputBlock(false);
		}

		private async UniTask HeartbeatLobby(CancellationToken token)
		{
			if (!_isDistant) return;
			
			try
			{
				while (!token.IsCancellationRequested && NetworkManager.Singleton.IsHost)
				{
					await LobbyService.Instance.SendHeartbeatPingAsync(_lobbyId);
					await UniTask.Delay(15000, cancellationToken: token);
				}
			}
			catch (OperationCanceledException)
			{
				//stopped
			}
		}

		protected override void TogglePublicSearch(bool isOn)
		{
			if (!_isDistant) return;
			
			if (_publicSearchOn && !isOn)
			{
				if (_publicSearchCancellationToken != null)
				{
					_publicSearchCancellationToken.Cancel();
					_publicSearchCancellationToken.Dispose();
					_publicSearchCancellationToken = null;
				}
			}
			else if (!_publicSearchOn && isOn)
			{
				_publicSearchCancellationToken = new CancellationTokenSource();
				RefreshPublicLobbies(_publicSearchCancellationToken.Token).Forget();
			}

			_publicSearchOn = isOn;
		}

		public async UniTask RefreshPublicLobbies(CancellationToken token)
		{
			if (!_isDistant) return;
			
			try
			{
				while (!token.IsCancellationRequested)
				{
					QueryResponse result = await LobbyService.Instance.QueryLobbiesAsync();

					List<PublicSessionVisu> sessions = _networkUI.PublicHostsContainer.GetComponentsInChildren<PublicSessionVisu>().ToList();
					foreach (PublicSessionVisu s in sessions)
						Destroy(s.gameObject);

					foreach (Lobby lobby in result.Results)
					{
						lobby.Data.TryGetValue("relayJoinCode", out DataObject dataObject);
						PublicSessionVisu visu = Instantiate(_displayPublicPrefab, _networkUI.PublicHostsContainer.transform)
							.GetComponent<PublicSessionVisu>();
						visu.SetUpVisu(lobby.Name, dataObject.Value, this);
					}

					await UniTask.Delay(2000, cancellationToken: token);
				}
			}
			catch (OperationCanceledException)
			{
				//exit on cancel
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error refreshing lobbies: {ex.Message}");
			}
		}

		protected override void DisconnectFromGame()
		{
			if (!_isDistant) return;
			
			base.DisconnectFromGame();
		}
		
		protected override void LaunchGame()
		{
			if (!_isDistant) return;
			
			_networkUI.CloseMenu();
			
			if (_publicSearchCancellationToken != null)
			{
				_publicSearchCancellationToken.Cancel();
				_publicSearchCancellationToken.Dispose();
				_publicSearchCancellationToken = null;
			}
			
			if (_heartbeatCancellationToken != null)
			{
				_heartbeatCancellationToken.Cancel();
				_heartbeatCancellationToken.Dispose();
				_heartbeatCancellationToken = null;
			}
			
			_netCommunication.LoadScene(_sceneName);
		}
		
		private void CallOnConnect(NetworkManager arg1, ConnectionEventData arg2)
		{
			_networkUI.UpdateAfterHost();
		}

		private void CallOnDisconnect(ulong obj)
		{
			_networkUI.UpdateAfterHost();
		}
	}
}