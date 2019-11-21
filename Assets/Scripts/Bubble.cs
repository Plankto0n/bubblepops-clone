using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BubbleClone;

public class Bubble : MonoBehaviour
{
    //Components
    [HideInInspector]
    public SpriteRenderer sr;
    private GameManager gm;
    private Rigidbody rb;
    private TextMesh tm;

    //SpawnIn
    private float currentLerpTime = 0f;
    private Vector3 initialSize;
    [HideInInspector]
    public float preQueueScale = 0.75f;
    private Color color;
    public int noValue;

    //Shooting
    [HideInInspector]
    public bool queue = false; //maybe clean up to queuestate
    public queueState qs;
    [HideInInspector]
    public bool collided = false; //maybe simplifiable

    //Positioning
    //[HideInInspector]
    public Vector2 offCoords;
    //[HideInInspector]
    public Vector2 hexCoords; //Axial Coords
    //[HideInInspector]
    public Vector3 cubeCoords;
    
    public Vector3 targetPos;

    private void Awake()
    {
        gm = GameManager.Instance;
        GoSmall();
        tm = GetComponentInChildren<TextMesh>();
    }

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();
        RollNumber();
    }

    // Update is called once per frame
    void Update()
    {
        switch (qs)
        {
            case queueState.preQueue:
                {
                    PopUpPreQueue();
                    break;
                }
            case queueState.queue:
                {
                    GetComponent<SphereCollider>().radius = (0.64f *.5f);
                    targetPos = Vector3.zero;
                    RealignFromPrequeue();
                    break;
                }
            case queueState.rdyQueue:
                {
                    break;
                }
            case queueState.settling:
                {
                    AlignToGrid();
                    qs = queueState.idle;
                    break;
                }
            case queueState.fusing:
                {
                    Fuse();
                    break;
                }
            case queueState.idle:
                {
                    break;
                }
            case queueState.gameStart:
                {
                    AlignToGrid();
                    SpawnGameStart(); //growing to size, then switch to idle
                    break;
                }
            default:
                {
                    Debug.Log("Bad things happended");
                    break;
                }
        }
    }

    void SpawnGameStart() //unique when game start
    {
        currentLerpTime += Time.deltaTime / gm.bubbleGrowTime;
        ReScale(currentLerpTime, Vector3.zero, initialSize);
        rb.constraints = RigidbodyConstraints.FreezeAll;
        if (transform.localScale == initialSize)
        {
            qs = queueState.idle;
            currentLerpTime = 0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (qs== queueState.rdyQueue)
        {
            if(collision.gameObject.layer == LayerMask.NameToLayer("Bubble"))
            {
                transform.parent = gm.bubbleManager.transform;
                rb.constraints = RigidbodyConstraints.FreezeAll;

                Vector2[] pos = gm.AllEmptyNeighbours(gm.AllPossibleNeighbours(collision.gameObject.GetComponent<Bubble>().hexCoords));
                if (pos.Length == 1)
                {
                    hexCoords = pos[0];
                }
                else //check for closest match
                {
                    float mag = 100000f;
                    foreach(Vector2 p in pos)
                    { 
                        float dist = Mathf.Abs(Vector3.Magnitude(gm.convertHexToPixel(p) - transform.position));
                        if (dist < mag)
                        {
                            mag = dist;
                            hexCoords = p;
                        }
                    }
                }
                gm.bubbleArray.Add(hexCoords, this);
                gm.newestBubble = this;
                GetComponent<SphereCollider>().radius = 0.64f;
                qs = queueState.settling;
                gm.queueForCleanup = true;
                collided = true;
            }
        }
    }

    void AlignToGrid()
    {
        transform.position = gm.convertHexToPixel(hexCoords);
    }

    //InitStuff
    void RollNumber()
    {
        int rdRoll = Random.Range(0, 100);
        if (rdRoll >= 0 && rdRoll <= 42)
        {
            //2
            AssignValues(2);
        }
        else if(rdRoll >= 43 && rdRoll <= 62)
        {
            //4
            AssignValues(4);
        }
        else if(rdRoll >= 63 && rdRoll <= 75)
        {
            //8
            AssignValues(8);
        }
        else if(rdRoll >= 76 && rdRoll <= 85)
        {
            //16
            AssignValues(16);
        }
        else if(rdRoll >= 86 && rdRoll <= 93)
        {
            //32
            AssignValues(32);
        }
        else if(rdRoll >= 94 && rdRoll <= 100)
        {
            //64
            AssignValues(64);
        }
    }



    void RealignFromPrequeue() //queue und Settle
    {
        currentLerpTime += Time.deltaTime / gm.bubbleGrowTime;

        Vector3 startSize = transform.localScale;
        ReScale(currentLerpTime, startSize, initialSize);

        RePosition(currentLerpTime);

        if (transform.localScale == initialSize && transform.localPosition == Vector3.zero)
        {
            GetComponent<SphereCollider>().isTrigger = false;
            currentLerpTime = 0f;
            qs = queueState.rdyQueue;
        }
    }
    void PopUpPreQueue() //nur prequeue
    {
        currentLerpTime += Time.deltaTime / gm.bubbleGrowTime;
        Vector3 startSize = transform.localScale;
        Vector3 targetSize = initialSize * preQueueScale;

        ReScale(currentLerpTime, startSize, targetSize);

        if (transform.localScale == targetSize)
        {
            qs = queueState.idle;
            currentLerpTime = 0f;
        }
    }

    void ReScale(float time, Vector3 startSize, Vector3 targetSize)
    {
        transform.localScale = Vector3.Lerp(startSize, targetSize, time);
    }

    void RePosition(float time)
    {
        Vector3 startPos = transform.localPosition;
        transform.localPosition = Vector3.Lerp(startPos, targetPos, time);
    }

    
    void Fuse() //probably changing
    {
        currentLerpTime += Time.deltaTime / gm.bubbleGrowTime;
        RePosition(currentLerpTime);
        if (transform.localPosition == targetPos)
        {
            Destroy(this.gameObject);
        }
    }

    void GoSmall()
    {
        initialSize = transform.localScale;
        transform.localScale = Vector3.zero;
    }
    public void AssignValues(int n)
    {
        color = gm.colorSet[n];
        noValue = n;
        sr.color = color;
        tm.text = noValue.ToString();
    }
}
