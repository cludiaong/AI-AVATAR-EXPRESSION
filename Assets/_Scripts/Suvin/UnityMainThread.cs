using System;
using System.Threading;
using UnityEngine;

public class UnityMainThread : MonoBehaviour
{
    public static SynchronizationContext Context;

    private void Awake()
    {
        Context = SynchronizationContext.Current;
        if (Context == null)
        {
            Debug.LogError("UnityMainThread: no SynchronizationContext on Awake.");
        }
    }

    public static void Post(Action action)
    {
        if (Context != null) Context.Post(_ => action(), null);
        else action?.Invoke(); // last resort
    }
}
