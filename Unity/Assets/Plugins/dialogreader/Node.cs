using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    public int NodeId = -1;
    public String NodeType;
    public String Text;
    public int Delay=-1;
    public int NextNodeAfter=-1;
    public int NextNode=-1;
    public String MovieTexture;
    public String Avatar;
    public List<Option> Options = new List<Option>();
    public int DefaultOption = -1;
    public float[] position = new float[2];
    public bool StartingNode;
    public int Grammar;
}
[System.Serializable]
public class NodeList
{
    public List<Node> nodes=new List<Node>();
}

[System.Serializable]
public class Option
{
    public String text;
    public String[] Keywords;
    public int NextNode=-1;
}
