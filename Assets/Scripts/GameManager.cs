using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleClone
{
    public class GameManager : MonoBehaviour
    {
        //BubblePositioning
        [HideInInspector]
        public GameObject bubbleManager;
        public int bubblesPerRow = 4;
        public int rowCount = 6;
        public float hexGridSize = 0.8f;

        private Vector2[] dirs = {   new Vector2(+1, 0),
                                    new Vector2(1, -1),
                                    new Vector2(0, -1),
                                    new Vector2(-1, 0),
                                    new Vector2(-1, 1),
                                    new Vector2(0, 1)
        };
        [HideInInspector]
        public Dictionary<Vector2, Bubble> bubbleArray = new Dictionary<Vector2, Bubble>();


        //BubbleInitialization
        [Range(0, 1)]
        public float bubbleGrowTime = 0.5f;
        public float bubbleSpeed = 10f;
        public GameObject bubblePrefab;
        public GameObject emptyGO;
        public Color[] colors = new Color[11];
        public Dictionary<int, Color> colorSet = new Dictionary<int, Color>();

        [HideInInspector]
        public int boundaryLM = 1 << 8;
        [HideInInspector]
        public int bubbleLM = 1 << 9;

        //NeighbourFinding
        private List<Bubble> whatToMerge = new List<Bubble>();
        private int newestEntriesCount = 0;
        private int referenceValue;
        public Bubble newestBubble;
        public bool queueForCleanup = false;


        //GMStuff
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject
                        {
                            name = typeof(GameManager).Name
                        };
                        _instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }


        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            Setup();
        }

        void Setup() //PreBubble / Feeding values
        {
            int i = 1;
            foreach (Color color in colors)
            {
                colorSet.Add((int)Mathf.Pow(2, i), color);
                i++;
            }
        }

        void Start()
        {
            bubbleManager = Instantiate(emptyGO);
            bubbleManager.name = "BubbleManager";
            SpawnBubbles();
        }

        private void Update()
        {
            if(queueForCleanup)
            {
                queueForCleanup = false;
                GatherSameNeighbours(newestBubble.hexCoords);
            }
        }

        //HexMethods
        void SpawnBubbles()
        {
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < bubblesPerRow; j++)
                {
                    CreateBubble(new Vector2(j, i));
                }
            }
        }
        void CreateBubble(Vector2 rq)
        {
            GameObject bubble = Instantiate(bubblePrefab, bubbleManager.transform);
            Bubble bC = bubble.GetComponent<Bubble>();
            bC.offCoords = rq;
            Vector3 cC = bC.cubeCoords = convertOffToCube(rq);
            Vector2 hC = bC.hexCoords = convertCubeToHex(cC);
            bubbleArray.Add(hC, bC);
            bC.qs = queueState.gameStart;
        }



        public Vector3 convertHexToPixel(Vector2 hex) //Positioning Wichtig
        {
            float x = hexGridSize * (Mathf.Sqrt(3) * hex.x + Mathf.Sqrt(3) / 2 * hex.y);
            float y = hexGridSize * (3f / 2f * hex.y);
            return new Vector3(x, y, 0);
        }
        public Vector2 convertPixelToHex(Vector3 point)
        {
            float q = (Mathf.Sqrt(3) / 3 * point.x - 1f / 3 * point.y) / hexGridSize;
            float r = (2f / 3 * point.y) / hexGridSize;
            return hex_round(new Vector2(q, r));
        }

        public void GatherSameNeighbours(Vector2 originHex)
        {
            whatToMerge.Clear();                                //obvious
            referenceValue = bubbleArray[originHex].noValue;    //sammelt den aktuellen wert
            whatToMerge.Add(bubbleArray[originHex]);            //bereitet den origin zum fusen vor


            CheckNeighbours(originHex);                         //sammelt vorhandene nachbarn & added sie zum fuse array ///REWORK

            int exp = (int)Mathf.Log(referenceValue, 2);        //rechnung für letzten exponenten
            int newExp = whatToMerge.Count + exp - 1;           //neuer exponent

            for (int i = 1; i < whatToMerge.Count; i++)
            {
                bubbleArray.Remove(whatToMerge[i].hexCoords);

                whatToMerge[i].targetPos = convertHexToPixel(whatToMerge[0].hexCoords);
                whatToMerge[i].GetComponent<SphereCollider>().isTrigger = true;
                whatToMerge[i].qs = queueState.fusing;
            }
            int newValue = (int)Mathf.Pow(2, newExp);
            if (newValue >= 2048)
            {
                //Find Neighbours and destroy
            }
            else
            {
                bubbleArray[originHex].AssignValues(newValue);
            }
        }
        void CheckNeighbours(Vector2 x)
        {
            newestEntriesCount = 0;
            foreach (Vector2 dir in dirs)
            {
                if (bubbleArray.ContainsKey(x + dir) && !whatToMerge.Contains(bubbleArray[x + dir]) && bubbleArray[x + dir].noValue == referenceValue)
                {
                    whatToMerge.Add(bubbleArray[x + dir]);
                    newestEntriesCount++;
                }
            }

            for (int i = (whatToMerge.Count - newestEntriesCount); i <= (whatToMerge.Count - 1); i++)
            {
                CheckNeighbours(whatToMerge[i].hexCoords);
            }
        }

        public Vector2[] AllPossibleNeighbours(Vector2 hexOrigin)
        {
            List<Vector2> pos = new List<Vector2>();
            foreach(Vector2 dir in dirs)
            {
                pos.Add(hexOrigin + dir);
            }
            return pos.ToArray();
        }
        public Vector2[] AllEmptyNeighbours(Vector2[] posArray)
        {
            List<Vector2> pos = new List<Vector2>();
            foreach(Vector2 p in posArray)
            {
                if (!bubbleArray.ContainsKey(p))
                {
                    pos.Add(p);
                }
            }
            return pos.ToArray();
        }





        private Vector3 convertOffToCube(Vector2 hex) //only CoordConversion
        {
            var x = hex.x - (hex.y + ((int)hex.y & 1)) / 2;
            var z = hex.y;
            var y = -x - z;
            return new Vector3(x, y, z);
        }
        private Vector3 cube_round(Vector3 cube)
        {
            var rx = Mathf.Round(cube.x);
            var ry = Mathf.Round(cube.y);
            var rz = Mathf.Round(cube.z);

            var x_diff = Mathf.Abs(rx - cube.x);
            var y_diff = Mathf.Abs(ry - cube.y);
            var z_diff = Mathf.Abs(rz - cube.z);

            if (x_diff > y_diff && x_diff > z_diff)
                rx = -ry - rz;
            else if (y_diff > z_diff)
                ry = -rx - rz;
            else
                rz = -rx - ry;

            return new Vector3(rx, ry, rz);
        } //part of CoordConversion
        private Vector2 hex_round(Vector2 hex)
        {
            return convertCubeToHex(cube_round(convertHexToCube(hex)));
        } //part of pixelToHexConversion
        private Vector3 convertHexToCube(Vector2 hex)
        {
            var x = hex.x;
            var z = hex.y;
            var y = -x - z;
            return new Vector3(x, y, z);
        }
        private Vector2 convertCubeToHex(Vector3 cube)
        {
            var q = cube.x;
            var r = cube.z;
            return new Vector2(q, r);
        }
    }
    public enum queueState
    {
        preQueue,
        queue,
        settling,
        fusing,
        idle,
        gameStart,
        rdyQueue
    }
}
