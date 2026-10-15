namespace Mapbox.Unity.Map
{
	using System.Collections;
	using Mapbox.Unity.Location;
	using UnityEngine;
	using UnityEngine.UI;
	using Mapbox.Utils;
	using System.Globalization;
	using TMPro;

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
		private TextMeshProUGUI _loadingPyramidsText;

		private void Awake()
		{
			// Prevent double initialization of the map. 
			_map.InitializeOnStart = false;
			print("funciona");
#if UNITY_WEBGL && !UNITY_EDITOR
			Iniciar.gameObject.SetActive(true);
			Iniciar.interactable = true;
#else
			Iniciar.gameObject.SetActive(false);
#endif

		}



		protected virtual IEnumerator Start()
		{
			yield return null;
			_locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
			_locationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;

			// WebGL starts with a safe default position. A later GPS result replaces it.
			var location = _locationProvider.CurrentLocation;
			if (location.LatitudeLongitude != Vector2d.zero)
			{
				_map.Initialize(location.LatitudeLongitude, _map.AbsoluteZoom);
				Debug.Log($"🗺️ Mapa inicializado con ubicación de respaldo: {location.LatitudeLongitude}");
			}
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
			ShowPyramidsLoadingMessage();
			// El primer toque inicia el flujo. Ocultamos el botón para impedir
			// solicitudes repetidas de ubicación e instanciaciones duplicadas.
			if (Iniciar != null)
			{
				Iniciar.gameObject.SetActive(false);
			}
		RequestLocation();
		
#else
			Debug.Log("📍 ObtenerUbicacion solo está implementado para WebGL.");
#endif
		}

		private void ShowPyramidsLoadingMessage()
		{
			if (_loadingPyramidsText == null)
			{
				var canvas = GetComponentInParent<Canvas>();
				if (canvas == null)
				{
					canvas = FindFirstObjectByType<Canvas>();
				}
				if (canvas == null)
				{
					Debug.LogWarning("No se encontró el Canvas para mostrar el estado de las pirámides.");
					return;
				}

				var statusObject = new GameObject("LoadingPyramidsText", typeof(RectTransform), typeof(TextMeshProUGUI));
				statusObject.transform.SetParent(canvas.transform, false);
				_loadingPyramidsText = statusObject.GetComponent<TextMeshProUGUI>();
				_loadingPyramidsText.font = TMP_Settings.defaultFontAsset;
				_loadingPyramidsText.fontSize = 42;
				_loadingPyramidsText.alignment = TextAlignmentOptions.Center;
				_loadingPyramidsText.color = Color.white;

				var rect = _loadingPyramidsText.rectTransform;
				rect.anchorMin = new Vector2(0.5f, 0.5f);
				rect.anchorMax = new Vector2(0.5f, 0.5f);
				rect.pivot = new Vector2(0.5f, 0.5f);
				rect.anchoredPosition = new Vector2(0f, -180f);
				rect.sizeDelta = new Vector2(1000f, 130f);
			}

			_loadingPyramidsText.text = "Cargando Pirámides…";
			_loadingPyramidsText.gameObject.SetActive(true);
			StopCoroutine(nameof(ShowPyramidsReadyMessage));
			StartCoroutine(nameof(ShowPyramidsReadyMessage));
		}

		private IEnumerator ShowPyramidsReadyMessage()
		{
			yield return new WaitForSeconds(5.5f);
			if (_loadingPyramidsText != null)
			{
				_loadingPyramidsText.text = "Pirámides listas. Toca una para entrar.";
			}
		}

		public void OnLocationFailed(string reason)
		{
#if UNITY_WEBGL && !UNITY_EDITOR
			Debug.LogWarning($"🌍 No se obtuvo GPS ({reason}). Se conserva la ubicación de respaldo.");
			if (LocationProviderFactory.Instance.DefaultLocationProvider is WebGLLocationProvider webGL)
			{
				webGL.UseFallbackLocation();
			}

#endif
		}
	}
}
