using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
}
