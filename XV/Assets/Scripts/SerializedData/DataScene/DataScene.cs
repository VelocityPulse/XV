using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

// When app built, the file is savec in : Build.app/Contents/SavedData/
[Serializable]
public sealed class DataScene
{
	public const string DEFAULT_FILE_NAME = "DataScene.xml";

	public const string RES_PATH = "/Resources/SavedData/";

	public const string DEBUG_NAME = "debug.xml";

	public string SceneName { get; set; }

	public HumanDataScene Human { get; set; }

	public List<ObjectDataScene> DataObjects { get; set; }

	public DataScene()
	{
		DataObjects = new List<ObjectDataScene>();
		SceneName = DEFAULT_FILE_NAME;
	}

	public DataScene(string iSceneName)
	{
		DataObjects = new List<ObjectDataScene>();
		SceneName = iSceneName;
	}

	public bool IsDataObjectsContains(ObjectDataScene iODS)
	{
		foreach (ObjectDataScene lItem in DataObjects) {
			if (lItem.GUID == iODS.GUID)
				return true;
		}
		return false;
	}

	public bool RemoveODS(ObjectDataScene iODS)
	{
		foreach (ObjectDataScene lItem in DataObjects) {
			if (lItem.GUID == iODS.GUID)
				return DataObjects.Remove(lItem);
		}
		return false;
	}

	public void AddODS(ObjectDataScene iODS)
	{
		if (SceneName.ToLower() == DEBUG_NAME)
			return;

		foreach (ObjectDataScene lItem in DataObjects) {

			if (lItem.GUID == iODS.GUID) {
				lItem.Name = iODS.Name;
				lItem.PrefabName = iODS.PrefabName;
				lItem.Position = iODS.Position;
				lItem.Rotation = iODS.Rotation;
				lItem.Scale = iODS.Scale;
				lItem.Type = iODS.Type;
				return;
			}
		}
		DataObjects.Add(iODS);
	}

	public void SetHDS(HumanDataScene iHDS)
	{
		if (SceneName.ToLower() == DEBUG_NAME)
			return;

		Human = iHDS;
	}

	static public string Serialize(DataScene iDataScene)
	{
		string lFolderPath = Application.dataPath + RES_PATH;
		Utils.CreateFolder(lFolderPath);

		using (StreamWriter writer = new StreamWriter(lFolderPath + iDataScene.SceneName)) {
			XmlSerializer serializer = new XmlSerializer(typeof(DataScene));
			serializer.Serialize(writer, iDataScene);
			writer.Flush();
		}

		return lFolderPath + DEFAULT_FILE_NAME;
	}

	public string Serialize()
	{
		string lFolderPath = Application.dataPath + RES_PATH;
		Utils.CreateFolder(lFolderPath);

		if (!SceneName.EndsWith(".xml"))
			SceneName += ".xml";

		using (StreamWriter writer = new StreamWriter(lFolderPath + SceneName)) {
			XmlSerializer serializer = new XmlSerializer(typeof(DataScene));
			serializer.Serialize(writer, this);
			writer.Flush();
		}

		return lFolderPath + DEFAULT_FILE_NAME;
	}

	static public DataScene Unserialize(string iSceneName)
	{
		string lFolderPath = Application.dataPath + RES_PATH;
		Utils.CreateFolder(lFolderPath);

		if (!iSceneName.EndsWith(".xml"))
			iSceneName += ".xml";

		XmlSerializer lSerializer = new XmlSerializer(typeof(DataScene));

		using (StreamReader lStreamReader = new StreamReader(lFolderPath + iSceneName, Encoding.UTF8, true)) {

			//System.Xml.XmlReader lReader = System.Xml.XmlReader.Create(lStreamReader);

			DataScene oDataScene = (DataScene)lSerializer.Deserialize(lStreamReader);

			if (oDataScene == null)
				oDataScene = new DataScene();
			return oDataScene;
		}
	}
}