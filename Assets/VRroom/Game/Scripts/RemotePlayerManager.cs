using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using VRroom.Base;
using VRroom.Game.Networking;
using Object = UnityEngine.Object;

namespace VRroom.Game {
	[PublicAPI]
	public static class RemotePlayerManager {
		public static readonly Dictionary<short, RemotePlayer> Players = new();
		public static float HideDistance = 15;
		private static bool _announceJoins = true;

		private static void Start() {
			GameStateManager.Subscribe("AvatarHideDistance", (_, v) => HideDistance = (float)v);
		
			NetworkManager.SubscribeToType(MessageType.PlayerJoin, OnPlayerJoin);
			NetworkManager.SubscribeToType(MessageType.PlayerLeave, OnPlayerLeave);
			NetworkManager.SubscribeToType(MessageType.PlayerList, OnJoinInstance);
		}

		private static void OnJoinInstance(short playerCount, NetMessage msg) {
			_announceJoins = false;
			for (int i = 0; i < playerCount; i++) {
				short networkId = msg.ReadShort();
				OnPlayerJoin(networkId, msg);
			}
			_announceJoins = true;
		}

		private static void OnPlayerJoin(short networkId, NetMessage msg) {
			Guid userId = Guid.Parse(msg.ReadString());
		
			GameObject playerObject = new();
			RemotePlayer player = playerObject.AddComponent<RemotePlayer>();
			player.networkId = networkId;
			player.PlayerId = userId;

			Players[networkId] = player;
		}

		private static void OnPlayerLeave(short networkId, NetMessage msg) {
			Object.Destroy(Players[networkId]);
		}
	}
}