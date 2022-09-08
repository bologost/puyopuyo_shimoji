using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/* ぷよ達のマス目管理と生成 */

public class PuyoManager : MonoBehaviour
{

    const int massWidth = 6;    // ステージ横幅
    const int massHeight = 11;  // ステージ高さ

    // フェイズunum
    enum Phase {
        MAKE,
        PAIR_FALL,
        SOLO_FALL,
        VANISH,
        WAIT,
        OVER
    }
    [SerializeField] Phase _phase;  // フェイズ
    bool reFallFlag = false;        // もう一度落ちるフラグ

    [SerializeField]
    GameObject[,] puyoesOnMasses = new GameObject[massWidth,11];    // ぷよマスのステータス

    [SerializeField] GameObject[] puyo;      // 生成するぷよPrefab

    [SerializeField] Transform firstMassPos; // 左上のマス

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
    [SerializeField] GameObject chainText;  // 連鎖テキスト
    int chain = 0;
    int bestChain = 0;

    // すくリプt
    [SerializeField] GameManager s_GameManager;

    [SerializeField] Text puyoPosText;    // デバッグ用のText
    [SerializeField] Text arrayText;      // デバッグ用のText

    void Start()
    {

        for(int x = 0;x < 6; x++)
        {
            for(int y = 0; y < 11; y++)
            {
                puyoesOnMasses[x,y] = null;
            }
        }

        // 最初のフェイズをPairFallに
        _phase = Phase.MAKE;
    }

    
    void Update()
    {
        // フェイズに応じて各処理をする
        switch (_phase){
            case Phase.MAKE:        MakePairPuyo(); break;  // ペアぷよを生成
            case Phase.PAIR_FALL:   PairFall();     break;  // ペアで落下
            case Phase.SOLO_FALL:   SoloFall();     break;  // 解散後の個別処理
            case Phase.VANISH:      SearchField();  break;  // そろったぷよを消す
            case Phase.WAIT:        WaitMode();     break;  // そろったぷよを消す
        }


        // スコア表示更新
        UpdateScore();

        // タイマーカウントダウン
        TimerCountDown();
    }

    /* ========== フェイズ関数 ==========*/

    // 縦にふたつ並んだぷよを作成
    void MakePairPuyo()
    {

        int line = UnityEngine.Random.Range(0, massWidth);  // ランダムな行を決める

        // 親オブジェクト
        GameObject parent = new GameObject();
        PairPuyoMover s_PairPuyoFall = parent.AddComponent<PairPuyoMover>();

        // 子オブジェクトととしてぷよ二つ
        for (int i = 0; i < 2; i++)
        {
            int num = UnityEngine.Random.Range(0, 6);   // ランダムに種類分け
            Vector2 pos = CalcLinePos(line);            // 生成時のx座標を更新
            pos.y += 0.85f * i;                         // ぷよAとBで並ばせる
            GameObject spawnedPuyo = Instantiate(puyo[num], pos, Quaternion.identity);  // 生成
            spawnedPuyo.transform.parent = parent.transform;            // ぷよAとBの親を一つにまとめる
            spawnedPuyo.GetComponent<PuyoTypeInfo>().puyoType = num;    // ぷよに自分を分からせる

            // 
            s_PairPuyoFall.puyoesBody[i] = spawnedPuyo;
        }

        s_PairPuyoFall.Init(line);

        // ぺあぷよ落下フェイズへ
        _phase = Phase.PAIR_FALL;
    }

