namespace Mapbox.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using Mapbox.Unity.Location;
    using Mapbox.Unity.Map;
    using Mapbox.Utils;
    using UnityEngine;

    public class SpawnOnMap : MonoBehaviour
    {
        const double EarthRadiusMeters = 6378137d;

        [SerializeField]
        AbstractMap _map;

        [SerializeField]
        float _spawnScale = 75f;

        [SerializeField]
        float yOffset = -2f;

        [SerializeField]
        GameObject _markerPrefab;

        [SerializeField]
        int _targetPyramidCount = 2;

        [SerializeField]
        float _spawnRadiusMeters = 60f;

        [SerializeField]
        float _minimumSpawnDistanceMeters = 30f;

        [SerializeField]
        Transform jugador;

        readonly List<GameObject> _spawnedObjects = new List<GameObject>();
        AbstractLocationProvider _locationProvider;
        bool _mapInitialized;
        int _nextEventId = 1;

        void Start()
        {
            if (_map == null)
            {
                Debug.LogWarning("SpawnOnMap: No se ha asignado el mapa en el Inspector.");
                return;
            }

            if (_markerPrefab == null)
            {
                Debug.LogWarning("SpawnOnMap: No se ha asignado el prefab de piramide en el Inspector.");
                return;
            }

            _map.OnInitialized += Map_OnInitialized;

            if (_map.WorldRelativeScale > 0f)
            {
                Map_OnInitialized();
            }
        }

        void OnDestroy()
        {
            if (_map != null)
            {
                _map.OnInitialized -= Map_OnInitialized;
            }

            if (_locationProvider != null)
            {
                _locationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;
            }
        }

        void Map_OnInitialized()
        {
            if (_mapInitialized)
            {
                return;
            }

            _mapInitialized = true;
            _map.OnInitialized -= Map_OnInitialized;

            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider as AbstractLocationProvider;
            if (_locationProvider == null)
            {
                Debug.LogWarning("SpawnOnMap: No se encontro un LocationProvider valido.");
                return;
            }

            _locationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
            StartCoroutine(EnsurePyramidsWhenLocationIsReady());
        }

        void LocationProvider_OnLocationUpdated(Location location)
        {
            Vector2d userLocation;
            if (!TryGetUserLocation(out userLocation))
            {
                return;
            }

            RemovePyramidsOutsideSpawnRadius(userLocation);
            EnsurePyramidCount();
        }

#if UNITY_EDITOR
        void Update()
        {
            if (!_mapInitialized)
            {
                return;
            }

            Vector2d userLocation;
            if (!TryGetUserLocation(out userLocation))
            {
                return;
            }

            RemovePyramidsOutsideSpawnRadius(userLocation);
            EnsurePyramidCount();
        }
#endif

        IEnumerator EnsurePyramidsWhenLocationIsReady()
        {
            Vector2d userLocation;
            while (!TryGetUserLocation(out userLocation))
            {
                yield return null;
            }

            EnsurePyramidCount();
        }

        public void ConsumePyramid(GameObject pyramid)
        {
            if (pyramid != null && _spawnedObjects.Remove(pyramid))
            {
                Destroy(pyramid);
            }

            EnsurePyramidCount();
        }

        void EnsurePyramidCount()
        {
            Vector2d userLocation;
            if (!_mapInitialized || !TryGetUserLocation(out userLocation))
            {
                return;
            }

            _spawnedObjects.RemoveAll(item => item == null);

            while (_spawnedObjects.Count < _targetPyramidCount)
            {
                SpawnPyramidNearUser();
            }

            while (_spawnedObjects.Count > _targetPyramidCount)
            {
                int lastIndex = _spawnedObjects.Count - 1;
                GameObject extra = _spawnedObjects[lastIndex];
                _spawnedObjects.RemoveAt(lastIndex);
                Destroy(extra);
            }
        }

        bool TryGetUserLocation(out Vector2d userLocation)
        {
#if UNITY_EDITOR
            if (jugador == null)
            {
                GameObject playerTarget = GameObject.Find("PlayerTarget");
                if (playerTarget != null)
                {
                    jugador = playerTarget.transform;
                }
            }

            if (jugador != null && _map != null)
            {
                userLocation = _map.WorldToGeoPosition(jugador.position);
                return !userLocation.Equals(Vector2d.zero);
            }
#endif

            if (_locationProvider == null)
            {
                userLocation = Vector2d.zero;
                return false;
            }

            Location location = _locationProvider.CurrentLocation;
            userLocation = location.LatitudeLongitude;
            return location.IsLocationServiceEnabled && !userLocation.Equals(Vector2d.zero);
        }

        void RemovePyramidsOutsideSpawnRadius(Vector2d userLocation)
        {
            for (int i = _spawnedObjects.Count - 1; i >= 0; i--)
            {
                GameObject pyramid = _spawnedObjects[i];
                if (pyramid == null)
                {
                    _spawnedObjects.RemoveAt(i);
                    continue;
                }

                EventPointer pointer = pyramid.GetComponentInChildren<EventPointer>();
                if (pointer == null)
                {
                    continue;
                }

                if (DistanceMeters(userLocation, pointer.eventPose) > _spawnRadiusMeters)
                {
                    _spawnedObjects.RemoveAt(i);
                    Destroy(pyramid);
                }
            }
        }

        void SpawnPyramidNearUser()
        {
            Vector2d eventLocation = GetRandomLocationNearUser();
            GameObject instance = Instantiate(_markerPrefab);

            instance.name = string.Format("PiramideEvento_{0}", _nextEventId);
            instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
            PositionPyramid(instance, eventLocation);

            EventPointer pointer = instance.GetComponentInChildren<EventPointer>();
            if (pointer != null)
            {
                pointer.eventPose = eventLocation;
                pointer.eventID = _nextEventId;
                pointer.SetSpawner(this, instance);
            }

            _spawnedObjects.Add(instance);
            _nextEventId++;
        }

        void PositionPyramid(GameObject instance, Vector2d location)
        {
            Vector3 pos = _map.GeoToWorldPosition(location, true);
            pos.y += yOffset;
            instance.transform.localPosition = pos;
        }

        Vector2d GetRandomLocationNearUser()
        {
            Vector2d userLocation;
            if (!TryGetUserLocation(out userLocation))
            {
                return Vector2d.zero;
            }

            float maxDistance = Mathf.Max(1f, _spawnRadiusMeters);
            float minDistance = Mathf.Clamp(_minimumSpawnDistanceMeters, 0f, maxDistance);
            float distance = Random.Range(minDistance, maxDistance);
            float bearing = Random.Range(0f, Mathf.PI * 2f);

            double angularDistance = distance / EarthRadiusMeters;
            double bearingRadians = bearing;
            double lat1 = userLocation.x * Mathf.Deg2Rad;
            double lon1 = userLocation.y * Mathf.Deg2Rad;

            double lat2 = System.Math.Asin(
                System.Math.Sin(lat1) * System.Math.Cos(angularDistance) +
                System.Math.Cos(lat1) * System.Math.Sin(angularDistance) * System.Math.Cos(bearingRadians));
            double lon2 = lon1 + System.Math.Atan2(
                System.Math.Sin(bearingRadians) * System.Math.Sin(angularDistance) * System.Math.Cos(lat1),
                System.Math.Cos(angularDistance) - System.Math.Sin(lat1) * System.Math.Sin(lat2));

            return new Vector2d(lat2 * Mathf.Rad2Deg, lon2 * Mathf.Rad2Deg);
        }

        static double DistanceMeters(Vector2d from, Vector2d to)
        {
            double lat1 = from.x * Mathf.Deg2Rad;
            double lat2 = to.x * Mathf.Deg2Rad;
            double deltaLat = (to.x - from.x) * Mathf.Deg2Rad;
            double deltaLon = (to.y - from.y) * Mathf.Deg2Rad;

            double a =
                System.Math.Sin(deltaLat / 2d) * System.Math.Sin(deltaLat / 2d) +
                System.Math.Cos(lat1) * System.Math.Cos(lat2) *
                System.Math.Sin(deltaLon / 2d) * System.Math.Sin(deltaLon / 2d);
            double c = 2d * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1d - a));
            return EarthRadiusMeters * c;
        }

        public void IniciarProceso()
        {
            EnsurePyramidCount();
        }
    }
}
