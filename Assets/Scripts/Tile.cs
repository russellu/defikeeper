using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile 
{

    public int x;
    public int y;
    public bool reachable;
    public GameObject labeledCube;
    public string currentImp;
    public bool occupied; // is there an impr currently working on this tile
    public int hitPoints; 

    public Tile(int x, int y, GameObject labeledCube, bool reachable)
    {
        this.x = x;
        this.y = y;
        this.labeledCube = labeledCube;
        this.reachable = reachable;
        hitPoints = 1;  
    }

    public Vector3 WorldCoords()
    {
        return labeledCube.transform.position; 
    }

}
