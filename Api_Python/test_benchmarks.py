#!/usr/bin/env python3
"""
Script de test des benchmarks
Usage: python test_benchmarks.py [options]
"""
import requests
import argparse
import json
import time
from typing import Optional


class BenchmarkTester:
    """Classe pour tester les benchmarks de l'API"""
    
    def __init__(self, base_url: str = "http://localhost:8000"):
        self.base_url = base_url
    
    def test_info(self):
        """Test de l'endpoint d'information"""
        print("\n" + "=" * 70)
        print("📋 TEST: Information sur les benchmarks")
        print("=" * 70)
        
        response = requests.get(f"{self.base_url}/benchmark/info")
        
        if response.status_code == 200:
            data = response.json()
            print("✅ Succès!")
            print(f"\nBenchmarks disponibles: {', '.join(data['available_benchmarks'])}")
            print("\nDescriptions:")
            for key, desc in data['description'].items():
                print(f"  • {key}: {desc}")
        else:
            print(f"❌ Erreur: {response.status_code}")
            print(response.text)
    
    def test_cnn_benchmark(self, iterations: int = 10, detailed: bool = False):
        """Test du benchmark CNN"""
        print("\n" + "=" * 70)
        print(f"🖼️ TEST: Benchmark CNN ({iterations} itérations)")
        print("=" * 70)
        
        payload = {
            "type": "cnn",
            "num_iterations": iterations,
            "include_detailed_results": detailed
        }
        
        start = time.time()
        response = requests.post(f"{self.base_url}/benchmark", json=payload)
        duration = time.time() - start
        
        if response.status_code == 200:
            data = response.json()
            stats = data['stats']
            
            print("✅ Succès!")
            print(f"\n⏱️ Durée du test: {duration:.2f}s")
            print(f"\n📊 Résultats:")
            print(f"  • Itérations: {stats['total_iterations']}")
            print(f"  • Succès: {stats['successful_predictions']} ({stats['success_rate_percent']:.1f}%)")
            print(f"  • Échecs: {stats['failed_predictions']}")
            print(f"  • Temps moyen: {stats['avg_inference_time_ms']:.2f}ms")
            print(f"  • Temps min/max: {stats['min_inference_time_ms']:.2f}ms / {stats['max_inference_time_ms']:.2f}ms")
            print(f"  • Écart-type: {stats['std_inference_time_ms']:.2f}ms")
            print(f"  • Débit: {stats['predictions_per_second']:.2f} pred/s")
            
            if stats.get('avg_confidence'):
                print(f"  • Confiance moyenne: {stats['avg_confidence']:.2%}")
            
            if data.get('system_info'):
                print(f"\n💻 Système:")
                si = data['system_info']
                print(f"  • Plateforme: {si.get('platform', 'N/A')}")
                print(f"  • CPUs: {si.get('cpu_count', 'N/A')}")
                print(f"  • RAM disponible: {si.get('memory_available_gb', 'N/A')} GB")
        else:
            print(f"❌ Erreur: {response.status_code}")
            print(response.text)
    
    def test_price_benchmark(self, iterations: int = 10, detailed: bool = False):
        """Test du benchmark de prédiction de prix"""
        print("\n" + "=" * 70)
        print(f"💰 TEST: Benchmark Prédiction de Prix ({iterations} itérations)")
        print("=" * 70)
        
        payload = {
            "type": "prediction",
            "num_iterations": iterations,
            "include_detailed_results": detailed
        }
        
        start = time.time()
        response = requests.post(f"{self.base_url}/benchmark", json=payload)
        duration = time.time() - start
        
        if response.status_code == 200:
            data = response.json()
            stats = data['stats']
            
            print("✅ Succès!")
            print(f"\n⏱️ Durée du test: {duration:.2f}s")
            print(f"\n📊 Résultats:")
            print(f"  • Itérations: {stats['total_iterations']}")
            print(f"  • Succès: {stats['successful_predictions']} ({stats['success_rate_percent']:.1f}%)")
            print(f"  • Temps moyen: {stats['avg_inference_time_ms']:.2f}ms")
            print(f"  • Débit: {stats['predictions_per_second']:.2f} pred/s")
            
            if stats.get('avg_predicted_price'):
                print(f"\n💵 Prix:")
                print(f"  • Prix moyen: {stats['avg_predicted_price']:.2f}€")
                print(f"  • Prix min/max: {stats['min_predicted_price']:.2f}€ / {stats['max_predicted_price']:.2f}€")
                print(f"  • Écart-type: {stats['std_predicted_price']:.2f}€")
                print(f"  • Plage: {stats['price_range']:.2f}€")
        else:
            print(f"❌ Erreur: {response.status_code}")
            print(response.text)
    
    def test_adjustment_benchmark(self, iterations: int = 10, detailed: bool = False):
        """Test du benchmark d'ajustement"""
        print("\n" + "=" * 70)
        print(f"📝 TEST: Benchmark Ajustement ({iterations} itérations)")
        print("=" * 70)
        
        payload = {
            "type": "ajustement",
            "num_iterations": iterations,
            "include_detailed_results": detailed
        }
        
        start = time.time()
        response = requests.post(f"{self.base_url}/benchmark", json=payload)
        duration = time.time() - start
        
        if response.status_code == 200:
            data = response.json()
            stats = data['stats']
            
            print("✅ Succès!")
            print(f"\n⏱️ Durée du test: {duration:.2f}s")
            print(f"\n📊 Résultats:")
            print(f"  • Itérations: {stats['total_iterations']}")
            print(f"  • Succès: {stats['successful_predictions']} ({stats['success_rate_percent']:.1f}%)")
            print(f"  • Temps moyen: {stats['avg_inference_time_ms']:.2f}ms")
            print(f"  • Débit: {stats['predictions_per_second']:.2f} pred/s")
            
            if stats.get('avg_reduction_percent') is not None:
                print(f"\n📉 Ajustements:")
                print(f"  • Réduction moyenne: {stats['avg_reduction_percent']:.2f}%")
                print(f"  • Réduction min/max: {stats['min_reduction_percent']:.2f}% / {stats['max_reduction_percent']:.2f}%")
            
            if stats.get('category_distribution'):
                print(f"\n📊 Distribution des catégories:")
                for category, count in stats['category_distribution'].items():
                    print(f"  • {category}: {count}")
        else:
            print(f"❌ Erreur: {response.status_code}")
            print(response.text)
    
    def test_all_benchmarks(self, iterations: int = 10):
        """Test de benchmark complet"""
        print("\n" + "=" * 70)
        print(f"🚀 TEST: Benchmark COMPLET ({iterations} itérations/modèle)")
        print("=" * 70)
        
        start = time.time()
        response = requests.post(f"{self.base_url}/benchmark/all?num_iterations={iterations}")
        duration = time.time() - start
        
        if response.status_code == 200:
            data = response.json()
            
            print("✅ Succès!")
            print(f"\n⏱️ Durée totale: {duration:.2f}s")
            
            # CNN
            if data.get('cnn_benchmark'):
                print("\n" + "-" * 70)
                print("🖼️ CNN (Reconnaissance visuelle)")
                print("-" * 70)
                stats = data['cnn_benchmark']['stats']
                print(f"  • Taux de succès: {stats['success_rate_percent']:.1f}%")
                print(f"  • Temps moyen: {stats['avg_inference_time_ms']:.2f}ms")
                print(f"  • Débit: {stats['predictions_per_second']:.2f} pred/s")
            
            # Prix
            if data.get('price_benchmark'):
                print("\n" + "-" * 70)
                print("💰 Prédiction de Prix")
                print("-" * 70)
                stats = data['price_benchmark']['stats']
                print(f"  • Taux de succès: {stats['success_rate_percent']:.1f}%")
                print(f"  • Temps moyen: {stats['avg_inference_time_ms']:.2f}ms")
                print(f"  • Débit: {stats['predictions_per_second']:.2f} pred/s")
                if stats.get('avg_predicted_price'):
                    print(f"  • Prix moyen: {stats['avg_predicted_price']:.2f}€")
            
            # Ajustement
            if data.get('adjustment_benchmark'):
                print("\n" + "-" * 70)
                print("📝 Ajustement de Prix")
                print("-" * 70)
                stats = data['adjustment_benchmark']['stats']
                print(f"  • Taux de succès: {stats['success_rate_percent']:.1f}%")
                print(f"  • Temps moyen: {stats['avg_inference_time_ms']:.2f}ms")
                print(f"  • Débit: {stats['predictions_per_second']:.2f} pred/s")
                if stats.get('avg_reduction_percent') is not None:
                    print(f"  • Réduction moyenne: {stats['avg_reduction_percent']:.2f}%")
            
            # Résumé global
            print("\n" + "=" * 70)
            print("📊 RÉSUMÉ GLOBAL")
            print("=" * 70)
            summary = data['overall_summary']
            print(f"  • Modèles testés: {summary['total_models_tested']}/3")
            print(f"  • Tous réussis: {'✅ Oui' if summary['all_successful'] else '❌ Non'}")
            print(f"  • Temps moyen global: {summary['global_avg_inference_time_ms']:.2f}ms")
            print(f"  • Total prédictions: {summary['total_predictions_made']}")
        else:
            print(f"❌ Erreur: {response.status_code}")
            print(response.text)
    
    def run_all_tests(self, iterations: int = 10, detailed: bool = False):
        """Exécute tous les tests"""
        print("\n" + "=" * 70)
        print("🧪 SUITE DE TESTS COMPLÈTE")
        print("=" * 70)
        
        overall_start = time.time()
        
        try:
            self.test_info()
            self.test_cnn_benchmark(iterations, detailed)
            self.test_price_benchmark(iterations, detailed)
            self.test_adjustment_benchmark(iterations, detailed)
            self.test_all_benchmarks(iterations)
        except Exception as e:
            print(f"\n❌ Erreur lors des tests: {e}")
        
        overall_duration = time.time() - overall_start
        
        print("\n" + "=" * 70)
        print(f"✅ TESTS TERMINÉS en {overall_duration:.2f}s")
        print("=" * 70 + "\n")


