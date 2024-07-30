using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    private static readonly ConcurrentDictionary<Type, MsgReceivedHandler> MsgReceivedEvents = new();
    public delegate void MsgReceivedHandler(Guid id, INetObject obj);
    
    public static void Subscribe(Type type, MsgReceivedHandler handler) {
        if (handler == null || type == null) return;
        MsgReceivedEvents.AddOrUpdate(type, _ => handler, (_, existingHandler) => existingHandler + handler);
    }

    public static void Unsubscribe(Type type, MsgReceivedHandler handler) {
        if (handler == null || type == null) return;
        MsgReceivedEvents.AddOrUpdate(type, _ => null, (_, existingHandler) => {
            existingHandler -= handler;
            if (existingHandler == null) MsgReceivedEvents.Remove(type, out var _);
            return existingHandler;
        });
    }
    
    private void Start() {
        StartCoroutine(HandleIncomingMessages());
    }
    
    private static IEnumerator HandleIncomingMessages() {
        while (true) {
            yield return new WaitForEndOfFrame();
            while (NetworkMessenger.Messages.Count > 0) {
                Message msg = NetworkMessenger.Messages.Dequeue();
                Guid id = new Guid(msg.Data[..16]);
                INetObject netObject = null;
                try {
                    netObject = MemoryPackSerializer.Deserialize<INetObject>(msg.Data[16..]);
                } catch (Exception e) {
                    Debug.LogError($"Error while deserializable type: {e}");
                }
                if (netObject == null) continue;
                MsgReceivedEvents.GetValueOrDefault(netObject.GetType())?.Invoke(id, netObject);
            }
        }
    }
}