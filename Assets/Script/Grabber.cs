using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM && UNITY_NEW_INPUT_SYSTEM_INSTALLED
using UnityEngine.InputSystem;
#endif

public class Grabber : MonoBehaviour
{

    public OVRInput.Controller controller;

    [Tooltip("Speed that the grabed object have to follow the hand-controller")]
    [SerializeField]
    float FollowHandSpeed = 10.0f;

    [Tooltip("Reference to the object that rapresent the hand-controller")]
    [SerializeField]
    GameObject handGameobjectToHide;

    // Object grabbed by the hand
    Grabbable grabbed;

    //If true the "grabbed" object followthe hand-controller
    bool OnGrabPerform;

    // Event thet run when the "grabbed" object start to be grabbed
    UnityEvent onGrabStart;

    // Event thet run when the "grabbed" object end to be grabbed
    UnityEvent onGrabEnd;

    AudioSource grabAudioSource;

    void Start(){

        grabAudioSource = GetComponent<AudioSource>();

        onGrabStart = new UnityEvent();
        onGrabStart.AddListener(() => {
            grabbed.OnGrabb = true;
            OnGrabPerform = true;
            StartCoroutine(OnGrabVibration(500.0f, 0.5f));
            if(grabAudioSource is not null){
                grabAudioSource.PlayOneShot(grabAudioSource.clip);
            }
            if(handGameobjectToHide is not null){
                handGameobjectToHide.SetActive(false);
            }
        });

        onGrabEnd = new UnityEvent();
        onGrabEnd.AddListener(() => {
            grabbed.Controller = OVRInput.Controller.None;
            grabbed.OnGrabb = false;
            OnGrabPerform = false;
            grabbed = null;
            if(handGameobjectToHide is not null){
                handGameobjectToHide.SetActive(true);
            }
            StartCoroutine(OnGrabVibration(500.0f, 0.5f));
        });
    }

    void OnTriggerEnter(Collider other)
    { 
        Grabbable grabbable = other.GetComponent<Grabbable>();
        if(grabbable is not null && !grabbable.OnOverlap)
        {
            grabbed = grabbable;
            grabbable.OnOverlap = true;
            grabbable.Controller = controller;
            StartCoroutine(OnGrabVibration(200.0f, 0.5f, 0.1f));
        }   
    }

    void OnTriggerExit(Collider other)
    {
        Grabbable grabbable = other.GetComponent<Grabbable>();
        if(grabbable is not null && !grabbable.OnGrabb && grabbable.OnOverlap)
        {
            grabbed = null;
            grabbable.OnOverlap = false;
            grabbable.Controller = OVRInput.Controller.None;
            StartCoroutine(OnGrabVibration(200.0f, 0.5f, 0.1f));
        }   
    }

    void ManageGrabPerfom(){

        if(grabbed is not null && !grabbed.OnGrabb && OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controller))
        {
            onGrabStart.Invoke();
        }
        else if (grabbed.OnGrabb && !OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controller))
        {
            onGrabEnd.Invoke();
        }
        
    }
    void MoveGrabbed()
    {
        if(OnGrabPerform)
        {
            grabbed.transform.position = Vector3.Lerp(grabbed.transform.position, transform.position, Time.deltaTime * FollowHandSpeed);
            grabbed.transform.rotation = Quaternion.Slerp(grabbed.transform.rotation, transform.rotation, Time.deltaTime * FollowHandSpeed);
            grabbed.OnGrabb = true;
        }
    }

    // TODO: To manage in one class 
    IEnumerator OnGrabVibration(float frequency, float amplitude, float vibrationTime = 0.25f){
        OVRInput.SetControllerVibration(frequency, amplitude,controller);
        yield return new WaitForSeconds(vibrationTime);
        OVRInput.SetControllerVibration(0.0f, 0.0f, controller);
    }
        

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();
        ManageGrabPerfom();
        MoveGrabbed();

    }
}
