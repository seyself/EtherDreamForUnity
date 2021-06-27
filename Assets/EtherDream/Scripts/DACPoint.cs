using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// typedef struct dac_point {
// 	uint16_t control;
// 	int16_t x;
// 	int16_t y;
// 	uint16_t r;
// 	uint16_t g;
// 	uint16_t b;
// 	uint16_t i;
// 	uint16_t u1;
// 	uint16_t u2;
// } dac_point_t;
namespace DAC 
{
	[System.Serializable]
	public class DACPoint
	{
		public static int bufferSize = 18;
		
		public ushort control; // uint16
		public short x; // int16
		public short y; // int16
		public ushort r; // uint16
		public ushort g; // uint16
		public ushort b; // uint16
		public ushort i; // uint16
		public ushort u1; // uint16
		public ushort u2; // uint16

		public DACPoint()
		{
			
		}

		public byte[] ToBytes()
		{
			byte[] bytes = new byte[2 + 2+2 + 2+2+2+2 + 2+2]; // 18 bytes
			SetBytes(ref bytes, 0, System.BitConverter.GetBytes(control) );
			SetBytes(ref bytes, 2, System.BitConverter.GetBytes(x) );
			SetBytes(ref bytes, 4, System.BitConverter.GetBytes(y) );
			SetBytes(ref bytes, 6, System.BitConverter.GetBytes(r) );
			SetBytes(ref bytes, 8, System.BitConverter.GetBytes(g) );
			SetBytes(ref bytes, 10, System.BitConverter.GetBytes(b) );
			SetBytes(ref bytes, 12, System.BitConverter.GetBytes(i) );
			SetBytes(ref bytes, 14, System.BitConverter.GetBytes(u1) );
			SetBytes(ref bytes, 16, System.BitConverter.GetBytes(u2) );
			return bytes;
		}

		public void WriteStream(ref byte[] bytes, int startIndex)
		{
			SetBytes(ref bytes, startIndex + 0, System.BitConverter.GetBytes(control) );
			SetBytes(ref bytes, startIndex + 2, System.BitConverter.GetBytes(x) );
			SetBytes(ref bytes, startIndex + 4, System.BitConverter.GetBytes(y) );
			SetBytes(ref bytes, startIndex + 6, System.BitConverter.GetBytes(r) );
			SetBytes(ref bytes, startIndex + 8, System.BitConverter.GetBytes(g) );
			SetBytes(ref bytes, startIndex + 10, System.BitConverter.GetBytes(b) );
			SetBytes(ref bytes, startIndex + 12, System.BitConverter.GetBytes(i) );
			SetBytes(ref bytes, startIndex + 14, System.BitConverter.GetBytes(u1) );
			SetBytes(ref bytes, startIndex + 16, System.BitConverter.GetBytes(u2) );
		}

		void SetBytes(ref byte[] bytes, int index, byte[] src)
		{
			bytes[index    ] = src[0];
			bytes[index + 1] = src[1];
		}
	}
}