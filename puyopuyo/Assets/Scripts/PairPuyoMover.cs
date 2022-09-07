using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PairPuyoMover : MonoBehaviour
{

    // 内包ぷよの情報
    public Vector2[] puyoesPos = new Vector2[2];        // ぷよの座標
    public GameObject[] puyoesBody = new GameObject[2]; // ぷよのオブジェクト情報

    // ぷよ落下カウントダウン
    private const int fallInterval = 30;        // 落下の間隔
    private int intervalCount = fallInterval;   // 次の落下までのカウント

    // 位置関係enum
    enum TwoPosition { 
        UP,
        RIGHT,
        LOW,
        Left
    }
    [SerializeField] TwoPosition twoPos = TwoPosition.UP;


    // 初期座標を渡す
    public void Init(int lineNum)
    {
        puyoesPos[0] = new Vector2(lineNum, 0);
        puyoesPos[1] = new Vector2(lineNum, -1);
    }

    // ぺあぷよ更新関数
    public GameObject[,] PairPuyoUpdate(GameObject[,] massState) {

        // 一段落ちる処理
        FallDown();

        // 回転の処理
        PuyoRotate(massState);

        // 矢印キーで操作
        PuyoMove(massState);

        // 着地チェッカー
        massState = PuyoLandingCheck(massState);

        // 最新の状態を返す
        return massState;
    }

    // ペアで下に落ちる
    void FallDown() {

        if (!CountToNextFall()) return;

        // 1ライン落下する
        for (int i = 0; i < 2; i++) {
            puyoesPos[i].y += 1;
            puyoesBody[i].transform.position += Vector3.down * 0.85f;
        }
        
    }

    // ペアで移動する
    void PuyoMove(GameObject[,] massState) {

        if (Input.GetKey(KeyCode.S)) {  // Sキーで加速
            PuyoSlide(massState, slideX: 0, slideY: 1);
        }
        if (Input.GetKeyDown(KeyCode.A)) {  // Aキーで1マス左に
            PuyoSlide(massState, slideX: -1, slideY: 0);
        }
        if (Input.GetKeyDown(KeyCode.D)) {  // Dキーで1マス右に
            PuyoSlide(massState, slideX: 1, slideY: 0);
        }
    }

    // 内包ぷよを移動させる
    void PuyoSlide(GameObject[,] massState, int slideX, int slideY) {

        Vector2[] posKeeper = new Vector2[2];   // 移動前の位置保存用
        bool moveable = true;                   // 移動可能フラグを立てる

        for (int i = 0; i < 2; i++) {
            posKeeper[i] = puyoesPos[i];                    // 移動前の位置を保存
            puyoesPos[i] += new Vector2(slideX, slideY);    // 移動

            if (puyoesPos[i].y < 0) continue;

            // ステージ外or既ぷよで移動を打ち消す
            if (puyoesPos[i].x < 0 || puyoesPos[i].x > 5)   // ステージ幅を超えたら戻す
            {
                moveable = false;
            }
            else if (massState[(int)puyoesPos[i].x, (int)puyoesPos[i].y] != null)// 既にぷよがいるなら
            {
                moveable = false;
            }
        }

        // 移動可能フラグがおられなければ移動
        for (int i = 0; i < 2; i++)
        {
            if (moveable)
            {

                puyoesBody[i].transform.position += new Vector3(slideX, -slideY, 0) * 0.85f;
            }
            else
            {

                puyoesPos[i] = posKeeper[i];
            }
        }

    }

    // ペアで回る (時計周り)
    void PuyoRotate(GameObject[,] massState) {

        // スペースキーを押すと
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        // 両隣が壁もしくはぷよだったら回転できない
        int hitCount = 0;
        if (puyoesPos[0].x == 0 || puyoesPos[0].x == 5) hitCount++;
        if (puyoesPos[0].x > 0 && massState[(int)puyoesPos[0].x - 1, (int)puyoesPos[0].y] != null) hitCount++;
        if (puyoesPos[0].x < 5 && massState[(int)puyoesPos[0].x + 1, (int)puyoesPos[0].y]) hitCount++;
        if (hitCount >= 2) return;


        // 衝突フラグ
        bool hitFlag = false;

        // 差分の計算用に移動前の座標を保持
        Vector2[] posKeeper = new Vector2[puyoesPos.Length];
        Array.Copy(puyoesPos, posKeeper, puyoesPos.Length);


        
        // 位置関係State更新
        switch (twoPos) {
            case TwoPosition.UP:    ChangePuyoState(1, 0, TwoPosition.RIGHT);   break;
            case TwoPosition.RIGHT: ChangePuyoState(0, 1, TwoPosition.LOW);     break;
            case TwoPosition.LOW:   ChangePuyoState(-1, 0, TwoPosition.Left);   break;
            case TwoPosition.Left:  ChangePuyoState(0, -1, TwoPosition.UP);     break;
        }
        

        if(puyoesPos[1].x < 0 || puyoesPos[1].x > 5) {   // 壁と衝突していないかチェック

            hitFlag = true;
        }
        else if (massState[(int)puyoesPos[1].x, (int)puyoesPos[1].y] != null) { // ぷよと衝突してないかチェック

            hitFlag = true;
        }

        /* 衝突していたらずらす */
        if (hitFlag)
        {

            switch (twoPos)
            {
                case TwoPosition.Left:
                    puyoesPos[0].x += 1;
                    puyoesPos[1].x += 1;
                    break;

                case TwoPosition.RIGHT:
                    puyoesPos[0].x -= 1;
                    puyoesPos[1].x -= 1;
                    break;
            }
        }

        // 移動差分を求める
        Vector2 diffA = new Vector2(posKeeper[0].x - puyoesPos[0].x, puyoesPos[0].y - posKeeper[0].y);
        Vector2 diffB = new Vector2(posKeeper[1].x - puyoesPos[1].x,puyoesPos[1].y - posKeeper[1].y);

        // 座標の更新
        puyoesBody[0].transform.position = puyoesBody[0].transform.position - (Vector3)diffA * 0.85f;
        puyoesBody[1].transform.position = puyoesBody[1].transform.position - (Vector3)diffB * 0.85f;
    }

    // ぷよBの位置関係Stateを変更
    void ChangePuyoState(int nextX, int nextY, TwoPosition nextState) {
        puyoesPos[1] = new Vector2(puyoesPos[0].x + nextX, puyoesPos[0].y + nextY);
        twoPos = nextState;
    }

    // ぷよの着地チェック
    GameObject[,] PuyoLandingCheck(GameObject[,] massState) {

        // 着地判定
        for (int i = 0; i < 2; i++)
        {
            if ((int)puyoesPos[i].x <= -1 || (int)puyoesPos[i].y <= -1) continue;

            // 一番下のライン
            if(puyoesPos[i].y >= 10){

                puyoesPos[i].y = 10;
                massState = Dissolusion(massState);
                break;
            } 
            else if (massState[(int)puyoesPos[i].x, (int)puyoesPos[i].y + 1] != null)  // 一つ下にぷよ
            {
                massState = Dissolusion(massState);
                break;
            }

        }

        return massState;
    }

    // 着地したときにペア解散
    GameObject[,] Dissolusion(GameObject[,] massState) {

        // ぷよがHierarchyに散在しないための空オブジェクト
        Transform g = GameObject.Find("PuyoColony").transform;

        // 親オブジェクトから切り離す (この時、y<0であればゲームオーバー)
        for (int i = 0; i < 2; i++) {

            massState[(int)puyoesPos[i].x, (int)puyoesPos[i].y] = puyoesBody[i];    // 解散後に配列に格納
            puyoesBody[i].transform.parent = g;                         // 親を変える
            var s_PuyoFall = puyoesBody[i].AddComponent<PuyoFall>();    // ぷよが自動で落ちるスクリプト
            s_PuyoFall.SetCurrentMass(puyoesPos[i]);                    // 現在の位置を渡してあげる
        }

        Destroy(this.gameObject);

        return massState;
    }

    /* ==== 落下インターバルのカウントダウン ==== */
    bool CountToNextFall()
    {

        // カウントが０じゃなければ
        if (intervalCount != 0)
        {

            // カウントを減らす
            intervalCount--;

            // カウントが０以下になったら
            if (intervalCount <= 0)
            {
                intervalCount = fallInterval;  // カウントを初期化
                return true;
            }
        }

        // カウントが0以外ならfalseを返す
        return false;
    }
}
