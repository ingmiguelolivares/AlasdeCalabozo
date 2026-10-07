using UnityEngine;

public class EventManager : MonoBehaviour
{
    public int MaxDistance = 35;
    [SerializeField] SphinxAccessController sphinxAccessController;
    [SerializeField] Vector3 runtimeSphinxPosition = new Vector3(3f, 2f, 3f);
    [SerializeField] float runtimeSphinxScale = 1.5f;

    void Start()
    {
        EnsureSphinxAccess();
        EnsureRuntimeSphinxInteraction();
    }

    public void ActivateEvent(int eventID)
    {
        if (eventID == SphinxAccessController.SphinxEventId)
        {
            EnsureSphinxAccess();
            sphinxAccessController.LoadFinalBossIfAllowed();
            return;
        }

        WebGLSceneLoader.Load(this, "MainMenu");
    }

    void EnsureSphinxAccess()
    {
        if (sphinxAccessController == null)
            sphinxAccessController = FindAnyObjectByType<SphinxAccessController>();

        if (sphinxAccessController == null)
            sphinxAccessController = gameObject.AddComponent<SphinxAccessController>();
    }

    void EnsureRuntimeSphinxInteraction()
    {
        EnsureSphinxAccess();
        if (FindAnyObjectByType<SphinxMapActivator>() != null)
            return;

        GameObject sphinx = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        sphinx.name = "EsfingeInteractiva_Runtime";
        sphinx.transform.position = runtimeSphinxPosition;
        sphinx.transform.localScale = Vector3.one * runtimeSphinxScale;

        SphinxMapActivator activator = sphinx.AddComponent<SphinxMapActivator>();
        activator.RefreshSphinxAvailability();
    }
}
