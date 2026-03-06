// =========================================
// AUTHOR: Radek Mikulka
// DATE:   27.06.2025
// =========================================

using AldaEngine;
using AldaEngine.Tcp;
using ServerData;

namespace Pharaoh
{
	public class CTcpClientFactory
	{
		private readonly ILogger _logger;

		public CTcpClientFactory(ILogger logger)
		{
			_logger = logger;
		}

		public ITcpClient CreateTcpClient(CServerTcpEndPoint endPoint)
		{
			ITcpCrypter crypter = new CTcpCrypter();
			IZipProvider zipProvider = new CZipProvider();
			ITcpDataPacker packer = new CEncryptedTcpDataPacker(crypter, zipProvider);
			ITcpDataConvertor convertor = new CJsonSerializer(packer);
			CTcpClient client = new(1, convertor, endPoint, _logger);

			if (CServerConfig.Instance.IgnoreHitTimeoutTime)
			{
				client.SetTimeoutTime(99999);
			}
			
			return client;
		}
	}
}