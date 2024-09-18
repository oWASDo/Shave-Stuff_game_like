using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    

    [Tooltip("Object that rapresent the overlap ommong this objet and the hand-controller")]
    [SerializeField]
    GameObject onOverlapVisibleGameobject;

    // Rapresent the hand-controller thet grab this gameobject
    OVRInput.Controller controller;
    public OVRInput.Controller Controller{
        get{return controller;}
        set{controller = value;}
        }

    // If true is on overlap
    bool onOverlap;
    public bool OnOverlap { 
        get{return onOverlap;} 
        set{
            onOverlap = value;
            if(onOverlapVisibleGameobject is not null){
                onOverlapVisibleGameobject.SetActive(value);
            }
        }}

    // If tue is on grab
    bool onGrabb;
    public bool OnGrabb { 
        get{return onGrabb;} 
        set{
            onGrabb = value;
            if(rigidBody is not null)
            {
                rigidBody.useGravity = !value;
                rigidBody.isKinematic = value;
            }     
            if(collider is not null){
                collider.isTrigger = value;
            }
            
        }

    }
    Rigidbody rigidBody;
    Collider collider;

    // Start is called before the first frame update
    void Start()
    {
       rigidBody = GetComponent<Rigidbody>();
       collider = GetComponent<Collider>();
    }

   
}
