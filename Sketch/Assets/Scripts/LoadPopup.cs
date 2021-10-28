using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class LoadPopup : MonoBehaviour
{
    public Dropdown fileSelect;
    public ScreenPixelOverwrite screen;
    public GameObject optionsMenu;

    public void OkClick()
    {
        if (fileSelect.options.Count() > 0)
        {
            //StartCoroutine(screen.LoadTexture(fileSelect.options[fileSelect.value].text));
            screen.LoadTexture(fileSelect.options[fileSelect.value].text);
            screen.isListeningForPlayer = true;
            this.gameObject.SetActive(false);
        }
    }

    public void CancelClick()
    {
        optionsMenu.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void RefreshFiles()
    {
        var files = Directory.GetFiles(Application.persistentDataPath, "*.pSeq");



        fileSelect.options = files.Select(sel => new Dropdown.OptionData(sel)).ToList();
    }
}
