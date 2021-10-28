using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class ScreenPixelOverwrite : MonoBehaviour
{
    public GameObject OptionsMenu;
    Texture2D texture;
    Renderer rend;
    // cursor position
    Vector2 cursorPos;
    Color startingColor;
    Color writeColor;
    [SerializeField]
    int resolution;
    [SerializeField]
    float speedMultiplier;
    public bool isListeningForPlayer;
    List<TextureSaveFormat> menuImages;
    public bool mirrorX;
    public GameObject leftDial;
    public GameObject rightDial;
    [SerializeField]
    float rotateSpeed = 0.1f;
    public bool menuIdle;

    // Start is called before the first frame update
    void Start()
    {
        // set global variables and get components
        rend = GetComponent<Renderer>();
        //resolution = 50;// 2048;
        startingColor = new Color(208f / 255f, 211f / 255f, 205f / 255f);
        writeColor = new Color(26f / 255f, 28f / 255f, 27f / 255f);
        speedMultiplier = 50f;
        isListeningForPlayer = false;
        menuImages = new List<TextureSaveFormat>();
        menuIdle = true;
        GetImagesFromDirectory();
        ProcessImagesFromDirectory();
        // create base texture
        //texture = Instantiate(rend.material.mainTexture) as Texture2D;
        texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        Color[] startingColors = Enumerable.Repeat<Color>(startingColor, resolution * resolution).ToArray();
        Debug.Log("Colors length: " + startingColors.Length);
        texture.SetPixels(startingColors);
        Debug.Log("Texture Format: " + texture.format.ToString());
        rend.material.mainTexture = texture;
        Debug.Log("MipMapLevels: " + texture.mipmapCount);
        Color[] tmp = texture.GetPixels();
        Debug.Log("Pixels: " + tmp.Length);
        Debug.Log("Height: " + texture.height);
        Debug.Log("Width: " + texture.width);

        for (int i = 0; i < texture.mipmapCount; i++)
        {
            tmp = texture.GetPixels(i);
            Debug.Log("MipMapLevel: " + i);
            Debug.Log("Height: " + texture.height);
            Debug.Log("Width: " + texture.width);
        }

        // set cursor start point
        ResetCursorPos();
        Debug.Log("Cursor Pos: " + cursorPos.ToString());


        texture.Apply(false);
        StartCoroutine(MenuIdle());
    }

    // Update is called once per frame
    void Update()
    {
        if (isListeningForPlayer)
        {
            // test writing over the entire texture
            Vector2 direction = GetInputTranslation();
            //float pixelsToWritePerSecond = resolution / 500;
            direction = direction * Time.deltaTime * speedMultiplier;

            if (direction.magnitude > 0)
            {
                Debug.Log("CursorPos: " + cursorPos.x + ", " + cursorPos.y);
                Debug.Log("Direction of mag: " + direction.magnitude);
                Debug.Log(direction.ToString());
                DrawLineToTexture(direction);

            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(ApplyColorToEntireScreen(startingColor));//  new Color(110f / 255f, 81f / 255f, 226f / 255f));
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.isListeningForPlayer = false;
                OptionsMenu.SetActive(true);
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                this.mirrorX = !this.mirrorX;
            }
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                // color picker popup
            }
        }
    }

    void GetImagesFromDirectory()
    {
        string assetDir = "Assets/MenuImages/";
        var pSeqFiles = Directory.GetFiles(assetDir, "*.pSeq");
        foreach(var file in pSeqFiles)
        {
            if (File.Exists(file))
            {
                Debug.Log("Loading in: " + file);
                var seq = TextureSaveLoad.ReadTextureData(file);

                menuImages.Add(seq);
            } 
            else
            {
                Debug.LogError("Error: Could not find file: " + file);
            }
        }
    }

    void ProcessImagesFromDirectory()
    {
        string assetDir = "Assets/MenuImages/";
        var pngFiles = Directory.GetFiles(assetDir, "*.png");
        foreach (var file in pngFiles)
        {
            if (File.Exists(file))
            {
                Debug.Log("Loading in: " + file);
                byte[] bytes = File.ReadAllBytes(file);
                Texture2D thisTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                thisTexture.LoadImage(bytes);

                var pixelSeq = GetPixelSequence(thisTexture);

                var outFile = file.Replace(".png", ".pSeq");

                TextureSaveLoad.SaveTextureData(pixelSeq, outFile, thisTexture.width);

                TextureSaveFormat seq = new TextureSaveFormat(thisTexture.width, pixelSeq.Select(sel => sel.Select(sel2 => new SerializablePixelLoc(sel2)).ToList()).ToList());

                menuImages.Add(seq);
            }
            else
            {
                Debug.LogError("Error: Could not find file: " + file);
            }
        }
    }

    void ResetCursorPos()
    {
        cursorPos = new Vector2(Mathf.Floor((float)texture.width / 2f), Mathf.Floor((float)texture.height / 2f));
    }

    void DrawLineToTexture(Vector2 direction)
    {
        Vector2 endPoint = cursorPos + direction;
        // check if cursor + direction will be out of bounds 
        // cheating for now and just truncating the values that are too high/low
        if (endPoint.x >= resolution - 1)
        {
            endPoint.x = resolution - 1;
        }
        if (endPoint.x < 0)
        {
            endPoint.x = 0;
        }
        if (endPoint.y >= resolution - 1)
        {
            endPoint.y = resolution - 1;
        }
        if (endPoint.y < 0)
        {
            endPoint.y = 0;
        }
        // write the current pixel and then iterate over each pixel thereafter
        for (float i = 0; i <= direction.magnitude; i+= 0.5f)
        {
            cursorPos = Vector2.MoveTowards(cursorPos, endPoint, 0.5f);
            WriteCurrentPixel();
        }
        texture.Apply(false);
    }

    void RotateDials(Vector2 direction)
    {
        Vector2 endPoint = cursorPos + direction;
        // check if cursor + direction will be out of bounds 
        // cheating for now and just truncating the values that are too high/low
        if (endPoint.x >= resolution - 1)
        {
            endPoint.x = resolution - 1;
        }
        if (endPoint.x < 0)
        {
            endPoint.x = 0;
        }
        if (endPoint.y >= resolution - 1)
        {
            endPoint.y = resolution - 1;
        }
        if (endPoint.y < 0)
        {
            endPoint.y = 0;
        }
        leftDial.transform.Rotate(0, (endPoint.x - cursorPos.x) * rotateSpeed, 0);
        rightDial.transform.Rotate(0, (endPoint.y - cursorPos.y) * rotateSpeed, 0);
    }

    public IEnumerator MenuIdle()
    {
        var oldRes = resolution;
        while (menuIdle)
        {
            foreach (var txt in menuImages)
            {
                yield return updateResolution(txt.resolution);
                yield return DrawSavedPixelSequence(txt.pixelSeq.Select(sel => sel.Select(sel2 => sel2.ConvertToPixelLoc()).ToList()).ToList(), 10, true);
                yield return new WaitForSeconds(2f);
            }
        }
        yield return updateResolution(oldRes);
    }

    IEnumerator DrawSavedTexture(Texture2D newTexture, int secondsToDraw = 3)
    {
        // set resolution by newTexture width (assume this texture is width==height)
        resolution = newTexture.width;
        // clear texture
        yield return ApplyColorToEntireScreen(startingColor);

        var pixelNeighborGroups = GetPixelSequence(newTexture);

        if(pixelNeighborGroups != null)
        {
            //yield return DrawSavedPixelSequence(pixelNeighborGroups);
        }
        
    }

    List<List<PixelLoc>> GetPixelSequence(Texture2D newTexture)
    {
        // get groupings of pixels by color and neighbor
        var pixels = newTexture.GetPixels().Select((color, index) => (color, index))
            .Where(wh => !isColorEqual(wh.color,startingColor))
            .Select(sel => (sel.color, ConvertIndexToCoord(sel.index, newTexture.width)))
            .Select(sel => new PixelLoc(sel.Item2.x, sel.Item2.y, sel.color)).ToList();
        Debug.Log("Got pixels");
        if (pixels.Count() > 0)
        {
            return PixelLocUtility.groupPixelsByNeighborsAndCalcDrawOrder(pixels);
        }
        else
        {
            return null;
        }
    }

    bool isColorEqual(Color color1, Color color2)
    {
        return Mathf.Round(100 * color1.r).Equals(Mathf.Round(100 * color2.r)) &&
            Mathf.Round(100 * color1.g).Equals(Mathf.Round(100 * color2.g)) &&
            Mathf.Round(100 * color1.b).Equals(Mathf.Round(100 * color2.b));
    }

    IEnumerator DrawSavedPixelSequence(List<List<PixelLoc>> pixelNeighborGroups, int secondsToDraw = 3, bool isIdle = false)
    {
        var lastTimeCheck = Time.time;
        // for now just flatten and take in sequence
        var allPixelsToDraw = pixelNeighborGroups.SelectMany(sel => sel).ToList();

        //float interval = 0.02f;// frequency to call update

        float pixelsToDrawPerSecond = (float)(pixelNeighborGroups.Count()) / (float)secondsToDraw;
        int drawn = 0;
        // yield so we get an updated time for the first draw
        yield return new WaitForEndOfFrame();
        while (drawn < allPixelsToDraw.Count())
        {
            var deltaTime = Time.time - lastTimeCheck;
            lastTimeCheck = Time.time;
            int numPixelsToDraw = Mathf.Min(Mathf.CeilToInt(pixelsToDrawPerSecond * deltaTime), allPixelsToDraw.Count() - drawn);
            if (numPixelsToDraw == 0)
            {
                numPixelsToDraw++;
            }

            var pixelsToDraw = allPixelsToDraw.Skip(drawn).Take(numPixelsToDraw);

            foreach (var pixel in pixelsToDraw)
            {
                WritePixel(pixel.xPos, pixel.yPos, pixel.color);
            }
            drawn += numPixelsToDraw;
            // update texture and wait
            texture.Apply();
            yield return new WaitForEndOfFrame();
            if (isIdle && !menuIdle)
            {
                break;
            }
        }
    }

    (int x, int y) ConvertIndexToCoord(int i, int res)
    {
        // assumes resolution is res * res
        return (i % res, i / res);
    }

    public IEnumerator updateResolution(int res, bool force = false)
    {
        if (force && texture != null && texture.GetPixels().Any(pix => !pix.Equals(startingColor)))
        {
            yield return ApplyColorToEntireScreen(startingColor);
        }
        resolution = res;
        texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        Color[] startingColors = Enumerable.Repeat<Color>(startingColor, resolution * resolution).ToArray();
        Debug.Log("Colors length: " + startingColors.Length);
        texture.SetPixels(startingColors);
        rend.material.mainTexture = texture;
        Debug.Log("Resolution set to: " + resolution);
        ResetCursorPos();
        Debug.Log("Cursor Pos: " + cursorPos.ToString());
        yield return null;
    }

    void WriteCurrentPixel()
    {
        WritePixel(Mathf.FloorToInt(cursorPos.x), Mathf.FloorToInt(cursorPos.y), writeColor);
        if (this.mirrorX)
        {
            WritePixel(Mathf.FloorToInt(-cursorPos.x), Mathf.FloorToInt(cursorPos.y), writeColor);
        }
    }

    void WritePixel(int x, int y, Color color)
    {
        texture.SetPixel(x, y, color);
    }

    /*Vector2 LineIntersect(Vector2 pointA1, Vector2 pointA2, Vector2 pointB1, Vector2 pointB2)
    {
        float A1 = pointA1.y;
        Vector3.Cross
        Vector2
    }*/

    IEnumerator ApplyColorToEntireScreen(Color newColor)
    {
        Color[] colors;
        Color[] currColors;
        for (int j = 0; j < 6; j++)
        {
            // only run as coroutine
            colors = Enumerable.Repeat<Color>(newColor, resolution * resolution).ToArray();
            currColors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.Lerp(currColors[i], colors[i], 0.5f);
            }
            texture.SetPixels(colors);
            texture.Apply();
            yield return new WaitForSeconds(0.2f);
        }
        // only run as coroutine
        colors = Enumerable.Repeat<Color>(newColor, resolution * resolution).ToArray();
        currColors = texture.GetPixels();
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.Lerp(currColors[i], colors[i], 1f);
        }
        texture.SetPixels(colors);
        texture.Apply();
    }

    public IEnumerator ClearScreenAndApplyResolution()
    {
        yield return updateResolution(resolution);
    }

    Vector2 GetInputTranslation()
    {
        Vector2 direction = new Vector2();
        if(Input.GetKey(KeyCode.UpArrow))
        {
            direction += Vector2.up;
        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
            direction += Vector2.down;
        }
        if(Input.GetKey(KeyCode.A))
        {
            direction += Vector2.left;
        }
        if(Input.GetKey(KeyCode.D))
        {
            direction += Vector2.right;
        }
        return direction;
    }

    public void SaveCurrentTexture(string fileName)
    {
        Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        newTexture.SetPixels(texture.GetPixels());
        Debug.Log("Copied texture");
        Debug.Log("Getting pixel Sequence");
        //yield return null;
        var pixelSequence = GetPixelSequence(texture);
        Debug.Log("Saving Texture data");
        //yield return null;
        string path = Application.persistentDataPath + "/" + fileName + ".pSeq";
        TextureSaveLoad.SaveTextureData(pixelSequence, path, resolution);
        //yield return null;
    }
    public void LoadTexture(string path)
    {
        var savedPixelSeq = TextureSaveLoad.ReadTextureData(path);

        StartCoroutine(updateResAndLoadTexture(savedPixelSeq));
    }
    IEnumerator updateResAndLoadTexture(TextureSaveFormat savedPixelSeq)
    {
        yield return updateResolution(savedPixelSeq.resolution);

        yield return DrawSavedPixelSequence(savedPixelSeq.ConvertToPixelLoc());
    }
}
