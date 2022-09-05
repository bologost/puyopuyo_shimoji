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

    
    void Update()
    {
        
    }
}
