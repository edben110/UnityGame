using System;
using System.Collections.Generic;

[Serializable]
public class CharacterGroupStateFile
{
    public List<CharacterGroupStateEntry> characters = new List<CharacterGroupStateEntry>();
}

[Serializable]
public class CharacterGroupStateEntry
{
    public string id;
    public float anxiety;
    public bool isInGroup = true;
    public bool cinematicPlayed;
}
