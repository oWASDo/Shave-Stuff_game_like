using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
#if ENABLE_INPUT_SYSTEM && UNITY_NEW_INPUT_SYSTEM_INSTALLED
using UnityEngine.InputSystem;
#endif

public class Painter : MonoBehaviour
{

    [Tooltip("Texture where the painter paint")]
    [SerializeField]
    Texture2D textureToModify;

    [Tooltip("Reference to grabbable component, to understand if is grabbed")]
    [SerializeField]
    Grabbable grabbable;

    [Tooltip("Spray particle effect gameobject to activate")]
    [SerializeField]
    GameObject particleEffectGaameobject;

    [Tooltip("Size of spry brush")]
    [SerializeField]
    int beamSizeInPixel;

    // runned on painting start
    UnityEvent onPressStart;

    // "runned on painting end
    UnityEvent onPressEnd;

    // kind of controller that grab the paiinter
    OVRInput.Controller controller;

    AudioSource paintAudioSource;

    // If true perform paint
    bool paint;
    
    void Start(){
        paintAudioSource = GetComponent<AudioSource>();
        onPressStart = new UnityEvent();
        onPressStart.AddListener(() => {
            paint = true;
            controller = grabbable.Controller;
            particleEffectGaameobject.SetActive(paint);
            if(paintAudioSource is not null){
                paintAudioSource.Play();
            }
            OnGrabStart(500.0f, 0.5f);

        });

        onPressEnd = new UnityEvent();
        onPressEnd.AddListener(() => {
            paint = false;
            particleEffectGaameobject.SetActive(paint);
            if(paintAudioSource is not null){
                paintAudioSource.Stop();
            }
            OnGrabEnd();
        });
    }

    // Understand if can start paint
    void ManagePaintingPerfom()
    {
        if(grabbable is not null && !paint && grabbable.OnGrabb && OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, grabbable.Controller))
        {
            onPressStart.Invoke();
        }
        else if(paint && !OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, grabbable.Controller))
        {
            onPressEnd.Invoke();
        }
    
    }

    // Perform painting logic
    void Paint()
    {
        
        if (textureToModify is null)
        {
            return;   
        }
        
        if(paint)
        {
            RaycastHit hit;
            // Raycast the Drawable object
            if(Physics.Raycast(transform.position,transform.forward, out hit, 0.5f, LayerMask.GetMask("Drawable")))
            {
                Vector2 textureCoord = hit.textureCoord;
                MeshRenderer meshRenderer = hit.collider.GetComponent<MeshRenderer>();
                if(meshRenderer.material.HasTexture("_PaintTexture")){
                    textureCoord.x *= textureToModify.width;
                    textureCoord.y *= textureToModify.height;

                    for(int i = (int)textureCoord.x - beamSizeInPixel; i < (int)textureCoord.x + beamSizeInPixel; i++)
                    {
                        for(int j = (int)textureCoord.y - beamSizeInPixel; j < (int)textureCoord.y + beamSizeInPixel; j++)
                        {
                            textureToModify.SetPixel(i, j, Color.red);
                        }
                    }

                    textureToModify.Apply();
                    meshRenderer.material.SetTexture("_PaintTexture",textureToModify);
                }
            }
        }
    }


    
    void OnGrabStart(float frequency, float amplitude){
        OVRInput.SetControllerVibration(frequency, amplitude, controller);
    }

    void OnGrabEnd(){
        OVRInput.SetControllerVibration(0.0f, 0.0f, controller);
    }


    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();
        ManagePaintingPerfom();
        Paint();
    }
}
