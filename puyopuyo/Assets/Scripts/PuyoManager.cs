using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/* ぷよ達のマス目管理と生成 */

public class PuyoManager : MonoBehaviour
{

    const int massWidth = 6;    // ステージ横幅
    const int massHeight = 11;  // ステージ高さ

    [SerializeField]
    GameObject[,] puyoesOnMasses = new GameObject[massWidth,11];    // ぷよマスのステータス

    [SerializeField]
    GameObject[] puyo;      // 生成するぷよPrefab

    [SerializeField]
    Transform firstMassPos; // 左上のマス

    // 消すぷよの情報を入れる構造体とリスト
    struct VanishPuyoState {
        public Vector2 pos;
        public GameObject puyoObj;
    }
    List<VanishPuyoState> li_vanishPuyo;    // わからせ構造体リスト
    List<Vector2Int> li_searchedPuyo;       // 既にみたぷよ

    // UI関連
    [SerializeField] Text scoreText;        // スコアテキスト
    int score = 0;
    [SerializeField] Text timerText;        // タイマーテキスト
    float timer = 99;

    [SerializeField]
    private UnityEngine.UI.Text puyoPosText;    // デバッグ用のText
    [SerializeField]
    private UnityEngine.UI.Text arrayText;      // デバッグ用のText

    void Start()
    {

        for(int x = 0;x < 6; x++)
        {
            for(int y = 0; y < 11; y++)
            {
                //puyoMasses[x,y] = 0;
                puyoesOnMasses[x,y] = null;
            }
        }
    }

    
    void Update()
    {
        // 操作中のペアぷよが落ち終えたら かつ 連鎖がなかったら
        var puyos = GameObject.FindObjectOfType<PairPuyoMover>();
        if (!puyos)
        {
            MakePairPuyo();
        }
        else {

            // 落下中のペアぷよ更新
            puyoesOnMasses = puyos.PairPuyoUpdate(puyoesOnMasses);

            // ペアぷよの座標デバッグ表示
            {
                puyoPosText.text = "ぷよA: " + puyos.puyoesPos[0] +
                        "\nぷよB: " + puyos.puyoesPos[1];
            }
        }

        // 離別ぷよ落下
        var divorcedPuyos = GameObject.FindObjectsOfType<PuyoFall>();
        foreach (PuyoFall p in divorcedPuyos) {
            puyoesOnMasses = p.FallDown(puyoesOnMasses);
        }

        // ペアぷよの座標デバッグ表示
        {
            arrayText.text = "";

            for (int y = 0; y < 11; y++) {
                for(int x = 0; x < 6; x++)
                {
                    if (puyoesOnMasses[x, y] != null)
                        arrayText.text += puyoesOnMasses[x, y].GetComponent<PuyoTypeInfo>().puyoType;
                    else
                        arrayText.text += "+";
                }

                arrayText.text += "\n";
            }
        }

        // ぷよ消す
        SearchField();

        // スコア表示更新
        UpdateScore();

        // タイマーカウントダウン
        TimerCountDown();
    }

    // 縦にふたつ並んだぷよを作成
    void MakePairPuyo() {

        int line = UnityEngine.Random.Range(0, massWidth);  // ランダムな行を決める

        // 親オブジェクト
        GameObject parent = new GameObject();
        PairPuyoMover s_PairPuyoFall = parent.AddComponent<PairPuyoMover>();

        // 子オブジェクトととしてぷよ二つ
        for (int i = 0; i < 2; i++) {
            int num = UnityEngine.Random.Range(0, 6);   // ランダムに種類分け
            Vector2 pos = CalcLinePos(line);            // 生成時のx座標を更新
            pos.y += 0.85f * i;                         // ぷよAとBで並ばせる
            GameObject spawnedPuyo = Instantiate(puyo[num], pos, Quaternion.identity);  // 生成
            spawnedPuyo.transform.parent = parent.transform;    // ぷよAとBの親を一つにまとめる
            spawnedPuyo.GetComponent<PuyoTypeInfo>().puyoType = num;    // ぷよに自分を分からせる

            // 
            s_PairPuyoFall.puyoesBody[i] = spawnedPuyo;
        }

        s_PairPuyoFall.Init(line);
    }

    // ランダムなラインにぷよを落とし、その座標を計算
    Vector2 CalcLinePos(int lineNum) {

        Vector2 calcedLinePos = firstMassPos.transform.position;
        calcedLinePos.x += 0.85f * lineNum;

        return calcedLinePos;
    }

    // ぷよ探索
    void SearchField() {

        for(int x = 0;x < massWidth; x++)
        {
            for (int y = 0; y < massHeight; y++) {

                if (puyoesOnMasses[x, y] != null)
                {
                    li_vanishPuyo = new List<VanishPuyoState>();
                    li_searchedPuyo = new List<Vector2Int>();

                    CountSamePuyo(puyoesOnMasses[x, y].GetComponent<PuyoTypeInfo>().puyoType, x, y);
                    if (li_vanishPuyo.Count >= 4) VanishPuyoes();
                }
            }
        }
    }

    // 同じ色のぷよの数を数える
    void CountSamePuyo(int typeNum, int x, int y) {

        // 同じ色じゃなければ戻る
        if (puyoesOnMasses[x, y].GetComponent<PuyoTypeInfo>().puyoType != typeNum) return;

        // 一度探索した場所なら戻る
        foreach (Vector2Int pos in li_searchedPuyo) {
            if (new Vector2(x, y) == pos) return;
        }

        VanishPuyoState vps;                // 構造体の生成
        vps.pos = new Vector2(x, y);        // 今見てるぷよの座標
        vps.puyoObj = puyoesOnMasses[x, y]; // 今みてるぷよ
        li_vanishPuyo.Add(vps);             // リストに追加
        li_searchedPuyo.Add(new Vector2Int(x,y));   // すでに見た座標として登録

        if (puyoesOnMasses[x, y - 1] != null)  // 上
        {
            if(puyoesOnMasses[x, y - 1].GetComponent<PuyoTypeInfo>().puyoType == typeNum)
                CountSamePuyo(typeNum, x, y - 1);
        }

        if (x < 5 && puyoesOnMasses[x + 1, y] != null)  // 右
        {
            if(puyoesOnMasses[x + 1, y].GetComponent<PuyoTypeInfo>().puyoType == typeNum)
                CountSamePuyo(typeNum, x + 1, y);
        }

        if (y < 10 && puyoesOnMasses[x, y + 1] != null)  // 下
        {
            if (puyoesOnMasses[x, y + 1].GetComponent<PuyoTypeInfo>().puyoType == typeNum)
                CountSamePuyo(typeNum, x, y + 1);
        }

        if (x > 0 && puyoesOnMasses[x - 1, y] != null)  // 左
        {
            if(puyoesOnMasses[x - 1, y].GetComponent<PuyoTypeInfo>().puyoType == typeNum)
                CountSamePuyo(typeNum, x - 1, y);
        }
    }

    // 渡されたリストに入ってるぷよを消す
    void VanishPuyoes()
    {
        // リストに入ってるぷよを消す
        foreach (var p in li_vanishPuyo) {
            Destroy(p.puyoObj);
            puyoesOnMasses[(int)p.pos.x, (int)p.pos.y] = null;
        }

        // スコア加算
        score += 400;
    }

    /* ==== タイマー ==== */
    void TimerCountDown() {
        timer -= Time.deltaTime;
        timerText.text = timer.ToString("00");
    }

    /* ==== スコア ==== */
    void UpdateScore() {
        scoreText.text = score.ToString("00000");
    }
}
