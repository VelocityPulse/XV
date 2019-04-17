using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundles
{
    [MenuItem("XV/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        Utils.CreateFolder(Path.Combine(Application.dataPath, "AssetBundles"));
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);
    }
}