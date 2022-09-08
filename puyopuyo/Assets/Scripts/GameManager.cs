using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* ゲームの設定など */

public class GameManager : MonoBehaviour
{
    private int frameRate = 60;     // フレームレート

    [SerializeField] GameObject resultBoard;    // リザルトボード
    [SerializeField] Text scoreText;            // 最終スコアテキスト
    [SerializeField] Text bestChainText;        // 最大連鎖数てきすと　
    
    void Start()
    {
        Application.targetFrameRate = frameRate;    // フレームレートを60に固定
    }


    // ゲームが終了したときに、リザルトを表示 (最終スコアと最大チェイン数を引数でとる)
    public void ShowReslut(int score, int bestChain) {

        // リザルトボードを表示する
        resultBoard.SetActive(true);
        scoreText.text = score.ToString();
        bestChainText.text = bestChain.ToString();
    }
}
