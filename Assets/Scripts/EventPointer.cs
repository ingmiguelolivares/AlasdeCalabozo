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
    private SpawnOnMap spawner;
    private GameObject pyramidRoot;

    public void SetSpawner(SpawnOnMap owner, GameObject root)
    {
        spawner = owner;
        pyramidRoot = root;
    }

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
            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
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
            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                OnTap();
            }
        }
#endif
    }

    void OnTap()
    {
        if (menuUIManager == null) return;
        if (playerLocation == null) return;

        //para restringir dependiendo de la distancia del usuario deshabilitado temporalmente
        var currentPlayerLocation = new GeoCoordinatePortable.GeoCoordinate(playerLocation.GetLocationLat(), playerLocation.GetLocationLon());
        var eventLocation = new GeoCoordinatePortable.GeoCoordinate(eventPose[0], eventPose[1]);
        var distance = currentPlayerLocation.GetDistanceTo(eventLocation);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("Distance is " + distance);
#endif

        if (eventManager != null && distance > eventManager.MaxDistance)
        {
            menuUIManager.DisplayUserNotInRangePanel();
            return;
        }

        menuUIManager.DisplayStartEventPanel(eventID);
        if (spawner != null)
        {
            spawner.ConsumePyramid(pyramidRoot != null ? pyramidRoot : gameObject);
        }
    }
}
