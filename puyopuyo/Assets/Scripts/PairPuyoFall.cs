using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PairPuyoFall : MonoBehaviour
{
    [SerializeField] Vector2[] puyoesPos = new Vector2[2];
    public GameObject[] puyoesBody = new GameObject[2];

    // 位置関係enum
    enum TwoPosition { 
        UP,
        RIGHT,
        LOW,
        Left
    }
    TwoPosition twoPos = TwoPosition.UP;

    public void Init(int lineNum)
    {
        puyoesPos[0].x = lineNum;
        puyoesPos[0].y = 0;
        puyoesPos[1].x = lineNum;
        puyoesPos[1].y = -1;
    }

    // ペアで下に落ちる
    public int[][] FallDown(int[][] massState) {

        // ペアで落下する
        for (int i = 0; i < 2; i++) {
            puyoesPos[i].y += 1;
            puyoesBody[i].transform.position += Vector3.down * 0.85f;
        }

        // 着地判定

        return massState;
    }

    // ペアで回る (時計周り)
    public void PuyoRotate() {

        switch (twoPos) {
            case TwoPosition.UP:
                puyoesPos[1] = new Vector2(puyoesPos[0].x + 1, puyoesPos[0].y);
                twoPos = TwoPosition.RIGHT;
                break;
            case TwoPosition.RIGHT:
                puyoesPos[1] = new Vector2(puyoesPos[0].x, puyoesPos[0].y - 1);
                twoPos = TwoPosition.LOW;
                break;
            case TwoPosition.LOW:
                puyoesPos[1] = new Vector2(puyoesPos[0].x - 1, puyoesPos[0].y);
                twoPos = TwoPosition.Left;
                break;
            case TwoPosition.Left:
                puyoesPos[1] = new Vector2(puyoesPos[0].x, puyoesPos[0].y + 1);
                twoPos = TwoPosition.UP;
                break;
        }

        Vector2 newPos = puyoesBody[0].transform.position;
        newPos.x += (puyoesPos[1].x - puyoesPos[0].x) * 0.85f;
        newPos.y += (puyoesPos[1].y - puyoesPos[0].y) * 0.85f;
        puyoesBody[1].transform.position = newPos;
    }


    // 着地したときにペア解散
    public void Dissolusion() {

        // 親オブジェクトから切り離す
        for (int i = 0; i < 2; i++) {
            puyoesBody[i].transform.parent = null;
        }
    }
}
