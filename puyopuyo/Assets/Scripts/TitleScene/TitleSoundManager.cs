using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSoundManager : MonoBehaviour
{
    // SE
    [SerializeField] AudioListener _SoundListener;
    [SerializeField] AudioClip se_PressButton;      // ボタン押したときの音
    [SerializeField] AudioClip se_MoveCursor;       // ボタンカーソル移動時の音

    // BGM
    [SerializeField] AudioListener _BGMListener;
    [SerializeField] AudioClip bgm_Title;           // タイトルBGM

    private void Start()
    {

    }
}
