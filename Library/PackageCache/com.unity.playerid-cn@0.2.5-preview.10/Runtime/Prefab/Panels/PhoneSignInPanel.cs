using System;
using System.Collections;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.PlayerIdentity;
using UnityEngine.PlayerIdentity.UI;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace UnityEngine.PlayerIdentity.UI
{
    public class PhoneSignInPanel : AbstractPanel
    {
        [SerializeField]
        TMP_InputField smsCode=default;

        [SerializeField]
        TMP_InputField phoneNumber=default;

        private string _verificationId = default;

        private Button _loginBtn;

        private const string SmsCodeButtonDefaultText = "发送";
        private Button _sendCodeBtn;
        private static short _codeCountDown = -1;
        private TMP_Text _smsCodeBtnText;
        
        private Dropdown _regionCode;
        
        public enum LoginActions
        {
            Link,
            SignIn,
            Undefined,
        }

        public static LoginActions LoginAction = LoginActions.Undefined;

        private void Start()
        {
            var buttons = GetComponentsInChildren<Button>();

            foreach (var button in buttons)
            {
                if (button.name == "Send Code")
                {
                    _sendCodeBtn = button;
                    _smsCodeBtnText = _sendCodeBtn.GetComponentInChildren<TMP_Text>();
                    _smsCodeBtnText.text = SmsCodeButtonDefaultText;
                    _sendCodeBtn.onClick.AddListener(OnCodeClicked);
                    button.interactable = false;

                }
                else if (button.name == "Sign In Button")
                {
                    _loginBtn = button;
                    _loginBtn.onClick.AddListener(OnLoginClicked);
                    button.interactable = false;
                }
            }

            smsCode.onValueChanged.AddListener(OnSmsCodeValueChanged);
            
            phoneNumber.onValueChanged.AddListener(OnPhoneNumberValueChanged);

            _regionCode = GetComponentInChildren<Dropdown>();
        }

        private void OnSmsCodeValueChanged(string text)
        {
            if(FormUtils.AreInputFieldsNotEmpty(smsCode.text, _verificationId))
            {
                if (!_loginBtn.IsInteractable())
                    _loginBtn.interactable = true;
            }
            else
            {
                if (_loginBtn.IsInteractable())
                    _loginBtn.interactable = false;
            }
        }

        private void OnPhoneNumberValueChanged(string text)
        {
            if((FormUtils.AreInputFieldsNotEmpty(phoneNumber.text) && IsCodeSendable())||FormValidator.IsValidPhoneNumber(GetPhoneNumber()))
            {
                if (!_sendCodeBtn.IsInteractable())
                {
                    _smsCodeBtnText.text = SmsCodeButtonDefaultText;
                    _codeCountDown = -1; //avoid redundant text set
                    _sendCodeBtn.interactable = true;
                }
            }
            else
            {
                if (_sendCodeBtn.IsInteractable())
                    _sendCodeBtn.interactable = false;
            }
        }

        private string GetPhoneNumber()
        {
            return _regionCode.options[_regionCode.value].text + phoneNumber.text;
        }

        public void OnLoginClicked()
        {
            if (FormValidator.IsValidSmsCode(smsCode.text))
            {
                switch (LoginAction)
                {
                    case LoginActions.Link:
                        PlayerIdentityManager.Current.LinkSmsCode(smsCode.text, _verificationId);
                        break;
                    case LoginActions.SignIn:
                        PlayerIdentityManager.Current.PhoneLogin(smsCode.text, _verificationId);
                        break;
                }
            }
            else
                MainController.Instance.
                    PopupController.ShowError(new Error() { message = "Sms code in invalid format" });
        }

        private void OnCodeClicked()
        {
            if (FormValidator.IsValidPhoneNumber(GetPhoneNumber()))
            {
                StartSmsCodeCountDown();
                PlayerIdentityManager.Current.RequestSmsCode(GetPhoneNumber(), OnCodeResult);
            }
            else
                MainController.Instance.
                    PopupController.ShowError(new Error() { message = "Phone number in invalid format" });
        }
        

        private void OnCodeResult(PlayerIdentityBackendSubsystem.CreateSmsCodeCallbackArgs args)
        {
            _verificationId = null;
            if (args.error == null)
            {
                _verificationId = args.verificationId;
            }
            else
            {
                MainController.Instance.
                    PopupController.
                    ShowError(new Error(){message = args.error.message});
            }
        }

        private IEnumerator RefreshNextCodeTime()
        {
            while (_codeCountDown >= 0)
            {
                yield return new WaitForSeconds(1);
                _codeCountDown--;
            }
        }

        private void StartSmsCodeCountDown()
        {
            if (_sendCodeBtn.IsInteractable())
                _sendCodeBtn.interactable = false;
            _codeCountDown = 60; // time in seconds;
            StartCoroutine(nameof(RefreshNextCodeTime));
        }

        private bool IsCodeSendable()
        {
            return _codeCountDown <= 0;
        }

        private void Update()
        {
            if (0 == _codeCountDown)
            {
                _smsCodeBtnText.text = SmsCodeButtonDefaultText;
                _codeCountDown = -1; //avoid redundant text set
                if (!_sendCodeBtn.IsInteractable())
                    _sendCodeBtn.interactable = true;
            }
            else if (0 < _codeCountDown)
            {
                _smsCodeBtnText.text = _codeCountDown.ToString();
            }

        }
    }
}