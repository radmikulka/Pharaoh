// =========================================
// NAME: Marek Karaba
// DATE: 01.10.2025
// =========================================

using AldaEngine;
using ServerData.Hits;
using TycoonBuilder;

public class CRateUsHandler : IRateUsHandler
{
	private readonly CHitBuilder _hitBuilder;
	private readonly IEventBus _eventBus;

	public CRateUsHandler(CHitBuilder hitBuilder, IEventBus eventBus)
	{
		_hitBuilder = hitBuilder;
		_eventBus = eventBus;
	}

	public void SendFeedback(string userText)
	{
		CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CSendRateUsFeedbackRequest(userText));
		hit.BuildAndSend();
			
		_eventBus.Send(new CRateUsFeedbackSentSignal());
	}
}
