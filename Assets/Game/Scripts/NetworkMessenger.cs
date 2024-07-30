using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class NetworkMessenger {
	private static readonly UdpClient Client = new();
	private static readonly Dictionary<IPEndPoint, Dictionary<int, Message>> AwaitingMessages = new();
	private static readonly Dictionary<IPEndPoint, Dictionary<byte, int>> ReceivedMessages = new();
	public static readonly Queue<Message> Messages = new();

	static NetworkMessenger() {
		_ = new Timer(HandleRetries, null, 1000, 1000);
		_ = ReceiveMessages();
	}

	public static void SendMessage(Message message) {
		byte[] bytes = message.ToBytes();
		Client.Send(bytes, bytes.Length, message.EndPoint);
		
		if (message.SendFlag < SendFlag.Reliable) return;
		if (!AwaitingMessages.ContainsKey(message.EndPoint)) {
			AwaitingMessages[message.EndPoint] = new Dictionary<int, Message>();
		}
		AwaitingMessages[message.EndPoint][message.SendTime] = message;
	}

	private static async Task ReceiveMessages() {
        while (true) {
	        try {
		        UdpReceiveResult result = await Client.ReceiveAsync();
		        Message message = Message.FromBytes(result.Buffer, result.RemoteEndPoint);

		        if (message.SendFlag == SendFlag.Ok) {
			        if (AwaitingMessages.TryGetValue(message.EndPoint, out Dictionary<int, Message> awaitingMessage)) {
				        awaitingMessage.Remove(message.SendTime);
			        }
			        continue;
		        }
		        
		        if (message.SendFlag >= SendFlag.Reliable) {
			        byte[] ok = new byte[9];
			        ok[0] = (byte)SendFlag.Ok;
			        Array.Copy(result.Buffer, 1, ok, 1, 8);
			        Client.Send(ok, ok.Length, result.RemoteEndPoint);
		        }
		        
		        if (message.SendFlag is SendFlag.UnreliableSequenced or SendFlag.ReliableSequenced) {
			        if (!ReceivedMessages.TryGetValue(message.EndPoint, out Dictionary<byte, int> channelSequences)) {
				        channelSequences = new Dictionary<byte, int>();
				        ReceivedMessages[message.EndPoint] = channelSequences;
			        }

			        if (channelSequences.TryGetValue(message.Channel, out int lastSequence)) {
				        if (message.SendTime <= lastSequence) {
					        continue; // Drop out of order or duplicate message
				        }
			        }

			        channelSequences[message.Channel] = message.SendTime;
		        }
		        
		        Messages.Enqueue(message);
	        }
	        catch (Exception e) {
		        Debug.LogError(e);
	        }
        }
    }

    private static void HandleRetries(object _) {
	    foreach (IPEndPoint endpoint in AwaitingMessages.Keys) {
		    Message[] messages = AwaitingMessages[endpoint].Values.ToArray();
		    foreach (Message m in messages) {
			    Message message = m;

			    if (message.Retries != 0) { // wait atleast 1 second before retrying
				    byte[] bytes = message.ToBytes();
				    Client.Send(bytes, bytes.Length, message.EndPoint);
			    } else if (message.Retries > 5) {
				    AwaitingMessages[endpoint].Remove(message.SendTime);
			    }

			    message.Retries++;
		    }

		    if (AwaitingMessages[endpoint].Count == 0) {
			    AwaitingMessages.Remove(endpoint);
		    }
	    }
	    
	    int cutoff = (int)DateTime.UtcNow.AddSeconds(-30).Ticks;
	    foreach (KeyValuePair<IPEndPoint, Dictionary<byte, int>> kvp in ReceivedMessages) {
		    foreach (KeyValuePair<byte, int> k in kvp.Value.Where(x => x.Value < cutoff)) {
			    kvp.Value.Remove(k.Key);
		    }

		    if (kvp.Value.Count == 0) {
			    ReceivedMessages.Remove(kvp.Key);
		    }
	    }
    }
}

public struct Message {
	public IPEndPoint EndPoint;
	public SendFlag SendFlag;
	public byte[] Data;
	public int Retries;
	public int SendTime;
	public byte Channel;

	public Message(byte[] bytes, IPEndPoint endPoint, byte channel, SendFlag sendFlag = SendFlag.Unreliable) {
		Data = new byte[bytes.Length + 17];
		EndPoint = endPoint;
		SendFlag = sendFlag;
		Data = bytes;
		Retries = 0;
		Channel = channel;
		SendTime = (int)DateTime.UtcNow.Ticks;
	}

	internal byte[] ToBytes() {
		byte[] bytes = new byte[Data.Length + 6];
		bytes[0] = (byte)SendFlag;
		bytes[1] = Channel;
		bytes[5] = (byte)SendTime;
		bytes[6] = (byte)(SendTime >> 8);
		bytes[7] = (byte)(SendTime >> 16);
		bytes[8] = (byte)(SendTime >> 24);
		Array.Copy(Data, 0, bytes, 9, Data.Length);
		return bytes;
	}

	internal static Message FromBytes(byte[] bytes, IPEndPoint endPoint) {
		SendFlag sendFlag = (SendFlag)bytes[0];
		byte channel = bytes[1];
		int sendTime = bytes[2] | (bytes[3] << 8) | (bytes[4] << 16) | (bytes[5] << 24);
		byte[] data = bytes[6..];

		return new Message {
			EndPoint = endPoint,
			SendFlag = sendFlag,
			Data = data,
			Retries = 0,
			Channel = channel,
			SendTime = sendTime
		};
	}
}
	
public enum SendFlag : byte {
	Unreliable,
	UnreliableSequenced,
	Reliable,
	ReliableSequenced,
	Ok,
}