using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SwitchSceneButton : MonoBehaviour {

    public string SceneName = "unnamed scene";
    
    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
        });
    }
}
