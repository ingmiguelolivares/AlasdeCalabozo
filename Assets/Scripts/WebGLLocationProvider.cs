using UnityEngine;
using Mapbox.Unity.Location;
using Mapbox.Utils;

public class WebGLLocationProvider : AbstractLocationProvider
{
    private Location _currentLocation = new Location();
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
            IsLocationUpdated = true
        };

        _locationReady = true;

        Debug.Log($"🌍 WebGLLocationProvider: ubicación inyectada -> lat: {latitude}, lon: {longitude}");
    }

    /// <summary>
    /// Unity llamará constantemente a esta propiedad para obtener la ubicación actual.
    /// </summary>
    public new Location CurrentLocation
{
	get
	{
		return _locationReady ? _currentLocation : new Location();
	}
}
}