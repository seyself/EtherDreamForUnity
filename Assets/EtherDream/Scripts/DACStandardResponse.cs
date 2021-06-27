using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DAC 
{
	public class DACStandardResponse
	{
		public byte response;
		public byte command;
		public DACStatus status;
		public bool success;
		public string str;

		public DACStandardResponse()
		{
			
		}

		public static DACStandardResponse Parse(byte[] bytes)
		{
			DACStandardResponse res = new DACStandardResponse();
			res.response = bytes[0];
			res.command = bytes[1];
			res.status = new DACStatus(){
				protocol = bytes[2],
				light_engine_state = bytes[3],
				playback_state = bytes[4],
				source = bytes[5],
				light_engine_flags = System.BitConverter.ToUInt16(bytes, 6),
				playback_flags = System.BitConverter.ToUInt16(bytes, 8),
				source_flags = System.BitConverter.ToUInt16(bytes, 10),
				buffer_fullness = System.BitConverter.ToUInt16(bytes, 12),
				point_rate = System.BitConverter.ToUInt32(bytes, 14),
				point_count = System.BitConverter.ToUInt32(bytes, 18),
			};
			res.success = res.response == (byte)'a';
			res.str = $"resp={res.response},fullness={res.status.buffer_fullness},raw={bytes.Length}";
			return res;
		}
	}
}