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
    public CreatureTask currentTask;
    public LevelMaker levelMakerInstance; 


    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>(); 
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    public float DistanceToTask(CreatureTask newTask)
    {
        return (transform.position - newTask.CurrentPosition()).magnitude;      
    }

    public bool PrefersTask(CreatureTask newTask)
    {

        if (currentTask == null)
        {
            return true;
        }
        else if ((newTask.priority <= currentTask.priority &&
                (transform.position - newTask.CurrentPosition()).magnitude <
                (transform.position - currentTask.CurrentPosition()).magnitude)
                || newTask.priority < currentTask.priority)
        {
            CancelCurrentTask();
            return true;
        }
        else
        {
            return false;
        }
    }

    private void CancelCurrentTask()
    {
        if (currentTask.taskType == CreatureTask.CLAIM_TILE)
        {
            anim.SetBool(currentTask.GetTaskAnimatorVariable(), false);
        }
        else if (currentTask.taskType == CreatureTask.DIG_TILE)
        {
            anim.SetBool(currentTask.GetTaskAnimatorVariable(), false); 
        }
    }

    public CreatureTask SetNewTaskAndReturnOldTask(CreatureTask newTask)
    {
        CreatureTask oldTask = currentTask;
        currentTask = newTask;

        if (newTask.taskType == CreatureTask.DIG_TILE)
            agent.destination = WorkerManager.GetDigPosition(currentTask.tile, transform.position);

        else if (newTask.taskType == CreatureTask.CLAIM_TILE)
            agent.destination = newTask.CurrentPosition();

        else if (newTask.taskType == CreatureTask.COLLECT_RESOURCE)
            agent.destination = newTask.CurrentPosition(); 

        return oldTask; 
    }

    void Update()
    {
        anim.SetFloat("speed", agent.velocity.magnitude);

        if (currentTask != null)
        {
            if ((agent.destination - transform.position).magnitude < 0.5f)
            {
                anim.SetBool(currentTask.GetTaskAnimatorVariable(), true); 
            }
        }
    }

    void HitTile()
    {    

        currentTask.tile.hitPoints--;

        if (currentTask.tile.hitPoints == 0)
        {
            anim.SetBool("mining", false);
            CreatureTask newTask = currentTask.CompleteSelf(levelMakerInstance, this);
            if (newTask != null)
                WorkerManager.AddTask(newTask); 
        }
    }   

    void ClaimTile()
    {       
        anim.SetBool("claiming", false);
        currentTask.CompleteSelf(levelMakerInstance, this);
        WorkerManager.ScheduleTasks();        
    }

    void Pickaxe()
    {
       // source.PlayOneShot(audios[1]);

    }

    public void TumbleRocks()
    {
        source.PlayOneShot(audios[3]); 
    }

    void Foot()
    { 
    
    }
    
    

}
