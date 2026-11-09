using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject objMic;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (objMic.activeSelf)
            {
                objMic.SetActive(false);
            }
            else
            {
                objMic.SetActive(true);
            }
        }

    }
}
