using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ぷよ達のマス目管理と生成 */

public class PuyoManager : MonoBehaviour
{

    const int massWidth = 6;                    // よこのマス幅
    int[,] puyoMasses = new int[massWidth,11];  // ぷよマスのステータス

    [SerializeField]
    GameObject[] puyo;  // 生成するぷよPrefab

    [SerializeField]
    Transform firstMassPos; // 左上のマス

    [SerializeField]
    private int fallInterval;   // 落下の間隔
    private int intervalCount;  // 次の落下までのカウント

    void Start()
    {
        intervalCount = fallInterval;   // 

        for(int x = 0;x < 6; x++)
        {
            for(int y = 0; y < 11; y++)
            {
                puyoMasses[x,y] = 0;
            }
        }
    }

    
    void Update()
    {
        // 操作中のぷよが落ち終えたら
        var puyos = GameObject.FindObjectsOfType<PairPuyoFall>();
        if (puyos.Length <= 0)
        {
            MakePairPuyo();

        }
        else {

            // インターバルのカウントの呼び出し
            if (CountToNextFall()) {

                foreach (var p in puyos)
                {
                    puyoMasses = p.GetComponent<PairPuyoFall>().FallDown(puyoMasses);
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 回転の処理
                foreach (var p in puyos)
                {
                    p.GetComponent<PairPuyoFall>().PuyoRotate();
                }
            }
        }

    }

    // 縦にふたつ並んだぷよを作成
    void MakePairPuyo() {

        int line = Random.Range(0, 5);  // ランダムな行を決める

        // 親オブジェクト
        GameObject parent = new GameObject();
        PairPuyoFall s_PairPuyoFall = parent.AddComponent<PairPuyoFall>();

        // 子オブジェクトととしてぷよ二つ
        for (int i = 0; i < 2; i++) {
            int num = Random.Range(0, 5);
            Vector2 pos = CalcLinePos(line);
            pos.y += 0.85f * i;
            GameObject spawnedPuyo = Instantiate(puyo[num],pos,Quaternion.identity);
            spawnedPuyo.transform.parent = parent.transform;

            //
            s_PairPuyoFall.puyoesBody[i] = spawnedPuyo;
        }

        s_PairPuyoFall.Init(line);
    }

    Vector2 CalcLinePos(int lineNum) {

        Vector2 calcedLinePos = firstMassPos.transform.position;
        calcedLinePos.x += 0.85f * lineNum;

        Debug.Log(lineNum);

        return calcedLinePos;
    }

    // インターバルのカウント
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
