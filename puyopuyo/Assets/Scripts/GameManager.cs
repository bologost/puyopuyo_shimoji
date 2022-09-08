using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ゲームの設定など */

public class GameManager : MonoBehaviour
{
    private int frameRate = 60;     // フレームレート

    
    void Start()
    {
        Application.targetFrameRate = frameRate;    // フレームレートを60に固定
    }


    // ゲームが終了したときに、リザルトを表示 (最終スコアと最大チェイン数を引数でとる)
    public void ShowReslut(int score, int bestChain) { 
        
    }
}
