"""
Service de benchmark pour tester les performances des modèles IA
"""
import logging
import time
import uuid
import numpy as np
import platform
import psutil
import tracemalloc
import gc
from datetime import datetime
from typing import List, Dict, Any, Optional
from schemas.benchmark_dto import (
    BenchmarkRequest,
    BenchmarkResult,
    PredictionMetrics,
    PerformanceStats,
    CNNBenchmarkStats,
    PriceBenchmarkStats,
    AdjustmentBenchmarkStats,
    AllBenchmarkResults
)
from schemas.dto import DataCNN, DataPrediction, DataAjustement
from services.service_factory import ModelServiceFactory

logger = logging.getLogger(__name__)


class BenchmarkService:
    """Service pour effectuer des benchmarks sur les modèles"""
    
    def __init__(self, service_factory: ModelServiceFactory):
        self.service_factory = service_factory
    
    def benchmark_cnn(
        self,
        request: 'CNNBenchmarkRequest'
    ) -> BenchmarkResult:
        """
        Benchmark du modèle CNN
        
        Args:
            request: Configuration du benchmark
            
        Returns:
            Résultats du benchmark
        """
        logger.info(f"🏁 Démarrage benchmark CNN ({request.num_iterations} itérations)")
        
        service = self.service_factory.get_service("cnn")
        test_images = self._get_test_images(request.test_images, request.num_iterations)
        
        metrics = []
        
        # ✅ Mesure de la RAM AVANT le benchmark
        gc.collect()  # Force garbage collection
        tracemalloc.start()
        snapshot_start = tracemalloc.take_snapshot()
        process = psutil.Process()
        memory_before = process.memory_info().rss / (1024 * 1024)  # MB
        
        start_time = time.time()
        
        for i in range(request.num_iterations):
            try:
                # Sélection d'une image de test
                image_b64 = test_images[i % len(test_images)]
                data = DataCNN(image_base64=image_b64)
                
                # Mesure du temps d'inférence
                iter_start = time.time()
                result = service.predict(data)
                iter_time = (time.time() - iter_start) * 1000  # ms
                
                # Enregistrement des métriques
                metric = PredictionMetrics(
                    iteration=i + 1,
                    success=result.success,
                    inference_time_ms=iter_time,
                    error=result.error,
                    result={
                        "manufacturer": result.manufacturer,
                        "model": result.model,
                        "confidence_score": result.confidence_score,
                        "top_predictions": len(result.top_predictions or [])
                    } if result.success else None
                )
                metrics.append(metric)
                
            except Exception as e:
                logger.error(f"❌ Erreur itération {i+1}: {e}")
                metrics.append(PredictionMetrics(
                    iteration=i + 1,
                    success=False,
                    inference_time_ms=0.0,
                    error=str(e)
                ))
        
        total_time = time.time() - start_time
        
        # ✅ Mesure de la RAM APRÈS le benchmark
        gc.collect()
        snapshot_end = tracemalloc.take_snapshot()
        memory_after = process.memory_info().rss / (1024 * 1024)  # MB
        
        # Calcul de la différence de RAM
        top_stats = snapshot_end.compare_to(snapshot_start, 'lineno')
        memory_diff = sum(stat.size_diff for stat in top_stats) / (1024 * 1024)
        memory_used = memory_after - memory_before
        
        tracemalloc.stop()
        
        # Calcul des statistiques
        stats = self._calculate_cnn_stats(metrics, total_time)
        
        return BenchmarkResult(
            benchmark_id=str(uuid.uuid4()),
            model_type="cnn",
            timestamp=datetime.now(),
            config={
                "num_iterations": request.num_iterations,
                "num_test_images": len(test_images)
            },
            stats=stats,
            detailed_results=metrics if request.include_detailed_results else None,
            system_info=self._get_system_info("cnn", memory_used, memory_diff)
        )
    
    def benchmark_price(
        self,
        request: 'PriceBenchmarkRequest'
    ) -> BenchmarkResult:
        """
        Benchmark du modèle de prédiction de prix
        
        Args:
            request: Configuration du benchmark
            
        Returns:
            Résultats du benchmark
        """
        logger.info(f"🏁 Démarrage benchmark Prix ({request.num_iterations} itérations)")
        
        service = self.service_factory.get_service("prediction")
        test_cases = self._get_test_price_cases(request.test_cases, request.num_iterations)
        
        metrics = []
        
        # ✅ Mesure de la RAM AVANT le benchmark
        gc.collect()
        tracemalloc.start()
        snapshot_start = tracemalloc.take_snapshot()
        process = psutil.Process()
        memory_before = process.memory_info().rss / (1024 * 1024)  # MB
        
        start_time = time.time()
        
        for i in range(request.num_iterations):
            try:
                # Sélection d'un cas de test
                case = test_cases[i % len(test_cases)]
                data = DataPrediction(**case)
                
                # Mesure du temps d'inférence
                iter_start = time.time()
                result = service.predict(data)
                iter_time = (time.time() - iter_start) * 1000  # ms
                
                # Enregistrement des métriques
                metric = PredictionMetrics(
                    iteration=i + 1,
                    success=result.success,
                    inference_time_ms=iter_time,
                    error=result.error,
                    result={
                        "predicted_price": result.predicted_price,
                        "currency": result.currency,
                        "confidence_score": result.confidence_score,
                        "num_factors": len(result.top_influencing_factors or [])
                    } if result.success else None
                )
                metrics.append(metric)
                
            except Exception as e:
                logger.error(f"❌ Erreur itération {i+1}: {e}")
                metrics.append(PredictionMetrics(
                    iteration=i + 1,
                    success=False,
                    inference_time_ms=0.0,
                    error=str(e)
                ))
        
        total_time = time.time() - start_time
        
        # ✅ Mesure de la RAM APRÈS le benchmark
        gc.collect()
        snapshot_end = tracemalloc.take_snapshot()
        memory_after = process.memory_info().rss / (1024 * 1024)  # MB
        
        # Calcul de la différence de RAM
        top_stats = snapshot_end.compare_to(snapshot_start, 'lineno')
        memory_diff = sum(stat.size_diff for stat in top_stats) / (1024 * 1024)
        memory_used = memory_after - memory_before
        
        tracemalloc.stop()
        
        # Calcul des statistiques
        stats = self._calculate_price_stats(metrics, total_time)
        
        return BenchmarkResult(
            benchmark_id=str(uuid.uuid4()),
            model_type="prediction",
            timestamp=datetime.now(),
            config={
                "num_iterations": request.num_iterations,
                "num_test_cases": len(test_cases)
            },
            stats=stats,
            detailed_results=metrics if request.include_detailed_results else None,
            system_info=self._get_system_info("prediction", memory_used, memory_diff)
        )
    
    def benchmark_adjustment(
        self,
        request: 'AdjustmentBenchmarkRequest'
    ) -> BenchmarkResult:
        """
        Benchmark du modèle d'ajustement de prix
        
        Args:
            request: Configuration du benchmark
            
        Returns:
            Résultats du benchmark
        """
        logger.info(f"🏁 Démarrage benchmark Ajustement ({request.num_iterations} itérations)")
        
        service = self.service_factory.get_service("ajustement")
        test_cases = self._get_test_adjustment_cases(request.test_cases, request.num_iterations)
        
        metrics = []
        
        # ✅ Mesure de la RAM AVANT le benchmark
        gc.collect()
        tracemalloc.start()
        snapshot_start = tracemalloc.take_snapshot()
        process = psutil.Process()
        memory_before = process.memory_info().rss / (1024 * 1024)  # MB
        
        start_time = time.time()
        
        for i in range(request.num_iterations):
            try:
                # Sélection d'un cas de test
                case = test_cases[i % len(test_cases)]
                data = DataAjustement(**case)
                
                # Mesure du temps d'inférence
                iter_start = time.time()
                result = service.predict(data)
                iter_time = (time.time() - iter_start) * 1000  # ms
                
                # Enregistrement des métriques
                metric = PredictionMetrics(
                    iteration=i + 1,
                    success=result.success,
                    inference_time_ms=iter_time,
                    error=result.error,
                    result={
                        "base_price": result.base_price,
                        "adjusted_price": result.adjusted_price,
                        "reduction_percent": result.reduction_percent,
                        "category": result.category
                    } if result.success else None
                )
                metrics.append(metric)
                
            except Exception as e:
                logger.error(f"❌ Erreur itération {i+1}: {e}")
                metrics.append(PredictionMetrics(
                    iteration=i + 1,
                    success=False,
                    inference_time_ms=0.0,
                    error=str(e)
                ))
        
        total_time = time.time() - start_time
        
        # ✅ Mesure de la RAM APRÈS le benchmark
        gc.collect()
        snapshot_end = tracemalloc.take_snapshot()
        memory_after = process.memory_info().rss / (1024 * 1024)  # MB
        
        # Calcul de la différence de RAM
        top_stats = snapshot_end.compare_to(snapshot_start, 'lineno')
        memory_diff = sum(stat.size_diff for stat in top_stats) / (1024 * 1024)
        memory_used = memory_after - memory_before
        
        tracemalloc.stop()
        
        # Calcul des statistiques
        stats = self._calculate_adjustment_stats(metrics, total_time)
        
        return BenchmarkResult(
            benchmark_id=str(uuid.uuid4()),
            model_type="ajustement",
            timestamp=datetime.now(),
            config={
                "num_iterations": request.num_iterations,
                "num_test_cases": len(test_cases)
            },
            stats=stats,
            detailed_results=metrics if request.include_detailed_results else None,
            system_info=self._get_system_info("ajustement", memory_used, memory_diff)
        )
    
    def benchmark_all(self, num_iterations: int = 10) -> AllBenchmarkResults:
        """
        Lance un benchmark complet sur toutes les IA
        
        Args:
            num_iterations: Nombre d'itérations par modèle
            
        Returns:
            Résultats de tous les benchmarks
        """
        logger.info(f"🚀 Démarrage benchmark complet ({num_iterations} itérations/modèle)")
        
        from schemas.benchmark_dto import CNNBenchmarkRequest, PriceBenchmarkRequest, AdjustmentBenchmarkRequest
        
        results = {}
        
        # Benchmark CNN
        try:
            cnn_request = CNNBenchmarkRequest(num_iterations=num_iterations)
            results['cnn_benchmark'] = self.benchmark_cnn(cnn_request)
        except Exception as e:
            logger.error(f"❌ Échec benchmark CNN: {e}")
            results['cnn_benchmark'] = None
        
        # Benchmark Prix
        try:
            price_request = PriceBenchmarkRequest(num_iterations=num_iterations)
            results['price_benchmark'] = self.benchmark_price(price_request)
        except Exception as e:
            logger.error(f"❌ Échec benchmark Prix: {e}")
            results['price_benchmark'] = None
        
        # Benchmark Ajustement
        try:
            adj_request = AdjustmentBenchmarkRequest(num_iterations=num_iterations)
            results['adjustment_benchmark'] = self.benchmark_adjustment(adj_request)
        except Exception as e:
            logger.error(f"❌ Échec benchmark Ajustement: {e}")
            results['adjustment_benchmark'] = None
        
        # Résumé global
        overall_summary = self._generate_overall_summary(results)
        
        return AllBenchmarkResults(
            **results,
            overall_summary=overall_summary,
            generated_at=datetime.now()
        )
    
    # ========================================================================
    # MÉTHODES UTILITAIRES
    # ========================================================================
    
    def _calculate_cnn_stats(
        self,
        metrics: List[PredictionMetrics],
        total_time: float
    ) -> CNNBenchmarkStats:
        """Calcule les statistiques pour le CNN"""
        successful = [m for m in metrics if m.success]
        failed = [m for m in metrics if not m.success]
        inference_times = [m.inference_time_ms for m in successful]
        
        confidences = [
            m.result.get('confidence_score', 0) 
            for m in successful 
            if m.result
        ]
        
        return CNNBenchmarkStats(
            total_iterations=len(metrics),
            successful_predictions=len(successful),
            failed_predictions=len(failed),
            success_rate_percent=(len(successful) / len(metrics)) * 100,
            avg_inference_time_ms=np.mean(inference_times) if inference_times else 0,
            min_inference_time_ms=np.min(inference_times) if inference_times else 0,
            max_inference_time_ms=np.max(inference_times) if inference_times else 0,
            std_inference_time_ms=np.std(inference_times) if inference_times else 0,
            predictions_per_second=len(successful) / total_time if total_time > 0 else 0,
            total_time_seconds=total_time,
            avg_confidence=np.mean(confidences) if confidences else None
        )
    
    def _calculate_price_stats(
        self,
        metrics: List[PredictionMetrics],
        total_time: float
    ) -> PriceBenchmarkStats:
        """Calcule les statistiques pour la prédiction de prix"""
        successful = [m for m in metrics if m.success]
        failed = [m for m in metrics if not m.success]
        inference_times = [m.inference_time_ms for m in successful]
        
        prices = [
            m.result.get('predicted_price', 0)
            for m in successful
            if m.result
        ]
        
        return PriceBenchmarkStats(
            total_iterations=len(metrics),
            successful_predictions=len(successful),
            failed_predictions=len(failed),
            success_rate_percent=(len(successful) / len(metrics)) * 100,
            avg_inference_time_ms=np.mean(inference_times) if inference_times else 0,
            min_inference_time_ms=np.min(inference_times) if inference_times else 0,
            max_inference_time_ms=np.max(inference_times) if inference_times else 0,
            std_inference_time_ms=np.std(inference_times) if inference_times else 0,
            predictions_per_second=len(successful) / total_time if total_time > 0 else 0,
            total_time_seconds=total_time,
            avg_predicted_price=np.mean(prices) if prices else None,
            min_predicted_price=np.min(prices) if prices else None,
            max_predicted_price=np.max(prices) if prices else None,
            std_predicted_price=np.std(prices) if prices else None,
            price_range=(np.max(prices) - np.min(prices)) if prices else None
        )
    
    def _calculate_adjustment_stats(
        self,
        metrics: List[PredictionMetrics],
        total_time: float
    ) -> AdjustmentBenchmarkStats:
        """Calcule les statistiques pour l'ajustement"""
        successful = [m for m in metrics if m.success]
        failed = [m for m in metrics if not m.success]
        inference_times = [m.inference_time_ms for m in successful]
        
        reductions = [
            m.result.get('reduction_percent', 0)
            for m in successful
            if m.result
        ]
        
        categories = [
            m.result.get('category', 'Unknown')
            for m in successful
            if m.result
        ]
        category_dist = dict(zip(*np.unique(categories, return_counts=True))) if categories else {}
        
        return AdjustmentBenchmarkStats(
            total_iterations=len(metrics),
            successful_predictions=len(successful),
            failed_predictions=len(failed),
            success_rate_percent=(len(successful) / len(metrics)) * 100,
            avg_inference_time_ms=np.mean(inference_times) if inference_times else 0,
            min_inference_time_ms=np.min(inference_times) if inference_times else 0,
            max_inference_time_ms=np.max(inference_times) if inference_times else 0,
            std_inference_time_ms=np.std(inference_times) if inference_times else 0,
            predictions_per_second=len(successful) / total_time if total_time > 0 else 0,
            total_time_seconds=total_time,
            avg_reduction_percent=np.mean(reductions) if reductions else None,
            min_reduction_percent=np.min(reductions) if reductions else None,
            max_reduction_percent=np.max(reductions) if reductions else None,
            category_distribution=category_dist
        )
    
    def _get_test_images(
        self,
        provided_images: Optional[List[str]],
        num_iterations: int
    ) -> List[str]:
        """Récupère ou génère des images de test"""
        if provided_images and len(provided_images) > 0:
            return provided_images
        
        # Image de test minimale (1x1 pixel blanc en base64)
        minimal_image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg=="
        return [minimal_image] * min(5, num_iterations)
    
    def _get_test_price_cases(
        self,
        provided_cases: Optional[List[Dict[str, Any]]],
        num_iterations: int
    ) -> List[Dict[str, Any]]:
        """Récupère ou génère des cas de test pour les prix"""
        if provided_cases and len(provided_cases) > 0:
            return provided_cases
        
        # Cas de test par défaut
        return [
            {
                "manufacturer": "TOYOTA",
                "model": "Camry",
                "prod_year": 2020,
                "category": "Sedan",
                "leather_interior": "Yes",
                "fuel_type": "Petrol",
                "engine_volume": 2.5,
                "mileage": "50000",
                "cylinders": 4.0,
                "gear_box_type": "Automatic",
                "drive_wheels": "Front",
                "doors": "4",
                "wheel": "Left wheel",
                "color": "Black",
                "airbags": 8
            },
            {
                "manufacturer": "BMW",
                "model": "X5",
                "prod_year": 2018,
                "category": "SUV",
                "leather_interior": "Yes",
                "fuel_type": "Diesel",
                "engine_volume": 3.0,
                "mileage": "80000",
                "cylinders": 6.0,
                "gear_box_type": "Automatic",
                "drive_wheels": "4x4",
                "doors": "4",
                "wheel": "Left wheel",
                "color": "White",
                "airbags": 10
            },
            {
                "manufacturer": "VOLKSWAGEN",
                "model": "Golf",
                "prod_year": 2019,
                "category": "Hatchback",
                "leather_interior": "No",
                "fuel_type": "Petrol",
                "engine_volume": 1.4,
                "mileage": "60000",
                "cylinders": 4.0,
                "gear_box_type": "Manual",
                "drive_wheels": "Front",
                "doors": "4",
                "wheel": "Left wheel",
                "color": "Blue",
                "airbags": 6
            }
        ]
    
    def _get_test_adjustment_cases(
        self,
        provided_cases: Optional[List[Dict[str, Any]]],
        num_iterations: int
    ) -> List[Dict[str, Any]]:
        """Récupère ou génère des cas de test pour l'ajustement"""
        if provided_cases and len(provided_cases) > 0:
            return provided_cases
        
        # Cas de test par défaut
        return [
            {
                "base_price": 25000.0,
                "description": "Véhicule en excellent état, bien entretenu, carnet complet"
            },
            {
                "base_price": 15000.0,
                "description": "Quelques rayures sur la carrosserie, moteur en bon état"
            },
            {
                "base_price": 30000.0,
                "description": "Accident léger, réparations nécessaires, moteur OK"
            },
            {
                "base_price": 10000.0,
                "description": "Pour pièces uniquement, moteur HS, boîte de vitesse cassée"
            },
            {
                "base_price": 20000.0,
                "description": "Bon état général, petit entretien à prévoir"
            }
        ]
    
    def _get_system_info(
        self, 
        model_type: str = "unknown",
        memory_used_mb: float = 0,
        memory_diff_mb: float = 0
    ) -> Dict[str, Any]:
        """
        Récupère les informations système avec mesure de RAM spécifique au modèle
        
        Args:
            model_type: Type de modèle (cnn, prediction, ajustement)
            memory_used_mb: RAM utilisée par le processus (différence RSS)
            memory_diff_mb: RAM allouée spécifiquement (tracemalloc)
        """
        try:
            process = psutil.Process()
            
            return {
                # 🖥️ Informations système
                "platform": platform.platform(),
                "processor": platform.processor(),
                "python_version": platform.python_version(),
                "cpu_count": psutil.cpu_count(),
                
                # 🌍 RAM globale du système
                "system_memory_total_gb": round(psutil.virtual_memory().total / (1024**3), 2),
                "system_memory_available_gb": round(psutil.virtual_memory().available / (1024**3), 2),
                "system_memory_percent": round(psutil.virtual_memory().percent, 2),
                
                # 🔥 RAM du processus Python (Global)
                "process_memory_rss_mb": round(process.memory_info().rss / (1024**2), 2),
                "process_memory_vms_mb": round(process.memory_info().vms / (1024**2), 2),
                "process_memory_percent": round(process.memory_percent(), 2),
                "process_cpu_percent": round(process.cpu_percent(interval=0.1), 2),
                
                # ✅ RAM spécifique à CE benchmark
                "model_type": model_type,
                "model_memory_used_mb": round(memory_used_mb, 2),
                "model_memory_allocated_mb": round(memory_diff_mb, 2),
                
                # 📊 Informations additionnelles
                "num_threads": process.num_threads(),
                "num_fds": process.num_fds() if hasattr(process, 'num_fds') else None
            }
        except Exception as e:
            logger.warning(f"Impossible de récupérer les infos système: {e}")
            return {
                "model_type": model_type,
                "error": str(e)
            }
    
    def _generate_overall_summary(self, results: Dict[str, Any]) -> Dict[str, Any]:
        """Génère un résumé global de tous les benchmarks"""
        summary = {
            "total_models_tested": sum(1 for v in results.values() if v is not None),
            "all_successful": all(v is not None for v in results.values())
        }
        
        # Moyennes globales
        avg_times = []
        total_predictions = 0
        total_memory_used = 0
        
        for key, result in results.items():
            if result and hasattr(result, 'stats'):
                avg_times.append(result.stats.avg_inference_time_ms)
                total_predictions += result.stats.total_iterations
                
                # Ajouter la mémoire utilisée
                if result.system_info and 'model_memory_used_mb' in result.system_info:
                    total_memory_used += result.system_info['model_memory_used_mb']
        
        if avg_times:
            summary["global_avg_inference_time_ms"] = np.mean(avg_times)
            summary["total_predictions_made"] = total_predictions
            summary["total_model_memory_used_mb"] = round(total_memory_used, 2)
        
        return summary