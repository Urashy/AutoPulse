# 📊 Documentation des Benchmarks

## Vue d'ensemble

Le système de benchmark vous permet de tester les performances de vos modèles IA en mesurant :
- ⏱️ **Temps d'inférence** (moyen, min, max, écart-type)
- ✅ **Taux de succès** des prédictions
- 🚀 **Débit** (prédictions par seconde)
- 📈 **Métriques spécifiques** à chaque modèle

## Endpoints disponibles

### 1. Informations sur les benchmarks
```http
GET /benchmark/info
```

Retourne la liste des benchmarks disponibles et les métriques collectées.

**Exemple de réponse :**
```json
{
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
```

---

### 2. Benchmark d'un modèle spécifique
```http
POST /benchmark
```

Lance un benchmark sur un modèle particulier.

#### Benchmark CNN (Reconnaissance visuelle)

**Requête :**
```json
{
  "type": "cnn",
  "num_iterations": 50,
  "include_detailed_results": false,
  "test_images": [
    "base64_encoded_image_1",
    "base64_encoded_image_2"
  ]
}
```

**Réponse :**
```json
{
  "benchmark_id": "550e8400-e29b-41d4-a716-446655440000",
  "model_type": "cnn",
  "timestamp": "2025-12-24T10:30:00",
  "config": {
    "num_iterations": 50,
    "num_test_images": 2
  },
  "stats": {
    "total_iterations": 50,
    "successful_predictions": 48,
    "failed_predictions": 2,
    "success_rate_percent": 96.0,
    "avg_inference_time_ms": 245.3,
    "min_inference_time_ms": 198.5,
    "max_inference_time_ms": 312.7,
    "std_inference_time_ms": 28.4,
    "predictions_per_second": 4.08,
    "total_time_seconds": 12.25,
    "avg_confidence": 0.87,
    "top1_accuracy": null,
    "top5_accuracy": null,
    "detected_classes": null
  },
  "detailed_results": null,
  "system_info": {
    "platform": "Linux-5.15.0-1048-gcp-x86_64",
    "processor": "x86_64",
    "python_version": "3.10.12",
    "cpu_count": 4,
    "memory_total_gb": 16.0,
    "memory_available_gb": 8.5
  }
}
```

#### Benchmark Prédiction de Prix

**Requête :**
```json
{
  "type": "prediction",
  "num_iterations": 100,
  "include_detailed_results": false,
  "test_cases": [
    {
      "manufacturer": "TOYOTA",
      "model": "Camry",
      "prod_year": 2020,
      "category": "Sedan",
      "mileage": "50000",
      "fuel_type": "Petrol",
      "engine_volume": 2.5,
      "cylinders": 4.0
    }
  ]
}
```

**Réponse :**
```json
{
  "benchmark_id": "660e8400-e29b-41d4-a716-446655440001",
  "model_type": "prediction",
  "timestamp": "2025-12-24T10:35:00",
  "config": {
    "num_iterations": 100,
    "num_test_cases": 1
  },
  "stats": {
    "total_iterations": 100,
    "successful_predictions": 100,
    "failed_predictions": 0,
    "success_rate_percent": 100.0,
    "avg_inference_time_ms": 15.2,
    "min_inference_time_ms": 12.1,
    "max_inference_time_ms": 22.8,
    "std_inference_time_ms": 2.3,
    "predictions_per_second": 65.79,
    "total_time_seconds": 1.52,
    "avg_predicted_price": 24567.89,
    "min_predicted_price": 24500.12,
    "max_predicted_price": 24650.45,
    "std_predicted_price": 35.67,
    "price_range": 150.33
  }
}
```

#### Benchmark Ajustement de Prix

**Requête :**
```json
{
  "type": "ajustement",
  "num_iterations": 100,
  "include_detailed_results": true,
  "test_cases": [
    {
      "base_price": 25000.0,
      "description": "Véhicule en excellent état"
    },
    {
      "base_price": 10000.0,
      "description": "Pour pièces, moteur HS"
    }
  ]
}
```

**Réponse :**
```json
{
  "benchmark_id": "770e8400-e29b-41d4-a716-446655440002",
  "model_type": "ajustement",
  "timestamp": "2025-12-24T10:40:00",
  "config": {
    "num_iterations": 100,
    "num_test_cases": 2
  },
  "stats": {
    "total_iterations": 100,
    "successful_predictions": 100,
    "failed_predictions": 0,
    "success_rate_percent": 100.0,
    "avg_inference_time_ms": 8.5,
    "min_inference_time_ms": 6.2,
    "max_inference_time_ms": 14.3,
    "std_inference_time_ms": 1.8,
    "predictions_per_second": 117.65,
    "total_time_seconds": 0.85,
    "avg_reduction_percent": 15.2,
    "min_reduction_percent": 0.5,
    "max_reduction_percent": 45.3,
    "category_distribution": {
      "✅ EXCELLENT - État impeccable": 50,
      "🔴 CRITIQUE - Véhicule non roulant/épave": 50
    }
  },
  "detailed_results": [
    {
      "iteration": 1,
      "success": true,
      "inference_time_ms": 8.2,
      "error": null,
      "result": {
        "base_price": 25000.0,
        "adjusted_price": 24875.0,
        "reduction_percent": 0.5,
        "category": "✅ EXCELLENT - État impeccable"
      }
    }
  ]
}
```

