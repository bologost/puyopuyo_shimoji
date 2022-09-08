using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultProcess : MonoBehaviour
{
    
    void Update()
    {
        // スペースキーでリトライ
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}
