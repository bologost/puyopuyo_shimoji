using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PairPuyoMover : MonoBehaviour
{
    // 落とす位置の候補マス情報を格納する
    [Serializable] struct FallSpotList {
        public Vector2Int spotPos;  // 座標
        public int angle;           // その時の向き (angle数回だけ回転する)
        public int cost;            // 評価 (高さ、同じ色の数で判定)
    }
    [SerializeField] List<FallSpotList> li_fallSpots;
    bool isDecidedSpot = false;     // 落とす場所が決まったかどうか
    Vector2 fallSpot;               // 落とす場所
    int rotNum;                     // 回転する回数
    bool isReached = false;         // その真上に到達したかどうか

    int sameCount = 0;              // 同じ色のカウント
    List<Vector2Int> li_searchedPuyo;       // 既にみたぷよ

    // 内包ぷよの情報
    public Vector2[] puyoesPos = new Vector2[2];        // ぷよの座標
    public GameObject[] puyoesBody = new GameObject[2]; // ぷよのオブジェクト情報

    // ぷよ落下カウントダウン
    private const int fallInterval = 30;        // 落下の間隔
    private int intervalCount = fallInterval;   // 次の落下までのカウント

    public bool comMode = false;                // コンピュータモード

    public float thinkTime = 0.25f;       // AIの一回の行動当たり間隔
    private float thinkTimeCount;   // 〃のカウント


    // 位置関係enum
    enum TwoPosition { 
        UP,
        RIGHT,
        LOW,
        Left
    }
    [SerializeField] TwoPosition twoPos = TwoPosition.UP;

    // ぷよ定数
    const float puyoScale = 0.85f;
    const int puyoNum = 2;

    // 初期座標を渡す
    public void Init(int lineNum)
    {
        puyoesPos[0] = new Vector2(lineNum, 0);
        puyoesPos[1] = new Vector2(lineNum, -1);
        thinkTimeCount = thinkTime;
    }

    // ぺあぷよ更新関数
    public GameObject[,] PairPuyoUpdate(GameObject[,] massState) {

        // 一段落ちる処理
        FallDown();

        /* ============= 人間が行う操作 ============= */

        if (!comMode)
        {
            // 矢印キーで操作
            PuyoMove(massState);

            // 回転の処理
            PuyoRotate(massState);
        }


        /* ============= AIが行う操作 ============= */

        if (comMode)
        {

            // 落とせる場所探す
            if (!isDecidedSpot)
            {
                SearchPuyoSpot(massState);

                // 落とす場所を決める
                DecideFallSpot(massState);
            }
            else
            {

                // 決めた場所に向かう
                GoSpot(massState);
            }

            // 決めた場所の上に来たら
            if (isReached) FullAccelFall(massState);

        }


        /* ======================================== */

        // 着地チェッカー
        massState = PuyoLandingCheck(massState);


        // 最新の状態を返す
        return massState;
    }

    /* =============== AIが行っている関数 =============== */

    // 落とせるスポットを探す
    void SearchPuyoSpot(GameObject[,] massState) {

        // 落とす位置候補配列
        li_fallSpots = new List<FallSpotList>();

        // 落とせる位置を探して配列に入れとく
        for(int x = 0; x < massState.GetLength(0); x++) {
            for (int y = 0; y < massState.GetLength(1); y++) {

                FallSpotList _fallSpotList;
                _fallSpotList.spotPos = new Vector2Int(x, y);   // 
                _fallSpotList.angle = 0;                        // 角度の初期値は回転無し
                _fallSpotList.cost = y;                         // コストの初期値は高さyを入れる

                // ぷよ無し　したにぷよあり
                if (massState[x, y] == null && y < 10 && massState[x, y + 1]) {
                    li_fallSpots.Add(_fallSpotList);
                }

                // ぷよ無し　一番下のライン
                if (massState[x, y] == null && y == 10)
                {
                    li_fallSpots.Add(_fallSpotList);
                }

            }
        }

    }

    // 探した候補の中から落とす場所を決める
    void DecideFallSpot(GameObject[,] massState) {

        // コスト比較用
        int highestCost = 0;

        // 同じ色が多い所を見つけてコストを上げていく (あと回転)
        for(int i=0;i<li_fallSpots.Count;i++)
        {
            FallSpotList temp = li_fallSpots[i];

            li_searchedPuyo = new List<Vector2Int>();
            int patternA = CountSamePuyo(massState, puyoesBody[0].GetComponent<PuyoTypeInfo>().puyoType, li_fallSpots[i].spotPos.x, li_fallSpots[i].spotPos.y);
            li_searchedPuyo = new List<Vector2Int>();
            int patternB = CountSamePuyo(massState, puyoesBody[1].GetComponent<PuyoTypeInfo>().puyoType, li_fallSpots[i].spotPos.x, li_fallSpots[i].spotPos.y);

            // 多いほうをコストに足す
            temp.cost += (patternA > patternB ? patternA : patternB) * 5;

            // Bが多ければ2回転する
            if (patternA < patternB) temp.angle = 2;

            li_fallSpots[i] = temp;
        }

        // 一番コストが高いところ
        foreach (var fp in li_fallSpots)
        {
            if(highestCost < fp.cost)
            {
                highestCost = fp.cost;
                fallSpot = fp.spotPos;
                rotNum = fp.angle;
            }
        }

        isDecidedSpot = true;
    }

    // 探した場所に向かっていく
    void GoSpot(GameObject[,] massState) {

        thinkTimeCount -= Time.deltaTime;

        if(thinkTimeCount <= 0f) {

            thinkTimeCount = thinkTime;

            // 理想の状態になるように回転する
            if (rotNum-- > 0)
            {
                PuyoRotate(massState);
                return;
            }

            // 予定の場所に1ライン近づく
            if (fallSpot.x > puyoesPos[0].x) { 
                PuyoSlide(massState, 1, 0);
                return;
            }
            if (fallSpot.x < puyoesPos[0].x) { 
                PuyoSlide(massState, -1, 0);
                return;
            }

            isReached = true;
        }
    }

    // 真上にきた後はひたすら加速する
    void FullAccelFall(GameObject[,] massState) {
        PuyoSlide(massState, slideX: 0, slideY: 1);
    }


    // 落とす場所候補マスの周辺を探索し、自分と同じ色の数をカウントする
    int CountSamePuyo(GameObject[,] massState, int typeNum, int x, int y)
    {
        int sameNum = 0;

        // 同じ色じゃなければ戻る
        if (massState[x, y] != null)
        {
            if (massState[x, y].GetComponent<PuyoTypeInfo>().puyoType != typeNum) return sameNum;


            // 一度探索した場所なら戻る
            foreach (Vector2Int pos in li_searchedPuyo)
            {
                if (new Vector2(x, y) == pos) return sameNum;
            }

            sameNum++;
            li_searchedPuyo.Add(new Vector2Int(x, y));   // すでに見た座標として登録
        }


        if (y > 0 && massState[x, y - 1] != null)  // 上
        {
            if (massState[x, y - 1].GetComponent<PuyoTypeInfo>().puyoType == typeNum)
                sameNum += CountSamePuyo(massState, typeNum, x, y - 1);
        }

        if (x < 5 && massState[x + 1, y] != null)  // 右
        {
            if (massState[x + 1, y].GetComponent<PuyoTypeInfo>().puyoType == typeNum)
                sameNum += CountSamePuyo(massState, typeNum, x + 1, y);
        }

        if (y < 10 && massState[x, y + 1] != null)  // 下
        {
            if (massState[x, y + 1].GetComponent<PuyoTypeInfo>().puyoType == typeNum)
                sameNum += CountSamePuyo(massState, typeNum, x, y + 1);
        }

        if (x > 0 && massState[x - 1, y] != null)  // 左
        {
            if (massState[x - 1, y].GetComponent<PuyoTypeInfo>().puyoType == typeNum)
                sameNum += CountSamePuyo(massState, typeNum, x - 1, y);
        }

        return sameNum;
    }

    /* =============== プレイヤーが行っている関数 =============== */

    // ペアで下に落ちる
    void FallDown() {

        if (!CountToNextFall()) return;

        // 1ライン落下する
        for (int i = 0; i < puyoNum; i++) {
            puyoesPos[i].y++;
            puyoesBody[i].transform.position += Vector3.down * puyoScale;
        }
        
    }

    // ペアで移動する
    void PuyoMove(GameObject[,] massState) {

        if (Input.GetKey(KeyCode.DownArrow)) {      // Sキーで加速
            PuyoSlide(massState, slideX: 0, slideY: 1);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {  // Aキーで1マス左に
            PuyoSlide(massState, slideX: -1, slideY: 0);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { // Dキーで1マス右に
            PuyoSlide(massState, slideX: 1, slideY: 0);
        }
    }

    // ペアで回る (時計周り)
    void PuyoRotate(GameObject[,] massState) {

        // スペースキーを押すと
        if (!Input.GetKeyDown(KeyCode.Space) && !comMode) return;


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
        puyoesBody[0].transform.position = puyoesBody[0].transform.position - (Vector3)diffA * puyoScale;
        puyoesBody[1].transform.position = puyoesBody[1].transform.position - (Vector3)diffB * puyoScale;
    }


    /* =============== ここから下はAI関係ない =============== */

    // 内包ぷよを移動させる
    void PuyoSlide(GameObject[,] massState, int slideX, int slideY)
    {

        Vector2[] posKeeper = new Vector2[2];   // 移動前の位置保存用
        bool moveable = true;                   // 移動可能フラグを立てる

        for (int i = 0; i < puyoNum; i++)
        {
            posKeeper[i] = puyoesPos[i];                    // 移動前の位置を保存
            puyoesPos[i] += new Vector2(slideX, slideY);    // 移動

            // ぷよが配列外ならチェックしない
            if (puyoesPos[i].y < 0) continue;

            // ステージ外or既ぷよで移動を打ち消す
            if (puyoesPos[i].x < 0 || puyoesPos[i].x > 5 || puyoesPos[i].y >= 11)   // ステージ幅を超えたら戻す
            {
                moveable = false;
            }
            else if (massState[(int)puyoesPos[i].x, (int)puyoesPos[i].y] != null)// 既にぷよがいるなら
            {
                moveable = false;
            }
        }

        // 移動可能フラグがおられなければ移動
        for (int i = 0; i < puyoNum; i++)
        {
            if (moveable)
            {

                puyoesBody[i].transform.position += new Vector3(slideX, -slideY, 0) * puyoScale;
            }
            else
            {

                puyoesPos[i] = posKeeper[i];
            }
        }

    }

    // ぷよBの位置関係Stateを変更
    void ChangePuyoState(int nextX, int nextY, TwoPosition nextState) {
        puyoesPos[1] = new Vector2(puyoesPos[0].x + nextX, puyoesPos[0].y + nextY);
        twoPos = nextState;
    }

    // ぷよの着地チェック
    GameObject[,] PuyoLandingCheck(GameObject[,] massState) {

        // 着地判定
        for (int i = 0; i < puyoNum; i++)
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
        for (int i = 0; i < puyoNum; i++) {

            // yがマイナスでなければ
            if (puyoesPos[i].y >= 0f)
            {

                massState[(int)puyoesPos[i].x, (int)puyoesPos[i].y] = puyoesBody[i];    // 解散後に配列に格納

                var s_PuyoFall = puyoesBody[i].AddComponent<PuyoFall>();    // ぷよが自動で落ちるスクリプト
                s_PuyoFall.SetCurrentMass(puyoesPos[i]);                    // 現在の位置を渡してあげる
                s_PuyoFall.comMode = comMode;
            }
            else {
                puyoesBody[i].GetComponent<SpriteRenderer>().color = Color.black;
            }

            puyoesBody[i].transform.parent = g;                             // 親を変える
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
