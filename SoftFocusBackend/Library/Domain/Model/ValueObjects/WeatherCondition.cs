namespace SoftFocusBackend.Library.Domain.Model.ValueObjects;

/// <summary>
/// Value Object que representa las condiciones climáticas obtenidas de OpenWeather
/// </summary>
public class WeatherCondition
{
    /// <summary>
    /// Condición principal del clima (Clear, Clouds, Rain, etc.)
    /// </summary>
    public string Condition { get; }

    /// <summary>
    /// Descripción detallada en español (ej: "Cielo despejado")
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Temperatura en grados Celsius
    /// </summary>
    public double Temperature { get; }

    /// <summary>
    /// Humedad en porcentaje
    /// </summary>
    public int Humidity { get; }

    /// <summary>
    /// Nombre de la ciudad/localidad
    /// </summary>
    public string CityName { get; }

    private WeatherCondition(
        string condition,
        string description,
        double temperature,
        int humidity,
        string cityName)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condición climática no puede estar vacía", nameof(condition));

        if (temperature < -100 || temperature > 60)
            throw new ArgumentException("Temperatura fuera de rango razonable", nameof(temperature));

        if (humidity < 0 || humidity > 100)
            throw new ArgumentException("Humedad debe estar entre 0 y 100", nameof(humidity));

        Condition = condition;
        Description = description;
        Temperature = temperature;
        Humidity = humidity;
        CityName = cityName;
    }

    /// <summary>
    /// Crea una nueva instancia de WeatherCondition
    /// </summary>
    public static WeatherCondition Create(
        string condition,
        string description,
        double temperature,
        int humidity,
        string cityName)
    {
        return new WeatherCondition(condition, description, temperature, humidity, cityName);
    }

    /// <summary>
    /// Determina si el clima es favorable para actividades al aire libre
    /// </summary>
    public bool IsOutdoorFriendly()
    {
        return Condition switch
        {
            "Clear" => true,
            "Clouds" => Temperature > 15,
            "Drizzle" => false,
            "Rain" => false,
            "Thunderstorm" => false,
            "Snow" => false,
            _ => Temperature > 20
        };
    }

    /// <summary>
    /// Determina si el clima sugiere actividades bajo techo
    /// </summary>
    public bool IsIndoorRecommended()
    {
        return !IsOutdoorFriendly();
    }

    public override string ToString()
    {
        return $"{Description} - {Temperature}°C, {Humidity}% humedad en {CityName}";
    }
}
