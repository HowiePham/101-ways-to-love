using System;
using System.Collections.Generic;

[Serializable]
public class LifeData
{
    public int CurrentLifeCount;
    public List<string> AddedNextTime = new List<string>();
}