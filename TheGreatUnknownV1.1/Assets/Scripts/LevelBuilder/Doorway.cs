using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Doorway : MonoBehaviour
{
    void onDrawGizmos()
    {
        Ray ray = new Ray(transform.position, transform.rotation * Vector3.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(ray);
    }
}
