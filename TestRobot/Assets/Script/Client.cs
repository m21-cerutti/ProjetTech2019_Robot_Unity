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

[RequireComponent(typeof(StereoCamera))]
public class Client : MonoBehaviour
{
    System.Threading.Thread SocketThread;
    volatile bool keepReading = true;

	StereoCamera cameras;

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;
		cameras = GetComponent<StereoCamera>();
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

    void echoCommand(string message)
    {
        Debug.Log("Resend: " + message);
        sender.Send(Encoding.ASCII.GetBytes("r_echo;" + message+ "<EOF>"));
    }

    private void rechoCommand(string data)
    {
        Debug.Log("Return echo complete: " + data);
    }

    void refreshCommand(string number)
    {
        int value;
        try
        {
            value = int.Parse(number);
            Debug.Log("New value refresh :" + value);
        }
        catch(Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    void sendLargeFile(string identifier, byte[] data)
    {
        IList<ArraySegment<byte>> cmd = new List<ArraySegment<byte>>();
        cmd.Add(new ArraySegment<byte>(Encoding.ASCII.GetBytes(identifier + ":"+data.Length+";")));
        cmd.Add(new ArraySegment<byte>(data));
        cmd.Add(new ArraySegment<byte>(Encoding.ASCII.GetBytes("<EOF>")));
        int bytesSentStart = sender.Send(cmd);
    }

    void sendCamerasCommand()
    {
        byte[][] images = cameras.getCamerasImages();
        int bytesSentStart = sender.Send(Encoding.ASCII.GetBytes("stereo;"));
        sendLargeFile("left", images[0]);
        sendLargeFile("right", images[1]);
    }

    private void receiveImageCommand(string cmd, string data, byte[] bytes, int nbread)
    {
        Debug.Log(cmd);
        Debug.Log(data);
    }

    int parseCommand(byte[] bytes, int nbread)
    {
        string buff = Encoding.ASCII.GetString(bytes, 0, nbread);

        int endCommand = 0;
        while((endCommand = buff.IndexOf("<EOF>")) > 0)
        {
            //Debug.Log("buff debug = " + buff);

            string cmd = buff.Substring(0, endCommand);
            Debug.Log("Command debug = " + cmd);

            buff = buff.Remove(0, endCommand + "<EOF>".Length);
            //Debug.Log("newbuff debug = " + buff);

            if (cmd.Equals(""))
                throw new Exception("Server shutdown.");

            int beginData = cmd.IndexOf(";")+1;
            string data = cmd.Substring(beginData);
            Debug.Log("Data debug = "+ data);

            if (cmd.StartsWith("echo"))
            {
                echoCommand(data);
            }
            else if (cmd.StartsWith("r_echo"))
            {
                rechoCommand(data);
            }
            else if (cmd.StartsWith("req_img"))
            {
                sendCamerasCommand();
            }
            else if (cmd.StartsWith("img"))
            {
                receiveImageCommand(cmd, data, bytes, nbread);
            }
            else if (cmd.StartsWith("refresh"))
            {
                refreshCommand(data);
            }
            else
            {
                throw new Exception("Illegal instruction.");
            }
        }
        return 0;
    }

    void networkCode()
    {
        // Data buffer for incoming data.
        byte[] bytes = new byte[1024];

        // host running the application.
        Debug.Log("Locals Ip " + getIPAddress().ToString());
        Debug.Log("Try to connect to " + adressIp + ":" + port);
        IPAddress[] ipArray = Dns.GetHostAddresses(adressIp);

        IPEndPoint remoteEndPoint = new IPEndPoint(ipArray[0], port);
        // Create a TCP/IP socket.
        sender = new Socket(ipArray[0].AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        try
        {
            sender.Connect(remoteEndPoint);
            Debug.Log("Socket connected to " + sender.RemoteEndPoint.ToString());

            //Send refresh req
            int bytesSentStart = sender.Send(Encoding.ASCII.GetBytes("start;" + cameras.refreshTime.ToString() + ";<EOF>"));

            while (keepReading)
            {
                // Receive the response from the remote device. Block.  
                int nbread = sender.Receive(bytes);

                //Parse command
                parseCommand(bytes, nbread);
                
                if (nbread == 0)
                {
                    keepReading = false;
                }
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
    }

    void stopClient()
    {
        keepReading = false;

        //stop thread
        if (SocketThread != null)
        {
            if (sender != null && sender.Connected)
            {
                sender.Disconnect(false);
                Debug.Log("Disconnected!");
                sender.Close();
            }
            SocketThread.Interrupt();
        }
    }

    void OnDisable()
    {
        stopClient();
    }

    void OnDestroy()
    {
        if (SocketThread != null)
        {
            SocketThread.Abort();
        }
    }
}
