using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace DAC 
{
	public class EtherDreamSearch
	{
		public static int RECEIVE_PORT { get; private set; } = 7654;

		public static void Find(int limit, float timeout, System.Action<EtherDreamFindArgs> callback) 
		{
			List<string> ipList = new List<string>();
			EtherDreamFindArgs args = new EtherDreamFindArgs();

			System.Timers.Timer timer = new System.Timers.Timer();
			UDPServer server = new UDPServer();
			server.OnReceiveBytes += (bytes, endpoint) => {
				EtherDreamDeviceInfo info = EtherDreamDeviceInfo.Create(bytes, endpoint);
				if (ipList.Contains(info.ip)) return;
				
				ipList.Add(info.ip);
				args.infoList.Add(info);
				
				if (args.infoList.Count >= limit)
				{
					timer.Enabled = false;
					timer.Dispose();
					server.Dispose();
					callback(args);
				}
			};
			server.Open(RECEIVE_PORT);

			timer.Interval = timeout * 1000;
			timer.Elapsed += (source, tArgs) => {
				timer.Enabled = false;
				timer.Dispose();
				server.Dispose();
				callback(args);
			};
			timer.Enabled = true;
		}
	}
}