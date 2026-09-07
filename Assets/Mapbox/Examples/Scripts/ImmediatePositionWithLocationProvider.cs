using UnityEngine;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class ImmediatePositionWithLocationProvider : MonoBehaviour
{
    [SerializeField]
    private AbstractMap _map;

    private ILocationProvider _locationProvider;
    private bool _hasMovedInWebGL = false;

    void Start()
    {
        _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
    }

    // 🔄 WebGL: mover el personaje una sola vez cuando se tenga la ubicación
    void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_hasMovedInWebGL)
        {
            if (ActualizarPosicion())
            {
                _hasMovedInWebGL = true;
                Debug.Log($"🌐 WebGL: Posición inicial del jugador establecida en {transform.position}");
            }
        }
#endif
    }

    // 🔁 Editor y dispositivos móviles: actualizar cada frame
    void LateUpdate()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        ActualizarPosicion();
#endif
    }

    /// <summary>
    /// Calcula y aplica la posición según la ubicación actual del proveedor.
    /// </summary>
    /// <returns>true si se actualizó, false si no hay datos</returns>
    bool ActualizarPosicion()
{
    if (_locationProvider == null || _map == null || _map.Root == null || !_map.Root.gameObject.activeInHierarchy)
        return false;

    var location = _locationProvider.CurrentLocation;

    if (!location.IsLocationUpdated || location.LatitudeLongitude == Vector2d.zero)
        return false;

    Vector3 worldPos = _map.GeoToWorldPosition(location.LatitudeLongitude, true);
    worldPos.y = transform.position.y;
    transform.position = worldPos;

    return true;
}
}