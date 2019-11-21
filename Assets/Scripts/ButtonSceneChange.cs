using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonSceneChange : MonoBehaviour
{
    public void SwitchToGameScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
