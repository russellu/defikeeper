using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
    manages the frame-by-frame interaction of the given creature (imp) with 
    environment (including navmesh, collisions, attack, etc.)

    imps still getting stuck, need to find a way to unstick them (due to navmesh agent not arriving all the way)
*/


public class CreatureBehavior : MonoBehaviour
{

    public AudioClip[] audios;

    private AudioSource source; 

    public Transform destination;
    private NavMeshAgent agent;
    private Animator anim;
    public bool seekingTile = false; 
    public Tile currentTile;
    private Vector3 destPlacement; 
    public int impId; 


    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>(); 
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

    }

    public Tile AssignDigTile(Tile tile)
    {
        Tile oldTile = currentTile; 

        destPlacement = WorkerManager.GetDigPosition(tile, transform.position);
        agent.destination = destPlacement;
        currentTile = tile;
        seekingTile = true;

        return oldTile; 
    }

    void Update()
    {
        anim.SetFloat("speed", agent.velocity.magnitude);

        if (seekingTile)
            if ((transform.position - currentTile.WorldCoords()).magnitude < 2.5f)
            {
                anim.SetBool("mining", true);
            }
    }

    void HitTile()
    {
        currentTile.hitPoints--;
        if (currentTile.hitPoints == 0)
        {
            anim.SetBool("mining", false);
            seekingTile = false;
            WorkerManager.FinishTile(currentTile, this);
        }
    }


    void Pickaxe()
    {
        source.PlayOneShot(audios[1]);

    }

    public void TumbleRocks()
    {
        source.PlayOneShot(audios[3]); 
    }

    void Foot()
    { 
    
    }
    
    

}
