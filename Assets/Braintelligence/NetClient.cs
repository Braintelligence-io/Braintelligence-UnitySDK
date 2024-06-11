using System;
using System.Collections;
using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Braintelligence
{
    public static class NetClient
    {
        private static EventBasedNetListener _listener;
        private static NetManager _client;
        private static NetPeer _server;
        private static NetDataWriter _writer;

        private static string _gameKey;

        private static Action _onConnected;

        private static bool _connected = false;

        private static bool _connecting = false;

        private enum MessageType : byte
        {
            Text,
            Binary
        }

        public static void Connect(string ip, int port, string gameKey, Action onConnect)
        {
            _connecting = true;

            _onConnected = onConnect;
            _gameKey = gameKey;

            _listener = new EventBasedNetListener();
            _listener.PeerConnectedEvent += OnPeerConnected;
            _listener.NetworkReceiveEvent += OnReceiveEvent;
            _listener.NetworkReceiveUnconnectedEvent += OnReceiveUnconnected;

            _client = new NetManager(_listener)
            {
                UnconnectedMessagesEnabled = true,
                BroadcastReceiveEnabled = true,
            };
            _client.Start();
            _writer = new NetDataWriter();

            if (string.IsNullOrWhiteSpace(ip) == false)
            {
                _server = _client.Connect(ip, port, _gameKey);
                return;
            }

            RetrieveServerEndPoint(port);
        }

        private static IEnumerator RetrieveServerEndPoint(int port)
        {
            _writer.Put(_gameKey);
            if (_client.SendBroadcast(_writer, port) == false)
            {
                _connecting = false;
                Log.Error($"Could not send broadcast message (Port:{port})");
                _writer.Reset();
                yield break;
            }

            Log.Info("Retrieving Server Endpoint...");

            _writer.Reset();

            // Wait for a couple of seconds for the response
            yield return new WaitForSeconds(7.0f);

            // Check if connected, if not handle the timeout
            if (!_connected)
            {
                _connecting = false;
            }
        }

        private static void OnReceiveUnconnected(IPEndPoint remote, NetPacketReader reader, UnconnectedMessageType type)
        {
            string msg = reader.GetString();
            if (msg != _gameKey) return;
            _writer.Put(_gameKey);
            Log.Info($"Connecting to {remote} with key {_gameKey}");
            _server = _client.Connect(remote, _gameKey);
            _writer.Reset();
        }

        internal static bool Update()
        {
            if(_connecting || _connected)
                _client?.PollEvents();
            return _connected;
        }

        private static void OnPeerConnected(NetPeer server)
        {
            Log.Info($"Connected to server: {server.EndPoint}");
            _onConnected?.Invoke();
            _connected = true;
        }

        private static void OnReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod delivery)
        {
        }

        public static void Send(string text)
        {
            if (CanSend() == false) return;
            Log.Info($"Sending Message: {text}");

            _writer.Put((byte)MessageType.Text);
            _writer.Put(text);
            Send(DeliveryMethod.Sequenced);
        }

        public static void Send(byte[] bytes)
        {
            if (CanSend() == false) return;
            Log.Info($"Sending {bytes.Length} bytes");
            _writer.Put((byte)MessageType.Binary);
            _writer.Put(bytes);
            Send(DeliveryMethod.ReliableUnordered);
        }

        private static bool CanSend()
        {
            if (_client == null)
            {
                Log.Error("NetClient is not initialized.");
                return false;
            }

            if (_server == null)
            {
                Log.Error("Unable connect to the server.");
                return false;
            }

            if (_server.ConnectionState != ConnectionState.Connected)
            {
                Log.Error($"NetClient is not connected. Connection state:{_server.ConnectionState}");
                return false;
            }

            return true;
        }

        private static void Send(DeliveryMethod deliveryMethod)
        {
            try
            {
                _server.Send(_writer, 0, deliveryMethod);
            }
            catch (TooBigPacketException e)
            {
                Log.Error($"Too Big Packet Exception \n{e.StackTrace}");
                _writer.Reset();
                throw;
            }

            _writer.Reset();
        }

        public static void Close(string message = null)
        {
            _connected = false;
            _connecting = false;
            _client?.Stop();
            if (string.IsNullOrWhiteSpace(message) == false)
            {
                _writer?.Put(message);
                _server?.Disconnect(_writer);
                _writer?.Reset();
            }
            else
            {
                _server?.Disconnect();
            }
        }
    }
}