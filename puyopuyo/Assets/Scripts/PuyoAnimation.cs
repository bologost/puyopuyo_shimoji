using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuyoAnimation : MonoBehaviour
{
    private SpriteRenderer myRenderer;
    [SerializeField] Sprite[] twinkleImages;
    [SerializeField] Sprite[] vanishImages;

    const float animTime = 5f;
    float animCount = animTime;

    bool isVanished = false;

    private void Start()
    {
        myRenderer = this.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        CountDown();
    }

    // カウントを減らす
    void CountDown() {
        animCount -= Time.deltaTime;

        if (animCount <= 0) {
            StartCoroutine("TwinkleAnim");
        }
    }

    // アニメーションコルーチン
    IEnumerator TwinkleAnim() {

        float imgChangeCount = 0f;

        while (imgChangeCount < 5.0f)
        {
            myRenderer.sprite = twinkleImages[(int)imgChangeCount];

            imgChangeCount += Time.deltaTime * 10.0f;

            yield return null;
        }

        myRenderer.sprite = twinkleImages[0];

        // カウント初期化
        animCount = animTime;
    }


    /* ==== 外から呼び出す用 ====*/
    public void StartVanish() {

        // 消滅フラグを立てる
        isVanished = true;

        // 再生中のコルーチンを止める
        StopCoroutine(TwinkleAnim());

        // 消滅アニメーション再生
        StartCoroutine("VanishAnim");
        
    }

    // 消滅アニメーション
    IEnumerator VanishAnim() {
        float imgChangeCount = 0f;

        while (imgChangeCount < 7.0f)
        {
            myRenderer.sprite = vanishImages[(int)imgChangeCount];

            imgChangeCount += Time.deltaTime * 15.0f;

            yield return null;
        }

        Destroy(this.gameObject);

    }

    public bool GetIsVanished() {
        return isVanished;
    }
}
