// =========================================
// AUTHOR: Juraj Joscak
// DATE:   29.07.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using UnityEngine;

namespace TycoonBuilder
{
	public abstract class CApplyHandler<T>
	{
		public void BindTo(CField<T> field)
		{
			Apply(field.Value);
			field.OnValueChanged += Apply;
		}

		protected abstract void Apply(T value);
	}

	public class CMusicHandler : CApplyHandler<bool>
	{
		protected override void Apply(bool value)
		{
			
		}
	}
	
	public class CVibrationHandler : CApplyHandler<bool>
	{
		protected override void Apply(bool value)
		{
			
		}
	}
	
	public class CSoundHandler : CApplyHandler<bool>
	{
		protected override void Apply(bool value)
		{
			
		}
	}
	
	public class CBatterySaverHandler : CApplyHandler<bool>
	{
		private readonly IEventBus _eventBus;

		public CBatterySaverHandler(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		protected override void Apply(bool value)
		{
			
		}
	}
	
	public class CNotificationsHandler : CApplyHandler<bool>
	{
		private readonly IEventBus _eventBus;

		public CNotificationsHandler(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		protected override void Apply(bool value)
		{
			
		}
	}
	
	public class CLanguageHandler : CApplyHandler<int>
	{
		private static readonly Dictionary<string, ELanguageCode> LanguagesDb = new()
		{
			{"cs", ELanguageCode.Cs},
			{"en", ELanguageCode.En},
			//{"ja", ELanguageCode.Ja},
			//{"de", ELanguageCode.De},
			{"es", ELanguageCode.Es},
			//{"fr", ELanguageCode.Fr},
			//{"it", ELanguageCode.It},
			//{"pl", ELanguageCode.Pl},
			//{"pt-br", ELanguageCode.Pt_Br},
			//{"se", ELanguageCode.Sv},
			//{"tr", ELanguageCode.Tr},
			//{"ko", ELanguageCode.Ko},
			//{"nl", ELanguageCode.Nl},
			//{"id", ELanguageCode.Id},
		};

		private readonly ITranslation _translation;

		public CLanguageHandler(ITranslation translation)
		{
			_translation = translation;
		}

		protected override void Apply(int value)
		{
			_translation.SetLanguage((ELanguageCode)value);
		}

		public static ELanguageCode[] ValidLanguages()
		{
			return LanguagesDb.Values.ToArray();
		}
	}
	
	public class CGraphicsHandler : CApplyHandler<int>
	{
		protected override void Apply(int value)
		{
			// Apply graphics settings
		}
	}
	
	public class CMeasurementSystemHandler : CApplyHandler<int>
	{
		protected override void Apply(int value)
		{
			// Apply measurement system settings
		}
	}
}