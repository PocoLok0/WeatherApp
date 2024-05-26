using log4net;
using Newtonsoft.Json;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
public class WeatherService
{
    private readonly string apiKey = "8e81a95785b16940fcf52c9b4cdb8459";
    private readonly HttpClient httpClient;

    public WeatherService()
    {
        httpClient = new HttpClient();
    }

    public async Task<string> GetWeatherAsync(string city)
    {
        
        var url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
        try
        {
            Console.WriteLine($"Запит погоди для міста: {city}");
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Отримана відповідь: {responseBody}");
            return responseBody;
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine("Помилка HTTP запиту", httpEx);
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Загальна помилка", ex);
            throw;
        }
    }
}


public class Main
{
    public double Temp { get; set; }
    public int Humidity { get; set; }
}
public class Weather
{
    public string Description { get; set; }
}
public class WeatherResponse
{
    public Main Main { get; set; }
    public List<Weather> Weather { get; set; }
}



public class FavoriteCities
{
    private  List<String> favoriteCities = new List<string>();
    private readonly string filePath = "favoriteCities.json";


    public FavoriteCities() 
    { 
        LoadCitiesFromFile(); 
    }
    
        
    

    public void AddCity(string cityName)
    {
        if (!favoriteCities.Contains(cityName))
        {
            favoriteCities.Add(cityName);
            SaveCitiesToFile();
        }
    }

    public void RemoveCity(string cityName)
    {
        if (favoriteCities.Contains(cityName))
        {
            favoriteCities.Remove(cityName);
            SaveCitiesToFile();
        }
    }

    public List<string> GetFavoriteCities()
    {
        return favoriteCities;
    }

    private void LoadCitiesFromFile()
    {
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            favoriteCities = JsonConvert.DeserializeObject<List<string>>(json);
        }
    }

    private void SaveCitiesToFile()
    {
        var json = JsonConvert.SerializeObject(favoriteCities, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
}

public class Program
{
    private static WeatherService weatherService = new WeatherService();
    private static FavoriteCities favoriteCitiesService = new FavoriteCities();

    private static readonly ILog log = LogManager.GetLogger(typeof(Program));

    public static async Task Main(string[] args)
    {
        log.Info("Програма почала роботу");
        try
        {
            while (true)
            {
                Console.WriteLine("1. Показати погоду");
                Console.WriteLine("2. Додати місто в обране");
                Console.WriteLine("3. Видалити місто з обраного");
                Console.WriteLine("4. Показати обрані міста");
                Console.WriteLine("5. Вихід");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await ShowWeather();
                        break;
                    case "2":
                        AddFavoriteCity();
                        break;
                    case "3":
                        RemoveFavoriteCity();
                        break;
                    case "4":
                        ShowFavoriteCities();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            log.Error("Виникла помилка", ex);
        }
        finally
        {
            log.Info("Програма завершила роботу");
        }
    }

    private static async Task ShowWeather()
    {
        Console.Write("Введіть назву міста: ");
        var city = Console.ReadLine();
        try
        {
            var weatherJson = await weatherService.GetWeatherAsync(city);
            var weather = JsonConvert.DeserializeObject<WeatherResponse>(weatherJson);
            Console.WriteLine($"Погода в {city}: {weather.Main.Temp}°C, {weather.Weather[0].Description}, Вологість: {weather.Main.Humidity}%");
            
            var fileName = $"{city}_weather_response.json"; 
            File.WriteAllText(fileName, weatherJson);
            Console.WriteLine($"Відповідь збережено у файлі: {fileName}");
        }
        catch (Exception ex)
        {
            log.Error($"Помилка при отриманні погоди для міста {city}", ex);
            Console.WriteLine("Не вдалося отримати погоду. Перевірте назву міста та спробуйте знову.");
        }
    }

    private static void AddFavoriteCity()
    {
        Console.Write("Введіть назву міста: ");
        var city = Console.ReadLine();
        favoriteCitiesService.AddCity(city);
        Console.WriteLine($"{city} додано в обране.");
    }

    private static void RemoveFavoriteCity()
    {
        Console.Write("Введіть назву міста для видалення з обраного: ");
        var city = Console.ReadLine();
        try
        {
            favoriteCitiesService.RemoveCity(city);
            Console.WriteLine($"{city} видалено з обраного.");
        }
        catch (Exception ex)
        {
            log.Error($"Помилка при видаленні міста {city} з обраних", ex);
            Console.WriteLine("Не вдалося видалити місто з обраних. Спробуйте знову.");
        }
    }

    private static void ShowFavoriteCities()
    {
        try
        {
            var cities = favoriteCitiesService.GetFavoriteCities();
            Console.WriteLine("Обрані міста:");
            foreach (var city in cities)
            {
                Console.WriteLine(city);
            }
        }
        catch (Exception ex)
        {
            log.Error("Помилка при відображенні обраних міст", ex);
            Console.WriteLine("Не вдалося відобразити обрані міста. Спробуйте знову.");
        }
    }
}