using System;

using System.Net;
using System.Net.Sockets;
using System.Text;

using System.Threading;
using Newtonsoft.Json;

using ICities;
using UnityEngine;
using ColossalFramework.Plugins;

namespace NetworkInterface
{
    public class Server
    {
        UdpClient listener;
        Thread listenerThread;
        NetworkInterface.Network networkAPI;

        public void ListenerThreadFunc()
        {
            while (true)
            {
                byte[] data = new byte[1024];
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                try
                {
                    data = listener.Receive(ref sender);
                }
                catch (Exception e)
                {
                    continue;
                }

                string command = Encoding.ASCII.GetString(data, 0, data.Length);

                string message = "Got connection from: " + sender.ToString();
                Log(message);

                string response = "";
                try
                {
                    response = JsonConvert.SerializeObject(networkAPI.HandleRequest(command));
                }
                catch (Exception e)
                {
                    Log(e.Message);
                    response = JsonConvert.SerializeObject(e.Message);
                }
                
                data = Encoding.ASCII.GetBytes(response);
                listener.Send(data, data.Length, sender);
            }
        }

        public void Log(string message)
        {
            //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, message);
            //UnityEngine.Debug.Log(message);
            //Debug.Log(message);
            //Console.WriteLine(message);
        }

        public void Start()
        {
            try
            {
                networkAPI = new NetworkInterface.Network();

                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 11000);
                listener = new UdpClient(ipep);
                listener.Client.ReceiveTimeout = 50;
                listenerThread = new Thread(new ThreadStart(this.ListenerThreadFunc));
                listenerThread.Start();
                string message = "Server up";
                Log(message);
            }
            catch (Exception e)
            {
                string message = "Error starting server: " + e.Message;
                Log(message);
            }
        }

        public void Stop()
        {
            listenerThread.Abort();
            listener.Close();
        }

    }

} 
