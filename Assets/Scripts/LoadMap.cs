using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using TMPro;
using UnityEngine.EventSystems;


public class LoadMap : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown_Map, dropdown_Algorithm;
    public TextAsset[] mapFiles;
    public GameObject[] mapTiles;
    Hashtable mapTable = new Hashtable();
    Node startNode, endNode, pendingNode;

    public int mapWidth, mapHeight;
    static Func<Node, Node, float> heuristic = null;

    // Start is called before the first frame update
    void Start()
    {
        mapFiles = Resources.LoadAll<TextAsset>("Maps");
        mapTiles = Resources.LoadAll<GameObject>("Tiles");

        // dropdown_Map
        dropdown_Map.ClearOptions();
        List<string> options = new List<string>();
        foreach(TextAsset map in mapFiles)
        {
            options.Add(map.name);
        }  
        dropdown_Map.AddOptions(options);
        dropdown_Map.onValueChanged.AddListener(delegate {
            MapCreate(dropdown_Map.value);
        });
        MapCreate(0);

        // dropdown_Algorithm
        dropdown_Algorithm.ClearOptions();
        options = new List<string>();
        options.Add("Manhattan");
        options.Add("Euclidean");
        dropdown_Algorithm.AddOptions(options);
        dropdown_Algorithm.onValueChanged.AddListener(delegate {
            switch(dropdown_Algorithm.value){
                case 0:
                    heuristic = Heuristic_Manhattan;
                    break;
                case 1:
                    heuristic = Heuristic_Manhattan;
                    break;
            }
        });
        heuristic = Heuristic_Manhattan;
    }

    // Update is called once per frame
    void Update()
    {
        if(EventSystem.current.IsPointerOverGameObject())
            return;
        if(pendingNode != null){
            if(Input.GetKeyDown(KeyCode.S)){
                // start 
                if(startNode != null)
                    startNode.SetColor(Color.white);
                startNode = pendingNode;
                pendingNode = null;
                startNode.SetColor(Color.red);
            }
            else if(Input.GetKeyDown(KeyCode.E)){
                // end
                if(endNode != null)
                    endNode.SetColor(Color.white);
                endNode = pendingNode;
                pendingNode = null;
                endNode.SetColor(Color.green);
            }
            else if(Input.GetKeyDown(KeyCode.O)){
                // obstacle
                if(pendingNode == startNode || pendingNode == endNode){}
                else{
                    GameObject tmp = Instantiate(mapTiles[1], pendingNode.tile.transform.position, Quaternion.identity);
                    tmp.transform.parent = transform;
                    Destroy(pendingNode.tile);
                    pendingNode.ChangeTile(tmp);
                    pendingNode = null;
                }
            }
        }

        if(Input.GetButtonDown("Fire1")){
            Vector3 mousePos = Input.mousePosition;
            {
                Ray ray = Camera.main.ScreenPointToRay(mousePos);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                if(hit.collider.CompareTag("Walkable"))
                {
                    int x = Mathf.FloorToInt(hit.collider.transform.position.x);
                    int y = Mathf.FloorToInt(hit.collider.transform.position.y);
                    Node tmp = (Node)mapTable[new Vector2(x, y)];
                    if(tmp == null || tmp == startNode || tmp == endNode){}
                    else if(pendingNode == null){
                        pendingNode = tmp;
                        pendingNode.SetColor(Color.grey);
                    }
                    else if(tmp != pendingNode){
                        pendingNode.SetColor(Color.white);
                        pendingNode = tmp;
                        pendingNode.SetColor(Color.grey);
                    }
                    else if(pendingNode == tmp){
                        pendingNode.SetColor(Color.white);
                        pendingNode = null;
                    }
                }

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
                node.ChangeTile(tiletmp);
                mapTable.Add(new Vector2(x, y), node);
                node.AddNeighbor(ref mapTable, x, y);
            }
        }

        Camera.main.transform.position = new Vector3(mapWidth/2, mapHeight/2, -10);
        Camera.main.orthographicSize = Mathf.Min(mapWidth, mapHeight)/4;
    }

    public void StartAStar(){
        StartCoroutine(Astar_tile());
    }

    IEnumerator Astar_tile(){
        if(startNode == null || endNode == null)
            yield break;
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();
        openList.Add(startNode);
        
        while(openList.Count > 0){
            Node q = null;
            foreach (Node n in openList){
                if(q == null || n.fCost < q.fCost){
                    q = n;
                }
            }
            openList.Remove(q);

            foreach(Node n in q.neighbors){
                if(n.tile.CompareTag("Obstacle"))
                    continue;
                if(n == endNode){
                    endNode.parent = q;
                    Debug.Log("Found path");
                    yield return StartCoroutine(RetracePath());
                    yield break;
                }
                float newGCost = q.gCost + heuristic(n, q);
                float newHCost = heuristic(n, endNode);
                float newFCost = newGCost + newHCost;

                if(openList.Contains(n) && n.fCost < newFCost)
                    continue;
                if(closedList.Contains(n) && n.fCost < newFCost)
                    continue;

                n.gCost = newGCost;
                n.hCost = newHCost;
                n.fCost = newFCost;
                n.parent = q;

                if(!openList.Contains(n)){
                    openList.Add(n);
                    n.SetColor(Color.blue);
                }
            }
            closedList.Add(q);
            q.SetColor(Color.yellow);
            yield return new WaitForSeconds(0.1f);
        }


    }

    IEnumerator RetracePath(){
        Node tmp = endNode;
        while(tmp != startNode){
            tmp.SetColor(Color.red);
            tmp = tmp.parent;
            yield return new WaitForSeconds(0.1f);
        }
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

    static float Heuristic_Manhattan(Node a, Node b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    static float Heuristic_Euclidean(Node a, Node b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
    }
}
