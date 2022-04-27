using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/* 
    manages interaction between the user's interaction with the level (selecting tiles)
    and the worker creatures that will perform actions on the level

    IMPROVEMENTS:     

    1) make the sublists in the priority queue into stacks instead (so most recent tile is always checked first) 
        this ensures imp will always go to closest tile when new one is added

    2) make imps able to 'preempt' other imps and steal their tasks (if beneficial to do so). 
        allow imps to trade tasks which means a closer imp can finish the job faster

*/


public class WorkerManager
{
    public static List<CreatureBehavior> imps;

    public static List<CreatureTask> unReachableTasks;
    public static List<List<CreatureTask>> tasks; 

    public static GameObject[,] tileLabels;
    public static GameObject[,] tilePrefabs;
    public static FloorTile[,] floorTiles;
    public static LevelMaker levelMaker;

    public static void InitImps(Vector3 bottomLeft, int nStartingImps, GameObject impPrefab)
    {
        imps = new List<CreatureBehavior>();
        unReachableTasks = new List<CreatureTask>();
        tasks = new List<List<CreatureTask>>();
        tasks.Add(new List<CreatureTask>());
        tasks.Add(new List<CreatureTask>()); 


        for (int i = 0; i < nStartingImps; i++)
        {
            Vector3 offset = new Vector3(0, 0, 0);
            offset += new Vector3(Random.Range(0, 4000)/1000f, 0, Random.Range(0, 4000)/1000f);
            CreatureBehavior imp = LevelMaker.Instantiate(
                impPrefab, bottomLeft + offset, Quaternion.identity).GetComponent<CreatureBehavior>();

            imp.levelMakerInstance = levelMaker; 
            imps.Add(imp);
        }
    }

    public static void InitTiles(GameObject[,] tileLabels, GameObject[,] tilePrefabs, FloorTile[,] floorTiles, LevelMaker levelMaker)
    {
        WorkerManager.tileLabels = tileLabels;
        WorkerManager.tilePrefabs = tilePrefabs;
        WorkerManager.levelMaker = levelMaker;
        WorkerManager.floorTiles = floorTiles;
    }

    public static void AddGoldTask(Tile dugTile)
    {
        GameObject gold = Object.Instantiate(levelMaker.dugGold, dugTile.WorldCoords(), Quaternion.identity);
        CreatureTask goldTask = new CreatureTask(dugTile, CreatureTask.COLLECT_RESOURCE, 0);
        goldTask.SetResource(gold); 

    }

    public static void AddTask(CreatureTask newTask)
    {
        if (!newTask.Reachable())
        {
            unReachableTasks.Add(newTask);          
        }
        else
        {
            tasks[newTask.priority].Add(newTask);
            ScheduleTasks(); 
        }
    }

    public static void RemoveTask(CreatureTask task)
    {
        tasks[task.priority].Remove(task); 
    }

    public static int NumberOfRemainingTasks()
    {
        int remaining = 0;
        for (int i = 0; i < tasks.Count; i++)
            remaining += tasks[i].Count;

        return remaining; 
    }

