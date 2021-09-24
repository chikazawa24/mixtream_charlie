using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Effekseer;
using UnityEngine;

public enum TapStyle
{
    Tap, Hold
}

/// <summary>
/// ノーツの開始地点と終了地点の対を一つのレーンと定義し、それにアタッチするスクリプト
/// </summary>
[System.Serializable]
public class Lane : MonoBehaviour
{
    #region ========== variables ==========

    /// <summary>
    /// GameManagerのインスタンス
    /// </summary>
    private GameManager gm;

    /// <summary>
    /// レーン上にあるノーツのリスト インデックスが小さいほど先に生成されている
    /// </summary>
    protected List<Note> onLineNotes;
    
    /// <summary>
    /// ノーツ発射座標
    /// </summary>
    [SerializeField] public Transform generatePointTransform;

    /// <summary>
    /// ノーツ到達座標
    /// </summary>
    [SerializeField] public Transform attackPointTransform;

    /// <summary>
    /// ホールド判定を許可するかどうかのフラグ
    /// </summary>
    private bool allowHold = true;

    /// <summary>
    /// ホールド入力されているかを示す
    /// </summary>
    private readonly Dictionary<NotesAttribute, bool> isHold = new Dictionary<NotesAttribute, bool>()
    {
        { NotesAttribute.Knob, false},
        { NotesAttribute.Fader, false},
        { NotesAttribute.Pad, false}
    };
    
    #endregion =======================================

        
    
    #region ========== public Properties & Methods ==========

    public virtual void AddNote(Note note)
    {
        onLineNotes.Add(note);
    }

    /// <summary>
    /// JoyCon入力クラスとノートのタップ処理を中継する。
    /// 後方ノーツを隠蔽し、最前方ノーツだけのAttackを呼び出す。
    /// </summary>
    public virtual void Attack(NotesAttribute attribute)
    {
        Evaluation e;
        if (onLineNotes.Count != 0)
        {
            var note = onLineNotes.First();
            if (note.Attribute == attribute)
            {
                if ((note.Attack()) != Evaluation.None)
                {
                    onLineNotes.Remove(note);
                    return;
                }
            }
        }
        //Debug.Log("None");

        // 何もないところでタップした場合、一度手を放すまでホールド判定が行われなくなる。
        StartCoroutine(HoldLock());
    }

    /// <summary>
    /// ホールド判定読み込み用
    /// Knob、Fader、Padそれぞれに対してホールドが有効であるかどうか
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public virtual bool IsHoldActive(NotesAttribute attribute) => isHold[attribute] && allowHold;
    //こいつに聞く

    /// <summary>
    /// ホールドの書き込みをする。Knob、Fader、Padそれぞれに対して真偽値を取る
    /// </summary>
    /// <param name="attribute">ホールドするあるいは離す種類</param>
    /// <param name="isHold"></param>
    public virtual void HoldSet(NotesAttribute attribute, bool isHold) => this.isHold[attribute] = isHold;

    #endregion =======================================



    #region ========== private Methods ==========

    /// <summary>
    /// ホールド解除するまでホールド禁止
    /// </summary>
    /// <returns></returns>
    private IEnumerator HoldLock()
    {
        if(!allowHold) yield break;
        allowHold = false;
        yield return null;
        while (HoldAny)
            yield return null;
        allowHold = true;
    }

    /// <summary>
    /// 何らかの属性でホールド入力されている
    /// </summary>
    private bool HoldAny => isHold.Values.Count(atr => atr == true) != 0;
    

    // Start is called before the first frame update
    virtual public void Start()
    {
        gm = GameManager.Instance;
        onLineNotes = new List<Note>();
    }

    void Update()
    {
        ManageOnLineNotes();
    }

    private void ManageOnLineNotes()
    {
        if (onLineNotes.Count != 0)
        {
            var missTime = onLineNotes[0].StartTime + GameManager.FloatingTime +
                           GameManager.EvaluationRanges[1].halfActiveRange;
            if (GameManager.CurrentTime > missTime)
            {
                //Debug.Log("miss:"+onLineNotes[0].StartTime);
                onLineNotes.RemoveAt(0);
                ScoreManager.AddScore(Evaluation.Miss);
            }
        }
    }

    #endregion ========================
}
