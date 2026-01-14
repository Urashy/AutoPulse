"""
Controller pour les endpoints de benchmark
"""
import logging
from typing import Union
from schemas.benchmark_dto import (
    CNNBenchmarkRequest,
    PriceBenchmarkRequest,
    AdjustmentBenchmarkRequest,
    BenchmarkResult,
    AllBenchmarkResults
)
from services.benchmark_service import BenchmarkService

logger = logging.getLogger(__name__)


class BenchmarkController:
    """Contrôleur pour les endpoints de benchmark"""
    
    def __init__(self, benchmark_service: BenchmarkService):
        self.benchmark_service = benchmark_service
    
    def run_benchmark(
        self,
        request: Union[CNNBenchmarkRequest, PriceBenchmarkRequest, AdjustmentBenchmarkRequest]
    ) -> BenchmarkResult:
        """
        Lance un benchmark sur un modèle spécifique
        
        Args:
            request: Configuration du benchmark
            
        Returns:
            Résultats du benchmark
        """
        logger.info(f"📊 Benchmark demandé: {request.type}")
        
        if request.type == "cnn":
            return self.benchmark_service.benchmark_cnn(request)
        elif request.type == "prediction":
            return self.benchmark_service.benchmark_price(request)
        elif request.type == "ajustement":
            return self.benchmark_service.benchmark_adjustment(request)
        else:
            raise ValueError(f"Type de benchmark inconnu: {request.type}")
    
    def run_all_benchmarks(self, num_iterations: int = 10) -> AllBenchmarkResults:
        """
        Lance un benchmark complet sur toutes les IA
        
        Args:
            num_iterations: Nombre d'itérations par modèle
            
        Returns:
            Résultats de tous les benchmarks
        """
        logger.info(f"📊 Benchmark complet demandé ({num_iterations} itérations)")
        return self.benchmark_service.benchmark_all(num_iterations)
    
    def get_benchmark_info(self) -> dict:
        """
        Retourne les informations sur les benchmarks disponibles
        
        Returns:
            Informations sur les benchmarks
        """
        return {
            "available_benchmarks": ["cnn", "prediction", "ajustement"],
            "endpoints": {
                "single": "/benchmark",
                "all": "/benchmark/all"
            },
            "description": {
                "cnn": "Benchmark du modèle de reconnaissance visuelle",
                "prediction": "Benchmark du modèle de prédiction de prix",
                "ajustement": "Benchmark du modèle d'ajustement de prix"
            },
            "metrics_collected": [
                "inference_time_ms",
                "success_rate",
                "predictions_per_second",
                "model_specific_metrics"
            ]
        }
