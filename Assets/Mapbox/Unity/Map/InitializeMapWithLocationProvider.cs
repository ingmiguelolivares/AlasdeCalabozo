namespace Mapbox.Unity.Map
{
	using System.Collections;
	using System.Globalization;
	using Mapbox.Examples;
	using Mapbox.Unity.Location;
	using Mapbox.Unity.Utilities;
	using Mapbox.Utils;
	using UnityEngine;
	using UnityEngine.UI;

	public class InitializeMapWithLocationProvider : MonoBehaviour
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		[System.Runtime.InteropServices.DllImport("__Internal")]
		private static extern void RequestLocation();
#endif
		[SerializeField]
		AbstractMap _map;

		[SerializeField]
		float _webGLLocationTimeoutSeconds = 5f;

		ILocationProvider _locationProvider;
		bool _mapInitialized;
		bool _locationRequestPending;

		public Button Iniciar;

		private void Awake()
		{
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
			_locationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
		}

		void LocationProvider_OnLocationUpdated(Mapbox.Unity.Location.Location location)
		{
			if (!location.IsLocationServiceEnabled || location.LatitudeLongitude.Equals(Vector2d.zero))
			{
				return;
			}

			_locationRequestPending = false;
			InitializeOrUpdateMap(location.LatitudeLongitude, "coordenadas");
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
						_locationRequestPending = false;
						InitializeOrUpdateMap(new Vector2d(lat, lon), "coordenadas");
					}
				}
				else
				{
					Debug.LogError($"❌ Error de conversión: lat={partes[0]} lon={partes[1]} (formato inválido)");
					StartScenePositionFallback();
				}
			}
			else
			{
				Debug.LogError("❌ Coordenadas inválidas: se esperaban 2 elementos separados por ';'");
				StartScenePositionFallback();
			}
#endif
		}

		public void OnLocationFailed(string reason)
		{
#if UNITY_WEBGL && !UNITY_EDITOR
			Debug.LogWarning($"⚠️ No se pudo obtener ubicación WebGL: {reason}. Se usará la posición del jugador en escena.");
			StartScenePositionFallback();
#endif
		}

		public void ObtenerUbicacion()
		{
#if UNITY_WEBGL && !UNITY_EDITOR
			_locationRequestPending = true;
			RequestLocation();
			StartCoroutine(UseScenePositionIfLocationDoesNotArrive());
			Iniciar.gameObject.SetActive(false);
#else
			Debug.Log("📍 ObtenerUbicacion solo está implementado para WebGL.");
#endif
		}

		IEnumerator UseScenePositionIfLocationDoesNotArrive()
		{
			yield return new WaitForSeconds(_webGLLocationTimeoutSeconds);

			if (_locationRequestPending && !_mapInitialized)
			{
				Debug.LogWarning("⚠️ Tiempo de espera agotado para ubicación WebGL. Se usará la posición del jugador en escena.");
				StartScenePositionFallback();
			}
		}

		void StartScenePositionFallback()
		{
			if (_mapInitialized)
			{
				return;
			}

			_locationRequestPending = false;

			SpawnOnMap spawner = FindObjectOfType<SpawnOnMap>();
			if (spawner != null)
			{
				spawner.UseScenePlayerPositionFallback();
			}

			InitializeMapOnce(GetConfiguredMapLocation(), "fallback de escena");
		}

		void InitializeOrUpdateMap(Vector2d location, string source)
		{
			if (_map == null)
			{
				return;
			}

			if (!_mapInitialized)
			{
				InitializeMapOnce(location, source);
				return;
			}

			_map.UpdateMap(location);
			Debug.Log($"🗺️ Mapa actualizado con {source}: {location}");
		}

		void InitializeMapOnce(Vector2d location, string source)
		{
			if (_mapInitialized || _map == null)
			{
				return;
			}

			_mapInitialized = true;

			if (_locationProvider != null)
			{
				_locationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;
			}

			_map.Initialize(location, _map.AbsoluteZoom);
			Debug.Log($"🗺️ Mapa inicializado con {source}: {location}");
		}

		Vector2d GetConfiguredMapLocation()
		{
			if (_map == null || _map.Options == null || _map.Options.locationOptions == null)
			{
				return Vector2d.zero;
			}

			Vector2d configuredLocation;
			if (TryParseLatLon(_map.Options.locationOptions.latitudeLongitude, out configuredLocation))
			{
				return configuredLocation;
			}

			return new Vector2d(4.9426350479773165, -74.01285712209108);
		}

		bool TryParseLatLon(string value, out Vector2d location)
		{
			location = Vector2d.zero;
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			char[] separators = { ',', ';' };
			string[] parts = value.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2)
			{
				Debug.LogWarning($"⚠️ Coordenada configurada inválida: '{value}'");
				return false;
			}

			double lat;
			double lon;
			if (!double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out lat) ||
				!double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out lon))
			{
				Debug.LogWarning($"⚠️ No se pudo convertir la coordenada configurada: '{value}'");
				return false;
			}

			location = new Vector2d(lat, lon);
			return true;
		}
	}
}
