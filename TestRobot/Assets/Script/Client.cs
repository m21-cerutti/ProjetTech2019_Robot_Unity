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
using System.Linq;

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
	TcpClient client;
	NetworkStream sender;
	int size_command;

	void inEchoCommand(byte[] data)
    {
		string message = Encoding.ASCII.GetString(data);
		Debug.Log("Resend: " + message);
		sendCommand(Encoding.ASCII.GetBytes("r;" + message));
    }

    private void inRechoCommand(byte[] data)
    {
		string message = Encoding.ASCII.GetString(data);
		Debug.Log("Return echo complete: " + message);
    }

    void inRefreshCommand(byte[] data)
    {
		int value = (int)BitConverter.ToInt32(data,0);
        Debug.Log("New value refresh :" + value);
		cameras.refreshTime = value;
    }

	void sendCommand(byte[] data)
	{
		int size_message = sizeof(Int32) + data.Length;
		byte[] message = new byte[size_message];
		BitConverter.GetBytes((Int32)data.Length).CopyTo(message, 0);
		data.CopyTo(message, sizeof(Int32));
		sender.Write(message, 0, size_message);
	}

	private byte[] Combine(params byte[][] arrays)
	{
		byte[] rv = new byte[arrays.Sum(a => a.Length)];
		int offset = 0;
		foreach (byte[] array in arrays)
		{
			System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
			offset += array.Length;
		}
		return rv;
	}

	void sendCamerasCommand()
    {
        byte[][] images = cameras.getCamerasImages();

		byte[] command = Combine(
			Encoding.ASCII.GetBytes("s"),
			BitConverter.GetBytes((Int32)images[0].Length),
			images[0],
			BitConverter.GetBytes((Int32)images[1].Length),
			images[1]);

		sendCommand(command);
	}

	void sendRefreshCommand(int refresh_time)
	{
		byte[] command = Combine(
			Encoding.ASCII.GetBytes("t;"),
			BitConverter.GetBytes(refresh_time));

		sendCommand(command);
	}


	int parseCommand(byte[] data)
	{
		string cmd = Encoding.ASCII.GetString(data,0,2);
		Debug.Log("Command read = " + cmd);
		//send echo
		if (cmd.StartsWith("e;"))
		{
			inEchoCommand(data);
		}
		//send recho
		else if (cmd.StartsWith("r;"))
		{
			inRechoCommand(data);
		}
		//send camera
		else if (cmd.StartsWith("s;"))
		{
			sendCamerasCommand();
		}
		//send time refresh
		else if (cmd.StartsWith("t;"))
		{
			inRefreshCommand(data);
		}
		else
		{
			throw new Exception("Illegal instruction.");
		}

		return 0;
	}

    void networkCode()
    {

		TcpClient client = new TcpClient();

        try
        {
			Debug.Log("Try to connect to " + adressIp + ":" + port);
			Debug.Log("Connecting...");

			client.Connect(adressIp, port);

			Debug.Log("Connected.");

			int bufferSize = client.ReceiveBufferSize;
			Debug.Log("Size Buffer : " + bufferSize + " bytes.");
			Debug.Log("Time Out : " + client.ReceiveTimeout + " bytes.");


			using (sender = client.GetStream())
			{
				//Send refresh req start
				sendRefreshCommand((int)cameras.refreshTime);
				Debug.Log("Refresh send.");


				while (keepReading && client.Client.Connected)
				{
					// Data buffer for incoming data.
					byte[] buf = new byte[bufferSize];
					int len = 0;
					int size_command = 0;

					if (sender.CanRead)
					{
						int read;
						while (len < sizeof(Int32))
						{
							read = sender.Read(buf, len, buf.Length - len);
							len += read;
						}

						//Buff have size package
						size_command = BitConverter.ToInt32(buf, 0);
						Debug.Log("Size package : " + size_command + " bytes.");
						if (size_command > buf.Length)
						{
							bufferSize = size_command + 1;
							Debug.Log("New size Buffer : " + bufferSize + " bytes.");
							byte[] newbuf = new byte[bufferSize];
							buf.CopyTo(newbuf, 0);
							buf = newbuf;
						}

						Debug.Log("Transfer...");

						//Loop full package
						while ((sizeof(Int32) + size_command) > len)
						{
							read = sender.Read(buf, len, buf.Length - len);
							len += read;
							if (len >= buf.Length)
							{
								Debug.LogError("Buffer overflow");
							}
						}

						Debug.Log("Package arrived.");
						//Buff have package
						parseCommand(buf.Skip(sizeof(Int32)).ToArray());
						size_command = 0;
						len = 0;

					}
				}
			}
			Debug.Log("End connection...");

			// Release the socket.  
			client.Client.Shutdown(SocketShutdown.Both);
			client.Close();
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
            if (client != null && client.Connected)
            {
                Debug.Log("Disconnected!");
				client.Client.Shutdown(SocketShutdown.Both);
				client.Client.Close();
				client.Close();
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
