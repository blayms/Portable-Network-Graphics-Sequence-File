namespace Krashan.LZ4
{
	internal class Safe64LZ4Service : ILZ4Service
	{
		public string CodecName => "Safe 64";

		public int Encode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
		{
			return LZ4CodecPS.Encode64(input, inputOffset, inputLength, output, outputOffset, outputLength);
		}

		public int Decode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength, bool knownOutputLength)
		{
			return LZ4CodecPS.Decode64(input, inputOffset, inputLength, output, outputOffset, outputLength, knownOutputLength);
		}

		public int EncodeHC(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
		{
			return LZ4CodecPS.Encode64HC(input, inputOffset, inputLength, output, outputOffset, outputLength);
		}
	}
}
