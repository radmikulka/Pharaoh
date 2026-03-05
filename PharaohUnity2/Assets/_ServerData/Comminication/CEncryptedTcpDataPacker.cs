// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System.Collections.Generic;
using System;
using AldaEngine.Tcp;

namespace ServerData
{
	public class CEncryptedTcpDataPacker : ITcpDataPacker
	{
		private readonly ITcpCrypter _crypter;
		private readonly IZipProvider _zipper;

		public CEncryptedTcpDataPacker(ITcpCrypter crypter, IZipProvider zipper)
		{
			_crypter = crypter;
			_zipper = zipper;
		}

		public byte[] PackData(byte[] data)
		{
			byte[] result = data;

			result = _crypter.Encrypt(result);
			result = _zipper.Zip(result);
			
			return result;
		}

		public byte[] UnpackData(byte[] data)
		{
			byte[] result = _zipper.UnZip(data);
			result = _crypter.Decrypt(result);
			
			return result;
		}
	}
}