using Api_c_sharp.Models.Repository.AI;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using Microsoft.AspNetCore.Mvc;

namespace Api_c_sharp.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class IAController : ControllerBase
{
    private readonly IIAService _iaService;
    private readonly ILogger<IAController> _logger;

    public IAController(IIAService iaService, ILogger<IAController> logger)
    {
        _iaService = iaService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint polymorphe pour toutes les prédictions IA.
    /// Accepte 3 types de requêtes : CNN, Prediction, Ajustement.
    /// </summary>
    /// <param name="data">Données polymorphes (DataCNN, DataPrediction ou DataAjustement)</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="ResultatCNN"/> pour la reconnaissance visuelle (200 OK)</description></item>
    /// <item><description><see cref="ResultatPrediction"/> pour la prédiction de prix (200 OK)</description></item>
    /// <item><description><see cref="ResultatAjustement"/> pour l'ajustement de prix (200 OK)</description></item>
    /// <item><description><see cref="BadRequestResult"/> si les données sont invalides (400)</description></item>
    /// <item><description><see cref="StatusCodeResult"/> 503 si le service IA est indisponible</description></item>
    /// </list>
    /// </returns>
    [ActionName("Predict")]
    [HttpPost]
    [ProducesResponseType(typeof(ResultatCNN), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultatPrediction), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultatAjustement), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ResultatAI>> Predict([FromBody] DataAI data)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Modèle invalide reçu");
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Réception d'une requête IA de type: {Type}", data.Type);

            // Validation spécifique selon le type
            var validationError = ValidateData(data);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            // Appel au service Python
            var resultat = await _iaService.PredictAsync(data);

            // Vérifier si la prédiction a réussi
            if (!resultat.Success)
            {
                _logger.LogWarning(
                    "Prédiction échouée pour le type {Type}: {Error}",
                    resultat.Type,
                    resultat.Error
                );
                return BadRequest(new { message = resultat.Error });
            }

            _logger.LogInformation("Prédiction réussie pour le type {Type}", resultat.Type);

            // Retourner le résultat polymorphe
            return Ok(resultat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la prédiction IA");
            return StatusCode(503, new
            {
                message = "Le service IA est temporairement indisponible",
                detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Vérifie l'état du service IA Python
    /// </summary>
    [ActionName("Health")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> Health()
    {
        try
        {
            bool isHealthy = await _iaService.HealthCheckAsync();

            if (isHealthy)
            {
                return Ok(new { status = "healthy", message = "Service IA opérationnel" });
            }

            return StatusCode(503, new { status = "unhealthy", message = "Service IA indisponible" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du health check");
            return StatusCode(503, new
            {
                status = "error",
                message = "Impossible de vérifier l'état du service IA"
            });
        }
    }

    /// <summary>
    /// Valide les données selon leur type
    /// </summary>
    private string? ValidateData(DataAI data)
    {
        return data switch
        {
            DataCNN cnn when string.IsNullOrWhiteSpace(cnn.ImageBase64)
                => "L'image en base64 est requise pour la reconnaissance visuelle",

            DataPrediction pred when pred.ProdYear == null
                => "L'année de production est requise pour la prédiction",

            DataAjustement adj when adj.BasePrice <= 0
                => "Le prix de base doit être supérieur à 0",

            DataAjustement adj when string.IsNullOrWhiteSpace(adj.Description)
                => "La description est requise pour l'ajustement de prix",

            _ => null
        };
    }
}