using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureTask
{

    public static readonly string DIG_TILE = "DIG_TILE";
    public static readonly string CLAIM_TILE = "CLAIM_TILE";
    public static readonly string COLLECT_RESOURCE = "COLLECT_RESOURCE"; 

    List<TaskCheckPoint> taskCheckPoints;
    int currentCheckPointIndex;
    public int priority;
    public Tile tile;
    public string taskType;
    public bool complete;
    private GameObject resource; 

    public class TaskCheckPoint 
    {
        public Vector3 checkPointPosition; 
        public TaskCheckPoint(Vector3 position)
        {
            checkPointPosition = position; 
        }
    }

    public CreatureTask(Tile tile, string taskType, int priority)
    {
        taskCheckPoints = new List<TaskCheckPoint>();
        this.tile = tile;
        this.taskType = taskType; 
        this.priority = priority;

        if (taskType == DIG_TILE)
        {
            currentCheckPointIndex = 0;
            taskCheckPoints.Add(new TaskCheckPoint(tile.WorldCoords()));
        }
        else if (taskType == CLAIM_TILE)
        {
            currentCheckPointIndex = 0;
            taskCheckPoints.Add(new TaskCheckPoint(tile.WorldCoords()));
        }
        else if (taskType == COLLECT_RESOURCE)
        {
            currentCheckPointIndex = 0;
            taskCheckPoints.Add(new TaskCheckPoint(tile.WorldCoords()));
            taskCheckPoints.Add(new TaskCheckPoint(new Vector3(85,0,-87)));
        }
    }

    public void SetResource(GameObject resource)
    {
        this.resource = resource; 
    }

    public Vector3 CompleteCheckPoint()
    {
        if (taskType == COLLECT_RESOURCE)
        {
            resource.SetActive(false);
            currentCheckPointIndex++;
            return taskCheckPoints[currentCheckPointIndex].checkPointPosition;

        }
        else return Vector3.zero; 
    }

    public CreatureTask CompleteSelf(LevelMaker levelMaker, CreatureBehavior imp)
    {

        imp.currentTask = null; 

        if (taskType == DIG_TILE)
        {
            WorkerManager.RemoveTask(this);

            WorkerManager.AddGoldTask(tile); 

            WorkerManager.tileLabels[tile.x, tile.y].SetActive(false);
            WorkerManager.tilePrefabs[tile.x, tile.y].SetActive(false);
            Level.clearedTiles[tile.x, tile.y] = true;
            WorkerManager.OpenNeighbours(this);

            WorkerManager.floorTiles[tile.x, tile.y] = new FloorTile(tile.x, tile.y, tile.WorldCoords(),
                Object.Instantiate(levelMaker.unclaimedTile, tile.WorldCoords() - new Vector3(0, 0.5f, 0),
                Quaternion.identity), false);

            CreatureTask claimTask = new CreatureTask(tile, CLAIM_TILE, 1);
            complete = true;

            return claimTask;
        }
        else if (taskType == CLAIM_TILE)
        {
            WorkerManager.RemoveTask(this); 
            WorkerManager.floorTiles[tile.x, tile.y].Claim(levelMaker);
            complete = true; 
            return null;
        }
        else return null; 
    }

    public string GetTaskAnimatorVariable()
    {
        if (taskType.Equals(CreatureTask.DIG_TILE))
            return "mining";
        else if (taskType.Equals(CreatureTask.CLAIM_TILE))
            return "claiming";

        else return null; 
    
    }


    public bool Reachable()
    {
        if (taskType == DIG_TILE)
            return tile.reachable;

        else if (taskType == CLAIM_TILE)
            return true; 

        return false; 
    
    }

    public Vector3 CurrentPosition()
    {
        return taskCheckPoints[currentCheckPointIndex].checkPointPosition; 
    }

    public TaskCheckPoint GetNextTaskCheckPoint()
    {
        return taskCheckPoints[currentCheckPointIndex++];    
    }


}
