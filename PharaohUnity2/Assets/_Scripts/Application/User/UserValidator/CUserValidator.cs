// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using TycoonBuilder;
using TycoonBuilder.Signal;
using ILogger = AldaEngine.ILogger;
using Random = UnityEngine.Random;

namespace TycoonBuilder
{
	public class CUserValidator : IUserValidator, IInitializable
	{
		private readonly CValidatorTypeCache _validatorTypesCache = new();
		private readonly HashSet<CLockObject> _validationLockers = new();
		private readonly HashSet<object> _validatedObjects = new();
		private readonly ICtsProvider _ctsProvider;
		private readonly IEventBus _eventBus;
		private readonly ILogger _logger;
		private readonly CUser _user;

		private readonly CLockObject _hitProcessingLock = new("HitProcessingLock");

		private CancellationTokenSource _cts;

		public CUserValidator(
			ICtsProvider ctsProvider,
			IEventBus eventBus,
			ILogger logger,
			CUser user
		)
		{
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
			_logger = logger;
			_user = user;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CAllServerHitsProcessedSignal>(OnAllHitsProcessed);
			_eventBus.AddTaskHandler<CStopUserValidatorTask>(ProcessStopUserValidatorTask);
		}

		private void ProcessStopUserValidatorTask(CStopUserValidatorTask task)
		{
			// stops validation forever - new user validator instance is recreated in connecting screen
			PauseValidation(new CLockObject("InfinityLock"));
		}

		private void OnAllHitsProcessed(CAllServerHitsProcessedSignal _)
		{
			ResumeValidation(_hitProcessingLock);
		}

		public void PauseValidation(CLockObject lockObject)
		{
			_cts?.Cancel();
			_validationLockers.Add(lockObject);
		}

		public void ResumeValidation(CLockObject lockObject)
		{
			_validationLockers.Remove(lockObject);
			if (_validationLockers.Count == 0)
			{
				ValidateUser().Forget();
			}
		}

		private async UniTaskVoid ValidateUser()
		{
			_cts?.Cancel();
			_cts = _ctsProvider.GetNewLinkedCts();
			bool cancelled = await UniTask.DelayFrame(3, cancellationToken: _cts.Token).SuppressCancellationThrow();
			if (cancelled)
				return;
			DoValidateObject(_user);
			_validatedObjects.Clear();
		}

		private object GetMemberValue(MemberInfo memberInfo, object obj)
		{
			if (memberInfo is FieldInfo fieldInfo)
			{
				return fieldInfo.GetValue(obj);
			}

			if (memberInfo is PropertyInfo propertyInfo)
			{
				return propertyInfo.GetValue(obj, null);
			}

			throw new ArgumentException("MemberInfo must be of type FieldInfo or PropertyInfo", nameof(memberInfo));
		}

		private void DoValidateObject(object obj)
		{
			if (!_validatedObjects.Add(obj))
				return;

			Type type = obj.GetType();
			bool isValidatable = _validatorTypesCache.IsValidatable(type);
			if (!isValidatable)
				return;

			IReadOnlyList<MemberInfo> membersToValidate = _validatorTypesCache.GetValidatableMembers(type);
			foreach (MemberInfo memberInfo in membersToValidate)
			{
				ValidateMember(obj, memberInfo);
			}
		}

		private void ValidateMember(object obj, MemberInfo memberInfo)
		{
			object value = GetMemberValue(memberInfo, obj);
			switch (value)
			{
				case IIsConsistent isConsistent:
				{
					bool consistent = isConsistent.IsConsistent();
					if (!consistent)
					{
						LogInvalidProperty(memberInfo, value, obj);
					}

					return;
				}
				case IDictionary dictionary:
				{
					foreach (object dictionaryValue in dictionary.Values)
					{
						DoValidateObject(dictionaryValue);
					}

					return;
				}
				case IEnumerable enumerable:
				{
					foreach (object o in enumerable)
					{
						DoValidateObject(o);
					}

					return;
				}
			}

			if (value != obj)
			{
				DoValidateObject(value);
			}
		}

		private void LogInvalidProperty(MemberInfo memberInfo, object value, object targetObject)
		{
			_logger.LogWarning($"Inconsistent data in {targetObject.GetType().Name} on property {memberInfo.Name} with value {value} on {targetObject}");
		}
	}
}