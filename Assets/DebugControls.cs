using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugControls : MonoBehaviour
{

    public void OnGenerate(InputValue value)
    {
        Debug.Log("Debug generation requested");
        DungeonGenerator.instance.Generate();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
