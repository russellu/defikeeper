using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * 
 * class to represent a cleared floor tile (after breaking rocks). can be claimed or not
 * 
 * claimable only if it is adjacent to a pre-existing claimed tile 
 * 
 */

public class FloorTile
{
    public bool claimed;
    public bool claimable;
    public bool beingClaimed; 
    public int x;
    public int y;
    public Vector3 worldPosition;
    public GameObject tileObject;


    public FloorTile(int x, int y, Vector3 worldPosition, GameObject tileObject, bool claimed)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPosition;
        this.tileObject = tileObject;
        this.claimed = claimed;

    }

    public void Claim(LevelMaker levelMaker)
    {
        claimed = true;
        LevelMaker.Destroy(tileObject);
        tileObject = LevelMaker.Instantiate(levelMaker.claimedTile, worldPosition - new Vector3(0, 0.5f, 0), Quaternion.identity); 
    }

}
