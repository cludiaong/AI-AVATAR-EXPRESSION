using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class avatarAnimationControlHandle : MonoBehaviour
{
    public Animator animator;
    public string talkParamName;
    public string listeningParamName;
    public string expressionParamName;

    [SerializeReference] int currentExpressionID;
    public void OnTalk(bool enable)
    {
        animator.SetBool(talkParamName, enable);
    }
    public void OnListening(bool enable)
    {
        animator.SetBool(listeningParamName, enable);
    }

    public void SetExpression(int expressionId)
    {
        currentExpressionID = expressionId;
    }
    [Button]
    public void PlayExpression()
    {
        animator.SetInteger(expressionParamName, currentExpressionID);
    }
}
