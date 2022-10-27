using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{

    [SerializeField] float moveDuration;
    [SerializeField] AudioClip pacMovementClip;
    [SerializeField] AudioClip pacEatClip;
    [SerializeField] AudioClip pacWallClip;
    [SerializeField] ParticleSystem particleSys;

    private GameObject[] tiles;
    private GameObject[] collectibles;
    private Tweener tweener;
    private AudioSource audioSource;
    private Animator animator;
    private KeyCode lastInput;
    private KeyCode currentInput;
   

    // Start is called before the first frame update
    void Start()
    {
        tweener = GetComponent<Tweener>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.W))
        {
            lastInput = KeyCode.W;
        }
        if (Input.GetKey(KeyCode.A))
        {
            lastInput = KeyCode.A;
        }
        if (Input.GetKey(KeyCode.S))
        {
            lastInput = KeyCode.S;
        }
        if (Input.GetKey(KeyCode.D))
        {
            lastInput = KeyCode.D;
        }

        if (tweener.TweenExists(transform))
        {
            PlayMovementClip();
        }

        if(lastInput != KeyCode.None)
        {
            bool canWalk = ComputeInput(lastInput);
            if (!canWalk) ComputeInput(currentInput);
        }
        
    }

    private void FixedUpdate()
    {
        TryCollectItem();
    }

    private void PlayMovementClip()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = pacMovementClip;
            audioSource.Play();
        }
    }

    private void PlayWallClip()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = pacWallClip;
            audioSource.Play();
        }
    }

    private void PlayCollectClip()
    {
        audioSource.Stop();
        audioSource.clip = pacEatClip;
        audioSource.Play();
        
    }

    private bool IsWalkable(Vector3 newPos)
    {
        tiles = GameObject.FindGameObjectsWithTag("NonWalkable");
        foreach (GameObject tile in tiles)
        {
            float distance = Vector3.Distance(tile.transform.position, newPos);
            if(distance < 0.5f)
            {
                return false;
                
            }
        }
        return true;
    }

    private GameObject GetCollectible()
    { 
        collectibles = GameObject.FindGameObjectsWithTag("Collectible");

        for (int i = 0; i < collectibles.Length; i++)
        {
            GameObject obj = collectibles[i];
            if(obj != null)
            {
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                if (distance < 0.5f)
                {
                    return obj;
                }
            }
            
        }
        return null;
    }

    private bool ComputeInput(KeyCode input)
    {
        if (!tweener.TweenExists(transform))
        {
            animator.SetBool("walkUp", false);
            animator.SetBool("walkDown", false);
            animator.SetBool("walkRight", false);
            animator.SetBool("walkLeft", false);
            Vector3 newPos;

            switch (input)
            {
                case KeyCode.W:
                    newPos = new Vector3(transform.position.x, transform.position.y + 1);
                    if (IsWalkable(newPos))
                    {
                        currentInput = input;
                        tweener.AddTween(transform, transform.position, newPos, moveDuration);
                        animator.SetBool("walkUp", true);
                        PlayParticles();
                        return true;
                    }
                    break;
                case KeyCode.A:
                    newPos = new Vector3(transform.position.x - 1, transform.position.y);
                    if (IsWalkable(newPos))
                    {
                        currentInput = input;
                        tweener.AddTween(transform, transform.position, newPos, moveDuration);
                        animator.SetBool("walkLeft", true);
                        PlayParticles();
                        return true;
                    }
                    break;
                case KeyCode.S:
                    newPos = new Vector3(transform.position.x, transform.position.y - 1);
                    if (IsWalkable(newPos))
                    {
                        currentInput = input;
                        tweener.AddTween(transform, transform.position, newPos, moveDuration);
                        animator.SetBool("walkDown", true);
                        PlayParticles();
                        return true;
                    }

                    break;
                case KeyCode.D:
                    newPos = new Vector3(transform.position.x + 1, transform.position.y);
                    if (IsWalkable(newPos))
                    {
                        currentInput = input;
                        tweener.AddTween(transform, transform.position, newPos, moveDuration);
                        animator.SetBool("walkRight", true);
                        PlayParticles();
                        return true;
                    }
                    break;
                default:
                    break;
            }

            return false;
        }
        /* When there is no tween we dont want to check current input */
        return true;
    }

    private void TryCollectItem()
    {
        GameObject item = GetCollectible();
        if (item != null)
        {
            item.SetActive(false);
            PlayCollectClip();
        }
    }

    private void PlayParticles()
    {
        particleSys.Play();
    }
}