using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrenade : MonoBehaviour
{
    public GameObject grenadePrefab;
    public Transform throwPoint;
    public int maxGrenades = 3;
    private int currentGrenades;

    void Start()
    {
        currentGrenades = maxGrenades;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && currentGrenades > 0)
        {
            ThrowGrenade();
        }
    }

    void ThrowGrenade()
    {
        Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);
        currentGrenades--;
    }

    public void AddGrenade()
    {
        if (currentGrenades < maxGrenades)
        {
            currentGrenades++;
        }
    }
}
