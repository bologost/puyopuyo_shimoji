using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingChainText : MonoBehaviour
{
    [SerializeField] Canvas _Canvas;                // キャンバス
    [SerializeField] RectTransform _RectTransform;  // 位置変更用Transform
    [SerializeField] Text countText;                // 連鎖数テキスト
    [SerializeField] Text rensaText;                // れんさ！テキスト

    Vector2 pos;

    private void Start()
    {
        _Canvas.worldCamera = GameObject.Find("PuzzleCamera").GetComponent<Camera>();
    }

    public void StartFloating(Vector2 _pos, int chain) {

        // 最初の座標を入れる
        pos = _pos;

        countText.text = chain.ToString();

        // 徐々に上に上がるコルーチンを呼び出す
        StartCoroutine("Floating");
    }

    IEnumerator Floating() {

        // カウント初期化
        const float countMax = 1.5f;    // 生存時間
        float count = countMax;         // 生存カウント

        Color oc = countText.color;     // 連鎖数テキストの元の色を保持
        Color oc2 = rensaText.color;    // れんさ！テキストの  〃

        // 座標の初期化
        _RectTransform.position = pos;

        while (count > 0f) {
            count -= Time.deltaTime;

            //徐々に透明にしていく
            float per = count / countMax;
            countText.color = new Color(oc.r, oc.g, oc.b, per);
            rensaText.color = new Color(oc2.r, oc2.g, oc2.b, per);

            // 徐々に上に上がる
            _RectTransform.position += Vector3.up * 0.01f;

            yield return null; 
        }

        Destroy(gameObject);
    }
}
