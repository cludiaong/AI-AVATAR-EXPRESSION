using UnityEngine;
using UnityEngine.Events;

public class VR_EventController : MonoBehaviour
{
    public UnityEvent RIndexTriggerDown, LIndexTriggerDown;
    public UnityEvent RIndexTriggerUp, LIndexTriggerUp;

    // Update is called once per frame
    void Update()
    {
        //if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)) RIndexTriggerDown.Invoke();
        //if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger)) LIndexTriggerDown.Invoke();
        //
        //
        //if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger)) RIndexTriggerUp.Invoke();
        //if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger)) LIndexTriggerUp.Invoke();
    }
}
