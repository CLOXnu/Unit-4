using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    public enum prop: int
    {
        Powerup = 1,
        Bigger = 2,
        Smaller = 3,
        Superstar = 4
    }
    public int prop_id;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * 5.0f);
    }
}
