using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TitlePlayGuidManager : MonoBehaviour
{
    [SerializeField] EventSystem _EventSystem;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            _EventSystem.enabled = true;

            this.gameObject.SetActive(false);
        }
    }
}
