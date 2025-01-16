using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace VRroom.Game.Networking {
    [PublicAPI]
    public class NetworkManager : MonoBehaviour {
        private static readonly Dictionary<MessageType, Action<short, NetMessage>> MsgReceivedOfType = new();
        private static readonly Dictionary<short, Action<MessageType, NetMessage>> MsgReceivedFromObject = new();

        public static void SubscribeToType(MessageType type, Action<short, NetMessage> callback) {
            if (!MsgReceivedOfType.TryAdd(type, callback)) {
                MsgReceivedOfType[type] += callback;
            }
        }

        public static void UnsubscribeFromType(MessageType type, Action<short, NetMessage> callback) {
            if (!MsgReceivedOfType.ContainsKey(type)) return;
            MsgReceivedOfType[type] -= callback;
            if (MsgReceivedOfType[type] == null) {
                MsgReceivedOfType.Remove(type);
            }
        }

        public static void SubscribeToObject(short player, Action<MessageType, NetMessage> callback) {
            if (!MsgReceivedFromObject.TryAdd(player, callback)) {
                MsgReceivedFromObject[player] += callback;
            }
        }

        public static void UnsubscribeFromObject(short player, Action<MessageType, NetMessage> callback) {
            if (!MsgReceivedFromObject.ContainsKey(player)) return;
            MsgReceivedFromObject[player] -= callback;
            if (MsgReceivedFromObject[player] == null) {
                MsgReceivedFromObject.Remove(player);
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
                    MessageType type = (MessageType)msg.ReadShort();
                    short obj = msg.ReadShort();
                
                    try {
                        if (MsgReceivedOfType.TryGetValue(type, out Action<short, NetMessage> callback1)) callback1.Invoke(obj, msg);
                    } catch (Exception e) {
                        Debug.LogException(e);
                    }
                
                    try {
                        if (MsgReceivedFromObject.TryGetValue(obj, out Action<MessageType, NetMessage> callback2)) callback2.Invoke(type, msg);
                    } catch (Exception e) {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }

    public enum MessageType : short {
        /// Outbound
        JoinRequest = 0,
        /// Outbound
        Disconnect = 1,
        /// Outbound
        ClientState = 2,
        /// Outbound & Inbound
        VoiceData = 3,
        /// Outbound & Inbound
        PositionData = 4,
        /// Outbound & Inbound
        SkeletalData = 5,
        /// Inbound
        PlayerJoin = 200,
        /// Inbound
        PlayerLeave = 201,
        /// Inbound
        PlayerList = 202,
    }
}