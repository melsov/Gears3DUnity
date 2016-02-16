using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
	public string levelName;
	public List<SaveEntry> entries = new List<SaveEntry>();
}