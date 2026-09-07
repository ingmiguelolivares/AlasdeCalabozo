namespace Mapbox.Examples
{
    using UnityEngine;
    using Mapbox.Utils;
    using Mapbox.Unity.Map;
    using Mapbox.Unity.Utilities;
    using System.Collections.Generic;
    using System.Collections;
    using System.Diagnostics.Tracing;

    public class SpawnOnMap : MonoBehaviour
    {
        [SerializeField]
        AbstractMap _map;

        [SerializeField]
        [Geocode]
        string[] _locationStrings;

        Vector2d[] _locations;

        [SerializeField]
        float _spawnScale = 75f;

        [SerializeField]
        float yOffset = -2f; // 👈 Valor editable desde el Inspector

        [SerializeField]
        GameObject _markerPrefab;

        List<GameObject> _spawnedObjects;

        [SerializeField]
        private Transform jugador;

        public float tiempoReinstancia = 5f;

        void Start()
        {
            _locations = new Vector2d[_locationStrings.Length];
            _spawnedObjects = new List<GameObject>();
            #if UNITY_ANDROID && !UNITY_EDITOR
            for (int i = 0; i < _locationStrings.Length; i++)
            {
                var locationString = _locationStrings[i];
                _locations[i] = Conversions.StringToLatLon(locationString);

                var instance = Instantiate(_markerPrefab);
                var pointer = instance.GetComponentInChildren<EventPointer>();

                if (pointer != null)
                {
                    pointer.eventPose = _locations[i];
                    pointer.eventID = i + 1;
                }

                Vector3 pos = _map.GeoToWorldPosition(_locations[i], true);
                pos.y += yOffset; // 👈 Aquí lo bajamos visualmente
                instance.transform.localPosition = pos;

                instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
                _spawnedObjects.Add(instance);
            }
            StartCoroutine(CambiarPosiciones());
            #endif
            //StartCoroutine(InstanciarEventosLoop());
        }

        IEnumerator CambiarPosiciones()
    {
        yield return new WaitForSeconds(0.5f);
            int count = _spawnedObjects.Count;
            for (int i = 0; i < count; i++)
            {
                var spawnedObject = _spawnedObjects[i];
                var location = _locations[i];

                Vector3 pos = _map.GeoToWorldPosition(location, true);
                pos.y += yOffset; // 👈 también lo aplicamos en Update
                spawnedObject.transform.localPosition = pos;

                spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
            }
    }

        void Update()
        {
            
        }
        
         public float tiempoDeEspera = 3f;
            public void IniciarProceso()
        {

            foreach (GameObject obj in _spawnedObjects)
            {
                Destroy(obj);
            }
            _spawnedObjects.Clear();
            StartCoroutine(InstanciarEventosLoop());
        }

        IEnumerator InstanciarEventosLoop()
        {
            yield return new WaitForSeconds(5);
            AgregarEventosCercanosAlJugador();
            /*
                                    while (true)
                                    {
                                        // ✅ Instanciar eventos
                                        AgregarEventosCercanosAlJugador();

                                        // ⏳ Esperar un tiempo
                                        yield return new WaitForSeconds(tiempoReinstancia);

                                        // ❌ Eliminar eventos anteriores
                                        foreach (GameObject obj in _spawnedObjects)
                                        {
                                            Destroy(obj);
                                        }
                                        _spawnedObjects.Clear();
                                    }*/
        }
       public void AgregarEventosCercanosAlJugador()
        {
            if (jugador == null)
            {
                Debug.LogWarning("⚠️ No se ha asignado el jugador en el Inspector.");
                return;
            }

            jugador = UnityEngine.GameObject.Find("PlayerTarget").transform;

            //jugador = new Vector3(0, 0, 0);

            for (int i = 0; i < 2; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle.normalized * 15f;
                Vector3 offset3D = new Vector3(randomOffset.x, 0, randomOffset.y);

                GameObject extraEvent = Instantiate(_markerPrefab);
                extraEvent.transform.localPosition = jugador.position + offset3D;
                extraEvent.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);

                // 🏷️ Asignar nombre identificable
                extraEvent.name = $"EventoAdicional_{i + 1}";

                _spawnedObjects.Add(extraEvent);
                Debug.Log("evento instanciado en: " + extraEvent.transform.position);
                Debug.Log("avatar jugador en: " + jugador.position);
            }

            Debug.Log("✅ Eventos adicionales instanciados alrededor del jugador.");
        }
    }
}
