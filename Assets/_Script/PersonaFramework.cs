using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PersonaFramework : MonoBehaviour
{
    [SerializeField]
    private Text textSentimentScore;
    [SerializeField]
    private Text textExpression;
    //[SerializeField]
    //private Text[] textPersonality;
    [SerializeField]
    private Text[] textAttitude;
    [SerializeField]
    private Text[] textEq;
    [SerializeField]
    private Text[] textIq;

    public void UpdatePersonaFramework1(float sentiment, string expression)
    {
        textSentimentScore.text = sentiment.ToString();
        textExpression.text = expression;
    }

    public void UpdatePersonaFramework2(int personality, int attitude, int eq, int iq)
    {
        for (int i = 0; i < textIq.Length; i++)
        {
            textIq[i].color = Color.grey;

            if (i < textAttitude.Length)
            {
                //textPersonality[i].color = Color.grey;
                textAttitude[i].color = Color.grey;
                textEq[i].color = Color.grey;
            }
        }

        //textPersonality[personality - 1].color = Color.yellow;
        textAttitude[math.clamp(attitude -1, 0, textAttitude.Length)].color = Color.yellow;
        textEq[math.clamp(eq-1, 0, textEq.Length)].color = Color.yellow;
        textIq[math.clamp(iq -1,0,textIq.Length)].color = Color.yellow;
    }
}
