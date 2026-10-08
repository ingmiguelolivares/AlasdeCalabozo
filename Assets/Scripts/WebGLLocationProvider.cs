using UnityEngine;
using Mapbox.Unity.Location;
using Mapbox.Utils;

public class WebGLLocationProvider : AbstractLocationProvider
{
    private bool _locationReady = false;

    /// <summary>
    /// Método público que puedes invocar desde JavaScript (usando SendMessage)
    /// para inyectar coordenadas desde el navegador.
    /// </summary>
    public void InjectLocation(double latitude, double longitude)
    {
        _currentLocation = new Location
        {
            LatitudeLongitude = new Vector2d(latitude, longitude),
            Timestamp = Time.time,
            Accuracy = 1f,
            IsLocationServiceEnabled = true,
            IsLocationUpdated = true
        };

        _locationReady = true;
        SendLocation(_currentLocation);

        Debug.Log($"🌍 WebGLLocationProvider: ubicación inyectada -> lat: {latitude}, lon: {longitude}");
    }

    /// <summary>
    /// Unity llamará constantemente a esta propiedad para obtener la ubicación actual.
    /// </summary>
    public bool HasLocationReady()
    {
        return _locationReady;
    }
}
