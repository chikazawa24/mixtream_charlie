using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
//using UnityEditor.iOS.Extensions.Common;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LongNote : Note
{
    public override TapStyle TapStyle { get; } = TapStyle.Hold;

    /// <summary>
    /// 16分を1としたノーツの長さ
    /// </summary>
    public int TickDuration { get; private set; }
    
    /// <summary>
    /// ノーツをホールドする時間(秒)
    /// </summary>
    public float SecondDuration { get; private set; }

    public LongNote(float startTime, Lane lane, NotesAttribute attribute, int tickDuration, float secondDuration) :
        base(startTime, lane, attribute)
    {
        TickDuration = tickDuration;
        SecondDuration = secondDuration;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Generate()
    {
        var behaviour = NotesGenerator.CreateObject(TapStyle, Attribute);
        var gptf = FloatingLane.generatePointTransform;
        var aptf = FloatingLane.attackPointTransform;

        behaviour.SetDestination(gptf, aptf, SecondDuration);
        FloatingLane.AddNote(this);
    }


    public override Evaluation Attack()
    {
        var evaluation = base.Attack();
        // MonoBehaviourを継承したクラスのインスタンスなら何でもいい
        FloatingLane.StartCoroutine(Hold());
        return evaluation;
    }

    /// <summary>
    /// ホールドの判定
    /// </summary>
    /// <returns></returns>
    private IEnumerator Hold()
    {
        var tickInterval = GameManager.Instance.holdScoreIntervalTick;
        var divider = TickDuration / tickInterval;
        var secondInterval = SecondDuration / divider;

        for (int i = 1; i < divider; i++)
        {
            yield return new WaitForSeconds(secondInterval);
            if (FloatingLane.IsHoldActive(Attribute))
            {
                //FloatingLane.PlayEffect(Evaluation.Hold);
                ScoreManager.AddScore(Evaluation.Hold);
            }
            else
            {
                ScoreManager.AddScore(Evaluation.Miss);
            }
        }
    }
}
