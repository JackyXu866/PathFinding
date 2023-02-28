using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public GameObject tile;
    public List<Node> neighbors = new List<Node>();

    // only add whats created and add `this` to them as well
    public void AddNeighbor(ref Hashtable mapTable, int x, int y)
    {
        if(mapTable.ContainsKey(new Vector2(x-1, y)))
        {
            neighbors.Add((Node)mapTable[new Vector2(x-1, y)]);
            ((Node)mapTable[new Vector2(x-1, y)]).neighbors.Add(this);
        }
        if(mapTable.ContainsKey(new Vector2(x-1, y-1)))
        {
            neighbors.Add((Node)mapTable[new Vector2(x-1, y-1)]);
            ((Node)mapTable[new Vector2(x-1, y-1)]).neighbors.Add(this);
        }
        if(mapTable.ContainsKey(new Vector2(x, y-1)))
        {
            neighbors.Add((Node)mapTable[new Vector2(x, y-1)]);
            ((Node)mapTable[new Vector2(x, y-1)]).neighbors.Add(this);
        }
        if(mapTable.ContainsKey(new Vector2(x+1, y-1)))
        {
            neighbors.Add((Node)mapTable[new Vector2(x+1, y-1)]);
            ((Node)mapTable[new Vector2(x+1, y-1)]).neighbors.Add(this);
        }

    }
}
