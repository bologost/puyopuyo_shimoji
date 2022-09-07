using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ぷよの落下すくりぷと */

public class PuyoFall : MonoBehaviour
{
    private const float puyoHeight = 0.85f;     // ぷよの高さ

    private Vector2 currentMass;                // 現在のマス

    public int puyoType = 0;                    // ぷよの種類 (0~5)

    private bool fallable = false;              // 落下が可能かフラグ

    private const float fallTime = 0.1f;
    private float fallCount = fallTime;


    // 離別時に現在のマスを渡される
    public void SetCurrentMass(Vector2 nowPos) {
        currentMass = nowPos;
    }

    // 下に空白があるかチェック
    public bool CheckFallable(GameObject[,] massState) {

        if ((currentMass.y < 10 && massState[(int)currentMass.x, (int)currentMass.y + 1] == null)) {
            fallable = true;
        }
        else {
            fallable = false;
        }

        return fallable;
    }

    // 現在の座標より一段下に移動
    public GameObject[,] FallDown(GameObject[,] massState) {

        if (FallCountDown() == false) return massState;

        int x = (int)currentMass.x;
        int y = (int)currentMass.y;

        // 今いるマスのStateを0に
        massState[x, y] = null;

        // 1マス下にずらす
        currentMass.y += 1;

        // ぷよの高さ分だけ、下にずらす
        transform.position += Vector3.down * puyoHeight;


        // 落ちた位置のマス状態を変更
        massState[(int)currentMass.x, (int)currentMass.y] = this.gameObject;

        return massState;
    }

    // 落ちるカウントダウン
    bool FallCountDown() {

        if (fallable) fallCount -= Time.deltaTime;  // カウントを減らす

        if (fallCount <= 0f) {
            fallCount = fallTime;
            return true;
        }

        return false;
    }
}
