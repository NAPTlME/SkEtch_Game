using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

public static class TextureSaveLoad
{
    public static void SaveTextureData(List<List<PixelLoc>> pixelNeighborGroups, string path, int resolution)
    {
        List<List<SerializablePixelLoc>> serializablePixels = pixelNeighborGroups.Select(sel => sel.Select(sel2 => new SerializablePixelLoc(sel2)).ToList()).ToList();

        TextureSaveFormat saveFormat = new TextureSaveFormat(resolution, serializablePixels);

        BinaryFormatter formatter = new BinaryFormatter();
        //string path = Application.persistentDataPath + "/" + fileName + ".pSeq";

        Debug.Log("Writing file to: " + path);

        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, saveFormat);
        stream.Close();
    }

    public static TextureSaveFormat ReadTextureData(string path)
    {
        //string path = Application.persistentDataPath + "/fileName" + ".pSeq";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            TextureSaveFormat savedPixelSeq = formatter.Deserialize(stream) as TextureSaveFormat;

            stream.Close();

            

            return savedPixelSeq;
        }
        else
        {
            Debug.LogError("Error: File not found in: " + path);
            return null;
        }
    }
    
}
