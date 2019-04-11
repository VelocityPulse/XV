using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public sealed class ModelLoader
{
    public class Model
    {
        public Model()
        {
        }

        public ObjectDataSceneType Type { get; set; }
        public GameObject  GameObject { get; set; }
        public Sprite Sprite { get; set; }
    };

    private static readonly ModelLoader instance = new ModelLoader();

    private Dictionary<string, Model> mModelPool;

    public int ModelPoolLenght { get; private set;}

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static ModelLoader()
    {
    }

    private void LoadImportModel()
    {
        GameObject lGm = null;
        string lName = null;

        string[] lModelFiles = Directory.GetFiles(Application.dataPath + "/Resources/SavedData/Models");
        Sprite lImportModelSprite = Resources.Load<Sprite>("Sprites/UI/ImportModel");

        foreach (string iModelFile in lModelFiles) {
            if (iModelFile.Contains(".fbx") && !iModelFile.Contains(".meta")) {
                if ((lName = iModelFile.Replace(Application.dataPath + "/Resources/SavedData/Models/", "")) == null) {
                    Debug.LogError("[MODEL_POOL] Error while removing data path.");
                    continue;
                }
                if ((lName = lName.Replace(".fbx", "")) == null) {
                    Debug.LogError("[MODEL_POOL] Error while removing extension name.");
                    continue;
                }
                if ((lGm = Resources.Load<GameObject>("SavedData/Models/" + lName)) == null) {
                    Debug.LogError("[MODEL_POOL] Error while loading prefab:" + "Prefabs/ItemBank/" + lName);
                    continue;
                }
                Debug.Log("---- " + lName + " loaded ----");
                lImportModelSprite.name = lName;
                if (mModelPool.ContainsKey(lName) == false)
                    mModelPool.Add(lName, new Model { Type = ObjectDataSceneType.EXTERN, GameObject = lGm, Sprite = lImportModelSprite, });
                else
                    Debug.LogError("[MODEL_POOL] Error, model name already exist.");
            }
        }
    }

    private void LoadInternModel()
    {
        GameObject lGm = null;
        Sprite lSprite = null;
        string lName = null;

        string[] lModelFiles = Directory.GetFiles(Application.dataPath + "/Resources/Prefabs/ItemBank");

        foreach (string iModelFile in lModelFiles) {
            if (iModelFile.Contains(".prefab") && !iModelFile.Contains(".meta")) {
                if ((lName = iModelFile.Replace(Application.dataPath + "/Resources/Prefabs/ItemBank/", "")) == null) {
                    Debug.LogError("[MODEL_POOL] Error while removing data path.");
                    continue;
                }
                if ((lName = lName.Replace(".prefab", "")) == null) {
                    Debug.LogError("[MODEL_POOL] Error while removing extension name.");
                    continue;
                }
                if ((lGm = Resources.Load<GameObject>("Prefabs/ItemBank/" + lName)) == null) {
                    Debug.LogError("[MODEL_POOL] Error while loading prefab:" + "Prefabs/ItemBank/" + lName);
                    continue;
                }
                if ((lSprite = Resources.Load<Sprite>("Sprites/UI/" + lName)) == null) {
                    Debug.LogError("[MODEL_POOL] Error while loading sprite:" + "Sprites/UI/" + lName);
                    continue;
                }
                Debug.Log("---- " + lName + " loaded ----");
                mModelPool.Add(lName, new Model { Type = ObjectDataSceneType.BUILT_IN, GameObject = lGm, Sprite = lSprite, });
            }
        }
    }

    private ModelLoader()
    {
        if ((mModelPool = new Dictionary<string, Model>()) == null) {
            Debug.LogError("[MODEL_POOL] Error while creating the dictionary.");
            ModelPoolLenght = 0;
            return;
        }
        LoadInternModel();
        LoadImportModel();
        ModelPoolLenght = mModelPool.Count;
    }

    public static ModelLoader Instance
    {
        get {
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
