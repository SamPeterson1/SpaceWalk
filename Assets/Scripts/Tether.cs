using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class Tether : MonoBehaviour
{

    Rigidbody body;
    private TetherConnection tetherConnection;
    public TetherNode tetherNode;
    private bool knockedByPlayer;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    public TetherNode CreateNode()
    {
        tetherNode = new TetherNode(this);
        return tetherNode;
    }

    void Update()
    {
        if (!knockedByPlayer && body.constraints == RigidbodyConstraints.None && Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, Mathf.Infinity))
        {
            Vector3 localUp = transform.up;
            Vector3 target = hit.normal;
            transform.rotation *= Quaternion.FromToRotation(localUp, target);

            if (hit.distance > 1.2f) body.constraints = RigidbodyConstraints.None;
            else body.constraints = RigidbodyConstraints.FreezeAll;
        }
        else if (!Physics.Raycast(transform.position, -transform.up, 1.2f))
        {
            body.constraints = RigidbodyConstraints.None;
        }
    }

    public void MakeConnection(Tether other, GameObject prefab)
    {
        if (tetherConnection == null)
        {
            tetherConnection = Instantiate(prefab).GetComponent<TetherConnection>();
        }
        tetherConnection.tetherA = this;
        tetherConnection.tetherB = other;
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("Chunk"))
        {
            body.constraints = RigidbodyConstraints.FreezeAll; 
        } else
        {
            knockedByPlayer = true;
            body.constraints = RigidbodyConstraints.None;
        }
    }
}
