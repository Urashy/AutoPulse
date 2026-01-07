using Api_c_sharp.Models.Repository.AI;
using AutoPulse.Shared.DTO.IA.Benchmark;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using Microsoft.AspNetCore.Mvc;

namespace Api_c_sharp.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class IAController : ControllerBase
{
    private readonly IIAService _iaService;

    public IAController(IIAService iaService)
    {
        _iaService = iaService;
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
            return BadRequest(ModelState);
        }

        try
        {
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
                return BadRequest(new { message = resultat.Error });
            }
            
            // Retourner le résultat polymorphe
            return Ok(resultat);
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                message = "Le service IA est temporairement indisponible",
                detail = ex.Message
            });
        }
    }

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
            return StatusCode(503, new
            {
                status = "error",
                message = "Impossible de vérifier l'état du service IA"
            });
        }
    }

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
    
    [ActionName("BenchmarkGetAll")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BenchmarkIAListDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BenchmarkIAListDTO>>> BenchmarkGetAll()
    {
        try
        {
            var benchmarks = await _iaService.GetAllBenchmarksAsync();
            return Ok(benchmarks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des benchmarks" });
        }
    }

    [ActionName("BenchmarkGetById")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BenchmarkIADTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BenchmarkIADTO>> BenchmarkGetById(int id)
    {
        try
        {
            var benchmark = await _iaService.GetBenchmarkByIdAsync(id);
            if (benchmark == null)
                return NotFound(new { message = $"Benchmark {id} introuvable" });

            return Ok(benchmark);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération du benchmark" });
        }
    }

    [ActionName("BenchmarkGetLatestByType")]
    [HttpGet]
    [ProducesResponseType(typeof(Dictionary<string, BenchmarkIADTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, BenchmarkIADTO>>> BenchmarkGetLatestByType()
    {
        try
        {
            var benchmarks = await _iaService.GetLatestBenchmarksByTypeAsync();
            return Ok(benchmarks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des derniers benchmarks" });
        }
    }

    [ActionName("BenchmarkGetStats")]
    [HttpGet]
    [ProducesResponseType(typeof(BenchmarkIAStatsDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<BenchmarkIAStatsDTO>> BenchmarkGetStats()
    {
        try
        {
            var stats = await _iaService.GetBenchmarkStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des statistiques" });
        }
    }

    [ActionName("BenchmarkGetHistoryByType")]
    [HttpGet("{modelType}")]
    [ProducesResponseType(typeof(IEnumerable<BenchmarkIAListDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BenchmarkIAListDTO>>> BenchmarkGetHistoryByType(
        string modelType,
        [FromQuery] int limit = 10)
    {
        try
        {
            var benchmarks = await _iaService.GetBenchmarkHistoryByTypeAsync(modelType, limit);
            return Ok(benchmarks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération de l'historique" });
        }
    }

    /// <summary>
    /// Crée un nouveau benchmark manuellement
    /// </summary>
    [ActionName("BenchmarkPost")]
    [HttpPost]
    [ProducesResponseType(typeof(BenchmarkIADTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BenchmarkIADTO>> BenchmarkPost([FromBody] BenchmarkIACreateDTO benchmark)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _iaService.CreateBenchmarkAsync(benchmark);
            return CreatedAtAction(nameof(BenchmarkGetById), new { id = created.IdBenchmark }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la création du benchmark" });
        }
    }

    /// <summary>
    /// Synchronise les benchmarks depuis l'API Python
    /// </summary>
    [ActionName("BenchmarkSync")]
    [HttpPost]
    [ProducesResponseType(typeof(IEnumerable<BenchmarkIADTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IEnumerable<BenchmarkIADTO>>> BenchmarkSync()
    {
        try
        {
            var benchmarks = await _iaService.SyncBenchmarksFromPythonAsync();
            return Ok(new
            {
                message = $"{benchmarks.Count()} benchmarks synchronisés avec succès",
                data = benchmarks
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new { message = "Service IA indisponible pour la synchronisation" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la synchronisation des benchmarks" });
        }
    }

    [ActionName("BenchmarkDelete")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> BenchmarkDelete(int id)
    {
        try
        {
            var deleted = await _iaService.DeleteBenchmarkAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Benchmark {id} introuvable" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la suppression du benchmark" });
        }
    }
}