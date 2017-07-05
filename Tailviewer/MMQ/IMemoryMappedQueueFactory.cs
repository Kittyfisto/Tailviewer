namespace Tailviewer.MMQ
{
	internal interface IMemoryMappedQueueFactory
	{
		IMemoryMappedQueueProducer CreateProducer();
		IMemoryMappedQueueConsumer CreateConsumer();
	}
}