    //　ペアぷよ落下
    void PairFall() {   // ペアで落下

        // 操作中のペアぷよが落ち終えたら かつ 連鎖がなかったら
        var puyos = GameObject.FindObjectOfType<PairPuyoMover>();
        if (!puyos)
        {
            // 一番上のラインにぷよがのったら終わり
            for (int i = 0; i < massWidth; i++)
            {
                if (puyoesOnMasses[i, 0] != null)
                {
                    _phase = Phase.OVER;
                    StartCoroutine("GameOverAnim");
                    return;
                }
            }

            _phase = Phase.SOLO_FALL;   // 個別ぷよ落下フェイズへ以降
        }
        else
        {

            // 落下中のペアぷよ更新
            puyoesOnMasses = puyos.PairPuyoUpdate(puyoesOnMasses);

            // ペアぷよの座標デバッグ表示
            {
                puyoPosText.text = "ぷよA: " + puyos.puyoesPos[0] +
                        "\nぷよB: " + puyos.puyoesPos[1];
            }
        }


        // ペアぷよの座標デバッグ表示
        {
            arrayText.text = "";

            for (int y = 0; y < 11; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    if (puyoesOnMasses[x, y] != null)
                        arrayText.text += puyoesOnMasses[x, y].GetComponent<PuyoTypeInfo>().puyoType;
                    else
                        arrayText.text += "+";
                }

                arrayText.text += "\n";
            }
        }

    }

    // 個別ぷよ落下
    void SoloFall() {   // 個別で落下

        // 落下かのうなぷよが無かったら次のフェイズへ
        if (CheckAnyFallablePuyo() == false)
        {
            _phase = Phase.VANISH;
            return;
        }

        var divorcedPuyos = GameObject.FindObjectsOfType<PuyoFall>();

        // 個別ぷよ落下
        foreach (PuyoFall p in divorcedPuyos){

            puyoesOnMasses = p.FallDown(puyoesOnMasses);
        }

    }

    // ぷよ探索
    void SearchField()
    {
        li_searchedPuyo = new List<Vector2Int>();

        // ステージ全体の中から同じ色のぷよを探す
        for (int x = 0; x < massWidth; x++)
        {
            for (int y = 0; y < massHeight; y++)
            {

                if (puyoesOnMasses[x, y] != null)
                {
                    li_vanishPuyo = new List<VanishPuyoState>();

                    CountSamePuyo(puyoesOnMasses[x, y].GetComponent<PuyoTypeInfo>().puyoType, x, y);
                    if (li_vanishPuyo.Count >= 4)
                    {
                        VanishPuyoes();
                        reFallFlag = true;

                        // スコア加算
                        score += 400 + chain * 200;
                        chain++;

                        // チェインテキストを出す
                        GameObject chainTextObj = Instantiate(chainText);
                        Vector2 textPos = li_vanishPuyo[0].puyoObj.transform.position;
                        chainTextObj.GetComponent<FloatingChainText>().StartFloating(textPos, chain);
                    }
                }
            }
        }

        // 待機フェイズに以降
        _phase = Phase.WAIT;
    }

    //待機
    void WaitMode() {

        var divorcedPuyos = GameObject.FindObjectsOfType<PuyoAnimation>();

        int vanishingNum = 0;


        foreach (var p in divorcedPuyos)
        {
            if (p.GetIsVanished())
            {
                vanishingNum++;
            }
        }

        // 削除アニメーションが終了したら
        if (vanishingNum == 0) {

            if (reFallFlag) _phase = Phase.SOLO_FALL;
            else
            {
                _phase = Phase.MAKE;
                if (chain > bestChain) bestChain = chain;
                chain = 0;
            }

            reFallFlag = false;
        }
    }


    /* ========== 内部処理 ==========*/

    // 落下可能なぷよがあるかチェックする
    bool CheckAnyFallablePuyo() {

        bool movedSomePuyo = false;

        // 落ちれるぷよはあるか判別
        foreach (PuyoFall p in GameObject.FindObjectsOfType<PuyoFall>())
        {

            if (p.CheckFallable(puyoesOnMasses)) movedSomePuyo = true;
        }

        return movedSomePuyo;
    }

    // ランダムなラインにぷよを落とし、その座標を計算
    Vector2 CalcLinePos(int lineNum) {

        Vector2 calcedLinePos = firstMassPos.transform.position;
        calcedLinePos.x += 0.85f * lineNum;

        return calcedLinePos;
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

        if (y > 0 && puyoesOnMasses[x, y - 1] != null)  // 上
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
            p.puyoObj.GetComponent<PuyoAnimation>().StartVanish();
        }
    }


    /* ==== タイマー ==== */
    void TimerCountDown() {
        timer -= Time.deltaTime;

        // タイムアップ
        if (timer <= 0)
        {
            timer = 0f;
            s_GameManager.ShowReslut(score, bestChain);
            Destroy(gameObject);
        }

        // テキストUIに反映
        timerText.text = timer.ToString("00");
    }

    /* ==== スコア ==== */
    void UpdateScore() {
        scoreText.text = score.ToString();
    }



    /* ========== ゲームオーバー ==========*/
    IEnumerator GameOverAnim() {
        float time = 3f;

        // ペアぷよはけす
        var pairPuyo = GameObject.FindObjectOfType<PairPuyoMover>();
        if(pairPuyo) Destroy(pairPuyo.gameObject);

        while (time > 0) {
            time -= Time.deltaTime;


            // 全ての個別ぷよを下におろす
            foreach (var o in GameObject.FindObjectsOfType<PuyoFall>()) {
                if (o == null) continue;
                o.transform.position += Vector3.down * 0.2f;
            }

            yield return null;
        }

        // リザルト表示
        s_GameManager.ShowReslut(score, bestChain);
        Destroy(this.gameObject);

    }
}
