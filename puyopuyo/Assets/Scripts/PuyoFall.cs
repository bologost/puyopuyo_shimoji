using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ぷよの落下すくりぷと */

public class PuyoFall : MonoBehaviour
{
    private const float puyoHeight = 0.85f;     // ぷよの高さ

    private Vector2 currentMass;                // 現在のマス

    public int puyoType = 0;    // ぷよの種類 (0~5)


    // 離別時に現在のマスを渡される
    public void SetCurrentMass(Vector2 nowPos) {
        currentMass = nowPos;
    }

    // 現在の座標より一段下に移動
    public GameObject[,] FallDown(GameObject[,] massState) {

        int x = (int)currentMass.x;
        int y = (int)currentMass.y;

        // 下のマスにブロック判定 or 一番下のライン
        while ( (y < 10 && massState[x, y + 1] == null))
        {
            // 今いるマスのStateを0に
            massState[x, y] = null;

            // 1マス下にずらす
            currentMass.y += 1;

            // ぷよの高さ分だけ、下にずらす
            transform.position += Vector3.down * puyoHeight;

            x = (int)currentMass.x;
            y = (int)currentMass.y;

            // 落ちた位置のマス状態を変更
            massState[x, y] = this.gameObject;
        }

        return massState;
    }
}
