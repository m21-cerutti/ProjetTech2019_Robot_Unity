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

	private byte[] getInteger(int value)
	{
		return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
	}

	private int toInteger(byte[] value, int offset)
	{
		return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(value, offset));
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

	
	int parseCommand(byte[] data)
	{
		if(data.Length == 0)
		{
			Debug.LogError("Size 0");
		}
		
		string cmd = Encoding.ASCII.GetString(data, 0, data.Length);
		Debug.Log("Command read = " + cmd);
		/*
		//send echo
		if (cmd.StartsWith("e"))
		{
			inEchoCommand(data);
		}
		//send recho
		else if (cmd.StartsWith("r"))
		{
			inRechoCommand(data);
		}
		//send camera
		else if (cmd.StartsWith("i"))
		{
			sendCameras();
		}
		//send time refresh
		else if (cmd.StartsWith("t"))
		{
			inRefreshCommand(data);
		}
		else
		{
			throw new Exception("Illegal instruction.");
		}
		*/
		return 0;
	}

    void networkCode()
    {

		TcpClient client = new TcpClient(AddressFamily.InterNetwork);

        try
        {
			Debug.Log("Try to connect to " + adressIp + ":" + port);
			Debug.Log("Connecting...");
			IPAddress[] adresses = Dns.GetHostAddresses(adressIp);
			foreach(IPAddress adress in adresses)
			{
				Debug.Log(adress.ToString());
			}
			client.Connect(adresses[0], port);

			Debug.Log("Connected.");

			int bufferSize = client.ReceiveBufferSize;
			//Debug.Log("Size Buffer : " + bufferSize + " bytes.");
			//Debug.Log("Time Out : " + client.ReceiveTimeout + " bytes.");


			using (sender = client.GetStream())
			{
				sendEcho("Ok echo");
				//sendEcho(Encoding.ASCII.GetBytes("Ok 2 echo"));
				//sendRefresh((int)cameras.refreshTime);
				//sendCameras();
				
				while (keepReading && client.Client.Connected)
				{
					// Data buffer for incoming data.
					byte[] buf = new byte[bufferSize];
					int len = 0;
					int size_command = 0;

					if (sender.CanRead)
					{
						int read = 0;
						while (len < sizeof(Int32))
						{
							Debug.Log("Waiting new command...");
							read = sender.Read(buf, len, sizeof(Int32) - len);
							len += read;
						}

						//Buff have size package
						size_command = toInteger(buf, 0);
						Debug.Log("Size package : " + size_command + " bytes.");
						if (size_command > buf.Length)
						{
							bufferSize = size_command + 1;
							Debug.Log("New size Buffer : " + bufferSize + " bytes.");
							byte[] newbuf = new byte[bufferSize];
							buf.CopyTo(newbuf, 0);
							buf = newbuf;
						}
						len = 0;

						//Loop full package
						Debug.Log("Transfer...");
						while (size_command > len)
						{
							Byte[] bytes = new byte[size_command];
							sender.Read(bytes, 0, size_command);
							string msg = Encoding.ASCII.GetString(bytes); //the message incoming
							Debug.Log(msg);
						}


							/*
							while (size_command > len)
							{
								read = sender.Read(buf, len, buf.Length - len);

								string cmd = Encoding.ASCII.GetString(buf, len, buf.Length);
								Debug.Log("Read = " + cmd);

								len += read;
								if (len >= buf.Length)
								{
									Debug.LogError("Buffer overflow");
								}
								Debug.Log(len +"/" + size_command + " bytes.");
								Debug.Log(len/(double)size_command*100+ "% done.");
							}

							//Buff have package
							Debug.Log("Package arrived.");

							parseCommand(buf);
							size_command = 0;
							len = 0;
							*/

						}
				}
			}
			Debug.Log("End connection...");

			// Release the socket.  
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

	void inEchoCommand(byte[] data)
	{
		string message = Encoding.ASCII.GetString(data);
		Debug.Log("Resend: " + message);
		sendCommand(Encoding.ASCII.GetBytes("r" + message));
	}

	private void inRechoCommand(byte[] data)
	{
		string message = Encoding.ASCII.GetString(data);
		Debug.Log("Return echo complete: " + message);
	}

	void inRefreshCommand(byte[] data)
	{
		int value = (int)BitConverter.ToInt32(data, 0);
		Debug.Log("New value refresh :" + value);
		cameras.refreshTime = value;
	}

	void sendCommand(byte[] data)
	{
		byte[] size = getInteger(data.Length);
		sender.Write(size, 0, size.Length);
		sender.Write(data, 0, data.Length);
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

	void sendEcho(string message)
	{
		byte[] command = Encoding.ASCII.GetBytes("e" + message);
		sendCommand(command);
		Debug.Log("Echo send.");
	}

	void sendEcho(byte[] message)
	{
		byte[] command = Combine(
			Encoding.ASCII.GetBytes("e"),
			message);
		sendCommand(command);
		Debug.Log("Echo send.");
	}

	void sendCameras()
	{
		byte[][] images = cameras.getCamerasImages();

		byte[] command = Combine(
			Encoding.ASCII.GetBytes("s"),
			getInteger(images[0].Length),
			images[0],
			getInteger(images[1].Length),
			images[1]);

		sendCommand(command);
		Debug.Log("Img send.");
	}

	void sendRefresh(int refresh_time)
	{
		byte[] command = Combine(
		Encoding.ASCII.GetBytes("t"),
		getInteger(refresh_time)); 
		sendCommand(command);
		Debug.Log("Refresh send.");
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
