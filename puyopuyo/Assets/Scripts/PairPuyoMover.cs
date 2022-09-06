using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        PuyoRotate();

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
        for (int i = 0; i < 2; i++) {
            Vector2 posKeeper = puyoesPos[i];               // 移動前の位置を保存
            puyoesPos[i] += new Vector2(slideX, slideY);    // 移動

            // ステージ外or既ぷよで移動を打ち消す
            if (puyoesPos[i].x < 0 || puyoesPos[i].x > 5)   // ステージ幅を超えたら戻す
            {
                puyoesPos[i] = posKeeper;
                break;
            }
            else if (massState[(int)puyoesPos[i].x, (int)puyoesPos[i].y] != null)// 既にぷよがいるなら
            {
                puyoesPos[i] = posKeeper;
                break;
            }
            else
                puyoesBody[i].transform.position += new Vector3(slideX, -slideY, 0) * 0.85f;
        }
    }

    // ペアで回る (時計周り)
    void PuyoRotate() {

        // スペースキーを押すと
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        // 変更前の値を保持

        // 位置関係State更新
        switch (twoPos) {
            case TwoPosition.UP:    ChangePuyoState(1, 0, TwoPosition.RIGHT);   break;
            case TwoPosition.RIGHT: ChangePuyoState(0, 1, TwoPosition.LOW);     break;
            case TwoPosition.LOW:   ChangePuyoState(-1, 0, TwoPosition.Left);   break;
            case TwoPosition.Left:  ChangePuyoState(0, -1, TwoPosition.UP);     break;
        }

        // 座標の更新
        Vector2 newPos = puyoesBody[0].transform.position;
        newPos.x += (puyoesPos[1].x - puyoesPos[0].x) * 0.85f;
        newPos.y += (puyoesPos[0].y - puyoesPos[1].y) * 0.85f;
        puyoesBody[1].transform.position = newPos;
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

            // ぷよの着地判定
            if (puyoesPos[i].y >= 10 ||                                         // 一番下のライン
                massState[(int)puyoesPos[i].x, (int)puyoesPos[i].y + 1] != null)  // 一つ下にぷよ
            {
                massState = Dissolusion(massState);
            }

        }

        return massState;
    }

    // 着地したときにペア解散
    GameObject[,] Dissolusion(GameObject[,] massState) {

        // ぷよがHierarchyに散在しないための空オブジェクト
        Transform g = GameObject.Find("PuyoColony").transform;

        // 親オブジェクトから切り離す
        for (int i = 0; i < 2; i++) {
            massState[(int)puyoesPos[i].x, (int)puyoesPos[i].y] = puyoesBody[i];    // 解散した位置を1に
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
