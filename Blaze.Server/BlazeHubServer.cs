﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blaze.Server
{
    class BlazeHubServer
    {
        private static Socket _socket;

        private static long _clientID;

        public static Dictionary<long, Client> Clients;

        static BlazeHubServer()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientID = 0;

            Clients = new Dictionary<long, Client>();
        }

        public static void Start()
        {
            Log.Info("Starting BlazeHubServer");

            _socket.Bind(new IPEndPoint(IPAddress.Any, 42127));
            _socket.Listen(1);

            _socket.BeginAccept(AcceptCallback, _socket);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            var socket = (Socket)ar.AsyncState;

            try
            {
                var clientSocket = (Socket)socket.EndAccept(ar);

                Log.Info($"Client connected from {clientSocket.RemoteEndPoint.ToString()}");

                var client = new Client();

                client.ID = Interlocked.Increment(ref _clientID);
                client.Socket = clientSocket;
                client.Stream = new NetworkStream(clientSocket);
                client.EndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;

                Clients.Add(client.ID, client);

                client.Stream.BeginRead(client.ReceiveBuffer, 0, client.ReceiveBuffer.Length, ReadCallback, client.ID);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            socket.BeginAccept(AcceptCallback, socket);
        }



        private static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                var clientID = (long)ar.AsyncState;
                var client = Clients[clientID];

                int length = client.Stream.EndRead(ar);

                if (length == 0)
                {
                    CloseSocket(ar);
                    return;
                }

                client.HandleRequest(client.ReceiveBuffer, length);

                client.Stream.BeginRead(client.ReceiveBuffer, 0, client.ReceiveBuffer.Length, ReadCallback, clientID);
            }
            catch
            {
                CloseSocket(ar);
            }
        }

        private static void CloseSocket(IAsyncResult ar)
        {
            try
            {
                var clientID = (long)ar.AsyncState;
                var client = Clients[clientID];

                lock (Clients)
                {
                    if (Clients.ContainsKey(client.ID))
                    {
                        Clients.Remove(client.ID);
                    }
                }

                try
                {
                    client.Socket.Shutdown(SocketShutdown.Both);
                }
                catch { }

                client.Socket.Close();

                Log.Info($"Client {clientID} disconnected");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
