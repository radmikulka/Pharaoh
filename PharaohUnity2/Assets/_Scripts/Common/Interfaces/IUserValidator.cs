// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public interface IUserValidator
	{
		void PauseValidation(CLockObject lockObject);
		void ResumeValidation(CLockObject lockObject);
	}
}