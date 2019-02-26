using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Text;

public class Client : MonoBehaviour
{
    System.Threading.Thread SocketThread;
    volatile bool keepReading = true;

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;
        startClient();
    }

    void startClient()
    {
        SocketThread = new System.Threading.Thread(networkCode);
        SocketThread.IsBackground = true;
        SocketThread.Start();
    }

    private string getIPAddress()
    {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
            }
        }
        return localIP;
    }

    public string adressIp = "192.168.0.40";
    public int port = 5260;
    Socket sender;
    Socket handler;

    void networkCode()
    {
        string data;

        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        // host running the application.
        Debug.Log("Locals Ip " + getIPAddress().ToString());
        Debug.Log("Try to connect to " + adressIp + ":" + port);
        IPAddress[] ipArray = Dns.GetHostAddresses(adressIp);

        IPEndPoint remoteEndPoint = new IPEndPoint(ipArray[0], port);
        // Create a TCP/IP socket.
        sender = new Socket(ipArray[0].AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and 
        // listen for incoming connections.

        try
        {
            sender.Connect(remoteEndPoint);
            Debug.Log("Socket connected to " + sender.RemoteEndPoint.ToString());

            while (keepReading)
            {
                // Encode the data string into a byte array.  
                byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                // Send the data through the socket.  
                int bytesSent = sender.Send(msg);

                // Receive the response from the remote device. Block.  
                int bytesRec = sender.Receive(bytes);
                Debug.Log("Echoed test = " + Encoding.ASCII.GetString(bytes, 0, bytesRec));
            }

            // Release the socket.  
            sender.Shutdown(SocketShutdown.Both);
        }
        catch (ArgumentNullException ane)
        {
            Debug.LogError("ArgumentNullException : " + ane.ToString());
        }
        catch (SocketException se)
        {
            Debug.LogError("SocketException : " + se.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError("Unexpected exception : " + e.ToString());
        }

        if (sender != null && sender.Connected)
        {
            sender.Disconnect(false);
            Debug.Log("Disconnected!");
        }
        sender.Close();
    }


    void stopClient()
    {
        keepReading = false;

        //stop thread
        if (SocketThread != null)
        {
            SocketThread.Abort();
        }
    }

    void OnDisable()
    {
        stopClient();
    }
}
