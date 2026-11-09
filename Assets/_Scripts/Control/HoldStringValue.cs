using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HoldStringValue : MonoBehaviour
{
    public string value;
    public UnityEvent<string> onSubmitValueString;
    public void SetValue(string value)
    {
        this.value = value;
    }
    public void SubmitString()
    {
        onSubmitValueString.Invoke(value);
    }
}
