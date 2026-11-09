using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayedAnimationController : MonoBehaviour
{
    Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void SetActiveAnimator(string paramName) => animator.SetBool(paramName, true);
    public void SetDeactiveAnimator(string paramName) => animator.SetBool(paramName, false);
}
