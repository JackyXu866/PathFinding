using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using System;

public class LoadMap : MonoBehaviour
{
    public TextAsset[] mapFiles;
    public GameObject[] mapTiles;
    Hashtable mapTable = new Hashtable();

    public int mapWidth, mapHeight;

    // Start is called before the first frame update
    void Start()
    {
        mapFiles = Resources.LoadAll<TextAsset>("Maps");
        mapTiles = Resources.LoadAll<GameObject>("Tiles");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.inputString != ""){
            int number;
            bool is_a_number = Int32.TryParse(Input.inputString, out number);
            if (is_a_number && number-1 >= 0 && number-1 < mapFiles.Length){
                MapCreate(number-1);
            }
        }
    }

    private byte[][] ParseMapFile(int index)
    {
        if(index < 0 || index >= mapFiles.Length)
        {
            return null;
        }

        TextAsset data = mapFiles[index];

        string[] lines = data.text.Split('\n');
        mapHeight = int.Parse(Regex.Replace(lines[1], @"\D", ""));
        mapWidth = int.Parse(Regex.Replace(lines[2], @"\D", ""));


        char[][] map = new char[mapHeight][]; // create 2D array

        for(int i = 0; i < mapHeight; i++)
        {
            map[i] = lines[(mapHeight - i - 1)+4].ToCharArray();
        }
        Debug.Log("mapWidth: " + map[0].Length + " mapHeight: " + map.Length);

        // convert each 2x2 tile to a single tile
        mapHeight = Mathf.FloorToInt((mapHeight) / 2);
        mapWidth = Mathf.FloorToInt((mapWidth) / 2);
        byte[][] map_byte = new byte[mapHeight][];
        for(int i = 0; i < mapHeight; i++)
        {
            map_byte[i] = new byte[mapWidth];
            for(int j = 0; j < mapWidth; j++)
            {
                byte[] tile = new byte[3];
                tile[GetTileNum(map[i*2][j*2])]++;
                tile[GetTileNum(map[i*2][j*2+1])]++;
                tile[GetTileNum(map[i*2+1][j*2])]++;
                tile[GetTileNum(map[i*2+1][j*2+1])]++;

                int max = Array.IndexOf(tile, tile.Max());
                map_byte[i][j] = (byte)max;
            }
        }
        

        return map_byte;
    }

    private void MapCreate(int index){
        CleanUp();

        byte[][] map = ParseMapFile(index);

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                byte tile = map[y][x];
                GameObject tiletmp = Instantiate(mapTiles[tile], new Vector3(x, y, 0), Quaternion.identity);
                tiletmp.transform.parent = transform;
                Node node = new Node();
                node.tile = tiletmp;
                mapTable.Add(new Vector2(x, y), node);
                node.AddNeighbor(ref mapTable, x, y);
            }
        }

        Camera.main.transform.position = new Vector3(mapWidth/2, mapHeight/2, -10);
    }

    int GetTileNum(char tile)
    {
        switch(tile)
        {
            case '.':
                return 0;
            case 'T':
                return 1;
            case '@':
                return 2;
            default:
                return 2;
        }
    }

    private void CleanUp()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        mapTable.Clear();
    }
}
