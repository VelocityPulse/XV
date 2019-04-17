using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

// When app built, the file is savec in : Build.app/Contents/SavedData/
[Serializable]
public sealed class DataScene
{
	private const string FILE_NAME = "DataScene.xml";

	public string SceneName { get; set; }

	public List<ObjectDataScene> DataObjects { get; set; }

	public DataScene()
	{
		DataObjects = new List<ObjectDataScene>();
	}

	static public string Serialize(DataScene iDataScene)
	{
		string lFolderPath = Application.dataPath + "/Resources/SavedData/";
		Utils.CreateFolder(lFolderPath);

		using (StreamWriter writer = new StreamWriter(lFolderPath + FILE_NAME)) {
			XmlSerializer serializer = new XmlSerializer(typeof(DataScene));
			serializer.Serialize(writer, iDataScene);
			writer.Flush();
		}

		return lFolderPath + FILE_NAME;
	}

	public string Serialize()
	{
		string lFolderPath = Application.dataPath + "/Resources/SavedData/";
		Utils.CreateFolder(lFolderPath);

		using (StreamWriter writer = new StreamWriter(lFolderPath + FILE_NAME)) {
			XmlSerializer serializer = new XmlSerializer(typeof(DataScene));
			serializer.Serialize(writer, this);
			writer.Flush();
		}

		return lFolderPath + FILE_NAME;
	}

	static public DataScene Unserialize()
	{

		string lFolderPath = Application.dataPath + "/Resources/SavedData/";
		Utils.CreateFolder(lFolderPath);

		XmlSerializer lSerializer = new XmlSerializer(typeof(DataScene));

		using (StringReader lStringReader = new StringReader(lFolderPath + FILE_NAME)) {
			DataScene oDataScene = (DataScene)lSerializer.Deserialize(lStringReader);

			if (oDataScene == null)
				oDataScene = new DataScene();
			return oDataScene;
		}
	}
}
