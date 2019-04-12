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
	InputRobot input;

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;
		cameras = GetComponent<StereoCamera>();
		input = GetComponent<InputRobot>();
		startClient();
    }

    void startClient()
    {
        SocketThread = new System.Threading.Thread(networkCode);
        SocketThread.IsBackground = true;
        SocketThread.Start();
    }

	public void startClient(UnityEngine.UI.Text text)
	{
		adressIp = text.text;
		startClient();
	}

	private byte[] getIntegerBytes(int value)
	{
		return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
	}

	private int BytestoInteger(byte[] value, int offset)
	{
		if(value.Length < sizeof(Int32))
		{
			throw new Exception("Not enough bytes to convert to integer.");
		}
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

	
	void parseCommand(byte[] command)
	{
		if(command.Length == 0)
		{
			Debug.LogError("Size 0.");
		}
		
		string cmd = Encoding.ASCII.GetString(command, 0, 1);
		Debug.Log("Command read = " + cmd);

		byte[] data = command.Skip(1).ToArray();

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
		//actualise control
		else if (cmd.StartsWith("c"))
		{
			inControlsCommand(data);
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
		int value = BytestoInteger(data, 0);
		Debug.Log("New value refresh :" + value);
		cameras.refreshTime = value;
	}

	void inControlsCommand(byte[] data)
	{
		string[] sep = { ";" };
		string[] message = Encoding.ASCII.GetString(data).Split(sep, StringSplitOptions.RemoveEmptyEntries);
		Debug.Log("Controls: \"" + message[0] +"\" \""+ message[1] + "\" \"" + message[2] +"\"");
		input.Forward = Single.Parse(message[0], System.Globalization.CultureInfo.InvariantCulture);
		input.Aside = Single.Parse(message[1], System.Globalization.CultureInfo.InvariantCulture);
		input.Rotation = Single.Parse(message[2], System.Globalization.CultureInfo.InvariantCulture);
	}

	int receiveSizeCommand()
	{
		int len = 0;
		byte[] buf = new byte[sizeof(Int32)];
		while (len < sizeof(Int32))
		{
			if (sender.CanRead)
			{
				len += sender.Read(buf, len, sizeof(Int32) - len);
			}
        }
		//Buff have size package
		int size_command = BytestoInteger(buf, 0);
		Debug.Log("Size package : " + size_command + " bytes.");
		return size_command;
	}

	void receivePackage(out byte[] command)
	{
		int size_command = receiveSizeCommand();
		command = new byte[size_command];
		int len = 0;
		//Loop full package
		Debug.Log("Transfer...");
		while (size_command > len )
		{
			if (sender.CanRead)
			{
				len += sender.Read(command, 0, command.Length - len);
				if (len > command.Length)
				{
					Debug.LogError("Command too long");
				}
				Debug.Log(len + "/" + size_command + " bytes.");
				Debug.Log(len / (double)size_command * 100 + "% done.");
			}
		}
		Debug.Log("End transfer.");
	}

    void networkCode()
    {
		client = new TcpClient(AddressFamily.InterNetwork);

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
                sendRefresh((int)cameras.refreshTime);
                sendCameras();

                while (keepReading)
                {
                    if (sender.DataAvailable)
                    {
						Debug.Log("New command.");
						byte[] command;
                        receivePackage(out command);
                        parseCommand(command);
                    }
					Thread.Sleep((int)cameras.refreshTime);
					sendCameras();

				}
            }
			Debug.Log("End connection.");

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

	void sendCommand(byte[] data)
	{
		byte[] size = getIntegerBytes(data.Length);
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
		Debug.Log("Send :" + images[0].Length);

		byte[] command = Combine(
			Encoding.ASCII.GetBytes("s"),
			Encoding.ASCII.GetBytes(images[0].Length.ToString() + "\0"),
			images[0],
			Encoding.ASCII.GetBytes(images[1].Length.ToString() + "\0"),
			images[1]);

		sendCommand(command);
		Debug.Log("Img send.");
		Debug.Log("Left size "+ images[0].Length + ".");
		Debug.Log("Right size "+ images[1].Length + ".");
	}

	void sendRefresh(int refresh_time)
	{
		byte[] command = Encoding.ASCII.GetBytes("t" + refresh_time);
		sendCommand(command);
		Debug.Log("Refresh send " + refresh_time+".");
	}

	void stopClient()
    {
		Debug.Log("Disconnected.");
		keepReading = false;
        //stop thread
        if (SocketThread != null)
        {
            if (client != null && client.Connected)
            {
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
}
