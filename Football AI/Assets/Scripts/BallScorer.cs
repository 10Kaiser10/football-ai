using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScorer : MonoBehaviour
{
    public int t1Goals;
    public int t2Goals;
    public int t1Touches;
    public int t2Touches;

    public Transform t1p1;
    public Transform t1p2;
    public Transform t1p3;
    public Transform t2p1;
    public Transform t2p2;
    public Transform t2p3;

    public float mint1p1;
    public float mint1p2;
    public float mint1p3;
    public float mint2p1;
    public float mint2p2;
    public float mint2p3;

    float timer = 0;

    Rigidbody rb;

    void Start()
    {
        t1Goals = 0;
        t1Touches = 0;
        t2Goals = 0;
        t2Touches = 0;
        rb = gameObject.GetComponent<Rigidbody>();

        mint1p1 = 0;
        mint1p2 = 0;
        mint1p3 = 0;
        mint2p1 = 0;
        mint2p2 = 0;
        mint2p3 = 0;

        t1p1 = gameObject.transform.parent.GetChild(2).GetChild(1).GetChild(1).transform;
        t1p2 = gameObject.transform.parent.GetChild(2).GetChild(2).GetChild(1).transform;
        t1p3 = gameObject.transform.parent.GetChild(2).GetChild(3).GetChild(1).transform;
        t2p1 = gameObject.transform.parent.GetChild(3).GetChild(1).GetChild(1).transform;
        t2p2 = gameObject.transform.parent.GetChild(3).GetChild(2).GetChild(1).transform;
        t2p3 = gameObject.transform.parent.GetChild(3).GetChild(3).GetChild(1).transform;

    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (timer > 0.5)
        {
            mint1p1 += Vector3.Distance(t1p1.localPosition, transform.localPosition);
            mint1p2 += Vector3.Distance(t1p2.localPosition, transform.localPosition);
            mint1p3 += Vector3.Distance(t1p3.localPosition, transform.localPosition);
            mint2p1 += Vector3.Distance(t2p1.localPosition, transform.localPosition);
            mint2p2 += Vector3.Distance(t2p2.localPosition, transform.localPosition);
            mint2p3 += Vector3.Distance(t2p3.localPosition, transform.localPosition);
            timer = 0;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Team1"))
        {
            t1Touches++;
        }
        if (collision.gameObject.CompareTag("Team2"))
        {
            t2Touches++;
        }
        if (collision.gameObject.CompareTag("Goal1"))
        {
            t1Goals++;
            transform.localPosition = new Vector3(0, 0.5f, 0);
            rb.velocity = Vector3.zero;
        }
        if (collision.gameObject.CompareTag("Goal2"))
        {
            t2Goals++;
            transform.localPosition = new Vector3(0, 0.5f, 0);
            rb.velocity = Vector3.zero;
        }
    }
}
