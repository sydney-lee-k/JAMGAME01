using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//Runs in edit mode to mirror the control points. You can disable this but its a lot more painful to work with
[ExecuteInEditMode]
public class BezierPoint : MonoBehaviour
{
    [SerializeField] private bool unlockPoints;
    public GameObject Control1;
    public GameObject Control2;

    private Vector3 a;
    private Vector3 b;

    private bool updateAorB;

    public bool DrawLines = true;
    public bool DrawPoints = true;
    
    public float speedMultiplier = 1.0f;
    
    private void Update()
    {
        // Ensure Control1 exists
        if (!Control1)
        {
            Control1 = new GameObject("Control A");
            Control1.transform.parent = transform;
            Control1.transform.position = transform.position + new Vector3(1, 0, 0);
        }

        // Ensure Control2 exists
        if (!Control2)
        {
            Control2 = new GameObject("Control B");
            Control2.transform.parent = transform;
            Control2.transform.position = transform.position + new Vector3(-1, 0, 0);
        }

        // Get current positions
        Vector3 newA = Control1.transform.position;
        Vector3 newB = Control2.transform.position;

        if (!unlockPoints)
        {
            // Maintain mirrored positioning relative to transform
            if (newA != a)
            {
                Control2.transform.position = transform.position - (newA - transform.position);
            }
            else if (newB != b)
            {
                Control1.transform.position = transform.position - (newB - transform.position);
            }
        }

        // Update stored positions
        a = Control1.transform.position;
        b = Control2.transform.position;
    }
    
    public Vector3 GetControl1()
    {
        return Control1.transform.position;
    }

    public Vector3 GetControl2()
    {
        return Control2.transform.position;
    }

    private void OnDrawGizmos()
    {
        
        if (DrawLines)
        {
            Handles.color = Color.white;
            Handles.DrawLine(transform.position, Control1.transform.position, 1f);
            Handles.DrawLine(transform.position, Control2.transform.position, 1f);
        }

        if (DrawPoints)
        {
            Gizmos.color = Color.darkRed;
            Gizmos.DrawSphere(Control1.transform.position, 0.15f);
            Gizmos.DrawSphere(Control2.transform.position, 0.15f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.2f);

        }

    }

}
