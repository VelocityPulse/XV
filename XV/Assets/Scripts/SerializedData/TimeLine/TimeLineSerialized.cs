using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public sealed class TimeLineSerialized
{
	public const string DEFAULT_FILE_NAME = "TimeLine.xml";

	public const string RES_PATH = "/Resources/SavedData/Timeline/";

	public const string DEBUG_NAME = "debug.xml";

	public string TimeLineName { get; set; }

	public List<MovableAnimation> MovableAnimationList { get; set; }

	public List<HumanInteraction> HumanInteractionList { get; set; }

	public List<ManifactureInteraction> ManifactureInteractionList { get; set; }

	public TimeLineSerialized()
	{
		MovableAnimationList = new List<MovableAnimation>();
		HumanInteractionList = new List<HumanInteraction>();
		ManifactureInteractionList = new List<ManifactureInteraction>();
	}

	public string Serialize()
	{
		string lFolderPath = Application.dataPath + RES_PATH;
		Utils.CreateFolder(lFolderPath);

		if (!TimeLineName.EndsWith(".xml"))
			TimeLineName += ".xml";

		using (StreamWriter lWriter = new StreamWriter(lFolderPath + TimeLineName)) {
			XmlSerializer lSerializer = new XmlSerializer(typeof(TimeLineSerialized));
			lSerializer.Serialize(lWriter, this);
			lWriter.Flush();
		}

		return lFolderPath + TimeLineName;
	}

	static public TimeLineSerialized Unserialize(string iTimeLineName)
	{
		string lFolderPath = Application.dataPath + RES_PATH;
		Utils.CreateFolder(lFolderPath);

		if (!iTimeLineName.EndsWith(".xml"))
			iTimeLineName += ".xml";

		XmlSerializer lSerializer = new XmlSerializer(typeof(TimeLineSerialized));

		using (StreamReader lStreamReader = new StreamReader(lFolderPath + iTimeLineName, Encoding.UTF8, true)) {

			TimeLineSerialized oTimeLineSerialized = (TimeLineSerialized)lSerializer.Deserialize(lStreamReader);

			if (oTimeLineSerialized == null)
				oTimeLineSerialized = new TimeLineSerialized();
			return oTimeLineSerialized;
		}
	}
}
