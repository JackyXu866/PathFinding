using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Node Color: white: normal, red: start, green: end, blue: open, yellow: closed, grey: pending
public class Node
{
    public GameObject tile;
    public List<Node> neighbors = new List<Node>();
    public List<Node> waypointNeighbors = new List<Node>();
    public int x, y;
    public float gCost=0, hCost=0, fCost=0;
    public Node parent;

    public void ChangeTile(GameObject tile){
        this.tile = tile;
        x = (int)tile.transform.position.x;
        y = (int)tile.transform.position.y;
    }

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

    //add pointway neighbors
    public void AddWaypointConnection(ref Hashtable table, Node targetNode)
    {
        if(targetNode.tile.CompareTag("Obstacle"))
            return;

        if(this == targetNode)
            return;
        // Ray2D ray = new Ray2D(new Vector2(this.x, this.y), new Vector2(targetNode.x - this.x, targetNode.y - this.y).normalized);
        RaycastHit2D hit = Physics2D.Raycast(this.tile.transform.position,  (targetNode.tile.transform.position - this.tile.transform.position).normalized , Vector2.Distance(this.tile.transform.position, targetNode.tile.transform.position), LayerMask.GetMask("Obstacle"));
        if(hit.collider == null)
        {
            waypointNeighbors.Add(targetNode);
            ((Node)table[new Vector2(targetNode.x, targetNode.y)]).waypointNeighbors.Add(this);
        }

    }

    public ref List<Node> GetNeighbor(int index){
        if(index == 0) return ref neighbors;
        else return ref waypointNeighbors;
    }

    public void SetColor(Color color)
    {
        Color currentColor = tile.GetComponent<SpriteRenderer>().color;
        if((color == Color.blue || color == Color.yellow)
        && (currentColor == Color.red || currentColor == Color.green)){
            return;
        }
        tile.GetComponent<SpriteRenderer>().color = color;
    }

    public static bool operator ==(Node a, Node b)
    {
        if((object)a == null && (object)b == null)
            return true;
        else if ((object)a == null || (object)b == null)
            return false;
        return a.tile == b.tile;
    } 

    public static bool operator !=(Node a, Node b)
    {
        if((object)a == null && (object)b == null)
            return false;
        else if ((object)a == null || (object)b == null)
            return true;
        return a.tile != b.tile;
    }

    public override bool Equals(object obj)
    {
        if(obj == null)
            return false;
        Node node = obj as Node;
        if((object)node == null)
            return false;
        return tile == node.tile;
    }
    
    public override int GetHashCode()
    {
        return tile.GetHashCode();
    }
}
