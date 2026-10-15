using UnityEngine;
using Mapbox.Unity.Location;
using Mapbox.Utils;

public class WebGLLocationProvider : AbstractLocationProvider
{
    [Header("Ubicación usada cuando el navegador no concede GPS")]
    [SerializeField] private double fallbackLatitude = 4.9426350479773165;
    [SerializeField] private double fallbackLongitude = -74.01285712209108;

    private void Awake()
    {
        SetLocation(fallbackLatitude, fallbackLongitude, false);
    }

    /// <summary>
    /// Método público que puedes invocar desde JavaScript (usando SendMessage)
    /// para inyectar coordenadas desde el navegador.
    /// </summary>
    public void InjectLocation(double latitude, double longitude)
    {
        SetLocation(latitude, longitude, true);
        Debug.Log($"🌍 WebGLLocationProvider: ubicación inyectada -> lat: {latitude}, lon: {longitude}");
    }

    public void UseFallbackLocation()
    {
        SetLocation(fallbackLatitude, fallbackLongitude, true);
        Debug.LogWarning("🌍 WebGLLocationProvider: se usa la ubicación de respaldo porque no se autorizó el GPS.");
    }

    private void SetLocation(double latitude, double longitude, bool notifyListeners)
    {
        _currentLocation = new Location
        {
            LatitudeLongitude = new Vector2d(latitude, longitude),
            Timestamp = Time.time,
            Accuracy = 1f,
            IsLocationUpdated = true,
            IsLocationServiceEnabled = true,
        };

        if (notifyListeners)
        {
            SendLocation(_currentLocation);
        }
    }
}