---

### 3. Benchmark complet (tous les modèles)
```http
POST /benchmark/all?num_iterations=50
```

Lance un benchmark sur tous les modèles en une seule requête.

**Paramètres de requête :**
- `num_iterations` (optionnel) : Nombre d'itérations par modèle (défaut: 10)

**Réponse :**
```json
{
  "cnn_benchmark": {
    "benchmark_id": "880e8400-e29b-41d4-a716-446655440003",
    "model_type": "cnn",
    "stats": { /* ... */ }
  },
  "price_benchmark": {
    "benchmark_id": "990e8400-e29b-41d4-a716-446655440004",
    "model_type": "prediction",
    "stats": { /* ... */ }
  },
  "adjustment_benchmark": {
    "benchmark_id": "aa0e8400-e29b-41d4-a716-446655440005",
    "model_type": "ajustement",
    "stats": { /* ... */ }
  },
  "overall_summary": {
    "total_models_tested": 3,
    "all_successful": true,
    "global_avg_inference_time_ms": 89.67,
    "total_predictions_made": 150
  },
  "generated_at": "2025-12-24T10:45:00"
}
```

---

## 📝 Exemples Python

### Exemple 1 : Benchmark simple du modèle CNN
```python
import requests

# Benchmark du CNN avec 20 itérations
response = requests.post(
    "http://localhost:8000/benchmark",
    json={
        "type": "cnn",
        "num_iterations": 20,
        "include_detailed_results": False
    }
)

results = response.json()
print(f"Taux de succès: {results['stats']['success_rate_percent']}%")
print(f"Temps moyen: {results['stats']['avg_inference_time_ms']:.2f}ms")
print(f"Débit: {results['stats']['predictions_per_second']:.2f} pred/s")
```

### Exemple 2 : Benchmark de prédiction avec cas personnalisés
```python
import requests

test_cases = [
    {
        "manufacturer": "TOYOTA",
        "model": "Camry",
        "prod_year": 2020,
        "mileage": "50000",
        "fuel_type": "Petrol"
    },
    {
        "manufacturer": "BMW",
        "model": "X5",
        "prod_year": 2018,
        "mileage": "80000",
        "fuel_type": "Diesel"
    }
]

response = requests.post(
    "http://localhost:8000/benchmark",
    json={
        "type": "prediction",
        "num_iterations": 50,
        "test_cases": test_cases,
        "include_detailed_results": True
    }
)

results = response.json()
print(f"Prix moyen prédit: {results['stats']['avg_predicted_price']:.2f}€")
print(f"Écart-type: {results['stats']['std_predicted_price']:.2f}€")
```

### Exemple 3 : Benchmark complet de tous les modèles
```python
import requests

# Lance un benchmark sur tous les modèles
response = requests.post(
    "http://localhost:8000/benchmark/all?num_iterations=30"
)

results = response.json()

print("=" * 60)
print("RÉSULTATS BENCHMARK COMPLET")
print("=" * 60)

for model_name in ['cnn_benchmark', 'price_benchmark', 'adjustment_benchmark']:
    if results.get(model_name):
        model_data = results[model_name]
        stats = model_data['stats']
        print(f"\n{model_data['model_type'].upper()}:")
        print(f"  ✓ Succès: {stats['success_rate_percent']:.1f}%")
        print(f"  ⏱ Temps moyen: {stats['avg_inference_time_ms']:.2f}ms")
        print(f"  🚀 Débit: {stats['predictions_per_second']:.2f} pred/s")

print("\n" + "=" * 60)
summary = results['overall_summary']
print(f"Temps moyen global: {summary['global_avg_inference_time_ms']:.2f}ms")
print(f"Total prédictions: {summary['total_predictions_made']}")
```

---

## 🎯 Cas d'usage

