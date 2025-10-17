using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitFunctionality : MonoBehaviour
{
    public void QuitApplication()
    {
        Debug.Log("Exiting application...");
        Application.Quit();

    }
}
