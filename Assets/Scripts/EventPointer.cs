using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Examples;
using Mapbox.Utils;

public class EventPointer : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 50f;
    [SerializeField] float amplitude = 2.0f;
    [SerializeField] float frequency = 0.50f;

    [SerializeField] private LocationStatus playerLocation;
    [SerializeField] private Camera raycastCamera;
    public Vector2d eventPose;
    public int eventID;
    [SerializeField] private MenuUIManager menuUIManager;
    [SerializeField] private EventManager eventManager;

    void Start()
    {
        if (raycastCamera == null) raycastCamera = Camera.main;

        if (menuUIManager == null)
        {
            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo != null) menuUIManager = canvasGo.GetComponent<MenuUIManager>();
        }

        if (eventManager == null)
        {
            var go = GameObject.Find("botonasos");
            if (go != null) eventManager = go.GetComponent<EventManager>();
        }

        if (playerLocation == null)
        {
            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo != null) playerLocation = canvasGo.GetComponent<LocationStatus>();
        }
    }

    void Update()
    {
        FloatAndRotatePointer();
        DetectTouchInput();
    }

    void FloatAndRotatePointer()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, (Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude) + 2, transform.position.z);
    }

    void DetectTouchInput()
    {
        if (raycastCamera == null) return;

        // Toque en pantalla (solo en dispositivos m�viles)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = raycastCamera.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.GetComponentInParent<EventPointer>() == this)
            {
                OnTap();
            }
        }

#if UNITY_EDITOR || UNITY_WEBGL
        // Clic con el mouse (solo en editor)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.GetComponentInParent<EventPointer>() == this)
            {
                OnTap();
            }
        }
#endif
    }

    void OnTap()
    {
        if (menuUIManager == null)
        {
            Debug.LogError("❌ No se encontró MenuUIManager; no se puede abrir el panel del calabozo.");
            return;
        }

        // Los eventos WebGL se generan alrededor del jugador, por lo que no se
        // bloquean por distancia. Esto abre el panel EventPanelUserInRangeID1.
        Debug.Log($"🏛️ Pirámide seleccionada: abriendo panel para el evento {eventID}.");
        menuUIManager.DisplayStartEventPanel(eventID);
    }
}
