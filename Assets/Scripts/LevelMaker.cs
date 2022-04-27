using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMaker : MonoBehaviour
{

    public Texture2D lvlTex;
    public GameObject plane; 
    public GameObject waterCube;
    public GameObject deepWaterCube; 
    public GameObject goldCube;
    public GameObject rockCube;
    public GameObject softRockCube;
    public GameObject dirtCube;
    public GameObject dirtTag;
    public GameObject tileIndicator;
    public GameObject claimedTile;
    public GameObject unclaimedTile; 
    public GameObject rockBreak; 
    public GameObject impPrefab;
    public GameObject openTileIndicator;
    public GameObject dugGold; 

    public int[,] markerTiles;
    GameObject[,] markerObjects; 
    static Level level;
    bool tagging = false;
    
    // Start is called before the first frame update
    void Start()
    {
        level = new Level(lvlTex, this);
        markerTiles = new int[lvlTex.width, lvlTex.height];
        markerObjects = new GameObject[lvlTex.width, lvlTex.height];
        WorkerManager.InitTiles(markerObjects, level.GetLevelPrefabs(), Level.floorTiles, this);
        WorkerManager.InitImps(level.lowestLeft, 8, impPrefab);
    }
    
    // Update is called once per frame
    void Update()
    {
        int[] inds = GetGridRaycast();
        int[,] levelMarkers = level.GetLevelMarkers();
        GameObject[,] levelPrefabs = level.GetLevelPrefabs();

        if (inds != null)
        {
            int x = inds[0];
            int y = inds[1];

            if (Input.GetMouseButtonDown(0))
            {
                if (markerTiles[x, y] == 0 && tagging == false)
                {
                    tagging = true;
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if (markerTiles[x, y] == 0 && tagging == true)
                {
                    markerObjects[x,y] = 
                        Instantiate(dirtTag, levelPrefabs[x, y].transform.position + new Vector3(0, .51f, 0), Quaternion.identity);
                    markerTiles[x, y] = 1;

                    bool reachable = CheckIfAvailable(x, y);
                    WorkerManager.AddTask(new CreatureTask(new Tile(x, y, levelPrefabs[x, y], reachable), CreatureTask.DIG_TILE, 0));

                }
                else if (markerTiles[x, y] == 1 && tagging == false)
                {                
                    markerObjects[x, y].SetActive(false);
                    markerTiles[x, y] = 0; 
                }
            }
            else if (tagging == true)
            {
                tagging = false;
            }
        }
    }


    private int[] GetGridRaycast()
    {
        LayerMask mask = LayerMask.GetMask("terrain"); 

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            if (hit.transform.tag == "Terrain")
            {
                Vector3 t = hit.transform.gameObject.transform.position;
                int arrIndX = (int)((128 - t.x - 1)/2); 
                int arrIndY = (int)((128 - t.z - 1)/2);           
                return new int[] { arrIndX, arrIndY };
            }
        }
        return null;
    }





   public bool CheckIfAvailable(int x, int y)
    {
        /*
            x=0 and y=0
            x=0 and y=height-1
            x=0 and y=anything
            y=0 and x=width-1
            y=0 and x=anything
            x=width-1 and y=height-1
        */

        bool[,] clearedTiles = level.GetClearedTiles();
        int width = lvlTex.width;
        int height = lvlTex.height;

        // get indicator

        if (x == 0 && y == 0)
        {
            if (clearedTiles[x + 1, y] || clearedTiles[x, y + 1])
                return true;
        }
        else if (x == 0 && y == height - 1)
        {
            if (clearedTiles[x + 1, y] || clearedTiles[x, y - 1])
                return true;
        }
        else if (x == 0)
        {
            if (clearedTiles[x + 1, y] || clearedTiles[x, y + 1] || clearedTiles[x, y - 1])
                return true;

        }
        else if (y == 0 && x == width - 1)
        {
            if (clearedTiles[x, y + 1] || clearedTiles[x - 1, y])
                return true;

        }
        else if (y == 0)
        {
            if (clearedTiles[x - 1, y] || clearedTiles[x + 1, y] || clearedTiles[x, y + 1])
                return true;
        }
        else if (x == width - 1 && y == height - 1)
        {
            if (clearedTiles[x, y - 1] || clearedTiles[x - 1, y])
                return true;
        }
        else if (y == height - 1)
        {
            if (clearedTiles[x, y - 1] || clearedTiles[x - 1, y] || clearedTiles[x + 1, y])
                return true;
        }
        else if (x == width - 1)
        {
            if (clearedTiles[x - 1, y] || clearedTiles[x, y - 1] || clearedTiles[x, y + 1])
                return true;
        }
        else if (clearedTiles[x - 1, y] || clearedTiles[x, y - 1] || clearedTiles[x + 1, y] || clearedTiles[x, y + 1])
            return true;
        
        return false; 
    }


}
