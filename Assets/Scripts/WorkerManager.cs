using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/* 
    manages interaction between the user's interaction with the level (selecting tiles)
    and the worker creatures that will perform actions on the level

*/


public class WorkerManager
{
    // available tiles

    // all tiles


    static List<CreatureBehavior> imps;
    static GameObject[,] tileLabels;
    static GameObject[,] tilePrefabs;
    static LevelMaker levelMaker;

    static Dictionary<string, Tile> pendingTiles;

    public static void InitImps(Vector3 bottomLeft, int nStartingImps, GameObject impPrefab)
    {
        imps = new List<CreatureBehavior>();

        for (int i = 0; i < nStartingImps; i++)
        {
            Vector3 offset = new Vector3(0, 0, 0);
            offset += new Vector3(Random.Range(0, 4), 0, Random.Range(0, 4));
            CreatureBehavior imp = LevelMaker.Instantiate(
                impPrefab, bottomLeft + offset, Quaternion.identity).GetComponent<CreatureBehavior>();
            imps.Add(imp);
            imp.impId = i;
        }

    }

    public static void InitTiles(GameObject[,] tileLabels, GameObject[,] tilePrefabs, LevelMaker levelMaker)
    {
        WorkerManager.tileLabels = tileLabels;
        WorkerManager.tilePrefabs = tilePrefabs;
        WorkerManager.levelMaker = levelMaker;
        pendingTiles = new Dictionary<string, Tile>();
    }

    public static void AddTile(Tile tile)
    {
        pendingTiles.Add(tile.x + " " + tile.y, tile);
        if (tile.reachable)
            NotifyImpsDig(tile);
    }

    // get the closest idle imp to the tile (if it exists)
    public static void NotifyImpsDig(Tile newTile)
    {
        CreatureBehavior bestImp = null;
        float shortest = 99999999;

        foreach (CreatureBehavior imp in imps)
        {
            float distance = (imp.transform.position - newTile.WorldCoords()).magnitude;

            if (distance < shortest)
            {
                if (!imp.seekingTile) // imp is unoccupied
                {
                    shortest = distance;
                    bestImp = imp;
                }

                else // imp is occupied
                {
                    float impCurrentDistance = (imp.currentTile.WorldCoords() - imp.transform.position).magnitude;
                    if (distance < impCurrentDistance)
                    {
                        shortest = distance;
                        bestImp = imp;
                    }
                }
            }
        }
        if (bestImp != null)
        {

            if (bestImp.seekingTile)
            {
                Tile temp = bestImp.currentTile;
                AssignDigTileToImp(newTile, bestImp);
                NotifyImpsDig(temp);
            }
            else 
            {
                AssignDigTileToImp(newTile, bestImp);
            }
        }
    }

    public static void AssignDigTileToImp(Tile tile, CreatureBehavior imp)
    {
        pendingTiles.Remove(tile.x + " " + tile.y);
        Tile oldTile = imp.AssignDigTile(tile);
        if (oldTile != null)
            pendingTiles.Add(oldTile.x + " " + oldTile.y, oldTile);
    }

    public static void FinishTile(Tile tile, CreatureBehavior imp)
    {
        tileLabels[tile.x, tile.y].SetActive(false);
        tilePrefabs[tile.x, tile.y].SetActive(false);
        Level.clearedTiles[tile.x, tile.y] = true;
        imp.currentTile = null;
        LevelMaker.Instantiate(levelMaker.rockBreak, tile.WorldCoords(), Quaternion.identity);
        imp.TumbleRocks(); 
        AssignImpNewTile(imp);
        OpenNeighbours(tile);
    }

    private static bool Adjacent(Tile t1, Tile t2) // return true if t1 and t2 adjacent
    {
        if ((t1.x == t2.x && t1.y == t2.y - 1) || // t1 below t2
            (t1.x == t2.x && t1.y == t2.y + 1) || // t1 above t2
            (t1.x == t2.x - 1 && t1.y == t2.y) || // t1 left of t2
            (t1.x == t2.x + 1 && t1.y == t2.y))   // t1 right of t2
            return true;
        else 
            return false;   
    }


    public static void OpenNeighbours(Tile tile)
    {
        List<Tile> opens = new List<Tile>();
        foreach (KeyValuePair<string, Tile> entry in pendingTiles)
        {
            if (!entry.Value.reachable && Adjacent(tile, entry.Value))
            {
                opens.Add(entry.Value); 
            } 
        }
        foreach (Tile t in opens)
        {
            t.reachable = true; 
            NotifyImpsDig(t);
        }
    }

    public static void AssignImpNewTile(CreatureBehavior imp)
    {
        Tile closest = null;
        float shortest = 99999999;

        foreach (KeyValuePair<string, Tile> entry in pendingTiles)
        {
            if((entry.Value.WorldCoords() - imp.transform.position).magnitude < shortest)
            {
                closest = entry.Value;
                shortest = (entry.Value.WorldCoords() - imp.transform.position).magnitude; 
            }
        }

        if (closest != null)
            AssignDigTileToImp(closest, imp);    
    }


    public static Vector3 GetDigPosition(Tile tile, Vector3 impPosition)
    {
        bool[,] clearedTiles = Level.clearedTiles;
        List<Vector3> available = new List<Vector3>(); 

        bool[] surrounding = {clearedTiles[tile.x + 1, tile.y],     //right
                                clearedTiles[tile.x - 1, tile.y],   //left
                                clearedTiles[tile.x, tile.y + 1],   //upper
                                clearedTiles[tile.x, tile.y - 1]};  //lower

        float distAway = 1.5f;

        if (surrounding[0])
            available.Add(tile.WorldCoords() + new Vector3(-distAway, 0, 0));
        if (surrounding[1])
            available.Add(tile.WorldCoords() + new Vector3(distAway, 0, 0));
        if (surrounding[2])
            available.Add(tile.WorldCoords() + new Vector3(0, 0, -distAway));
        if (surrounding[3])
            available.Add(tile.WorldCoords() + new Vector3(0, 0, distAway));

        float minDist = 999999;
        Vector3 destination = tile.WorldCoords(); 
        for (int i = 0; i < available.Count; i++)
            if (Vector3.Magnitude(impPosition - available[i]) < minDist)
            {
                minDist = Vector3.Magnitude(impPosition - available[i]);
                destination = available[i];
            }

        return destination; 

    }

}
