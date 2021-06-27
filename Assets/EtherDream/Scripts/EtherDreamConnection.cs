using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace DAC 
{
	public class EtherDreamConnection
	{
		static int STANDARD_RESPONSE_SIZE = 22;
		// static int DAC_CTRL_RATE_CHANGE = 0x8000;
		// static int CALLBACK_DELAY = 1;
		
		List<byte> _inputQueue = new List<byte>();
		List<InputHandler> _inputHandlerQueue = new List<InputHandler>();

		float _fullness = 0;
		float _playbackState = 0;
		bool _beginsent = false;
		bool _valid = true;

		int _sendCount = 0;
		byte[] _latestSendData = new byte[]{};


		public int sendCount { get => _sendCount; }
		public byte[] latestSendData { get => _latestSendData; }
		
		public float fullness { get => _fullness; }


		int _rate = 0;
		public int scanRate { 
			get => _rate;
			set => _rate = value;
		}

		System.Action<int, System.Action<List<DACPoint>>> _streamSource;

		TCPClient _client;

		public EtherDreamConnection()
		{
			
		}

		void _Send(byte[] sendcommand)
		{
			_sendCount += 1;
			_latestSendData = sendcommand;
			_client.Send(sendcommand);
			_PopInputQueue();
		}

		void _PopInputQueue()
		{
			if (_inputHandlerQueue.Count > 0) 
			{
				InputHandler handler = _inputHandlerQueue[0];
				if (_inputQueue.Count >= handler.size) 
				{
					byte[] response = new byte[handler.size];
					for(int i=0; i<handler.size; i++)
					{
						response[i] = _inputQueue[0];
						_inputQueue.RemoveAt(0);
					}
					_inputHandlerQueue.RemoveAt(0);
					handler.callback(response);
				}
			}
		}

		void WaitForResponse(int size, System.Action<byte[]> callback)
		{
			_inputHandlerQueue.Add(new InputHandler() {
				size = size,
				callback = callback,
			});
			this._PopInputQueue();
		}

		public void Connect(string ip, int port, System.Action<bool> callback) 
		{
			_client = new TCPClient();
			_client.OnConnected += (args) => {
				Debug.Log("OnConnected");
				WaitForResponse(STANDARD_RESPONSE_SIZE, data => {
					DACStandardResponse st = DACStandardResponse.Parse(data);
					HandleStandardResponse(st);
					callback(true);
				});
				_PopInputQueue();
			};

			_client.OnDisconnected += (sender, args) => {
				Debug.Log("SOCKET END: client disconnected");
				callback(false);
			};

			_client.OnReceiveData += (sender, data) => {
				for(var i=0; i<data.Length; i++)
				{
					_inputQueue.Add(data[i]);
				}
				for(var i=0; i<data.Length; i++)
				{
					_PopInputQueue();
				}
			};
			_client.Connect(ip, port);
		}

		void HandleStandardResponse(DACStandardResponse data)
		{
			_fullness = data.status.buffer_fullness;
			_playbackState = data.status.playback_state;
			_valid = data.response == (byte)'a';

			if (data.status.playback_flags == 2) 
			{
				Debug.LogError("Laser buffer underrun.");
				_beginsent = false;
			}
		}

		void SendPing(System.Action callback)
		{
			byte[] cmd = new byte[]{ (byte)'?' };
			_Send(cmd);
			WaitForResponse(STANDARD_RESPONSE_SIZE, data => {
				var st = DACStandardResponse.Parse(data);
				HandleStandardResponse(st);
				callback();
			});
		}

		void SendStop(System.Action callback)
		{
			byte[] cmd = new byte[]{ (byte)'s' };
			_Send(cmd);
			WaitForResponse(STANDARD_RESPONSE_SIZE, data => {
				var st = DACStandardResponse.Parse(data);
				HandleStandardResponse(st);
				callback();
			});
		}

		void SendEmergencyStop(System.Action callback)
		{
			byte[] cmd = new byte[]{ 0xff };
			_Send(cmd);
			WaitForResponse(STANDARD_RESPONSE_SIZE, data => {
				var st = DACStandardResponse.Parse(data);
				HandleStandardResponse(st);
				callback();
			});
		}

		void SendPoints(List<DACPoint> points, System.Action callback)
		{
			int batch = points.Count;
			byte[] u_batch = System.BitConverter.GetBytes((ushort)batch);
			byte[] cmd = new byte[1+2+batch*DACPoint.bufferSize];
			cmd[0] = (byte)'d';
			cmd[1] = u_batch[0];
			cmd[2] = u_batch[1];
			for(int i=0; i<batch; i++)
			{
				points[i].WriteStream(ref cmd, i * DACPoint.bufferSize + 3);
			}
			_Send(cmd);
			WaitForResponse(STANDARD_RESPONSE_SIZE, data => {
				var st = DACStandardResponse.Parse(data);
				HandleStandardResponse(st);
				if (_valid)
				{
					if (!_beginsent)
					{
						SendBegin(_rate, callback);
					}
					else
					{
						callback();
					}
				}
				else 
				{
					callback();
				}
			});
		}

		void sendPrepare(System.Action callback)
		{
			byte[] cmd = new byte[]{ (byte)'p' };
			_Send(cmd);
			WaitForResponse(STANDARD_RESPONSE_SIZE, data => {
				var st = DACStandardResponse.Parse(data);
				HandleStandardResponse(st);
				callback();
			});
		}

		void SendBegin(int rate, System.Action callback)
		{
			ushort lwm = 0;
			byte[] u_rate = System.BitConverter.GetBytes((uint)rate);
			byte[] u_lwm = System.BitConverter.GetBytes(lwm);
			byte[] cmd = new byte[1 + 2 + 4];
			cmd[0] = (byte)'b';
			cmd[1] = u_lwm[0];
			cmd[2] = u_lwm[1];
			cmd[3] = u_rate[0];
			cmd[4] = u_rate[1];
			cmd[5] = u_rate[2];
			cmd[6] = u_rate[3];
			_Send(cmd);
			WaitForResponse(STANDARD_RESPONSE_SIZE, data => {
				var st = DACStandardResponse.Parse(data);
				HandleStandardResponse(st);
				_beginsent = true;
				callback();
			});
		}

		void SendUpdate(int rate, System.Action callback)
		{
			ushort lwm = 0;
			byte[] u_rate = System.BitConverter.GetBytes((uint)rate);
			byte[] u_lwm = System.BitConverter.GetBytes(lwm);
			byte[] cmd = new byte[1 + 2 + 4];
			cmd[0] = (byte)'u';
			cmd[1] = u_lwm[0];
			cmd[2] = u_lwm[1];
			cmd[3] = u_rate[0];
			cmd[4] = u_rate[1];
			cmd[5] = u_rate[2];
			cmd[6] = u_rate[3];
			_Send(cmd);
			WaitForResponse(STANDARD_RESPONSE_SIZE, data => {
				var st = DACStandardResponse.Parse(data);
				HandleStandardResponse(st);
				_beginsent = true;
				callback();
			});
		}

		void PollGotData(List<DACPoint> framedata)
		{
			if (framedata.Count > 0)
			{
				this.SendPoints(framedata, () => {
					SetTimeout(0, ()=>{
						PollStream();
					});
				});
			}
			else 
			{
				SetTimeout(0, ()=>{
					PollStream();
				});
			}
		}

		void PollStream()
		{
			if (_playbackState == 0) 
			{
				this.sendPrepare(() => {
					SetTimeout(0, () => PollStream() );
				});
			}
			else if (!_valid) 
			{
				SetTimeout(250, () => {
					sendPrepare(() => {
						SendBegin(_rate, () => {
							SetTimeout(0, ()=> PollStream() );
						});
					});
				});
			}
			else
			{
				int MAX = 1799; // 1799;
				int N = (int)Mathf.Max(0, MAX - _fullness);
				if( N > 50 ) 
				{
					SetTimeout(0, ()=> _streamSource(N, PollGotData) );
				}
				else
				{
					this.SendPing(() => {
						SetTimeout(0, ()=> PollStream() );
					});
				}
			}
		}

		void StreamPoints(int rate, System.Action<int, System.Action<List<DACPoint>>> pointSource)
		{
			_rate = rate;
			_streamSource = pointSource;
			SendStop(() => {
				SetTimeout(0, ()=> PollStream() );
			});
		}

		public void StreamFrames(int rate, System.Action<System.Action< List<DACPoint> >> frameSource)
		{
			List<DACPoint> frameBuffer = new List<DACPoint>();

			void innerStream(int numpoints, System.Action<List<DACPoint>> pointcallback)
			{
				if (frameBuffer.Count < numpoints)
				{
					frameSource( points => {
						for(var i=0; i<points.Count; i++)
						{
							frameBuffer.Add(points[i]);
						}
						SetTimeout(0, () => innerStream(numpoints, pointcallback) );
					});
				}
				else
				{
					List<DACPoint> points = new List<DACPoint>();
					for(int i=0; i<numpoints; i++)
					{
						points.Add(frameBuffer[0]);
						frameBuffer.RemoveAt(0);
					}
					pointcallback(points);
				}
			}

			this.StreamPoints(rate, innerStream);
		}

		public void Close() 
		{
			if (_client != null)
			{
				_client.Dispose();
				_client = null;
			}
		}

		void SetTimeout(int delay_ms, System.Action callback)
		{
			callback();

			// Thread thread = new Thread(()=>{
			// 	Thread.Sleep( delay_ms + 1 );
			// 	callback();
			// });
			// thread.IsBackground = true;
			// thread.Start();
		}
	}

	class InputHandler
	{
		public int size;
		public System.Action<byte[]> callback;

		public InputHandler()
		{
			
		}
	}
}