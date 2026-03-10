using System;
using AldaEngine;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CDialogScreen : CPharaohScreen
	{
		[SerializeField] private EScreenId _screenId;
		[SerializeField] private CUiComponentText _headerText;
		[SerializeField] private CUiComponentText _contentText;
		[SerializeField] private CUiButton _buttonOne;
		[SerializeField] private CUiButton _buttonTwo;
		[SerializeField] private CUiComponentText _buttonOneLabel;
		[SerializeField] private CUiComponentText _buttonTwoLabel;
		[SerializeField] private GameObject _buttonTwoRoot;

		private ITranslation _translation;
		private CShowDialogTask _activeTask;

		public override int Id => (int)_screenId;

		[Inject]
		private void Inject(ITranslation translation)
		{
			_translation = translation;
		}

		public void Configure(CShowDialogTask task)
		{
			_activeTask = task;
		}

		public override void OnScreenOpenStart()
		{
			base.OnScreenOpenStart();

			if (_activeTask == null)
				return;

			string header = _activeTask.IsHeaderLocalized
				? _translation.GetText(_activeTask.Header)
				: _activeTask.Header;
			_headerText.SetValue(header);

			string content;
			if (_activeTask.IsContentLocalized)
			{
				content = _activeTask.ContentArgs != null && _activeTask.ContentArgs.Length > 0
					? _translation.GetText(_activeTask.Content, _activeTask.ContentArgs)
					: _translation.GetText(_activeTask.Content);
			}
			else
			{
				content = _activeTask.Content;
			}
			_contentText.SetValue(content);

			SetupButton(_buttonOne, _buttonOneLabel, _activeTask.ButtonOne);

			bool hasTwoButtons = _activeTask.ButtonTwo != null;
			_buttonTwoRoot.SetActive(hasTwoButtons);
			if (hasTwoButtons)
			{
				SetupButton(_buttonTwo, _buttonTwoLabel, _activeTask.ButtonTwo);
			}

			_buttonOne.AddClickListener(OnButtonOneClick);
			_buttonTwo.AddClickListener(OnButtonTwoClick);
		}

		public override void OnScreenCloseEnd()
		{
			_buttonOne.RemoveClickListener(OnButtonOneClick);
			_buttonTwo.RemoveClickListener(OnButtonTwoClick);
			_activeTask = null;

			base.OnScreenCloseEnd();
		}

		public override bool CanBeClosedByEscape()
		{
			return _activeTask?.CanBeClosed ?? true;
		}

		public override string GetScreenName()
		{
			return _activeTask?.AnalyticsId ?? "Dialog";
		}

		private void SetupButton(CUiButton button, CUiComponentText label, CDialogButtonData data)
		{
			if (data == null)
				return;

			string text = data.IsLocalized
				? _translation.GetText(data.Label)
				: data.Label;
			label.SetValue(text);
		}

		private void OnButtonOneClick()
		{
			_activeTask?.ButtonOne?.OnClick?.Invoke();
			CloseThisScreen();
		}

		private void OnButtonTwoClick()
		{
			_activeTask?.ButtonTwo?.OnClick?.Invoke();
			CloseThisScreen();
		}
	}
}
