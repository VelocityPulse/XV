using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public sealed class ModelManager
{
    public class Model
    {
        public GameObject  GameObject { get; set; }
        public Sprite Sprite { get; set; }
    };

    private static readonly ModelManager instance = new ModelManager();

    private Dictionary<string, Model> mModelPool;

    public int ModelPoolLenght { get; private set;}

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static ModelManager()
    {
    }

    private ModelManager()
    {
        if ((mModelPool = new Dictionary<string, Model>()) == null)
        {
            Debug.LogError("[MODEL_POOL] Error while creating the dictionary.");
            ModelPoolLenght = 0;
            return;
        }

        GameObject lGm = null;
        Sprite lSprite = null;
        string lName = null;

        string[] lModelFiles = Directory.GetFiles(Application.dataPath + "/Resources/Prefabs");

        foreach (string iModelFile in lModelFiles)
        {
            if (iModelFile.Contains(".prefab") && !iModelFile.Contains(".meta"))
            {
                if ((lName = iModelFile.Replace(Application.dataPath + "/Resources/Prefabs/", "")) == null)
                {
                    Debug.LogError("[MODEL_POOL] Error while removing data path.");
                    continue;
                }
                if ((lName = lName.Replace(".prefab", "")) == null)
                {
                    Debug.LogError("[MODEL_POOL] Error while removing extension name.");
                    continue;
                }
                if ((lGm = Resources.Load<GameObject>("Prefabs/" + lName)) == null)
                {
                    Debug.LogError("[MODEL_POOL] Error while loading prefab:" + "Prefabs/" + lName);
                    continue;
                }
                if ((lSprite = Resources.Load<Sprite>("Sprites/UI/" + lName)) == null)
                {
                    Debug.LogError("[MODEL_POOL] Error while loading sprite:" + "Sprites/UI/" + lName);
                    continue;
                }
                Debug.Log("---- " + lName + " loaded ----");
                mModelPool.Add(lName, new Model { GameObject = lGm, Sprite = lSprite, });
            }
        }
        ModelPoolLenght = mModelPool.Count;
    }

    public static ModelManager Instance
    {
        get
        {
            return instance;
        }
    }

    public GameObject GetModelGameObject(string iName)
    {
        Model lModel;

        if (!(mModelPool.TryGetValue(iName, out lModel)))
            return null;
        return lModel.GameObject;
    }

    public Sprite GetModelSprite(string iName)
    {
        Model lModel;

        if (!(mModelPool.TryGetValue(iName, out lModel)))
            return null;
        return lModel.Sprite;
    }

    public List<Model> GetAllModel()
    {
        List<Model> lModels;

        if ((lModels = new List<Model>()) == null)
            return null;
        foreach (KeyValuePair<string, Model> lElement in mModelPool)
            lModels.Add(lElement.Value);
        return lModels;
    }
}
