using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ReferenceHolder : MonoBehaviour
{
    [System.Serializable]
    public class InternalClassTest
    {
        public UnityEvent internalEventTest;
    }

    public UnityEvent testEvent;
    public InternalClassTest nestedClass;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TestFunction()
    {

    }
}
