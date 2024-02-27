using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine;
using TMPro;

public class ErrorMessageController : MonoBehaviour
{
    private TMP_Text _messageText;
    private Animator _animator;
    private string _errorsTable = "ErrorMessages";

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _messageText = GetComponentInChildren<TMP_Text>();
    }

    public void ShowMessage(string _key)
    {   // Show the error message
        _animator.SetBool("Show", true);
        var _message = LocalizationSettings.StringDatabase.GetLocalizedString(_errorsTable, _key);
        _messageText.text = _message;
    }

    public void ShowTimedMessage(string _key, float _time)
    {   // Show the error message for a specific time
        ShowMessage(_key);
        Invoke("HideMessage", _time + 0.15f);
    }

    public void HideMessage()
    {   // Hide the error message
        _animator.SetBool("Show", false);
    }
}
