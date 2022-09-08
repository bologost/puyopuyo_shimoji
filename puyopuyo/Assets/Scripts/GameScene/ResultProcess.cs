using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// リザルトのボタンから呼び出される関数をまとめた

public class ResultProcess : MonoBehaviour
{

    [SerializeField] EventSystem _EventSystem;

    private void Update()
    {
        // EventSystemがアクティブじゃなければアクティブにする
        if (!_EventSystem.enabled) _EventSystem.enabled = true;
    }

    // リトライ
    public void Retry() {
        SceneManager.LoadScene("GameScene");
    }

    // タイトル
    public void Title() {
        SceneManager.LoadScene("TitleScene");
    }
}
