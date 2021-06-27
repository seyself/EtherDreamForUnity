using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace DAC
{
	public class UDPServer
	{
		public delegate void UDPReceiverHandler(byte[] bytes, IPEndPoint endpoint);
		public UDPReceiverHandler OnReceiveBytes;

		public int port = 5000;
		public int receiveTimeout = 24 * 60 * 60 * 1000;

		UdpClient _udpClient;
		Thread _thread;
		bool _isOpen;
		List<string> _receiveData = new List<string>();
		List<byte[]> _receiveBytes = new List<byte[]>();

		public void Open(int port) 
		{
			this.port = port;
			_udpClient = new UdpClient(port);

			_isOpen = true;
			_thread = new Thread( ReceiveThread );
			_thread.IsBackground = true;
			_thread.Start();
		}

		public void Dispose()
		{
			if (_thread != null)
			{
				_isOpen = false;
				_thread.Abort();
				_thread = null;
			}
			if (_udpClient != null)
			{
				_udpClient.Close();
				_udpClient = null;
			}
		}

		void ReceiveThread()
		{
			while( _isOpen && _udpClient != null )
			{
				try
				{
					IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
					byte[] data = _udpClient.Receive(ref remoteEndPoint);
					if (OnReceiveBytes != null) OnReceiveBytes(data, remoteEndPoint);
				}
				catch(System.Threading.ThreadAbortException e)
				{
					Debug.LogWarning(e);
					// Debug.Log("UDPReceiver / Threadを中断しました。");
				}
				catch(System.Exception e)
				{
					Debug.LogError(e);
				}
			}
		}
	}
}
