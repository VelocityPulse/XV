﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public sealed class ModelLoader
{
    public class Model
    {
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
        get
        {
            return instance;
        }
    }

    // Test if it's possible to unload resources after store it in the dico
    private void LoadImportModel()
    {
        GameObject[] lModelFiles = Resources.LoadAll<GameObject>(GameManager.ExternItemBankPath);
        if (lModelFiles == null) {
            Debug.LogError("[MODEL_POOL] Error while loading items: " + GameManager.ItemBankPath);
            return;
        }

        Sprite lImportModelSprite = Resources.Load<Sprite>("Sprites/UI/ImportModel");
        if (lImportModelSprite == null) {
            Debug.LogError("[MODEL_POOL] Error while loading sprite: Sprites/UI/ImportModel");
            return;
        }

        foreach (GameObject iModelFile in lModelFiles) {
            if (mModelPool.ContainsKey(iModelFile.name) == false) {
                mModelPool.Add(iModelFile.name, new Model { Type = ObjectDataSceneType.EXTERN, GameObject = iModelFile, Sprite = lImportModelSprite, });
                Debug.Log("---- " + iModelFile.name + " loaded ----");
            }
                else
                    Debug.LogError("[MODEL_POOL] Error, model name already exist.");
        }
    }

    // Test if it's possible to unload resources after store it in the dico
    private void LoadInternModel()
    {
        Sprite lSprite = null;

        GameObject[] lModelFiles = Resources.LoadAll<GameObject>(GameManager.ItemBankPath);
        if (lModelFiles == null) {
            Debug.LogError("[MODEL_POOL] Error while loading item:" + GameManager.ItemBankPath);
            return;
        }
        
        foreach (GameObject iModelFile in lModelFiles) {
            if ((lSprite = Resources.Load<Sprite>("Sprites/UI/" + iModelFile.name)) == null) {
                Debug.LogError("[MODEL_POOL] Error while loading sprite:" + "Sprites/UI/" + iModelFile.name);
                continue;
            }
            if (mModelPool.ContainsKey(iModelFile.name) == false) {
                mModelPool.Add(iModelFile.name, new Model { Type = ObjectDataSceneType.BUILT_IN, GameObject = iModelFile, Sprite = lSprite, });
                Debug.Log("---- " + iModelFile.name + " loaded ----");
            } 
            else
                Debug.LogError("[MODEL_POOL] Error, model name already exist.");
        }
    }

    // Update the model dictionary
    // Call when user import new model
    public void UpdatePool()
    {
        
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
