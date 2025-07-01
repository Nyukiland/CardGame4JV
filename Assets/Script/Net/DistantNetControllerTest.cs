using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Core;
using Unity.Collections;
using System.Threading;
using Unity.Netcode;
using System.Linq;
using UnityEngine;
using System;

namespace CardGame.Net
{
	public class DistantNetControllerTest : NetControllerParentTest
	{
		private string _lobbyId;
		private bool _publicSearchOn;
		private CancellationTokenSource _heartbeatCancellationToken;
		private CancellationTokenSource _publicSearchCancellationToken;

		protected override void Start()
		{
			base.Start();

			//should be called by a button later
			Launch();
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

			if (!string.IsNullOrEmpty(_lobbyId))
			{
				try
				{
					await LobbyService.Instance.DeleteLobbyAsync(_lobbyId);
				}
				catch (LobbyServiceException e)
				{
					Debug.LogWarning($"[{nameof(DistantNetControllerTest)}] Failed to delete lobby: {e.Message}");
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
			
			if (string.IsNullOrEmpty(_gameName.text)) return;
			_networkUI.Password = _passwordField.text;

			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_maxPlayer - 1);
			_networkUI.Code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

			if (_toggleHost.isOn)
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

				Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(_gameName.text, _maxPlayer, createOptions);
				_lobbyId = lobby.Id;

				_heartbeatCancellationToken = new CancellationTokenSource();
				_ = HeartbeatLobby(_heartbeatCancellationToken.Token);
			}

			_transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
			NetworkManager.Singleton.ConnectionApprovalCallback = ApproveConnection;
			NetworkManager.Singleton.StartHost();

			await GetNetComForThisClientAsync();
		}

		public override void JoinGame(string code = null)
		{
			if (!_isDistant) return;
			
			base.JoinGame(code);
			JoinGameAsync(code).Forget();
		}

		private async UniTask JoinGameAsync(string code = null)
		{
			if (!_isDistant) return;
			
			if (string.IsNullOrEmpty(code)) code = _connectCode.text;
			if (string.IsNullOrEmpty(code)) return;
			
			_networkUI.ToggleInputBlock(true); 

			try
			{
				JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(code);
				_transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

				FastBufferWriter writer = new(32, Allocator.Temp);
				writer.WriteValueSafe(new FixedString32Bytes(_passwordField.text));
				NetworkManager.Singleton.NetworkConfig.ConnectionData = writer.ToArray();

				SafeConnectAsync().Forget();
			}
			catch (RelayServiceException e)
			{
				Debug.LogError($"Failed to join relay: {e.Message}");
			}
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

					List<PublicSessionVisu> sessions = _publicSessionVerticalLayout.GetComponentsInChildren<PublicSessionVisu>().ToList();
					foreach (PublicSessionVisu s in sessions)
						Destroy(s.gameObject);

					foreach (Lobby lobby in result.Results)
					{
						lobby.Data.TryGetValue("relayJoinCode", out DataObject dataObject);
						PublicSessionVisu visu = UnityEngine.Object.Instantiate(_displayPublicPrefab, _publicSessionVerticalLayout.transform)
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

	}
}