def main():
    """Point d'entrée principal"""
    parser = argparse.ArgumentParser(description="Test des benchmarks de l'API")
    parser.add_argument(
        "--url",
        default="http://localhost:8000",
        help="URL de base de l'API (défaut: http://localhost:8000)"
    )
    parser.add_argument(
        "--test",
        choices=["info", "cnn", "price", "adjustment", "all", "complete"],
        default="complete",
        help="Type de test à exécuter (défaut: complete)"
    )
    parser.add_argument(
        "--iterations",
        type=int,
        default=10,
        help="Nombre d'itérations pour les benchmarks (défaut: 10)"
    )
    parser.add_argument(
        "--detailed",
        action="store_true",
        help="Inclure les résultats détaillés"
    )
    
    args = parser.parse_args()
    
    tester = BenchmarkTester(args.url)
    
    if args.test == "info":
        tester.test_info()
    elif args.test == "cnn":
        tester.test_cnn_benchmark(args.iterations, args.detailed)
    elif args.test == "price":
        tester.test_price_benchmark(args.iterations, args.detailed)
    elif args.test == "adjustment":
        tester.test_adjustment_benchmark(args.iterations, args.detailed)
    elif args.test == "all":
        tester.test_all_benchmarks(args.iterations)
    else:  # complete
        tester.run_all_tests(args.iterations, args.detailed)


if __name__ == "__main__":
    main()
