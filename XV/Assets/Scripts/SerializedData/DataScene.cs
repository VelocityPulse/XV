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
	private const string FILE_NAME = "DataScene.xml";

	public string SceneName { get; set; }

	public List<ObjectDataScene> DataObjects { get; set; }

	public DataScene()
	{
		DataObjects = new List<ObjectDataScene>();
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
		foreach (ObjectDataScene lItem in DataObjects) {

			if (lItem.GUID == iODS.GUID) {
				lItem.Name = iODS.Name;
				lItem.Position = iODS.Position;
				lItem.Rotation = iODS.Rotation;
				lItem.Scale = iODS.Scale;
				lItem.Type = iODS.Type;
			}
		}
		DataObjects.Add(iODS);
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

		using (StreamReader lStreamReader = new StreamReader(lFolderPath + FILE_NAME, Encoding.UTF8, true)) {

			//System.Xml.XmlReader lReader = System.Xml.XmlReader.Create(lStreamReader);

			DataScene oDataScene = (DataScene)lSerializer.Deserialize(lStreamReader);

			if (oDataScene == null)
				oDataScene = new DataScene();
			return oDataScene;
		}
	}
}
