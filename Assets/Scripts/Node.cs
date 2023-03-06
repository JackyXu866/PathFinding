using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Node Color: white: normal, red: start, green: end, blue: open, yellow: closed, grey: pending
public class Node
{
    public GameObject tile;
    public List<Node> neighbors = new List<Node>();
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
    public void AddWaypointConnection(ref Hashtable table, Node currentNode, Node targetNode)
    {
        //check if there is obstacle between current node and target node
        //use y = ax + b function and round up to see result
        //(0,0) --> (3,1)
        //check (0,0)(1,0)(2,1)(3,1)
        //float a = float(currentNode.y - targetNode.y) / float(currentNode.x - targetNode.x);
        //float b = a * currentNode.x - currentNode.y;
        for (int i = currentNode.x + 1; i < targetNode.x; i++)
        {
            for (int j = currentNode.y + 1; j < targetNode.y; j++)
            {
                if(LinePassesThroughGrid((float)currentNode.x, (float)currentNode.y, (float)targetNode.x, (float)targetNode.y, (float)i, (float)j, (float)1.0))
                {
                    Vector2 position = new Vector2(i, j);
                    Collider2D collider = Physics2D.OverlapPoint(position);
                    GameObject gameObject = collider.gameObject;
                    if (gameObject.CompareTag("obstacle"))
                    {
                        return;
                    }
                }
            }
        }
        ((Node)table[new Vector2(currentNode.x, currentNode.y)]).neighbors.Add(targetNode);
        ((Node)table[new Vector2(targetNode.x, targetNode.y)]).neighbors.Add(currentNode);
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

    public bool LinePassesThroughGrid(float x1, float y1, float x2, float y2, float gridCenterX, float gridCenterY, float gridSize)
    {
        float halfGridSize = gridSize / 2f;
        float xMin = Math.Min(x1, x2) - gridCenterX;
        float xMax = Math.Max(x1, x2) - gridCenterX;
        float yMin = Math.Min(y1, y2) - gridCenterY;
        float yMax = Math.Max(y1, y2) - gridCenterY;

        if (xMin < -halfGridSize && xMax > halfGridSize) return false; // Line is outside grid horizontally
        if (yMin < -halfGridSize && yMax > halfGridSize) return false; // Line is outside grid vertically
        if (xMin >= halfGridSize || xMax <= -halfGridSize) return false; // Line is entirely outside grid horizontally
        if (yMin >= halfGridSize || yMax <= -halfGridSize) return false; // Line is entirely outside grid vertically
        if (xMax - xMin == 0) return false; // Line is vertical and doesn't intersect with grid

        float slope = (y2 - y1) / (x2 - x1);
        float yAtX0 = y1 + slope * (0 - x1);
        float yAtX1 = y1 + slope * (gridSize - x1);

        if (yAtX0 >= -halfGridSize && yAtX0 <= halfGridSize) return true; // Line intersects with grid at x=0
        if (yAtX1 >= -halfGridSize && yAtX1 <= halfGridSize) return true; // Line intersects with grid at x=gridSize
        return false; // Line doesn't intersect with grid
    }
}
