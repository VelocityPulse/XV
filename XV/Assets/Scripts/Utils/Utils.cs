using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class Utils {
	
	public static bool CreateFolder(string iPath) {
		if (string.IsNullOrEmpty(iPath))
			return false;

		if (Directory.Exists(iPath))
			return true;
		Directory.CreateDirectory(iPath);

		return false;
	}

	public static void PrintStackTrace() {
		StackTrace t = new StackTrace();
		UnityEngine.Debug.Log(t.ToString());
	}

    public static bool SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null || newLayer < 0 || newLayer > 31)
            return false;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) {
            if (child == null)
                continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
        return true;
    }

    public static Vector3 FindCentroid (List<Vector3> iPoints)
    {
        Vector3 centroid;
        Vector3 minPoint = iPoints[0];
        Vector3 maxPoint = iPoints[0];
 
        for ( int i = 1; i < iPoints.Count; i ++ ) {
             if( iPoints[i].x < minPoint.x )
                 minPoint.x = iPoints[i].x;
             if( iPoints[i].x > maxPoint.x )
                 maxPoint.x = iPoints[i].x;
             if( iPoints[i].y < minPoint.y )
                 minPoint.y = iPoints[i].y;
             if( iPoints[i].y > maxPoint.y )
                 maxPoint.y = iPoints[i].y;
             if( iPoints[i].z < minPoint.z )
                 minPoint.z = iPoints[i].z;
             if( iPoints[i].z > maxPoint.z )
                 maxPoint.z = iPoints[i].z;
         }
         centroid = minPoint + 0.5f * ( maxPoint - minPoint );
         return centroid;
     }

    // Try to load all AssetBundle present in iPath
    // If iPath is not a directory, the iPath is used as an AssetBundle path
    // This function will warn the user using the Notifier when available
    public static GameObject[] LoadAllAssetBundle(string iPath)
    {
        List<GameObject> oAssets;
        GameObject[] lAssetBundleContent;

        if ((oAssets = new List<GameObject>()) == null) {
            UnityEngine.Debug.LogError("[ASSET BUNDLE LOADER] Error during allocation.");
            return null;
        }

        // If the path is not a directory, just try to import as an AssetBundle
        FileAttributes lAttr = File.GetAttributes(iPath);
        if ((lAttr & FileAttributes.Hidden) == FileAttributes.Hidden) {
            return null;
        }
        else if ((lAttr & FileAttributes.Directory) != FileAttributes.Directory)
            return LoadAllAssetBundle(iPath);

        // List all file in iPath
        List<string> lDirs = new List<string>(Directory.GetFileSystemEntries(iPath));

        foreach(string lFile in lDirs) {

            // If the current file is not a directory and is not an hidden file
            lAttr = File.GetAttributes(lFile);
            if ((lAttr & FileAttributes.Hidden) == FileAttributes.Hidden)
                continue;
            else if ((lAttr & FileAttributes.Directory) != FileAttributes.Directory) {

                if ((lAssetBundleContent = LoadAssetBundle(lFile)) == null)
                    continue;
                else {
                    foreach (GameObject lAsset in lAssetBundleContent)
                        oAssets.Add(lAsset);
                }
            }
        }
        return oAssets.ToArray();
    }

    // Try to load an AssetBundle file from iPath
    // This function will warn the user using the Notifier when available
    public static GameObject[] LoadAssetBundle(string iPath)
    {
        AssetBundle lAssetBundles = null;
        GameObject[] oAssets = null;
        FileInfo lFileInfo = null;

        if ((lFileInfo = new FileInfo(iPath)) != null) {
            if (!string.IsNullOrEmpty(lFileInfo.Extension)) {
                UnityEngine.Debug.Log("[ASSET BUNDLE LOADER] Error choosen file is not an AssetBundle.");
                // Warn user with notifier (TODO)
                return null;
            }
        }

        lAssetBundles = AssetBundle.LoadFromFile(iPath);

        // Load the AssetBundle file
        if (lAssetBundles == null) {
            UnityEngine.Debug.Log("[ASSET BUNDLE LOADER] Error while loading asset bundle:" + iPath);
            // Warn user with notifier (TODO)
            return null;
        }

        // Load all GameObject present in the AssetBundle
        if ((oAssets = lAssetBundles.LoadAllAssets<GameObject>()) == null) {
            UnityEngine.Debug.Log("[ASSET BUNDLE LOADER] Error while loading GameObject in bundle:" + iPath);
            AssetBundle.UnloadAllAssetBundles(true);
            // Warn user with notifier (TODO)
            return null;
        }
        AssetBundle.UnloadAllAssetBundles(false);
        return oAssets;
    }
}
