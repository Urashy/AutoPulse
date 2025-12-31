using System.Net.Http.Json;
using System.Globalization;
using AutoPulse.Shared.DTO.Immat;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class ImmatWebService : IImmatService
    {
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "https://api.apiplaqueimmatriculation.com";

        public ImmatWebService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ImmatResponseDTO> SearchByPlateNumberAsync(string plateNumber)
        {
            try
            {
                var url = $"{API_BASE_URL}/plaque?immatriculation={plateNumber}&token=TokenDemo2025A&pays=FR";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return new ImmatResponseDTO
                    {
                        Success = false,
                        Error = "Véhicule non trouvé ou erreur de l'API"
                    };
                }

                // Désérialiser la réponse de l'API
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiPlaquImmatriculationResponse>();

                if (apiResponse?.Data == null)
                {
                    return new ImmatResponseDTO
                    {
                        Success = false,
                        Error = "Réponse invalide de l'API"
                    };
                }

                // Vérifier si l'API a retourné une erreur
                if (!string.IsNullOrWhiteSpace(apiResponse.Data.Erreur))
                {
                    return new ImmatResponseDTO
                    {
                        Success = false,
                        Error = apiResponse.Data.Erreur
                    };
                }

                // Mapper les données de l'API vers notre DTO
                var vehicleData = MapApiResponseToVehicleData(apiResponse.Data, plateNumber);

                // Initialiser tous les champs comme sélectionnés par défaut
                InitializeSelectedFields(vehicleData);

                return new ImmatResponseDTO
                {
                    Success = true,
                    VehicleData = vehicleData
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la recherche par plaque: {ex.Message}");
                return new ImmatResponseDTO
                {
                    Success = false,
                    Error = $"Erreur: {ex.Message}"
                };
            }
        }

        private VehicleDataDTO MapApiResponseToVehicleData(ApiPlaquImmatriculationData data, string plateNumber)
        {
            double? engineVolume = null;

            if (!string.IsNullOrWhiteSpace(data.Ccm) &&
                double.TryParse(data.Ccm, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                engineVolume = parsed;
            }
            return new VehicleDataDTO
            {
                PlateNumber = plateNumber,
                Manufacturer = data.Marque,
                Model = data.Modele,
                Year = ExtractYearFromDate(data.Date1erCirUs),
                FuelType = data.EnergieNGC,
                GearBox = MapGearBox(data.BoiteVitesse),
                Category = data.GenreVCGNGC,
                Horsepower = ExtractHorsepower(data.PuisFiscReelCH),
                Torque = null,
                Cylinders = ParseIntOrNull(data.Cylindres),
                EngineVolume = engineVolume,
                DriveWheels = null,
                Doors = ParseIntOrNull(data.NbPortes),
                Seats = ParseIntOrNull(data.NrPassagers),
                Color = data.Couleur,
                Airbags = null,
                LeatherInterior = null,
                LeftHandDrive = data.Pays == "FR",
                FirstRegistration = ParseDate(data.Date1erCirUs)
            };
        }

        private void InitializeSelectedFields(VehicleDataDTO vehicleData)
        {
            vehicleData.SelectedFields = new Dictionary<string, bool>
            {
                { nameof(vehicleData.Manufacturer), !string.IsNullOrWhiteSpace(vehicleData.Manufacturer) },
                { nameof(vehicleData.Model), !string.IsNullOrWhiteSpace(vehicleData.Model) },
                { nameof(vehicleData.Year), vehicleData.Year.HasValue && vehicleData.Year > 0 },
                { nameof(vehicleData.FuelType), !string.IsNullOrWhiteSpace(vehicleData.FuelType) },
                { nameof(vehicleData.GearBox), !string.IsNullOrWhiteSpace(vehicleData.GearBox) },
                { nameof(vehicleData.Category), !string.IsNullOrWhiteSpace(vehicleData.Category) },
                { nameof(vehicleData.Horsepower), vehicleData.Horsepower.HasValue && vehicleData.Horsepower > 0 },
                { nameof(vehicleData.Torque), vehicleData.Torque.HasValue && vehicleData.Torque > 0 },
                { nameof(vehicleData.Cylinders), vehicleData.Cylinders.HasValue && vehicleData.Cylinders > 0 },
                { nameof(vehicleData.EngineVolume), vehicleData.EngineVolume.HasValue && vehicleData.EngineVolume > 0 },
                { nameof(vehicleData.DriveWheels), !string.IsNullOrWhiteSpace(vehicleData.DriveWheels) },
                { nameof(vehicleData.Doors), vehicleData.Doors.HasValue && vehicleData.Doors > 0 },
                { nameof(vehicleData.Seats), vehicleData.Seats.HasValue && vehicleData.Seats > 0 },
                { nameof(vehicleData.Color), !string.IsNullOrWhiteSpace(vehicleData.Color) },
                { nameof(vehicleData.Airbags), vehicleData.Airbags.HasValue && vehicleData.Airbags > 0 },
                { nameof(vehicleData.LeatherInterior), vehicleData.LeatherInterior.HasValue },
                { nameof(vehicleData.LeftHandDrive), vehicleData.LeftHandDrive.HasValue },
                { nameof(vehicleData.FirstRegistration), vehicleData.FirstRegistration.HasValue }
            };
        }

        // Méthodes utilitaires de parsing

        private int? ExtractYearFromDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return null;
            
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date.Year;
            }
            return null;
        }

        private DateTime? ParseDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return null;
            
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date;
            }
            return null;
        }

        private string? MapGearBox(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            
            return code.ToUpper() switch
            {
                "M" => "Manuelle",
                "A" => "Automatique",
                _ => code
            };
        }

        private int? ExtractHorsepower(string? powerString)
        {
            // "131 CH" -> 131
            if (string.IsNullOrWhiteSpace(powerString)) return null;
            
            var parts = powerString.Split(' ');
            if (parts.Length > 0 && int.TryParse(parts[0], out int power))
            {
                return power;
            }
            return null;
        }

        private int? ParseIntOrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return null;
        }
    }
}
