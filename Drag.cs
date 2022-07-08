using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Drag : MonoBehaviour
{
    private Vector3 dragOffset;
    private Camera _camera;
    private bool isDrag;
    private bool isOriginPos;
    private bool isEndDrag = false;
    private bool isComplete;
    private bool isGameOver;
    public GameObject obstacle;
    private PolygonCollider2D centerOfCollider;
    private bool isTriggerWithObs;
    private Vector2 goalCenterPos;
    private Rigidbody2D handRb;

    [SerializeField] Text completeText;
    [SerializeField] Text gameOverText;
    [SerializeField] private float speed = 1000.0f;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] LayerMask goalMask;
    [SerializeField] LayerMask sawMask;
    [SerializeField] GameObject goal;
    [SerializeField] LineRenderer lineRenderer;

    List<Vector2> wrapPoints = new List<Vector2>();
    [SerializeField] List<Vector2> lastWrapPoints = new List<Vector2>();

    Vector2 originalPoint;

    Vector2 currentWrapPoint;

    Vector2 centerOfObs;

    List<Vector2> normals = new List<Vector2>();

    Vector2 lastNormal = Vector2.zero;
    int index;


    private void Awake()
    {
       _camera = Camera.main;
        isOriginPos = true;
        isTriggerWithObs = false;
    }
    private void Update()
    {
        if (isDrag == false && isOriginPos == false)
        {
            if (index > 0)
            {
                transform.position = Vector2.MoveTowards(transform.position, lastWrapPoints[index], speed * Time.deltaTime);
                if (transform.position.x == lastWrapPoints[index].x && transform.position.y == lastWrapPoints[index].y)
                {
                    lastWrapPoints.RemoveAt(index);
                    index--;
                }
                    
            }
            if ( index == 0)
            {
                transform.position = Vector2.MoveTowards(transform.position, originalPoint, speed * Time.deltaTime);
            }
            //if (lastWrapPoints.Count >=2 )
            //for (int i = lastWrapPoints.Count - 1; i >= 0; i--)
            //{
                    //while (transform.position.x != lastWrapPoints[i].x && transform.position.y != lastWrapPoints[i].y)
                    //{
                        //transform.position = Vector2.MoveTowards(transform.position, lastWrapPoints[i], speed * Time.deltaTime);
                    //}
                //}
            //StartCoroutine(ComeBack(1.0f,i));

            //transform.position = Vector2.MoveTowards(transform.position, originalPoint, speed * Time.deltaTime);
        }
        DrawLine();
        if (isEndDrag) DrawBackLine();
        if (isGameOver)
        {
            gameOverText.gameObject.SetActive(true);
        }
    }
    private void FixedUpdate()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        handRb.MovePosition(Vector2.MoveTowards((Vector2)transform.position, mousePos, 10 * Time.deltaTime));
    }
    private void Start()
    {
        originalPoint = transform.position;
        centerOfCollider =  obstacle.GetComponent<PolygonCollider2D>();
        centerOfObs = centerOfCollider.bounds.center;
        completeText.gameObject.SetActive(false);
        goalCenterPos = goal.GetComponent<PolygonCollider2D>().bounds.center;
        handRb = GetComponent<Rigidbody2D>();
        handRb.isKinematic = true;
        isGameOver = false;
        gameOverText.gameObject.SetActive(false);
    }
    private void OnMouseDown()
    {
        Debug.Log("Clicked!");
        currentWrapPoint = originalPoint;
        wrapPoints.Add(currentWrapPoint);
        normals.Add(lastNormal);
        isDrag = true;
        isEndDrag = false;
        isGameOver = false;
        completeText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        handRb.bodyType = RigidbodyType2D.Dynamic;
    }
    private void OnMouseDrag()
    {
        Debug.Log("Drag!");
        isDrag = true;
        isOriginPos = false;
        isEndDrag = false;

        //Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //handRb.MovePosition(Vector2.MoveTowards((Vector2)transform.position, mousePos, 50 * Time.deltaTime));

        //transform.position = mousePos;

        //handRb.MovePosition(mousePos);
        //handRb.MovePosition(Vector2.MoveTowards((Vector2)transform.position, mousePos, 10 * Time.deltaTime));

        lastNormal = normals[normals.Count - 1];

        Vector2 anchorPoint = currentWrapPoint + lastNormal * 0.1f;
        //if (isTriggerWithObs == true) return ;
        //1
        RaycastHit2D hit = Physics2D.Raycast(anchorPoint, ((Vector2)transform.position - anchorPoint).normalized, ((Vector2)transform.position - currentWrapPoint).magnitude,obstacleMask);
        if (hit)
        {
            //AddPoint(hit);
            //Debug.Log(hit.collider.name);
            //Debug.Log(hit.point);
            //Debug.DrawLine(originalPoint, GetClosestColliderPointFromRaycastHit(hit,hit.collider as PolygonCollider2D), Color.red);

            Vector2 hitPoint = GetClosestColliderPointFromRaycastHit(hit, hit.collider as PolygonCollider2D);

            if(currentWrapPoint != hitPoint)
            {
                lastNormal = hit.normal;
                normals.Add(hit.normal);
                currentWrapPoint = hitPoint;
                wrapPoints.Add(currentWrapPoint);
            }
            
        }


        if (wrapPoints.Count > 1)
        {
            Vector2 current = currentWrapPoint;
            Vector2 previous = wrapPoints[wrapPoints.Count - 2];
            Vector2 next = transform.position;//2
            //Vector2 leftTangent = (normals[normals.Count - 1] - current).normalized;
            Vector2 leftTangent = (previous - current).normalized;
            Vector2 rightTangent = (next - current).normalized;
            Vector2 middleTangent = (current - centerOfObs).normalized;
            float angle1 = Vector2.Angle(leftTangent, middleTangent);
            float angle2 = Vector2.Angle(middleTangent, rightTangent);
            if (angle1 + angle2 < 175)
            {
                Debug.Log(leftTangent + " " + rightTangent + " " + angle1 + " "+ angle2);
                wrapPoints.RemoveAt(wrapPoints.Count - 1);
                normals.RemoveAt(normals.Count - 1);
                currentWrapPoint = wrapPoints[wrapPoints.Count - 1];
                Debug.Log("remove wrap point");
            }
        }
        if (wrapPoints.Count > 1)
        {
            RaycastHit2D sawhit1 = Physics2D.Raycast(transform.position, ((Vector2) (wrapPoints[1] - (Vector2)transform.position)).normalized, ((Vector2) (wrapPoints[1] - (Vector2) transform.position)).magnitude, sawMask);
            RaycastHit2D sawhit2 = Physics2D.Raycast(transform.position, ((Vector2)(wrapPoints[wrapPoints.Count-1] - (Vector2)transform.position)).normalized, ((Vector2)(wrapPoints[wrapPoints.Count-1] - (Vector2)transform.position)).magnitude, sawMask);
            if (sawhit1 || sawhit2)
            {
                Debug.Log("Game Over!!");
                isGameOver = true;
            }
        }else

        {
            RaycastHit2D sawhit = Physics2D.Raycast(transform.position, ((Vector2)(originalPoint - (Vector2)transform.position)).normalized, ((Vector2)(originalPoint - (Vector2)transform.position)).magnitude, sawMask);
            if (sawhit)
            {
                Debug.Log("Game Over!!");
                isGameOver = true;
            }
        }
    }
    void DrawLine()
    {
        List<Vector2> drawPoints = new List<Vector2>(wrapPoints);
        //3
        //drawPoints.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        drawPoints.Add((Vector2)transform.position);
        lineRenderer.positionCount = drawPoints.Count;
        if(wrapPoints.Count > 0)
        {
            lineRenderer.SetPosition(0, drawPoints[0]);
            for(int i = 1; i < drawPoints.Count; i++)
            {
                Debug.DrawLine(drawPoints[i], drawPoints[i - 1]);
                lineRenderer.SetPosition(i, drawPoints[i]);
                lineRenderer.transform.rotation = transform.rotation;
            }
        }

    }
    void DrawBackLine()
    {
        List<Vector2> drawPoints = new List<Vector2>(lastWrapPoints);
        drawPoints.Add(transform.position);
        lineRenderer.positionCount = drawPoints.Count;
        if (lastWrapPoints.Count > 0)
        {
            
            for (int i = drawPoints.Count-1;i>=0; i--)
            {
                lineRenderer.SetPosition(i, drawPoints[i]);
                lineRenderer.transform.rotation = transform.rotation;
            }
        }

    }
    private void OnMouseUp()
    {
        isDrag = false;
        isEndDrag = true;
        lastWrapPoints = new List<Vector2>(wrapPoints);
        index = lastWrapPoints.Count - 1;
        wrapPoints.Clear();
        normals.Clear();
        handRb.bodyType = RigidbodyType2D.Kinematic;
        if (isComplete)
        {
            completeText.gameObject.SetActive(true);
            Debug.Log("Complete!!");
            isComplete = false;
        } 
        //Vector2 mousePos = GetMousePos();
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, ((Vector2)transform.position - goalCenterPos).normalized, ((Vector2)transform.position - (Vector2)goalCenterPos).magnitude, goalMask);
        //if (hit)
        //{
        //    completeText.gameObject.SetActive(true);
        //}
    }
    Vector3 GetMousePos()
    {
        var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        isTriggerWithObs = false;
        return mousePos;
    }

    private Vector2 GetClosestColliderPointFromRaycastHit(RaycastHit2D hit, PolygonCollider2D polyCollider)
    {
        // 2
        var distanceDictionary = polyCollider.points.ToDictionary<Vector2, float, Vector2>(
            position => Vector2.Distance(hit.point, polyCollider.transform.TransformPoint(position)),
            position => polyCollider.transform.TransformPoint(position));

        // 3
        var orderedDictionary = distanceDictionary.OrderBy(e => e.Key);
        return orderedDictionary.Any() ? orderedDictionary.First().Value : Vector2.zero;
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Obstacle"))
    //    {
    //        Debug.Log("Is trigger with hand!!");
    //        isTriggerWithObs = true;
    //    }
    //    else if (collision.CompareTag("Goal"))
    //    {
    //        completeText.gameObject.SetActive(true);
    //        Debug.Log("Complete!!");
    //    }
    //}
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Goal"))
        {
            isComplete = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Goal"))
        {
            isComplete = false;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (CompareTag("Obstacle"))
        {
            Debug.Log("Is collider with Obstacle!!");
        }
    }
}
