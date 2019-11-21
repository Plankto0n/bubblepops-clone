using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BubbleClone;

public class ShooterBase : MonoBehaviour
{
    private GameManager gm;
    private GameObject currentBubble;
    private GameObject queueBubbleHolder;
    private GameObject nextBubble;
    [HideInInspector]
    public GameObject lrGO;
    private LineRenderer lr;
    private Vector3 tp;

    private bool bubbleShot = false;
    private bool prepareBuffer = false;
    private bool validShot;

    private void Awake()
    {
        tp = this.transform.position;
        gm = GameManager.Instance;
        queueBubbleHolder = transform.GetChild(0).gameObject;

        lr = lrGO.GetComponent<LineRenderer>();
        lr.enabled = false;
        lr.SetPosition(0, tp);
    }
    // Start is called before the first frame update
    void Start()
    {
        currentBubble = Instantiate(gm.bubblePrefab, this.transform);
        nextBubble = Instantiate(gm.bubblePrefab, queueBubbleHolder.transform);
        currentBubble.GetComponent<Bubble>().qs = queueState.queue;
        nextBubble.GetComponent<Bubble>().qs = queueState.preQueue;
        nextBubble.GetComponent<SphereCollider>().isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!bubbleShot)
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = tp.z;
                PredictRay(mousePos);
            }

            if (Input.GetMouseButtonUp(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = tp.z;
                Shoot(mousePos);
            }
        }
        if (bubbleShot && currentBubble.GetComponent<Bubble>().collided)
        {
            if (!prepareBuffer)
            {
                prepareBuffer = true;
                PrepareNextShot();
            }
        }
    }
    void Shoot(Vector3 mousePos)
    {
        if (validShot)
        {
            currentBubble.GetComponent<Rigidbody>().AddForce((mousePos - this.transform.position).normalized * gm.bubbleSpeed);
            bubbleShot = true;
            lr.enabled = false;
        }
    }
    void PredictRay(Vector3 mousePos)
    {
        Ray ray = new Ray(tp, mousePos - tp);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, gm.boundaryLM))
        {
            Vector3 redirect = Vector3.Reflect(ray.direction, hit.normal);
            lr.SetPosition(1, hit.point);
            Ray reRay = new Ray(hit.point, redirect);
            if (Physics.Raycast(reRay, out hit, Mathf.Infinity, gm.bubbleLM))
            {
                lr.SetPosition(2, hit.point);
                lr.enabled = true;
                validShot = true;
            }
            else
            {
                CancelAim();
            }

        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, gm.bubbleLM))
        {
            lr.SetPosition(1, tp);
            lr.SetPosition(2, hit.point);
            lr.enabled = true;
            validShot = true;
        }
        else
        {
            CancelAim();
        }
    }
    void CancelAim()
    {
        lr.enabled = false;
        validShot = false;
    }
    void PrepareNextShot()
    {
        currentBubble.GetComponent<Bubble>().qs = queueState.settling; //ShotBubble Finished pq
        currentBubble = nextBubble; //Setting next FireBubble
        currentBubble.GetComponent<Bubble>().qs = queueState.queue; //pq change to Queue
        currentBubble.transform.parent = this.transform; //change to shooterbase parent from queue parent
        
        //currentBubble.GetComponent<Bubble>().spawned = false; //setting spawned to false to initialize change in size and place

        nextBubble = Instantiate(gm.bubblePrefab, queueBubbleHolder.transform); //creating new bubble at queue parent
        nextBubble.GetComponent<SphereCollider>().isTrigger = true; //disabliung collision

        bubbleShot = false;
        prepareBuffer = false;
    }
}
