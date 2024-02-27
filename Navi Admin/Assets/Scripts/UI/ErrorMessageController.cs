using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ErrorMessageController : MonoBehaviour
{
    private TMP_Text _messageText;
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _messageText = GetComponentInChildren<TMP_Text>();
    }

    public void ShowMessage(string _message)
    {   // Show the error message
        _messageText.text = _message;
        _animator.SetBool("Show", true);
    }

    public void ShowTimedMessage(string _message, float _time)
    {   // Show the error message for a specific time
        ShowMessage(_message);
        Invoke("HideMessage", _time + 0.15f);
    }

    public void HideMessage()
    {   // Hide the error message
        _animator.SetBool("Show", false);
    }
}
