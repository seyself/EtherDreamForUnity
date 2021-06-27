using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace DAC 
{
	public class EtherDreamDeviceInfo
	{
		public string ip;
		public int port;
		public string name;
		public int hw_revision;
		public int sw_revision;

		public EtherDreamDeviceInfo()
		{
			
		}

		public override string ToString()
		{
			return $"{name} / {ip}:{port} ({hw_revision}) ({sw_revision})";
		}

		public static EtherDreamDeviceInfo Create(byte[] bytes, IPEndPoint endpoint)
		{
			EtherDreamDeviceInfo info = new EtherDreamDeviceInfo();
			info.ip = endpoint.Address.ToString();
			info.name = $"EtherDream @ {toHex(bytes[0])}:{toHex(bytes[1])}:{toHex(bytes[2])}:{toHex(bytes[3])}:{toHex(bytes[4])}:{toHex(bytes[5])}";
			info.port = 7765;
			info.hw_revision = bytes[6];
			info.sw_revision = bytes[7];
			return info;
		}

		static string toHex(int n)
		{
			return n.ToString("X2");
		}

	}
}