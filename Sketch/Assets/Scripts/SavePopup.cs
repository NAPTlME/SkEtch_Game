using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SavePopup : MonoBehaviour
{
    public InputField inText;
    public ScreenPixelOverwrite screen;
    public GameObject optionsMenu;
    public void OkClick()
    {
        Debug.Log("Text: " + inText.text);
        if (!inText.text.Equals(""))
        {
            //StartCoroutine(screen.SaveCurrentTexture(inText.text));
            screen.SaveCurrentTexture(inText.text);
            screen.isListeningForPlayer = true;
            this.gameObject.SetActive(false);
        }
    }

    public void CancelClick()
    {
        optionsMenu.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
