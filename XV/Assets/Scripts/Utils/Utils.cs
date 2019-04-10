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

}
