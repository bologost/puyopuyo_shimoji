using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// タイトルのボタンから呼び出す関数をまとめたスクリプト

public class TitleButtonManager : MonoBehaviour
{

    // ボタンセレクト用のイベントシステム
    [SerializeField] EventSystem _EventSystem;

    // プレイガイド
    [SerializeField] GameObject playGuideBoard;


    // はじめるボタン
    public void StartGame() {
        SceneManager.LoadScene("GameScene");
    }

    // あそびかたボタン
    public void ShowPlayGuide() {
        playGuideBoard.SetActive(true);

        _EventSystem.enabled = false;
    }

    // おわるボタン
    public void EndGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
        Application.Quit();                             //ゲームプレイ終了
#endif
    }
}
