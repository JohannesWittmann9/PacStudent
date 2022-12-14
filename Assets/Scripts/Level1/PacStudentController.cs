using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{

    [SerializeField] float moveDuration;
    [SerializeField] AudioClip pacMovementClip;
    [SerializeField] AudioClip pacEatClip;
    [SerializeField] AudioClip pacWallClip;
    [SerializeField] AudioClip pacDeadClip;
    [SerializeField] ParticleSystem pacPS;
    [SerializeField] ParticleSystem deathPS;
    [SerializeField] ParticleSystem bumpPS;

    private GameObject[] tiles;
    private List<GameObject> collectibles;
    private Tweener tweener;
    private AudioSource audioSource;
    private Animator animator;
    private KeyCode lastInput;
    private KeyCode currentInput;
    private bool hasBumped = false;
    private int pointsForPellet = 10;
    private int pointsForCherry = 100;
    private int pointsForGhost = 300;
    private Vector3 startPosition;

    private GameObject managers;
    private GameManager gameManager;
   

    // Start is called before the first frame update
    void Start()
    {
        tweener = GetComponent<Tweener>();
        animator = GetComponent<Animator>();
        animator.SetBool("normalState", true);
        audioSource = GetComponent<AudioSource>();

        managers = GameObject.Find("Managers");
        gameManager = managers.GetComponent<GameManager>();

        startPosition = transform.position;
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

        if (lastInput != KeyCode.None)
        {
            Vector3 newPos = Vector3.zero;
            bool canWalk = ComputeInput(lastInput, ref newPos);
            if (!canWalk)
            {
                canWalk = ComputeInput(currentInput, ref newPos);
                Vector3 pos = newPos + ((this.transform.position - newPos) / 2);
                if (!canWalk && !hasBumped) PlayWallBump(pos);
            }
            else
            {
                hasBumped = false;
            }
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
        collectibles = new List<GameObject>(GameObject.FindGameObjectsWithTag("Collectible"));
        collectibles.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("Pellet")));

        for (int i = 0; i < collectibles.Count; i++)
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

    private bool ComputeInput(KeyCode input, ref Vector3 newPos)
    {
        if (!tweener.TweenExists(transform))
        {
            animator.SetBool("walkUp", false);
            animator.SetBool("walkDown", false);
            animator.SetBool("walkRight", false);
            animator.SetBool("walkLeft", false);

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
                        ComputeTeleporters(ref newPos);

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
                        ComputeTeleporters(ref newPos);
                        
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
            CollectItem(item);
        }
    }

    private void PlayParticles()
    {
        pacPS.Play();
    }

    private void PlayWallBump(Vector3 pos)
    {
        audioSource.Stop();
        audioSource.clip = pacWallClip;
        audioSource.Play();
        bumpPS.transform.position = pos;
        bumpPS.Play();
        hasBumped = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject collider = collision.gameObject;
        if (collider.CompareTag("BonusCherry"))
        {
            CollectItem(collider);
        }
        if (collider.CompareTag("Ghost"))
        {
            bool normalState = animator.GetBool("normalState");
            bool powerState = animator.GetBool("powerState");
            if (normalState)
            {
                bool gameOver = gameManager.DecreaseLiveCount();
                tweener.StopTween(transform);
                deathPS.transform.position = transform.position;
                deathPS.Play();
                if (!gameOver)
                {
                    transform.position = startPosition;
                    ResetInput();
                }
                
            }
            else if (powerState)
            {
                gameManager.SetDeadState(collider);
                gameManager.AddPoints(pointsForGhost);
            }
        }

    }

    private void CollectItem(GameObject item)
    {
        item.SetActive(false);
        int points = 0;
        if (item.CompareTag("BonusCherry"))
        {
            points = pointsForCherry;
        }
        else if (item.CompareTag("Pellet"))
        {
            points = pointsForPellet;
        }
        else if (item.CompareTag("Collectible"))
        {
            gameManager.SetScaredState();
            ResetAnimatorStates();
            animator.SetBool("powerState", true);
            Timer timer = managers.GetComponent<Timer>();
            timer.StartTimer(10);
        }

        gameManager.AddPoints(points);
        PlayCollectClip();
    }

    private void ResetAnimatorStates()
    {
        animator.SetBool("powerState", false);
        animator.SetBool("normalState", false);
        animator.SetBool("deathState", false);
    }

    private void ResetInput()
    {
        animator.SetBool("walkLeft", false);
        animator.SetBool("walkRight", false);
        animator.SetBool("walkUp", false);
        animator.SetBool("walkDown", false);
        currentInput = KeyCode.None;
        lastInput = KeyCode.None;
        pacPS.Stop();
    }

    public void SetDeadState()
    {
        ResetAnimatorStates();
        animator.SetBool("deathState", true);
        audioSource.clip = pacDeadClip;
        audioSource.Play();
    }

    public void SetNormalState()
    {
        ResetAnimatorStates();
        animator.SetBool("normalState", true);
    }

    private void ComputeTeleporters(ref Vector3 newPos)
    {
        Vector3 edge = GameObject.Find("LevelMap").GetComponent<LevelGenerator>().topLeft;
        float size = 28.0f;
        float xLeft = edge.x;
        float xRight = edge.x + size;

        if (newPos.x < xLeft)
        {
            newPos = new Vector3(xRight - 1, newPos.y, 0);
            transform.position = new Vector3(xRight, transform.position.y, 0);
        }
        else if (newPos.x > xRight)
        {
            newPos = new Vector3(xLeft + 1, newPos.y, 0);
            transform.position = new Vector3(xLeft, transform.position.y, 0);
        }

    }
}
