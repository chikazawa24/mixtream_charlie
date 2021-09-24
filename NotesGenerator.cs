using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.Experimental.PlayerLoop;


/// <summary>
/// 各レーンオブジェクトのLaneコンポーネントにノーツ生成の指示を出す
/// </summary>
public class NotesGenerator : SingletonMonoBehaviour<NotesGenerator>
{

    #region ================== Nested Type ==============================

    private class Pool
    {
        private readonly GameObject original;
        private readonly List<GameObject> list;

        public Pool(GameObject original, int initialAmount)
        {
            this.original = original;
            list = new List<GameObject>();

            for (int i = 0; i < initialAmount; i++)
            {
                var obj = Instantiate(original);
                obj.SetActive(false);
                list.Add(obj);
            }
        }

        public GameObject Create()
        {
            for (int i = 0; i < list.Count; i++)
                if (!list[i].activeSelf)
                {
//                    Debug.Log( list.Count + " " + i + " " +list[i]);
                    list[i].SetActive(true);
                    return list[i];
                }

            // Debug.Log("added new instance");
            list.Add(Instantiate(original));
            list.Last().SetActive(true);
            return list.Last();
        }
    }

    /// <summary>
    /// ノーツの属性とプレハブを結び付けてシリアライズするための型
    /// </summary>
    [System.Serializable]
    private class NotesDictionaryElement
    {
        public NotesAttribute attribute;
        public TapStyle tapStyle;
        public GameObject prefab;
        public int initialPoolingAmount;
    }

    #endregion ============================================================



    #region ============== Member Variables ===============================

    /// <summary>
    /// シリアライズ用
    /// </summary>
    [SerializeField] private List<NotesDictionaryElement> notesPrefab;

    /// <summary>
    /// プールのコレクション
    /// </summary>
    private readonly Dictionary<NotesAttribute, Dictionary<TapStyle, Pool>> poolDictionary =
        new Dictionary<NotesAttribute, Dictionary<TapStyle, Pool>>();

    #endregion ==========================================================



    #region ==================== public methods =========================

    public static NotesBehaviour CreateObject(TapStyle tapStyle, NotesAttribute attribute) =>
        Instance.poolDictionary[attribute][tapStyle].Create().GetComponent<NotesBehaviour>();

    public static void DeleteObject(GameObject obj) => obj.SetActive(false);

    #endregion =============================================================



    #region =============== private methods ========================

    protected override void Awake() => InitializePools();

    /// <summary>
    /// プールを初期化する Awakeで実行
    /// </summary>
    private void InitializePools()
    {
        foreach (var element in notesPrefab)
        {
            var attribute = element.attribute;
            var tapStyle = element.tapStyle;
            var prefab = element.prefab;
            var amount = element.initialPoolingAmount;
            var pool = new Pool(prefab, amount);

            // Dictionaryの中にDictionaryを追加
            if (!poolDictionary.ContainsKey(attribute))
                poolDictionary.Add(attribute, new Dictionary<TapStyle, Pool>());

            // プールをDictionaryに格納
            if (!poolDictionary[attribute].ContainsKey(tapStyle))
                poolDictionary[attribute].Add(tapStyle, pool);
        }
    }

    # endregion ==========================================================
}