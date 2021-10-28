using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public GameObject MainMenu;
    public ScreenPixelOverwrite screen;
    public Slider resSlider;
    public GameObject saveMenu;
    public GameObject loadMenu;
    public LoadPopup loadScript;
    public CameraControl camera;
    
    // Start is called before the first frame update
    void Start()
    {
        // set up listener for slider
        resSlider.onValueChanged.AddListener(delegate { SliderValueChanged(); });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape key pressed");
            Debug.Log("screen listening: " + screen.isListeningForPlayer);
            screen.isListeningForPlayer = true;
            Debug.Log("screen listening: " + screen.isListeningForPlayer);
            this.gameObject.SetActive(false);
        }
    }

    void SliderValueChanged()
    {
        //todo: prompt user before erasing

        StartCoroutine(screen.updateResolution((int)resSlider.value));
    }

    public void MenuClick()
    {
        screen.ClearScreenAndApplyResolution();
        
        camera.allowMove = true;
        MainMenu.SetActive(true);
        screen.menuIdle = true;
        StartCoroutine(screen.MenuIdle());
        this.gameObject.SetActive(false);
        // start coroutine for drawing
    }

    public void SaveClick()
    {
        saveMenu.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void LoadClick()
    {
        loadScript.RefreshFiles();
        loadMenu.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
