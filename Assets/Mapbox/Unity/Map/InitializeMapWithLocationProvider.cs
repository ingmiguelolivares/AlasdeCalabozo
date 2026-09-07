namespace Mapbox.Unity.Map
{
	using System.Collections;
	using Mapbox.Unity.Location;
	using UnityEngine;
	using UnityEngine.UI;
	using Mapbox.Utils;
	using System.Globalization;

	public class InitializeMapWithLocationProvider : MonoBehaviour
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		[System.Runtime.InteropServices.DllImport("__Internal")]
		private static extern void RequestLocation();
#endif
		[SerializeField]
		AbstractMap _map;

		ILocationProvider _locationProvider;

		public Button Iniciar;

		private void Awake()
		{
			// Prevent double initialization of the map. 
			_map.InitializeOnStart = false;
			print("funciona");
#if UNITY_WEBGL && !UNITY_EDITOR
			Iniciar.gameObject.SetActive(true);
#else
			Iniciar.gameObject.SetActive(false);
#endif

		}



		protected virtual IEnumerator Start()
		{
			yield return null;
			_locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
			_locationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated; ;
		}

		void LocationProvider_OnLocationUpdated(Unity.Location.Location location)
		{
			_locationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;
			_map.Initialize(location.LatitudeLongitude, _map.AbsoluteZoom);
			Debug.Log($"🗺️ Mapa inicializado con coordenadas: {location.LatitudeLongitude}");
		}

		public void OnLocationReceived(string coordenadas)
		{
#if UNITY_WEBGL && !UNITY_EDITOR
	string[] partes = coordenadas.Split(';');
	if (partes.Length == 2)
{
	if (double.TryParse(partes[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
		double.TryParse(partes[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
	{
		if (LocationProviderFactory.Instance.DefaultLocationProvider is WebGLLocationProvider webGL)
		{
			webGL.InjectLocation(lat, lon);

			// 🔽 Forzar inicialización del mapa
			var location = new Vector2d(lat, lon);
			if (_map != null)
			{
				_map.Initialize(location, _map.AbsoluteZoom);
				Debug.Log($"🗺️ Mapa inicializado con coordenadas: {location}");
			}
		}
	}
	else
	{
		Debug.LogError($"❌ Error de conversión: lat={partes[0]} lon={partes[1]} (formato inválido)");
	}
}
else
{
	Debug.LogError("❌ Coordenadas inválidas: se esperaban 2 elementos separados por ';'");
}
#endif
		}
		public void ObtenerUbicacion()
		{
#if UNITY_WEBGL && !UNITY_EDITOR
		RequestLocation();
		Iniciar.gameObject.SetActive(false);
		
#else
			Debug.Log("📍 ObtenerUbicacion solo está implementado para WebGL.");
#endif
		}
	}
}
