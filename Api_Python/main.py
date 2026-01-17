"""
Application FastAPI - Point d'entrée principal
Architecture propre avec Repository pattern et Abstract Factory
"""
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager
import logging
from typing import Union
#az
from config.settings import API_TITLE, API_VERSION, API_DESCRIPTION, LOGGING_CONFIG
from schemas.dto import (
    PredictRequest,
    ResultatCNN,
    ResultatPrediction,
    ResultatAjustement,
    HealthResponse,
    ReloadResponse
)
from schemas.benchmark_dto import (
    CNNBenchmarkRequest,
    PriceBenchmarkRequest,
    AdjustmentBenchmarkRequest,
    BenchmarkResult,
    AllBenchmarkResults
)
from repositories.repository_manager import ModelRepositoryManager
from services.service_factory import ModelServiceFactory
from services.benchmark_service import BenchmarkService
from controllers.prediction_controller import PredictionController
from controllers.benchmark_controller import BenchmarkController

# Configuration du logging
logging.basicConfig(**LOGGING_CONFIG)
logger = logging.getLogger(__name__)


# ============================================================================
# INITIALISATION DES COMPOSANTS
# ============================================================================

# Repository Manager (gestion centralisée des modèles)
repo_manager = ModelRepositoryManager()

# Service Factory (création des services de prédiction)
service_factory = ModelServiceFactory(repo_manager)

# Benchmark Service (tests de performance)
benchmark_service = BenchmarkService(service_factory)

# Controllers
prediction_controller = PredictionController(repo_manager, service_factory)
benchmark_controller = BenchmarkController(benchmark_service)


# ============================================================================
# CYCLE DE VIE DE L'APPLICATION
# ============================================================================

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Gestion du cycle de vie de l'application"""
    logger.info("=" * 70)
    logger.info("🚀 DÉMARRAGE DE L'API")
    logger.info("=" * 70)
    
    # Chargement initial des modèles
    repo_manager.load_all_models()
    
    yield
    
    logger.info("=" * 70)
    logger.info("🛑 ARRÊT DE L'API")
    logger.info("=" * 70)


# ============================================================================
# APPLICATION FASTAPI
# ============================================================================

app = FastAPI(
    title=API_TITLE,
    version=API_VERSION,
    description=API_DESCRIPTION,
    lifespan=lifespan
)

# Configuration CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # À restreindre en production
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# ============================================================================
# ENDPOINTS
# ============================================================================

@app.get("/", tags=["Info"])
async def root():
    """Endpoint racine avec informations générales"""
    return {
        "message": f"{API_TITLE} - Architecture Clean",
        "version": API_VERSION,
        "models": ["CNN", "Prediction", "Ajustement"],
        "endpoints": {
            "predict": "/predict",
            "health": "/health",
            "reload": "/reload",
            "benchmark": "/benchmark",
            "benchmark_all": "/benchmark/all",
            "benchmark_info": "/benchmark/info",
            "docs": "/docs"
        }
    }


@app.get("/health", response_model=HealthResponse, tags=["Monitoring"])
async def health():
    """
    Vérification de l'état de santé de l'API
    Retourne le statut de tous les modèles chargés
    """
    return prediction_controller.get_health()


@app.post(
    "/predict",
    response_model=Union[ResultatCNN, ResultatPrediction, ResultatAjustement],
    tags=["Prédictions"]
)
async def predict(request: PredictRequest):
    """
    Endpoint polymorphe pour toutes les prédictions IA
    
    Types supportés:
    - "cnn": Reconnaissance visuelle de véhicules
    - "prediction": Prédiction du prix d'un véhicule
    - "ajustement": Ajustement du prix selon la description
    
    Returns:
        Résultat polymorphe selon le type de requête
    """
    return prediction_controller.predict(request)


@app.post("/reload", response_model=ReloadResponse, tags=["Administration"])
async def reload_models():
    """
    Recharge tous les modèles IA
    Utile après une mise à jour des fichiers de modèles
    
    Returns:
        Statut du rechargement pour chaque modèle
    """
    return prediction_controller.reload_models()


@app.get("/benchmark/info", tags=["Benchmark"])
async def benchmark_info():
    """
    Informations sur les benchmarks disponibles
    
    Returns:
        Description des benchmarks et métriques collectées
    """
    return benchmark_controller.get_benchmark_info()


@app.post(
    "/benchmark",
    response_model=BenchmarkResult,
    tags=["Benchmark"]
)
async def run_benchmark(
    request: Union[CNNBenchmarkRequest, PriceBenchmarkRequest, AdjustmentBenchmarkRequest]
):
    """
    Lance un benchmark sur un modèle spécifique
    
    Types supportés:
    - "cnn": Benchmark du modèle de reconnaissance visuelle
    - "prediction": Benchmark du modèle de prédiction de prix
    - "ajustement": Benchmark du modèle d'ajustement de prix
    
    Args:
        request: Configuration du benchmark
        - type: Type de modèle à benchmarker
        - num_iterations: Nombre d'itérations (défaut: 10)
        - include_detailed_results: Inclure les résultats détaillés
        - test_images/test_cases: Cas de test personnalisés (optionnel)
    
    Returns:
        Statistiques de performance détaillées incluant:
        - Temps d'inférence (moyen, min, max, écart-type)
        - Taux de succès
        - Prédictions par seconde
        - Métriques spécifiques au modèle
    """
    return benchmark_controller.run_benchmark(request)


@app.post(
    "/benchmark/all",
    response_model=AllBenchmarkResults,
    tags=["Benchmark"]
)
async def run_all_benchmarks(num_iterations: int = 10):
    """
    Lance un benchmark complet sur toutes les IA
    
    Args:
        num_iterations: Nombre d'itérations par modèle (défaut: 10)
    
    Returns:
        Résultats de benchmark pour tous les modèles avec:
        - Statistiques individuelles par modèle
        - Résumé global des performances
        - Informations système
    
    Note: Cette opération peut prendre plusieurs secondes selon le nombre d'itérations
    """
    return benchmark_controller.run_all_benchmarks(num_iterations)


# ============================================================================
# POINT D'ENTRÉE
# ============================================================================

if __name__ == "__main__":
    import uvicorn
    
    logger.info("=" * 70)
    logger.info("🚀 DÉMARRAGE DU SERVEUR")
    logger.info("📍 URL: http://0.0.0.0:8000")
    logger.info("📚 Documentation: http://0.0.0.0:8000/docs")
    logger.info("🔍 Health Check: http://0.0.0.0:8000/health")
    logger.info("🔄 Reload Models: http://0.0.0.0:8000/reload")
    logger.info("📊 Benchmarks: http://0.0.0.0:8000/benchmark/info")
    logger.info("=" * 70)
    
    uvicorn.run(
        app,
        host="0.0.0.0",
        port=8000,
        log_level="info"
    )