### 1. Test de régression après mise à jour d'un modèle
```python
# Avant la mise à jour
response_before = requests.post(
    "http://localhost:8000/benchmark",
    json={"type": "prediction", "num_iterations": 100}
)
time_before = response_before.json()['stats']['avg_inference_time_ms']

# [Mise à jour du modèle]

# Après la mise à jour
response_after = requests.post(
    "http://localhost:8000/benchmark",
    json={"type": "prediction", "num_iterations": 100}
)
time_after = response_after.json()['stats']['avg_inference_time_ms']

improvement = ((time_before - time_after) / time_before) * 100
print(f"Amélioration: {improvement:.1f}%")
```

### 2. Monitoring de production
```python
import schedule
import time

def monitor_performance():
    response = requests.post(
        "http://localhost:8000/benchmark/all?num_iterations=10"
    )
    results = response.json()
    
    # Vérifier les seuils
    for benchmark in [results['cnn_benchmark'], results['price_benchmark']]:
        if benchmark['stats']['avg_inference_time_ms'] > 500:
            send_alert(f"Ralentissement détecté sur {benchmark['model_type']}")

# Exécuter toutes les heures
schedule.every(1).hours.do(monitor_performance)

while True:
    schedule.run_pending()
    time.sleep(60)
```

### 3. Comparaison de configurations
```python
configs = [
    {"num_iterations": 10},
    {"num_iterations": 50},
    {"num_iterations": 100}
]

for config in configs:
    response = requests.post(
        "http://localhost:8000/benchmark",
        json={"type": "prediction", **config}
    )
    stats = response.json()['stats']
    print(f"{config['num_iterations']} itérations:")
    print(f"  Temps moyen: {stats['avg_inference_time_ms']:.2f}ms")
    print(f"  Écart-type: {stats['std_inference_time_ms']:.2f}ms")
```

---

## 📊 Métriques expliquées

### Métriques communes à tous les modèles

- **total_iterations** : Nombre total d'itérations exécutées
- **successful_predictions** : Nombre de prédictions réussies
- **failed_predictions** : Nombre de prédictions échouées
- **success_rate_percent** : Taux de succès en pourcentage
- **avg_inference_time_ms** : Temps d'inférence moyen en millisecondes
- **min_inference_time_ms** : Temps d'inférence minimum
- **max_inference_time_ms** : Temps d'inférence maximum
- **std_inference_time_ms** : Écart-type du temps d'inférence
- **predictions_per_second** : Débit en prédictions par seconde
- **total_time_seconds** : Temps total du benchmark

### Métriques spécifiques CNN

- **avg_confidence** : Confiance moyenne des prédictions
- **top1_accuracy** : Précision top-1 (si étiquettes fournies)
- **top5_accuracy** : Précision top-5 (si étiquettes fournies)
- **detected_classes** : Liste des classes détectées

### Métriques spécifiques Prédiction de Prix

- **avg_predicted_price** : Prix moyen prédit
- **min_predicted_price** : Prix minimum prédit
- **max_predicted_price** : Prix maximum prédit
- **std_predicted_price** : Écart-type des prix
- **price_range** : Plage de prix (max - min)

### Métriques spécifiques Ajustement

- **avg_reduction_percent** : Réduction moyenne en pourcentage
- **min_reduction_percent** : Réduction minimum
- **max_reduction_percent** : Réduction maximum
- **category_distribution** : Distribution des catégories (EXCELLENT, BON, MODÉRÉ, etc.)

---

## 🚀 Recommandations

1. **Itérations recommandées** :
   - Tests rapides : 10-20 itérations
   - Tests standard : 50-100 itérations
   - Tests approfondis : 500-1000 itérations

2. **Cas de test personnalisés** :
   - Utilisez vos propres données pour des benchmarks plus réalistes
   - Incluez des cas limites pour tester la robustesse

3. **Résultats détaillés** :
   - N'activez `include_detailed_results` que si nécessaire (volume de données)
   - Utilisez-le pour l'analyse fine des performances

4. **Interprétation** :
   - Un faible écart-type indique des performances stables
   - Un débit élevé est crucial pour les applications en production
   - Surveillez l'augmentation du temps max qui peut indiquer des problèmes

---

## 🔧 Dépannage

### Le benchmark est lent
- Réduisez `num_iterations`
- Vérifiez les ressources système disponibles
- Assurez-vous que les modèles sont bien chargés

### Erreurs fréquentes
- **Modèle non disponible** : Vérifiez `/health` avant de lancer le benchmark
- **Timeout** : Augmentez le timeout de votre client HTTP pour les benchmarks longs
- **Mémoire insuffisante** : Réduisez le nombre d'itérations ou désactivez les résultats détaillés

---

## 📌 Notes importantes

- Les benchmarks utilisent les mêmes modèles que les endpoints de production
- Les résultats peuvent varier selon la charge système
- Pour des résultats reproductibles, exécutez sur un système dédié
- Les temps d'inférence incluent uniquement le temps de prédiction, pas la préparation des données
