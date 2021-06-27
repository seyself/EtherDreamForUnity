using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// struct dac_broadcast {
// 	uint8_t mac_address[6];
// 	uint16_t hw_revision;
// 	uint16_t sw_revision;
// 	uint16_t buffer_capacity;
// 	uint32_t max_point_rate;
// 		struct dac_status status;
// } __attribute__ ((packed));
namespace DAC 
{
	[System.Serializable]
	public class DACBroadcast
	{
		public byte mac_address; // uint8_t
		public ushort hw_revision; // uint16_t
		public ushort sw_revision; // uint16_t
		public ushort buffer_capacity; // uint16_t
		public uint max_point_rate; // uint32_t
		public DACStatus status; // struct dac_status

		public DACBroadcast()
		{
			
		}
	}
}