using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace DAC 
{
	public class TCPClient
	{
		Socket _socket = null;
		MemoryStream _memoryStream;
		readonly object syncLock = new object();
		Encoding enc = Encoding.UTF8;
		
		public delegate void ReceiveEventHandler(object sender, byte[] data);
		public event ReceiveEventHandler OnReceiveData;
		
		public delegate void DisconnectedEventHandler(object sender, System.EventArgs e);
		public event DisconnectedEventHandler OnDisconnected;
		
		public delegate void ConnectedEventHandler(System.EventArgs e);
		public event ConnectedEventHandler OnConnected;
		
		public bool IsClosed
		{
			get { return (_socket == null); }
		}
		
		public virtual void Dispose()
		{
			Close();
		}
		
		public TCPClient()
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public TCPClient(Socket socket)
		{
			_socket = socket;
		}
		
		public void Close()
		{
			try
			{
				_socket.Shutdown(SocketShutdown.Both);
				_socket.Close();
				_socket = null;
			}
			catch(System.Exception e)
			{
				Debug.LogError(e);
			}

			try
			{
				if (_memoryStream != null)
				{
					_memoryStream.Close();
					_memoryStream = null;
				}
			}
			catch(System.Exception e)
			{
				Debug.LogError(e);
			}
			OnDisconnected(this, new System.EventArgs());
		}
		
		public void Connect(string host, int port)
		{
			IPEndPoint ipEnd = new IPEndPoint(Dns.GetHostAddresses(host)[0], port);
			_socket.BeginConnect(ipEnd, new System.AsyncCallback(ConnectCallback), _socket);
		}
		
		void ConnectCallback(System.IAsyncResult ar)
		{
			try
			{
				Socket client = (Socket)ar.AsyncState;
				client.EndConnect(ar);
				OnConnected(new System.EventArgs());
				StartReceive();
			}
			catch (System.Exception e)
			{
				Debug.LogError(e);
			}
		}

		public void StartReceive()
		{
			byte[] rcvBuff = new byte[1024];
			_memoryStream = new MemoryStream();
			_socket.BeginReceive(rcvBuff, 0, rcvBuff.Length, SocketFlags.None, new System.AsyncCallback(ReceiveDataCallback), rcvBuff);
		}
		
		void ReceiveDataCallback(System.IAsyncResult ar)
		{
			int len = -1;
			lock (syncLock)
			{
				if (IsClosed) return;
				len = _socket.EndReceive(ar);
			}
		
			if (len <= 0)
			{
				Close();
				return;
			}

			try
			{
				byte[] rcvBuff = (byte[])ar.AsyncState;
				_memoryStream.Write(rcvBuff, 0, len);
				if (_memoryStream.Length > 0)
				{
					byte[] bytes = _memoryStream.ToArray();
					_memoryStream.Close();
					_memoryStream = new MemoryStream();
					OnReceiveData(this, bytes);
				}
			}
			catch(System.Exception e)
			{
				Debug.LogError(e);
			}
		
			lock (syncLock)
			{
				if (!IsClosed)
				{
					byte[] rcvBuff = new byte[1024];
					_socket.BeginReceive(rcvBuff, 0, rcvBuff.Length, SocketFlags.None, new System.AsyncCallback(ReceiveDataCallback), rcvBuff);
				}
			}
		}
		
		public void Send(byte[] bytes)
		{
			if (!IsClosed)
			{
				lock (syncLock)
				{
					if (_socket != null)
					{
						_socket.Send(bytes);
					}
				}
			}
		}
	}
}