using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ぷよの落下すくりぷと */

public class PuyoFall : MonoBehaviour
{
    private Transform tf_Puyo;  // ぷよのTransform
    private Vector2 currentPos; // ぷよの現在座標
    
    [SerializeField]
    private float puyoHeight;   // ぷよの高さ

    [SerializeField]
    private int fallInterval;   // 落下の間隔
    private int intervalCount;  // 次の落下までのカウント

    public int currentMassX;    // 現在のマスx
    public int currentMassY;    // 現在のマスy

    void Start()
    {
        tf_Puyo = this.transform;       // 
        intervalCount = fallInterval;   // 
    }

    
    void Update()
    {

        // インターバルのカウントの呼び出し
        if (!CountToNextFall()) return;

        // 下に落下
        FallDown();
    }

    // インターバルのカウント
    bool CountToNextFall() {

        // カウントが０じゃなければ
        if (intervalCount != 0) {

            // カウントを減らす
            intervalCount--;

            // カウントが０以下になったら
            if (intervalCount <= 0) {
                intervalCount = fallInterval;  // カウントを初期化
                return true;
            }
        }

        // カウントが0以外ならfalseを返す
        return false;
    }

    // 現在の座標より一段下に移動
    void FallDown() {

        // 現在地の取得
        currentPos = tf_Puyo.position;

        // ぷよの高さ分だけ、下にずらす
        currentPos.y -= puyoHeight;

        // 座標の繁栄
        tf_Puyo.position = currentPos;
    }
}
