using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DAC;

public class EtherDreamDemo : MonoBehaviour
{
	EtherDream _etherDream;
	bool _connected;
	bool _started;

	// Laser ScanRate
	int _scanRate = 1000;
	// DAC Host
	string _host = "192.168.1.234";
	// DAC Port
	int _port = 7765;

	void Start ()
	{
		EtherDream.Find( args => {
			args.infoList.ForEach( device => {
				Debug.Log(device);
			});
		} );

		_etherDream = new EtherDream();
		_etherDream.Connect(_host, _port, connection => {
			if (connection != null)
			{
				_connected = true;
			}
			else
			{
				Debug.Log("Disconnect");
			}
		});
	}


	void Update ()
	{
		if (_connected && !_started)
		{
			_started = true;
			_etherDream.Start(RenderFrame, _scanRate);
		}
	}

	void RenderFrame(int phase, List<DACPoint> framedata)
	{
		ushort R = 0x6000;
		ushort G = 0x0000;
		ushort B = 0x0000;

		float v = 10000; // short range : -32768 to 32767
		float s1 = 0.97f;
		float c = 0;
		List<Vector3> points = new List<Vector3>();
		points.Add( new Vector3(c-v, c+v, 0) );

		points.Add( new Vector3(c-v*s1, c+v, 0) );
		points.Add( new Vector3(c+v*s1, c+v, 0) );

		points.Add( new Vector3(c+v, c+v, 0) );
		
		points.Add( new Vector3(c+v, c+v*s1, 0) );
		points.Add( new Vector3(c+v, c-v*s1, 0) );
		
		points.Add( new Vector3(c+v, c-v, 0) );
		
		points.Add( new Vector3(c+v*s1, c-v, 0) );
		points.Add( new Vector3(c-v*s1, c-v, 0) );
		
		points.Add( new Vector3(c-v, c-v, 0) );
		
		points.Add( new Vector3(c-v, c-v*s1, 0) );
		points.Add( new Vector3(c-v, c+v*s1, 0) );
		
		points.Add( new Vector3(c-v, c+v, 0) );
		
		_etherDream.DrawPath(framedata, points, R, G, B);
	}

	void OnDestroy()
	{
		if (_etherDream != null)
		{
			_etherDream.Disconnect();
			_etherDream = null;
		}
	}
}
