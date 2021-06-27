using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DAC 
{
	public class EtherDream 
	{
		EtherDreamConnection connection;

		public int scanRate {
			get => connection.scanRate;
			set => connection.scanRate = value;
		}

		public EtherDream()
		{
			connection = new EtherDreamConnection();
		}

		public static void Find(System.Action<EtherDreamFindArgs> callback) 
		{
			EtherDreamSearch.Find(99, 2.0f, callback);
		}

		public static void FindFirst(System.Action<EtherDreamFindArgs> callback) 
		{
			EtherDreamSearch.Find(1, 4.0f, callback);
		}

		public void Connect(string ip, int port, System.Action<EtherDreamConnection> callback) 
		{
			connection.Connect(ip, port, (success) => {
				callback(success ? connection : null);
			});
		}

		public void Disconnect() 
		{
			if (connection != null)
			{
				connection.Close();
			}
		}

		public void Start(System.Action<int, List<DACPoint>> renderFrame, int rate=10000)
		{
			int g_phase = 0;
			void frameProvider(System.Action<List<DACPoint>> callback)
			{
				g_phase += 1;
				var framedata = new List<DACPoint>();
				renderFrame(g_phase, framedata);
				callback(framedata);
			}
			connection.StreamFrames(rate, frameProvider);
		}

		public void EndPoint(List<DACPoint> framedata, float x, float y)
		{
			for(int i=0; i<5; i++)
			{
				DACPoint pt = new DACPoint();
				pt.x = (short)Mathf.RoundToInt(x);
				pt.y = (short)Mathf.RoundToInt(y);
				pt.r = 0;
				pt.g = 0;
				pt.b = 0;
				pt.control = 0;
				pt.i = 0;
				pt.u1 = 0;
				pt.u2 = 0;
				framedata.Add(pt);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="framedata">List&lt;DACPoint&gt;</param>
		/// <param name="x0">short : -32768 to 32767</param>
		/// <param name="y0">short : -32768 to 32767</param>
		/// <param name="x1">short : -32768 to 32767</param>
		/// <param name="y1">short : -32768 to 32767</param>
		/// <param name="r">ushort : 0 to 65535</param>
		/// <param name="g">ushort : 0 to 65535</param>
		/// <param name="b">ushort : 0 to 65535</param>
		public void DrawLine(List<DACPoint> framedata, float x0, float y0, float x1, float y1, ushort r, ushort g, ushort b) 
		{
			short dx = (short)Mathf.Abs(x1 - x0);
			short dy = (short)Mathf.Abs(y1 - y0);
			int d = Mathf.RoundToInt(4 + (Mathf.Sqrt(dx*dx + dy*dy) / 400));
			int lineframes = d;

			EndPoint(framedata, x0, y0);

			for(int i=0; i<lineframes; i++)
			{
				DACPoint pt = new DACPoint();
				pt.x = (short)Mathf.RoundToInt(x0 + (x1 - x0) * ((float)i / ((float)lineframes - 1f)));
				pt.y = (short)Mathf.RoundToInt(y0 + (y1 - y0) * ((float)i / ((float)lineframes - 1f)));
				pt.r = r;
				pt.g = g;
				pt.b = b;
				pt.control = 0;
				pt.i = 0;
				pt.u1 = 0;
				pt.u2 = 0;
				framedata.Add(pt);
			}

			EndPoint(framedata, x1, y1);
		}

		public void DrawPath(List<DACPoint> framedata, List<Vector3> path, ushort r, ushort g, ushort b) 
		{
			float x0 = path[0].x;
			float y0 = path[0].y;
			float x1 = path[path.Count - 1].x;
			float y1 = path[path.Count - 1].y;
			
			EndPoint(framedata, x0, y0);

			int pathCount = path.Count;
			for(int i=0; i<pathCount; i++)
			{
				float x2 = path[i].x;
				float y2 = path[i].y;

				DACPoint pt = new DACPoint();
				pt.x = (short)Mathf.RoundToInt(x2);
				pt.y = (short)Mathf.RoundToInt(y2);
				pt.r = r;
				pt.g = g;
				pt.b = b;
				pt.control = 0;
				pt.i = 0;
				pt.u1 = 0;
				pt.u2 = 0;
				framedata.Add(pt);
			}

			EndPoint(framedata, x1, y1);
		}


		public void DrawSpline(List<DACPoint> framedata, List<Vector3> path, ushort r, ushort g, ushort b) 
		{
			float x0 = path[0].x;
			float y0 = path[0].y;
			float x1 = path[path.Count - 1].x;
			float y1 = path[path.Count - 1].y;

			EndPoint(framedata, x0, y0);

			int pathCount = path.Count - 1;
			for(int i=0; i<pathCount; i++)
			{
				int i1 = i - 1;
				int i2 = i;
				int i3 = i + 1;
				int i4 = i + 2;
				if (i1 < 0) i1 = 0;
				if (i4 >= pathCount) i4 = pathCount;

				Vector2 v1 = path[i1];
				Vector2 v2 = path[i2];
				Vector2 v3 = path[i3];
				Vector2 v4 = path[i4];

				for(int j=0; j<9; j++)
				{
					Vector2 vec = CatmullRom(v1, v2, v3, v4, (float)j/8);
					DACPoint pt = new DACPoint();
					pt.x = (short)Mathf.RoundToInt(vec.x);
					pt.y = (short)Mathf.RoundToInt(vec.y);
					pt.r = r;
					pt.g = g;
					pt.b = b;
					pt.control = 0;
					pt.i = 0;
					pt.u1 = 0;
					pt.u2 = 0;
					framedata.Add(pt);
				}
			}

			EndPoint(framedata, x1, y1);
		}

		// public void DrawSVG(List<DACPoint> framedata, List<SVGPath> svgPathList, ushort r, ushort g, ushort b) 
		// {
		// 	for(int i=0; i<svgPathList.Count; i++)
		// 	{
		// 		DrawPath(framedata, new List<Vector3>(svgPathList[i].points), r, g, b);
		// 	}
		// }


		private static Vector3 CatmullRom(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
		{
			return new Vector3(
				CatmullRom(a.x, b.x, c.x, d.x, t),
				CatmullRom(a.y, b.y, c.y, d.y, t),
				CatmullRom(a.z, b.z, c.z, d.z, t)
			);
		}

		private static Vector2 CatmullRom(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
		{
			return new Vector2(
				CatmullRom(a.x, b.x, c.x, d.x, t),
				CatmullRom(a.y, b.y, c.y, d.y, t)
			);
		}

		private static float CatmullRom(float a, float b, float c, float d, float t)
		{
			float v0 = (c - a) * 0.5f;
			float v1 = (d - b) * 0.5f;
			float t2 = t * t;
			float t3 = t2 * t;
			return (2*b-2*c+v0+v1) * t3+(-3*b+3*c-2*v0-v1) * t2+v0*t+b;
		}
	}
}
