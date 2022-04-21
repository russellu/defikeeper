using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level {

    public Vector3 lowestLeft; 

    private int[,] levelMarkers;
    public static bool[,] clearedTiles;
    private GameObject[,] levelPrefabs;
    private LevelMaker maker; 

    int[] WATER = { 63, 72, 204 };
    int[] GOLD = { 255, 201, 14 };
    int[] ROCK = { 127, 127, 127 };
    int[] DIRT = { 0, 0, 0 };
    int[] DEEP_WATER = { 163, 73, 164 };
    int[] SOFT_ROCK = { 195,195,195};
    int[] BASE = { 255, 174, 201 };
    int[] PORTAL = { 237, 28, 36 }; 

    public Level(Texture2D lvlTex, LevelMaker maker)
    {
        this.maker = maker; 
        levelPrefabs = new GameObject[lvlTex.width, lvlTex.height];
        clearedTiles = new bool[lvlTex.width, lvlTex.height]; 
        levelMarkers = new int[lvlTex.width, lvlTex.height];

        ParseLvlTex(lvlTex); 
    }

    public int[,] GetLevelMarkers()
    {
        return levelMarkers; 
    }

    public GameObject[,] GetLevelPrefabs()
    {
        return levelPrefabs; 
    }

    public bool[,] GetClearedTiles()
    {
        return clearedTiles; 
    }

    void ParseLvlTex(Texture2D lvlTex)
    {

        lowestLeft = new Vector3(99999, 0, 99999); 

        for (int i = 0; i < lvlTex.width; i++)
            for (int j = 0; j < lvlTex.height; j++)
            {
                Color pix = lvlTex.GetPixel(i, j);
                int[] color = { (int)(pix.r * 255), (int)(pix.g * 255), (int)(pix.b * 255) };

                Vector3 position = new Vector3(128 - i*2 - 1, 0.5f, 128 - j*2 - 1);

                if (color[0] == WATER[0] && color[1] == WATER[1] && color[2] == WATER[2])
                {
                    levelPrefabs[i, j] = LevelMaker.Instantiate(maker.waterCube, position, Quaternion.identity);
                    levelMarkers[i, j] = 1;
                }
                else if (color[0] == GOLD[0] && color[1] == GOLD[1] && color[2] == GOLD[2])
                {
                    levelPrefabs[i, j] = LevelMaker.Instantiate(maker.goldCube, position, Quaternion.identity);
                    levelMarkers[i, j] = 0;
                }
                else if (color[0] == ROCK[0] && color[1] == ROCK[1] && color[2] == ROCK[2])
                {
                    levelPrefabs[i, j] = LevelMaker.Instantiate(maker.rockCube, position, Quaternion.identity);
                    levelMarkers[i, j] = 2;
                }
                else if (color[0] == DIRT[0] && color[1] == DIRT[1] && color[2] == DIRT[2])
                {
                    levelPrefabs[i, j] = LevelMaker.Instantiate(maker.dirtCube, position, Quaternion.identity);
                    levelMarkers[i, j] = 0;
                }
                else if (color[0] == DEEP_WATER[0] && color[1] == DEEP_WATER[1] && color[2] == DEEP_WATER[2])
                {
                    levelPrefabs[i, j] = LevelMaker.Instantiate(maker.deepWaterCube, position, Quaternion.identity);
                    levelMarkers[i, j] = 0;
                }
                else if (color[0] == SOFT_ROCK[0] && color[1] == SOFT_ROCK[1] && color[2] == SOFT_ROCK[2])
                {
                    levelPrefabs[i, j] = LevelMaker.Instantiate(maker.softRockCube, position, Quaternion.identity);
                    levelMarkers[i, j] = 0;
                }
                else if (color[0] == BASE[0] && color[1] == BASE[1] && color[2] == BASE[2])
                {
                    levelMarkers[i, j] = 0;
                    clearedTiles[i, j] = true;
                    if (position.x < lowestLeft.x && position.z < lowestLeft.z)
                        lowestLeft = position; 
                }
                else if (color[0] == PORTAL[0] && color[1] == PORTAL[1] && color[2] == PORTAL[2])
                {
                    levelMarkers[i, j] = 0;
                    clearedTiles[i, j] = true;
                }
            }

    }


}
