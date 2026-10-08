mergeInto(LibraryManager.library, {
  RequestLocation: function () {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        function (position) {
          var lat = position.coords.latitude;
          var lon = position.coords.longitude;
          SendMessage("Map", "OnLocationReceived", lat.toFixed(10) + ";" + lon.toFixed(10));
        },
        function (error) {
          console.error("❌ Error al obtener ubicación:", error.message);
          SendMessage("Map", "OnLocationFailed", error.message || "unknown");
        }
      );
    } else {
      console.error("❌ Geolocalización no es compatible en este navegador.");
      SendMessage("Map", "OnLocationFailed", "geolocation-not-supported");
    }
  }
});
