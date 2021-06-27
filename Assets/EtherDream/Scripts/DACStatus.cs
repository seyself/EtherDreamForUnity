using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// struct dac_status {
// 	uint8_t protocol;
// 	uint8_t light_engine_state;
// 	uint8_t playback_state;
// 	uint8_t source;
// 	uint16_t light_engine_flags;
// 	uint16_t playback_flags;
// 	uint16_t source_flags;
// 	uint16_t buffer_fullness;
// 	uint32_t point_rate;
// 	uint32_t point_count;
// } __attribute__ ((packed)); // 20 bytes 4+4+4+4+4
namespace DAC 
{
	[System.Serializable]
	public class DACStatus
	{
		public byte protocol; // uint8_t
		public byte light_engine_state; // uint8_t
		public byte playback_state; // uint8_t
		public byte source; // uint8_t
		public ushort light_engine_flags; // uint16_t
		public ushort playback_flags; // uint16_t
		public ushort source_flags; // uint16_t
		public ushort buffer_fullness; // uint16_t
		public uint point_rate; // uint32_t
		public uint point_count; // uint32_t
		
		public DACStatus()
		{
			
		}
	}
}