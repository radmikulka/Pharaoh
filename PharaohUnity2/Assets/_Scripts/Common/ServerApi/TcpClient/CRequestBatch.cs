// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using AldaEngine.Tcp;

namespace Pharaoh
{
	internal class CRequestBatch
	{
		public readonly CHitInfoHeader Header;
		public readonly bool SuppressAutomaticErrorHandling;
		public readonly CRequestRecord[] Records;

		public CRequestBatch(
			CHitInfoHeader header,
			CRequestRecord[] records,
			bool suppressAutomaticErrorHandling)
		{
			SuppressAutomaticErrorHandling = suppressAutomaticErrorHandling;
			Records = records;
			Header = header;
		}

		public void FailAll(EErrorCode errorCode)
		{
			foreach (CRequestRecord record in Records)
			{
				record.OnFail?.Invoke(errorCode);
			}
		}

		public CRequestPacket GetPackedData()
		{
			IHit[] allHits = new IHit[Records.Length];
			for (int i = 0; i < allHits.Length; i++)
			{
				allHits[i] = Records[i].Hit;
			}
			return new CRequestPacket(Header, allHits);
		}

	}
}