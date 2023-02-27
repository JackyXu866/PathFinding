using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class LoadMap : MonoBehaviour
{
    public TextAsset[] mapFiles;
    public GameObject[] mapTiles;

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
        if(Input.GetKeyDown(KeyCode.Alpha1)) MapCreate(3);
    }

    private char[][] ParseMapFile(int index)
    {
        if(index < 0 || index >= mapFiles.Length)
        {
            return null;
        }

        TextAsset data = mapFiles[index];

        string[] lines = data.text.Split('\n');
        mapWidth = int.Parse(Regex.Replace(lines[1], @"\D", ""));
        mapHeight = int.Parse(Regex.Replace(lines[2], @"\D", ""));


        char[][] map = new char[mapHeight][]; // create 2D array

        for(int i = 0; i < mapHeight; i++)
        {
            map[i] = lines[(mapHeight - i - 1)+4].ToCharArray();
        }

        return map;
    }

    private void MapCreate(int index){
        char[][] map = ParseMapFile(index);

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                char tile = map[y][x];
                if(tile == '.'){
                    GameObject tiletmp = Instantiate(mapTiles[0], new Vector3(x, y, 0), Quaternion.identity);
                    tiletmp.transform.parent = transform;
                }
                else if(tile == 'T'){
                    GameObject tiletmp = Instantiate(mapTiles[1], new Vector3(x, y, 0), Quaternion.identity);
                    tiletmp.transform.parent = transform;
                }
            }
        }
    }
}