    public static CreatureTask HighestPriorityTask()
    {

        Vector3 idleImpPosition = Vector3.zero;
        bool foundIdleImp = false; 
        for (int i = 0; i < imps.Count; i++)
            if (imps[i].currentTask == null)
            {
                foundIdleImp = true; 
                idleImpPosition = imps[i].transform.position;
                break; 
            }

        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].Count > 0)
                if (!foundIdleImp)
                    return tasks[i][tasks[i].Count - 1];
                else
                {
                    float dist = 999999f;
                    CreatureTask closest = null; 
                    for (int j = 0; j < tasks[i].Count; j++)
                    {
                        float currDist = (idleImpPosition - tasks[i][j].CurrentPosition()).magnitude;
                        if (currDist < dist)
                        {
                            dist = currDist;
                            closest = tasks[i][j]; 
                        }
                    }
                    return closest; 
                }
        }
        return null; 
    }


    public static void ScheduleTasks()
    {
        bool allImpsRejected = false; 

        while (NumberOfRemainingTasks() > 0 && !allImpsRejected)
        {
            CreatureTask highestPriorityTask = HighestPriorityTask();

            float minImpDistance = 999999f;
            CreatureBehavior closestImp = null;

            foreach (CreatureBehavior imp in imps)
            {
                float impDist = imp.DistanceToTask(highestPriorityTask); 

                if (impDist < minImpDistance && imp.PrefersTask(highestPriorityTask))
                {
                    closestImp = imp;
                    minImpDistance = impDist;
                }
            }
            if (closestImp != null)
            {
                CreatureTask oldTask = closestImp.SetNewTaskAndReturnOldTask(highestPriorityTask);
                RemoveTask(highestPriorityTask);

                if (oldTask != null)
                {
                    AddTask(oldTask);
                }
            }
            else allImpsRejected = true;
        }
    }


    public static void OpenNeighbours(CreatureTask task)
    {
        List<CreatureTask> removeTasks = new List<CreatureTask>(); 
        for (int i=0;i<unReachableTasks.Count;i++)
        {
            if (Adjacent(task.tile, unReachableTasks[i].tile))
            {
                unReachableTasks[i].tile.reachable = true;
                AddTask(unReachableTasks[i]);
                removeTasks.Add(unReachableTasks[i]); 
            }
        }
        foreach (CreatureTask removeTask in removeTasks)
            unReachableTasks.Remove(removeTask); 
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

    public static bool CheckClaimable(int x, int y)
    {
        if (floorTiles[x + 1, y].claimed || 
            floorTiles[x - 1, y].claimed || 
            floorTiles[x, y + 1].claimed || 
            floorTiles[x, y - 1].claimed)
            return true;
        else return false; 
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





/*

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
    //LevelMaker.Instantiate(levelMaker.rockBreak, tile.WorldCoords(), Quaternion.identity);
    //imp.TumbleRocks(); 

    bool claimable = CheckClaimable(tile.x, tile.y);

    floorTiles[tile.x, tile.y] = new FloorTile(tile.x, tile.y, tile.WorldCoords(),
                LevelMaker.Instantiate(levelMaker.unclaimedTile, tile.WorldCoords() - new Vector3(0,0.5f,0), Quaternion.identity),
                false);

    if (claimable)
    {
        floorTiles[tile.x, tile.y].claimable = true; 
        pendingClaims.Add(tile.x + " " + tile.y, floorTiles[tile.x, tile.y]);
    }

    AssignImpNewTile(imp);
    OpenNeighbours(tile);
}

public static void FinishClaim(FloorTile tile, CreatureBehavior imp)
{
    floorTiles[tile.x, tile.y].Claim(levelMaker);
    imp.currentClaimTile = null;
    UpdateSurroundingClaimTiles(tile.x, tile.y);
    AssignImpNewTile(imp); 
}

public static void AssignImpNewTile(CreatureBehavior imp)
{

    Tile closest = null;
    float shortest = 99999999;

    foreach (KeyValuePair<string, Tile> entry in pendingTiles)
    {
        if((entry.Value.WorldCoords() - imp.transform.position).magnitude < shortest && entry.Value.reachable)
        {
            closest = entry.Value;
            shortest = (entry.Value.WorldCoords() - imp.transform.position).magnitude; 
        }
    }

    if (closest != null) // found a new brick to work on
        AssignDigTileToImp(closest, imp);
    else if (!imp.seekingClaim)// no more bricks available, look for floor tiles to claim
    {
        AssignImpClaimTile(imp); 
    }

}

public static void AssignImpClaimTile(CreatureBehavior imp)
{

    FloorTile closest = null;
    float shortest = 99999999;

    foreach (KeyValuePair<string, FloorTile> entry in pendingClaims)
    {
        float dist = (imp.transform.position - entry.Value.worldPosition).magnitude;
        if (dist < shortest && entry.Value.claimable && !entry.Value.beingClaimed)
        {
            shortest = dist;
            closest = entry.Value; 
        }
    }

    if(closest != null) 
    {
        closest.beingClaimed = true;
        pendingClaims.Remove(closest.x + " " + closest.y);
        imp.AssignClaimTile(closest);
    }

}




public static void ClaimWithClosestImp(FloorTile floorTile)
{
    CreatureBehavior closestImp = null;
    float distance = 9999999;

    for (int j = 0; j < imps.Count; j++)
        if (!imps[j].seekingClaim && !imps[j].seekingTile)
            if ((imps[j].transform.position - floorTile.worldPosition).magnitude < distance)
            {
                distance = (imps[j].transform.position - floorTile.worldPosition).magnitude;
                closestImp = imps[j]; 
            }

    if (closestImp != null && !floorTile.beingClaimed)
    {
        pendingClaims.Remove(floorTile.x + " " + floorTile.y); 
        closestImp.AssignClaimTile(floorTile);
    }
}



public static void UpdateSurroundingClaimTiles(int x, int y)
{
    floorTiles[x + 1, y].claimable = true;
    floorTiles[x - 1, y].claimable = true;
    floorTiles[x, y + 1].claimable = true;
    floorTiles[x, y - 1].claimable = true;

    int[,] inds = { { x + 1, y }, { x - 1, y }, { x, y + 1 }, { x, y - 1 } };
    for (int i = 0; i < 4; i++)
    {
        if (Level.clearedTiles[inds[i, 0], inds[i, 1]] &&
                !floorTiles[inds[i, 0], inds[i, 1]].claimed &&
                !pendingClaims.ContainsKey(inds[i, 0] + " " + inds[i, 1]))
        {
            pendingClaims.Add(inds[i, 0] + " " + inds[i, 1], floorTiles[inds[i, 0], inds[i, 1]]);
            ClaimWithClosestImp(floorTiles[inds[i,0],inds[i,1]]); 


        }
    }
}



 while (NumberOfRemainingTasks() > 0 && !allImpsRejected)
        {


            // this should also be closest to the imps? 
            CreatureTask highestPriorityTask = HighestPriorityTask();


            float minImpDistance = 999999f;
            CreatureBehavior closestImp = null;

            foreach (CreatureBehavior imp in imps)
            {
                float impDist = imp.DistanceToTask(highestPriorityTask);

                if (impDist < minImpDistance && imp.PrefersTask(highestPriorityTask))
                {
                    closestImp = imp;
                    minImpDistance = impDist;
                }
            }
            if (closestImp != null)
            {
                CreatureTask oldTask = closestImp.SetNewTaskAndReturnOldTask(highestPriorityTask);
                RemoveTask(highestPriorityTask);

                if (oldTask != null)
                {
                    AddTask(oldTask);
                }
            }
            else allImpsRejected = true;
        }


*/
