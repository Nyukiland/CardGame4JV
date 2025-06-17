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
	public class DistantNetController : NetControllerParent
	{
		private string _lobbyId;
		private CancellationTokenSource _heartbeatTokenSource;

		protected override void Start()
		{
			base.Start();

			//should be called by a button later
			Launch();
		}

		public override void Launch()
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

		public override void EndUseNet(bool shutdownNet = true)
		{
			base.EndUseNet();
			_heartbeatTokenSource?.Cancel();
		}

		public override void StartHost()
		{
			base.StartHost();

			StartHostAsynch().Forget();
		}

		private async UniTask StartHostAsynch()
		{
			if (string.IsNullOrEmpty(_gameName.text)) return;
			_joinPassword = _passwordField.text;

			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
			_joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

			CreateLobbyOptions createOptions = new()
			{
				IsPrivate = !_toggleHost.isOn,
				Data = new Dictionary<string, DataObject>
				{
					{ "relayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, _joinCode) },
					{ "password", new DataObject(DataObject.VisibilityOptions.Private, _passwordField.text) }
				}
			};

			Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(_gameName.text, _maxPlayer, createOptions);
			_lobbyId = lobby.Id;

			_transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
			NetworkManager.Singleton.ConnectionApprovalCallback = ApproveConnection;
			NetworkManager.Singleton.StartHost();

			await GetNetComForThisClientAsync();
			_heartbeatTokenSource = new CancellationTokenSource();
			_ = HeartbeatLobby(_heartbeatTokenSource.Token);

		}

		public override void JoinGame(string code = null)
		{
			base.JoinGame();
			JoinGameAsynch().Forget();
		}

		private async UniTask JoinGameAsynch(string code = null)
		{
			if (string.IsNullOrEmpty(code)) code = _connectCode.text;
			if (string.IsNullOrEmpty(code)) return;

			Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
			_joinCode = lobby.Data["relayJoinCode"].Value;

			JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(_joinCode);
			_transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

			FastBufferWriter writer = new(32, Allocator.Temp);
			writer.WriteValueSafe(new FixedString32Bytes(_passwordField.text));
			NetworkManager.Singleton.NetworkConfig.ConnectionData = writer.ToArray();

			SafeConnectAsync().Forget();
		}

		private async UniTask HeartbeatLobby(CancellationToken token)
		{
			try
			{
				while (!token.IsCancellationRequested && NetworkManager.Singleton.IsHost)
				{
					await LobbyService.Instance.SendHeartbeatPingAsync(_lobbyId);
					await UniTask.Delay(15000);
				}
			}
			catch (OperationCanceledException)
			{
				//stopped
			}
		}

		public async UniTask RefreshPublicLobbies()
		{
			QueryResponse result = await LobbyService.Instance.QueryLobbiesAsync();
			List<PublicSessionVisu> sessions = _publicSessionVerticalLayout.GetComponentsInChildren<PublicSessionVisu>().ToList();
			foreach (PublicSessionVisu s in sessions) Destroy(s.gameObject);

			foreach (Lobby lobby in result.Results)
			{
				PublicSessionVisu visu = Instantiate(_displayPublic, _publicSessionVerticalLayout.transform).GetComponent<PublicSessionVisu>();
				visu.SetUpVisu(lobby.Name, lobby.LobbyCode, this);
			}
		}
	}
}