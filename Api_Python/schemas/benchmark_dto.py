"""
Schémas Pydantic pour les benchmarks des IA
"""
from pydantic import BaseModel, Field
from typing import List, Optional, Dict, Any, Literal
from datetime import datetime


# ============================================================================
# SCHÉMAS DE BENCHMARK - REQUÊTES
# ============================================================================

class BenchmarkRequest(BaseModel):
    """Requête de base pour lancer un benchmark"""
    type: Literal["cnn", "prediction", "ajustement"]
    num_iterations: int = Field(default=10, ge=1, le=1000, description="Nombre d'itérations")
    include_detailed_results: bool = Field(default=False, description="Inclure tous les résultats détaillés")


class CNNBenchmarkRequest(BenchmarkRequest):
    """Benchmark spécifique pour le modèle CNN"""
    type: Literal["cnn"] = "cnn"
    test_images: Optional[List[str]] = Field(
        default=None, 
        description="Liste d'images base64 à tester (si vide, génère des images de test)"
    )


class PriceBenchmarkRequest(BenchmarkRequest):
    """Benchmark spécifique pour le modèle de prédiction de prix"""
    type: Literal["prediction"] = "prediction"
    test_cases: Optional[List[Dict[str, Any]]] = Field(
        default=None,
        description="Liste de cas de test avec caractéristiques véhicules"
    )


class AdjustmentBenchmarkRequest(BenchmarkRequest):
    """Benchmark spécifique pour le modèle d'ajustement"""
    type: Literal["ajustement"] = "ajustement"
    test_cases: Optional[List[Dict[str, Any]]] = Field(
        default=None,
        description="Liste de cas de test avec prix et descriptions"
    )


# ============================================================================
# SCHÉMAS DE BENCHMARK - RÉSULTATS
# ============================================================================

class PredictionMetrics(BaseModel):
    """Métriques pour une prédiction individuelle"""
    iteration: int
    success: bool
    inference_time_ms: float
    error: Optional[str] = None
    result: Optional[Dict[str, Any]] = None


class PerformanceStats(BaseModel):
    """Statistiques de performance agrégées"""
    total_iterations: int
    successful_predictions: int
    failed_predictions: int
    success_rate_percent: float
    
    # Temps d'inférence
    avg_inference_time_ms: float
    min_inference_time_ms: float
    max_inference_time_ms: float
    std_inference_time_ms: float
    
    # Débit
    predictions_per_second: float
    total_time_seconds: float


class CNNBenchmarkStats(PerformanceStats):
    """Statistiques spécifiques au CNN"""
    avg_confidence: Optional[float] = None
    top1_accuracy: Optional[float] = None
    top5_accuracy: Optional[float] = None
    detected_classes: Optional[List[str]] = None


class PriceBenchmarkStats(PerformanceStats):
    """Statistiques spécifiques à la prédiction de prix"""
    avg_predicted_price: Optional[float] = None
    min_predicted_price: Optional[float] = None
    max_predicted_price: Optional[float] = None
    std_predicted_price: Optional[float] = None
    price_range: Optional[float] = None


class AdjustmentBenchmarkStats(PerformanceStats):
    """Statistiques spécifiques à l'ajustement"""
    avg_reduction_percent: Optional[float] = None
    min_reduction_percent: Optional[float] = None
    max_reduction_percent: Optional[float] = None
    category_distribution: Optional[Dict[str, int]] = None


class BenchmarkResult(BaseModel):
    """Résultat complet d'un benchmark"""
    benchmark_id: str
    model_type: str
    timestamp: datetime
    
    # Configuration
    config: Dict[str, Any]
    
    # Statistiques
    stats: PerformanceStats
    
    # Résultats détaillés (optionnel)
    detailed_results: Optional[List[PredictionMetrics]] = None
    
    # Métadonnées système
    system_info: Optional[Dict[str, Any]] = None


class AllBenchmarkResults(BaseModel):
    """Résultats de benchmark pour toutes les IA"""
    cnn_benchmark: Optional[BenchmarkResult] = None
    price_benchmark: Optional[BenchmarkResult] = None
    adjustment_benchmark: Optional[BenchmarkResult] = None
    
    overall_summary: Dict[str, Any]
    generated_at: datetime
