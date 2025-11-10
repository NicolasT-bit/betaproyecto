using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace ManoVecina.Api.Services;

public class DistanceMatrixService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<DistanceMatrixService> _logger;

    public DistanceMatrixService(HttpClient http, IConfiguration config, ILogger<DistanceMatrixService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene la distancia (en kilómetros) entre dos coordenadas usando Google Distance Matrix API.
    /// Retorna null si no se puede calcular o si no hay API Key configurada.
    /// </summary>
    public async Task<double?> GetDistanceKmAsync(double fromLat, double fromLng, double toLat, double toLng)
    {
        var key = _config["GoogleApis:DistanceMatrixKey"];

        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("API key de Google Distance Matrix no configurada. Se omite cálculo de distancia.");
            return null;
        }

        var url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={fromLat},{fromLng}&destinations={toLat},{toLng}&key={key}&units=metric";

        try
        {
            var res = await _http.GetFromJsonAsync<DistanceMatrixResponse>(url);

            if (res?.rows == null || res.rows.Length == 0)
            {
                _logger.LogWarning("Respuesta inválida de Google Distance Matrix: sin filas.");
                return null;
            }

            var element = res.rows.First().elements?.FirstOrDefault();
            if (element?.status != "OK" || element.distance == null)
            {
                _logger.LogWarning("Google Distance Matrix devolvió estado {Status}", element?.status);
                return null;
            }

            var meters = element.distance.value;
            return meters / 1000d; // convertir a km
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar Google Distance Matrix API");
            return null;
        }
    }

    // Clases para deserialización JSON
    private sealed class DistanceMatrixResponse
    {
        public Row[]? rows { get; set; }
        public string? status { get; set; }

        public sealed class Row
        {
            public Element[]? elements { get; set; }
        }

        public sealed class Element
        {
            public Distance? distance { get; set; }
            public string? status { get; set; }
        }

        public sealed class Distance
        {
            public int value { get; set; } // metros
            public string? text { get; set; }
        }
    }
}
