using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherNode
{

    public List<TetherNode> supplies = new List<TetherNode>();
    public TetherNode supplier = null;
    public Tether supplierObject;
    public TetherNetwork network;
    public Tether tetherObject;
    public Vector3 tetherPos;
    public bool hasOxygen = false;
    public bool isSupplier = false;
    public bool freezeOxygen = false;
    public bool loaded = true;

    public TetherNode(Tether tetherObject)
    {
        Debug.Log("New node");
        network = GameObject.FindGameObjectWithTag("Planet").GetComponent<TetherNetwork>();
        this.tetherObject = tetherObject;
        if(tetherObject != null) tetherPos = tetherObject.gameObject.transform.position;
    }

    public void Unload()
    {
        loaded = false;
        tetherPos = tetherObject.gameObject.transform.position;
        Object.Destroy(tetherObject.gameObject);
    }

    public Vector3 GetPos()
    {
        if(loaded)
        {
            return tetherObject.gameObject.transform.position;
        } else
        {
            return tetherPos;
        }
    }

    public void Reload()
    {
        loaded = true;
        tetherObject = Object.Instantiate(network.tetherPrefab, tetherPos, Quaternion.identity).GetComponent<Tether>();
        tetherObject.tetherNode = this;
    }

    public void RemakeConnections()
    {
        if (supplier != null)
        {
            tetherObject.MakeConnection(supplier.tetherObject, network.connectionPrefab);
        }
    }

    public void UpdateConnections()
    {
        if (loaded && (supplier == null || supplier.loaded))
        {
            if (!isSupplier && (supplier == null || !InRange(supplier, this) || !hasOxygen))
            {
                AttemptConnection();
            }

            UpdateHasOxygen();
        }
    }

    public void UpdateHasOxygen()
    {
        if (loaded && (supplier == null || supplier.loaded))
        {
            if (isSupplier)
            {
                if (!hasOxygen) network.updatingOxygen = true;
                hasOxygen = true;
            }
            else if (supplier == null)
            {
                if (hasOxygen) network.updatingOxygen = true;
                hasOxygen = false;
            }
            else if (!InRange(this, supplier))
            {
                if (hasOxygen) network.updatingOxygen = true;
                hasOxygen = false;
            }
            else
            {
                if (hasOxygen != supplier.hasOxygen) network.updatingOxygen = true;
                hasOxygen = supplier.hasOxygen;
            }
        }
    }

    void AttemptConnection()
    {
        TetherNode closest = FindNearestNodeWithOxygen(!hasOxygen);
        if (closest != null && !supplies.Contains(closest))
        {
            if (supplier != null)
            {
                supplier.supplies.Remove(this);
            }
            supplier = closest;
            supplierObject = closest.tetherObject;
            closest.supplies.Add(this);
            tetherObject.MakeConnection(closest.tetherObject, network.connectionPrefab);
        }
    }

    float Dist(TetherNode a, TetherNode b)
    {
        Vector3 posA = a.tetherObject.transform.position;
        Vector3 posB = b.tetherObject.transform.position;
        return (posA - posB).magnitude;
    }

    bool InRange(TetherNode a, TetherNode b)
    {
        return (Dist(a, b) < network.connectDist);
    }

    TetherNode FindNearestNodeWithOxygen(bool oxygen)
    {
        TetherNode closest = null;
        float minDist = -1;
        foreach(TetherNode tether in network.loadedNodes)
        {
            if(tether != this && InRange(tether, this) && tether.hasOxygen == oxygen)
            {
                float dist = Dist(tether, this);
                if(minDist == -1 || dist < minDist)
                {
                    minDist = dist;
                    closest = tether;
                }
            }
        }

        return closest;
    }
}
