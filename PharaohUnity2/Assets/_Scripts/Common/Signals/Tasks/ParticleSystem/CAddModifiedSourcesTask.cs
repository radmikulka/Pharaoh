// =========================================
// AUTHOR: Marek Karaba
// DATE:   31.07.2025
// =========================================

namespace TycoonBuilder
{
	public class CAddModifiedSourcesTask
	{
		public IParticleSource[] ModifiedSources { get; private set; }

		public CAddModifiedSourcesTask(IParticleSource modifiedSource)
		{
			ModifiedSources = new [] { modifiedSource };
		}

		public CAddModifiedSourcesTask(IParticleSource[] modifiedSources)
		{
			ModifiedSources = modifiedSources;
		}
	}
}