using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class LoadMap : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown_Map, dropdown_Algorithm;
    [SerializeField] Slider time;
    [SerializeField] TMP_Text text_Time;
    [SerializeField] Toggle toggle_Waypoint;
    LineRenderer lineRenderer;
    TextAsset[] mapFiles;
    GameObject[] mapTiles;
    Hashtable mapTable = new Hashtable();
    Node startNode, endNode, pendingNode;
    int map_index = 0;

    int mapWidth, mapHeight;
    static Func<Node, Node, float> heuristic = null;

    // Start is called before the first frame update
    void Start()
    {
        mapFiles = Resources.LoadAll<TextAsset>("Maps");
        mapTiles = Resources.LoadAll<GameObject>("Tiles");
        lineRenderer = GetComponent<LineRenderer>();

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

        //create waypoint system

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
            if(pendingNode.tile.CompareTag("Walkable")){
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
            else if(Input.GetKeyDown(KeyCode.R)){
                // walkable
                if(pendingNode == startNode || pendingNode == endNode){}
                else{
                    GameObject tmp = Instantiate(mapTiles[0], pendingNode.tile.transform.position, Quaternion.identity);
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
                if(hit.collider.CompareTag("Walkable") || hit.collider.CompareTag("Obstacle"))
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

        //connect way points
        List<Vector2> waypointsData = new List<Vector2>();
        if (index == 0)
            waypointsData = create_database_ar0011sr();
        else if (index == 1)
            waypointsData = create_database_Arena2();
        else if (index == 2)
            waypointsData = create_database_hrt201n();
        else if (index == 3)
            waypointsData = create_database_lak104d();

        for(int i = 0; i < waypointsData.Count; i++)
        {
            for(int j = i+1; j < waypointsData.Count; j++)
                ((Node)mapTable[(Vector2) waypointsData[i]]).AddWaypointConnection(ref mapTable, (Node)mapTable[(Vector2)waypointsData[j]]);
                ((Node)mapTable[(Vector2) waypointsData[i]]).SetColor(Color.cyan);
        }

        map_index = index;
    }

    public void StartAStar(){
        StartCoroutine(Astar_tile());
    }

    IEnumerator Astar_tile(){
        if(startNode == null || endNode == null)
            yield break;

        // check if waypoint
        int way = toggle_Waypoint.isOn ? 1 : 0;

        if(way == 1){
            List<Vector2> waypointsData = new List<Vector2>();
            if (map_index == 0)
                waypointsData = create_database_ar0011sr();
            else if (map_index == 1)
                waypointsData = create_database_Arena2();
            else if (map_index == 2)
                waypointsData = create_database_hrt201n();
            else if (map_index == 3)
                waypointsData = create_database_lak104d();
            for(int i=0; i<waypointsData.Count; i++){
                startNode.AddWaypointConnection(ref mapTable, (Node)mapTable[(Vector2)waypointsData[i]]);
                endNode.AddWaypointConnection(ref mapTable, (Node)mapTable[(Vector2)waypointsData[i]]);
            }
        }

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

            foreach(Node n in q.GetNeighbor(way)){
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
                else if(closedList.Contains(n) && n.fCost >= newFCost)
                    closedList.Remove(n);

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
            yield return new WaitForSeconds(time.value);
        }


    }

    IEnumerator RetracePath(){
        Node tmp = endNode;
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, new Vector3(tmp.tile.transform.position.x, tmp.tile.transform.position.y, -1));
        while(tmp != startNode){
            tmp.SetColor(Color.red);
            tmp = tmp.parent;
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount-1, new Vector3(tmp.tile.transform.position.x, tmp.tile.transform.position.y, -1));
            yield return new WaitForSeconds(time.value);
        }
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount-1, new Vector3(startNode.tile.transform.position.x, startNode.tile.transform.position.y, -1));
    }

    public void UpdateTimer(){
        text_Time.text = Mathf.Round(time.value * 10f)/10f + "s";
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
        StopAllCoroutines();
        startNode = null;
        endNode = null;
        pendingNode = null;
        lineRenderer.positionCount = 0;
        
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        mapTable.Clear();
    }

    public void ResetMap(){
        StopAllCoroutines();
        foreach(Transform child in transform)
        {
            child.GetComponent<SpriteRenderer>().color = Color.white;
        }
        startNode = null;
        endNode = null;
        pendingNode = null;
        lineRenderer.positionCount = 0;
    }

    static float Heuristic_Manhattan(Node a, Node b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    static float Heuristic_Euclidean(Node a, Node b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
    }

    private List<Vector2> create_database_lak104d()
    {
        int[,] data = new int[,] {{18,2},{17,3},{13,2},{14,2},{11,1},{11,2},{1,4},{2,5},{6,4},{7,1},{7,4},{5,7},{6,8},{15,6},
        {17,10},{15,10},{12,9},{12,10},{8,11},{9,12},{4,9},{5,9},{2,10},{3,12},{11,14},{9,16},{10,17},{4,17}};
        List<Vector2> termsList = new List<Vector2>();
        for (int i = 0; i < data.GetLength(0); i++)
        {
            termsList.Add(new Vector2(data[i,0],data[i,1]));
        }
        return termsList;
    }

    private List<Vector2> create_database_Arena2()
    {
        int[,] data = new int[,] {{2,55},{7,55},{2,51},{7,51},{6,48},{11,51},{12,52},{15,52},{17,54},{18,55},{22,52},{34,52},
        {40,76},{40,68},{40,59},{40,52},{40,44},{40,36},{40,28},{56,76},{48,84},{48,76},{48,68},{48,59},{48,52},
        {48,44},{48,36},{48,28},{48,20},{56,68},{56,59},{56,52},{56,44},{56,36},{56,28},{56,20},{64,84},{64,76},
        {64,68},{64,59},{64,52},{64,44},{64,36},{64,28},{64,20},{72,84},{72,76},{72,68},{72,59},{72,52},{72,44},
        {72,36},{72,28},{72,20},{80,84},{80,76},{80,68},{80,59},{80,52},{88,44},{88,36},{88,28},{96,68},{96,59},
        {96,52},{96,44},{96,36},{102,52},{113,52},{118,52},{125,52},{128,57},{128,65},{123,72},{134,76},{128,46},
        {128,38},{134,32},{123,26},{115,21},{120,9},{126,9},{134,9},{64,89},{64,97},{59,99},{51,99}};
        List<Vector2> termsList = new List<Vector2>();
        for (int i = 0; i < data.GetLength(0); i++)
        {
            termsList.Add(new Vector2(data[i,0], data[i,1]));
        }
        return termsList;
    }

    private List<Vector2> create_database_hrt201n()
    {
        int[,] data = new int[,] {{22,148},{16,147},{16,142},{22,142},{20,137},{20,129},{20,126},{20,120},{8,126},{8,120},
        {4,125},{4,121},{4,117},{1,125},{30,123},{37,125},{31,120},{36,115},{8,108},{8,104},{8,100},{20,104},
        {33,107},{31,104},{33,102},{33,98},{20,91},{20,89},{20,74},{20,68},{12,68},{8,72},{4,68},{8,65},{28,68},
        {33,68},{42,68},{42,73},{42,62},{45,68},{49,68},{59,68},{59,58},{59,52},{52,52},{52,57},{31,57},{31,49},
        {33,42},{36,45},{38,52},{45,52},{45,44},{52,42},{65,83},{50,83},{50,78},{48,90},{48,97},{42,100},{42,105},
        {45,104},{48,105},{53,102},{42,84},{36,84},{36,89},{36,94},{30,91},{30,89},{32,85},{32,82},{36,80},{36,77},
        {30,77},{76,100},{75,108},{68,100},{65,96},{61,94},{58,98},{60,103},{59,110},{64,110},{69,110},{59,119},
        {64,119},{64,126},{84,83},{92,83},{92,90},{98,90},{87,94},{91,94},{96,94},{101,94},{101,93},{97,83},
        {104,83},{113,83},{121,85},{123,90},{123,98},{123,107},{131,107},{131,109},{131,111},{130,118},{123,118},
        {117,117},{117,110},{115,108},{123,113},{85,76},{92,76},{95,76},{83,73},{85,70},{85,62},{93,60},{97,68},
        {99,77},{99,75},{101,76},{102,61},{105,68},{108,76},{108,68},{108,60},{108,57},{125,33},{124,54},{124,58},
        {124,61},{133,61},{139,62},{141,68},{132,68},{122,68},{124,74},{124,77},{140,75},{93,53},{98,53},{101,53},
        {93,30},{87,29},{93,20},{98,19},{89,18},{88,16},{92,12},{97,13},{98,15},{107,29},{113,27},{119,29},{121,25},
        {102,35},{107,35},{112,36},{112,39},{112,42},{108,44},{104,42},{104,38},{86,51},{79,51},{73,51},{76,46},
        {76,43},{76,39},{76,36},{76,30},{76,23},{70,12},{81,12},{76,12},{70,7},{75,4},{79,5},{68,35},{65,39},
        {62,39},{60,35},{60,31},{57,32},{64,26},{64,20},{66,18},{65,18}};
        List<Vector2> termsList = new List<Vector2>();
        for (int i = 0; i < data.GetLength(0); i++)
        {
            termsList.Add(new Vector2(data[i,0], data[i,1]));
        }
        return termsList;
    }

    private List<Vector2> create_database_ar0011sr()
    {
        int[,] data = new int[,] {{21,19},{25,11},{35,9},{37,13},{38,16},{38,3},{44,3},{45,2},{10,34},{10,46},{8,52},{5,62},
        {6,68},{11,74},{10,81},{18,83},{26,89},{25,95},{30,96},{34,98},{44,95},{55,95},{63,94},{77,89},{81,93},
        {90,86},{90,78},{94,77},{97,65},{97,53},{99,46},{99,36},{92,32},{93,27},{83,24},{80,16},{76,14},{72,14},
        {71,8},{65,7},{61,5},{59,8},{59,13},{55,2},{24,82},{34,84},{39,75},{28,75},{24,67},{20,62},{25,51},{20,49},
        {33,53},{35,49},{37,47},{39,44},{37,42},{35,40},{33,38},{28,41},{42,43},{44,41},{46,39},{48,37},{46,35},
        {36,33},{41,29},{51,26},{56,31},{58,25},{63,21},{64,26},{62,31},{64,32},{65,34},{67,35},{63,45},{65,43},
        {67,41},{68,39},{69,37},{72,35},{75,32},{75,32},{75,32},{73,26},{81,27},{71,39},{73,40},{74,42},{76,43},
        {77,45},{82,33},{87,40},{82,44},{89,46},{84,51},{76,51},{83,62},{75,71},{80,69},{74,69},{77,77},{67,78},
        {67,69},{68,62},{66,64},{63,67},{60,69},{58,71},{56,73},{58,78},{50,80},{42,84}};
        List<Vector2> termsList = new List<Vector2>();
        for (int i = 0; i < data.GetLength(0); i++)
        {
            termsList.Add(new Vector2(data[i,0], data[i,1]));
        }
        return termsList;
    }
}
