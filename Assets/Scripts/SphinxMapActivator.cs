using UnityEngine;

public class SphinxMapActivator : MonoBehaviour
{
    [SerializeField] SphinxAccessController accessController;
    [SerializeField] float rotationSpeed = 35f;
    [SerializeField] float bobAmplitude = 0.4f;
    [SerializeField] float bobFrequency = 0.75f;

    Vector3 startPosition;

    void Awake()
    {
        if (accessController == null)
            accessController = FindAnyObjectByType<SphinxAccessController>();
    }

    void Start()
    {
        startPosition = transform.position;
        RefreshSphinxAvailability();
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * Mathf.PI * bobFrequency) * bobAmplitude);

        if (Input.GetMouseButtonDown(0))
            TryClick(Input.mousePosition);

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            TryClick(Input.GetTouch(0).position);
    }

    public void RefreshSphinxAvailability()
    {
        if (accessController == null)
            return;

        gameObject.SetActive(accessController.CanAccessSphinx(accessController.GetCollectedCardsCount()));
    }

    void TryClick(Vector2 screenPosition)
    {
        Camera cam = Camera.main;
        if (cam == null || accessController == null)
            return;

        Ray ray = cam.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            accessController.LoadFinalBossIfAllowed();
    }
}
