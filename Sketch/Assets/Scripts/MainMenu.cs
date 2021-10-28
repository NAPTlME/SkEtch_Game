using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public ScreenPixelOverwrite screen;
    public CameraControl camera;
    public void StartGame()
    {
        // stop currently running job (expect a coroutine of name MenuIdle)
        //StopCoroutine("MenuIdle");
        screen.menuIdle = false;
        // clear screen
        StartCoroutine(screen.ClearScreenAndApplyResolution());
        // set isActive variable so writes are allowed to screen
        screen.isListeningForPlayer = true;
        // move camera back to position
        camera.allowMove = false;
        camera.ReturnToStart(3);
        //StartCoroutine(camera.ReturnToStartPosition(2));
        // disable this menu
        this.gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
