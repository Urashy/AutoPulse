# 🚀 Système de Benchmark des IA

Ce système fournit des endpoints de benchmark complets pour tester les performances de vos 3 modèles IA :
- 🖼️ **CNN** : Reconnaissance visuelle de véhicules
- 💰 **Prediction** : Prédiction de prix
- 📝 **Ajustement** : Ajustement de prix selon description

## 📋 Table des matières

1. [Installation rapide](#installation-rapide)
2. [Utilisation](#utilisation)
3. [Endpoints disponibles](#endpoints-disponibles)
4. [Exemples](#exemples)
5. [Métriques](#métriques)
6. [Documentation complète](#documentation-complète)

---

## 🚀 Installation rapide

### Prérequis

Le système de benchmark nécessite quelques packages supplémentaires :

```bash
pip install psutil --break-system-packages
```

### Vérification

1. Démarrez votre serveur FastAPI :
```bash
python main.py
```

2. Vérifiez que l'API répond :
```bash
curl http://localhost:8000/
```

3. Accédez à la documentation interactive :
```
http://localhost:8000/docs
```

---

## 📖 Utilisation

### Option 1 : Documentation interactive (Swagger UI)

Accédez à `http://localhost:8000/docs` et utilisez l'interface graphique pour :
- 📋 Voir tous les endpoints de benchmark
- 🧪 Tester directement depuis le navigateur
- 📊 Voir les schémas de requêtes/réponses

### Option 2 : Script Python de test

Un script de test pratique est fourni :

```bash
# Test complet (tous les benchmarks)
python test_benchmarks.py

# Test d'un modèle spécifique
python test_benchmarks.py --test cnn --iterations 50

# Test avec résultats détaillés
python test_benchmarks.py --test price --iterations 100 --detailed

# Options disponibles
python test_benchmarks.py --help
```

### Option 3 : Requêtes HTTP directes

```bash
# Info sur les benchmarks
curl http://localhost:8000/benchmark/info

# Benchmark CNN simple
curl -X POST http://localhost:8000/benchmark \
  -H "Content-Type: application/json" \
  -d '{"type": "cnn", "num_iterations": 10}'

# Benchmark complet
curl -X POST "http://localhost:8000/benchmark/all?num_iterations=20"
```

---

## 🎯 Endpoints disponibles

### 1. `/benchmark/info` (GET)
Informations sur les benchmarks disponibles.

### 2. `/benchmark` (POST)
Benchmark d'un modèle spécifique.

**Paramètres :**
- `type` : "cnn" | "prediction" | "ajustement"
- `num_iterations` : Nombre d'itérations (1-1000)
- `include_detailed_results` : true/false
- `test_images` ou `test_cases` : Données de test personnalisées (optionnel)

### 3. `/benchmark/all` (POST)
Benchmark de tous les modèles en une seule requête.

**Paramètres :**
- `num_iterations` : Nombre d'itérations par modèle

---

## 💡 Exemples

### Exemple Python simple

```python
import requests

# Benchmark du CNN
response = requests.post(
    "http://localhost:8000/benchmark",
    json={
        "type": "cnn",
        "num_iterations": 20
    }
)

results = response.json()
print(f"Temps moyen: {results['stats']['avg_inference_time_ms']:.2f}ms")
print(f"Débit: {results['stats']['predictions_per_second']:.2f} pred/s")
```

### Exemple avec cas de test personnalisés

```python
# Benchmark de prédiction avec vos propres véhicules
test_vehicles = [
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
        "test_cases": test_vehicles
    }
)
```

### Exemple de monitoring continu

```python
import time
import schedule

def check_performance():
    """Vérifie les performances toutes les heures"""
    response = requests.post(
        "http://localhost:8000/benchmark/all?num_iterations=10"
    )
    results = response.json()
    
    # Alerter si performances dégradées
    for benchmark_name in ['cnn_benchmark', 'price_benchmark']:
        benchmark = results.get(benchmark_name)
        if benchmark:
            avg_time = benchmark['stats']['avg_inference_time_ms']
            if avg_time > 500:  # Seuil de 500ms
                print(f"⚠️ ALERTE: {benchmark_name} lent ({avg_time:.0f}ms)")

# Planifier toutes les heures
schedule.every(1).hours.do(check_performance)

while True:
    schedule.run_pending()
    time.sleep(60)
```

---

## 📊 Métriques collectées

### Métriques de base (tous les modèles)

| Métrique | Description | Utilité |
|----------|-------------|---------|
| `avg_inference_time_ms` | Temps moyen d'inférence | Performance globale |
| `min_inference_time_ms` | Temps minimum | Meilleur cas |
| `max_inference_time_ms` | Temps maximum | Pire cas |
| `std_inference_time_ms` | Écart-type | Stabilité |
| `success_rate_percent` | Taux de succès | Fiabilité |
| `predictions_per_second` | Débit | Capacité |

### Métriques spécifiques

**CNN :**
- `avg_confidence` : Confiance moyenne des prédictions
- `detected_classes` : Classes détectées

**Prédiction de prix :**
- `avg_predicted_price` : Prix moyen prédit
- `price_range` : Plage de prix (max - min)
- `std_predicted_price` : Variabilité des prix

**Ajustement :**
- `avg_reduction_percent` : Réduction moyenne
- `category_distribution` : Distribution des catégories

---

## 📈 Interprétation des résultats

### Temps d'inférence

- **< 50ms** : ⚡ Excellent (temps réel)
- **50-200ms** : ✅ Bon (interactif)
- **200-500ms** : ⚠️ Acceptable (batch)
- **> 500ms** : 🔴 À optimiser

### Stabilité (écart-type)

- **Écart-type faible** (<10% de la moyenne) : Performance stable ✅
- **Écart-type élevé** (>30% de la moyenne) : Performance instable ⚠️

### Débit

Le débit indique combien de prédictions peuvent être traitées par seconde :
- Utile pour dimensionner vos serveurs
- Important pour les applications à fort trafic

---

## 🎯 Cas d'usage pratiques

### 1. Test de régression après mise à jour

```python
# Avant mise à jour
baseline = benchmark_model("prediction", 100)

# Après mise à jour
new_perf = benchmark_model("prediction", 100)

# Comparer
improvement = (baseline['avg_time'] - new_perf['avg_time']) / baseline['avg_time']
print(f"Amélioration: {improvement * 100:.1f}%")
```

### 2. Validation avant déploiement

```python
# Benchmarks de validation
results = requests.post("http://localhost:8000/benchmark/all?num_iterations=100")
data = results.json()

# Vérifier les seuils
assert data['cnn_benchmark']['stats']['success_rate_percent'] > 95
assert data['price_benchmark']['stats']['avg_inference_time_ms'] < 100
```

### 3. Monitoring de production

```python
# À intégrer dans votre système de monitoring
def alert_if_slow():
    results = benchmark_all(10)
    for model, benchmark in results.items():
        if benchmark['avg_time'] > THRESHOLD:
            send_alert(f"{model} is slow: {benchmark['avg_time']:.0f}ms")
```

---

## 📚 Documentation complète

Pour une documentation détaillée avec tous les exemples et cas d'usage, consultez :

📖 **[BENCHMARK.md](./BENCHMARK.md)** - Documentation complète

---

## 🔧 Configuration

### Nombre d'itérations recommandé

| Usage | Itérations | Durée estimée |
|-------|-----------|---------------|
| Test rapide | 10-20 | < 1 min |
| Test standard | 50-100 | 1-5 min |
| Test approfondi | 500-1000 | 5-30 min |

### Cas de test personnalisés

Pour des benchmarks plus réalistes, fournissez vos propres cas de test :

```python
# Exemple pour le CNN avec vos images
my_images = [
    "base64_encoded_image_1",
    "base64_encoded_image_2",
    # ... plus d'images
]

benchmark_request = {
    "type": "cnn",
    "num_iterations": 50,
    "test_images": my_images
}
```

---

## 🐛 Dépannage

### Le benchmark est lent
- ✓ Réduisez le nombre d'itérations
- ✓ Vérifiez la charge CPU/RAM
- ✓ Assurez-vous que les modèles sont chargés (`/health`)

### Erreurs 503 ou timeout
- ✓ Augmentez le timeout de votre client HTTP
- ✓ Vérifiez que l'API est bien démarrée
- ✓ Consultez les logs du serveur

### Résultats variables
- ✓ Normal si le système est sous charge
- ✓ Exécutez sur un environnement dédié pour des résultats stables
- ✓ Augmentez le nombre d'itérations pour moyenner

---

## 📊 Exemples de résultats

### Résultat typique - CNN
```
✅ Succès!
⏱️ Durée du test: 5.23s

📊 Résultats:
  • Itérations: 20
  • Succès: 20 (100.0%)
  • Temps moyen: 245.30ms
  • Temps min/max: 198.50ms / 312.70ms
  • Écart-type: 28.40ms
  • Débit: 3.82 pred/s
  • Confiance moyenne: 87.00%
```

### Résultat typique - Prédiction Prix
```
✅ Succès!
⏱️ Durée du test: 0.52s

📊 Résultats:
  • Itérations: 50
  • Succès: 50 (100.0%)
  • Temps moyen: 8.50ms
  • Débit: 96.15 pred/s

💵 Prix:
  • Prix moyen: 24567.89€
  • Prix min/max: 24500.12€ / 24650.45€
  • Écart-type: 35.67€
```

---

## 🤝 Support

Pour toute question ou problème :
1. Consultez la [documentation complète](./BENCHMARK.md)
2. Vérifiez les logs du serveur
3. Testez avec le script de test fourni

---

## 📝 Licence

Ce système de benchmark fait partie du projet FastAPI IA.

---

## 🎉 Prochaines étapes

Maintenant que vous avez configuré les benchmarks :

1. ✅ Testez chaque modèle individuellement
2. ✅ Lancez un benchmark complet
3. ✅ Intégrez dans votre CI/CD
4. ✅ Mettez en place du monitoring

Bon benchmarking ! 🚀
