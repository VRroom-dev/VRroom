using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[PublicAPI]
public class NetworkManager : MonoBehaviour {
    private static readonly Dictionary<int, Action<Guid, NetMessage>> MsgReceivedOfType = new();
    private static readonly Dictionary<Guid, Action<int, NetMessage>> MsgReceivedFromPlayer = new();

    public static void SubscribeToType(int type, Action<Guid, NetMessage> callback) {
        if (!MsgReceivedOfType.TryAdd(type, callback)) {
            MsgReceivedOfType[type] += callback;
        }
    }

    public static void UnsubscribeFromType(int type, Action<Guid, NetMessage> callback) {
        if (!MsgReceivedOfType.ContainsKey(type)) return;
        MsgReceivedOfType[type] -= callback;
        if (MsgReceivedOfType[type] == null) {
            MsgReceivedOfType.Remove(type);
        }
    }

    public static void SubscribeToPlayer(Guid player, Action<int, NetMessage> callback) {
        if (!MsgReceivedFromPlayer.TryAdd(player, callback)) {
            MsgReceivedFromPlayer[player] += callback;
        }
    }

    public static void UnsubscribeFromPlayer(Guid player, Action<int, NetMessage> callback) {
        if (!MsgReceivedFromPlayer.ContainsKey(player)) return;
        MsgReceivedFromPlayer[player] -= callback;
        if (MsgReceivedFromPlayer[player] == null) {
            MsgReceivedFromPlayer.Remove(player);
        }
    }
    
    private void Start() {
        StartCoroutine(HandleIncomingMessages());
    }
    
    private static IEnumerator HandleIncomingMessages() {
        while (true) {
            yield return new WaitForEndOfFrame();
            while (NetworkMessenger.TryDequeue(out byte[] bytes)) {
                NetMessage msg = new NetMessage(bytes);
                Guid player = new Guid(msg.ReadBytes(16));
                int type = msg.ReadInt();
                
                if (MsgReceivedOfType.TryGetValue(type, out Action<Guid, NetMessage> callback1)) callback1.Invoke(player, msg);
                if (MsgReceivedFromPlayer.TryGetValue(player, out Action<int, NetMessage> callback2)) callback2.Invoke(type, msg);
            }
        }
    }
}