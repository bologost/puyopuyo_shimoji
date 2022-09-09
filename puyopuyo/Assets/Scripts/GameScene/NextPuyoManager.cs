using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextPuyoManager : MonoBehaviour
{

    [SerializeField] GameObject[] nextPuyo;

    // 次のぷよの設定
    public void SetNextPuyo(int num, GameObject _nextPuyo) {

        _nextPuyo.transform.parent = this.transform;                         // 新ぷよの親になる
        _nextPuyo.transform.position = nextPuyo[num].transform.position;     // 位置を同期
        _nextPuyo.transform.localScale = nextPuyo[num].transform.localScale; // 大きさを同期
        Destroy(nextPuyo[num]);                                              // 前のぷよ削除
        nextPuyo[num] = _nextPuyo;                                           // 次のぷよをセット
    }

    // 指定のぷよの情報を取得
    public int GetPuyoType(int num) {
        return nextPuyo[num].GetComponent<PuyoTypeInfo>().puyoType;
    }
}
