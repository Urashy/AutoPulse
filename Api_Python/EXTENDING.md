# Guide d'Extension - Ajouter de nouveaux modèles

## 🎯 Comment ajouter un nouveau type de prédiction

Ce guide explique comment étendre l'API avec un nouveau modèle IA.

### Exemple : Ajouter un système de recommandation de véhicules

---

## 📝 Étape 1 : Définir les schémas (DTOs)

**Fichier:** `schemas/dto.py`

```python
# Ajouter après les classes existantes

class DataRecommendation(DataAI):
    """Données pour la recommandation de véhicules"""
    type: Literal["recommendation"] = "recommendation"
    budget: float = Field(..., gt=0)
    preferred_manufacturer: Optional[str] = None
    preferred_category: Optional[str] = None
    max_mileage: Optional[int] = None
    min_year: Optional[int] = None
    fuel_preference: Optional[str] = None

class VehicleRecommendation(BaseModel):
    """Une recommandation de véhicule"""
    manufacturer: str
    model: str
    year: int
    predicted_price: float
    match_score: float
    reasons: List[str]

class ResultatRecommendation(ResultatAI):
    """Résultat de recommandation"""
    type: Literal["recommendation"] = "recommendation"
    recommendations: Optional[List[VehicleRecommendation]] = None
    total_matches: Optional[int] = None

# Mettre à jour PredictRequest
class PredictRequest(BaseModel):
    type: str
    data: Union[
        DataCNN,
        DataPrediction,
        DataAjustement,
        DataRecommendation  # ← AJOUT
    ]
```

---

## 🗄️ Étape 2 : Créer le Repository

**Fichier:** `repositories/recommendation_repository.py`

```python
"""
Repository pour le modèle de recommandation
"""
import joblib
import logging
from typing import Optional
from .base_repository import ModelRepositoryBase

logger = logging.getLogger(__name__)


class RecommendationModelRepository(ModelRepositoryBase):
    """Repository pour le système de recommandation"""
    
    def load(self, path: str) -> bool:
        """Charge le modèle de recommandation"""
        try:
            if not self._check_file_exists(path):
                return False
            
            logger.info(f"🎯 Chargement du modèle de recommandation: {path}")
            self._model = joblib.load(path)
            self._path = path
            self._status = "loaded"
            logger.info("✅ Modèle de recommandation chargé")
            return True
            
        except Exception as e:
            logger.error(f"❌ Erreur: {e}")
            self._model = None
            self._status = f"error: {str(e)}"
            return False
```

---

## ⚙️ Étape 3 : Créer le Service

**Fichier:** `services/recommendation_service.py`

```python
"""
Service de recommandation de véhicules
"""
import logging
from typing import List
from .base_service import IModelService
from schemas.dto import (
    DataRecommendation,
    ResultatRecommendation,
    VehicleRecommendation
)
from repositories.repository_manager import ModelRepositoryManager

logger = logging.getLogger(__name__)


class RecommendationService(IModelService):
    """Service pour recommander des véhicules"""
    
    def __init__(self, repo_manager: ModelRepositoryManager):
        self.repo_manager = repo_manager
    
    def is_available(self) -> bool:
        """Vérifie si le modèle est disponible"""
        return self.repo_manager.recommendation_repo.get_model() is not None
    
    def predict(self, data: DataRecommendation) -> ResultatRecommendation:
        """
        Recommande des véhicules selon les critères
        
        Args:
            data: Critères de recherche
            
        Returns:
            Liste de recommandations
        """
        model = self.repo_manager.recommendation_repo.get_model()
        
        if model is None:
            return ResultatRecommendation(
                success=False,
                error="Modèle de recommandation non disponible"
            )
        
        try:
            logger.info(f"🎯 Recherche de véhicules (budget: {data.budget}€)")
            
            # Logique de recommandation
            recommendations = self._generate_recommendations(data, model)
            
            logger.info(f"✅ {len(recommendations)} véhicules trouvés")
            
            return ResultatRecommendation(
                success=True,
                recommendations=recommendations,
                total_matches=len(recommendations)
            )
        
        except Exception as e:
            logger.error(f"❌ Erreur: {e}")
            return ResultatRecommendation(
                success=False,
                error=str(e)
            )
    
    def _generate_recommendations(
        self,
        data: DataRecommendation,
        model
    ) -> List[VehicleRecommendation]:
        """Génère les recommandations"""
        # Votre logique de recommandation ici
        recommendations = []
        
        # Exemple simplifié
        # candidates = model.find_similar(data.budget, data.preferred_category)
        # for candidate in candidates:
        #     score = model.calculate_score(candidate, data)
        #     recommendations.append(VehicleRecommendation(...))
        
        return recommendations
```

---

## 🏭 Étape 4 : Mettre à jour la Factory

**Fichier:** `services/service_factory.py`

```python
# Ajouter l'import
from .recommendation_service import RecommendationService

# Dans la méthode _create_service
def _create_service(self, service_type: str) -> Optional[IModelService]:
    if service_type == "cnn":
        return VisionRecognitionService(self.repo_manager)
    
    elif service_type == "prediction":
        return PricePredictionService(self.repo_manager)
    
    elif service_type == "ajustement":
        return PriceAdjustmentService(self.repo_manager)
    
    elif service_type == "recommendation":  # ← AJOUT
        return RecommendationService(self.repo_manager)
    
    else:
        logger.warning(f"⚠️ Type inconnu: {service_type}")
        return None
```

---

## 📦 Étape 5 : Mettre à jour le RepositoryManager

**Fichier:** `repositories/repository_manager.py`

```python
# Ajouter l'import
from .recommendation_repository import RecommendationModelRepository

class ModelRepositoryManager:
    def __init__(self):
        self.price_repo = PriceModelRepository()
        self.adjustment_repo = AdjustmentModelRepository()
        self.vision_repo = VisionModelRepository()
        self.recommendation_repo = RecommendationModelRepository()  # ← AJOUT
    
    def load_all_models(self) -> None:
        # ... code existant ...
        
        # Ajouter
        self.recommendation_repo.load(MODEL_PATHS['recommendation_model'])
    
    def reload_all_models(self) -> Dict[str, bool]:
        results = {
            'price_model': self.price_repo.reload(),
            'adjustment_model': self.adjustment_repo.reload(),
            'vision_model': self.vision_repo.reload(),
            'recommendation_model': self.recommendation_repo.reload()  # ← AJOUT
        }
        return results
    
    def get_models_status(self) -> Dict[str, str]:
        return {
            'price_model': self.price_repo.get_status(),
            'adjustment_model': self.adjustment_repo.get_status(),
            'vision_model': self.vision_repo.get_status(),
            'recommendation_model': self.recommendation_repo.get_status()  # ← AJOUT
        }
```

---

## ⚙️ Étape 6 : Ajouter la configuration

**Fichier:** `config/settings.py`

```python
MODEL_PATHS: Dict[str, str] = {
    'price_model': str(MODELS_DIR / 'rf_price_model.joblib'),
    'adjustment_model': str(MODELS_DIR / 'adjustment_model_api.pkl'),
    'vision_model': str(MODELS_DIR / 'car_classifier_best.pth'),
    'vision_metadata': str(MODELS_DIR / 'car_metadata.json'),
    'recommendation_model': str(MODELS_DIR / 'recommendation_model.joblib')  # ← AJOUT
}
```

---

## 🧪 Étape 7 : Tester

```python
import requests

# Test de la nouvelle API
data = {
    "type": "recommendation",
    "data": {
        "type": "recommendation",
        "budget": 20000,
        "preferred_manufacturer": "Toyota",
        "preferred_category": "SUV",
        "max_mileage": 80000,
        "min_year": 2018
    }
}

response = requests.post(
    "http://localhost:8000/predict",
    json=data
)

result = response.json()
print(f"Trouvé {result['total_matches']} véhicules")
for rec in result['recommendations']:
    print(f"- {rec['manufacturer']} {rec['model']} ({rec['year']})")
    print(f"  Prix: {rec['predicted_price']}€")
    print(f"  Score: {rec['match_score']}")
```

---

## ✅ Checklist d'extension

Pour chaque nouveau modèle:

- [ ] **Schémas (schemas/dto.py)**
  - [ ] Créer `DataXXX(DataAI)`
  - [ ] Créer `ResultatXXX(ResultatAI)`
  - [ ] Ajouter à `PredictRequest.data: Union[...]`

- [ ] **Repository (repositories/xxx_repository.py)**
  - [ ] Créer classe héritant de `ModelRepositoryBase`
  - [ ] Implémenter `load()`

- [ ] **Service (services/xxx_service.py)**
  - [ ] Créer classe implémentant `IModelService`
  - [ ] Implémenter `predict()` et `is_available()`

- [ ] **Factory (services/service_factory.py)**
  - [ ] Ajouter le nouveau type dans `_create_service()`

- [ ] **Manager (repositories/repository_manager.py)**
  - [ ] Ajouter le repository dans `__init__()`
  - [ ] Ajouter dans `load_all_models()`
  - [ ] Ajouter dans `reload_all_models()`
  - [ ] Ajouter dans `get_models_status()`

- [ ] **Configuration (config/settings.py)**
  - [ ] Ajouter le chemin du modèle dans `MODEL_PATHS`

- [ ] **Tests**
  - [ ] Tester le chargement
  - [ ] Tester la prédiction
  - [ ] Tester le rechargement

---

## 🎨 Bonnes pratiques

### 1. Respect de l'interface
```python
# ✅ BON - Respecte IModelService
class MonService(IModelService):
    def predict(self, data: DataAI) -> ResultatAI:
        pass
    
    def is_available(self) -> bool:
        pass

# ❌ MAUVAIS - N'implémente pas l'interface
class MonService:
    def do_prediction(self, data):
        pass
```

### 2. Gestion des erreurs
```python
# ✅ BON - Retourne toujours un résultat valide
def predict(self, data):
    try:
        # logique
        return ResultatXXX(success=True, ...)
    except Exception as e:
        logger.error(f"Erreur: {e}")
        return ResultatXXX(success=False, error=str(e))

# ❌ MAUVAIS - Lève une exception
def predict(self, data):
    # logique
    raise Exception("Erreur!")
```

### 3. Logging
```python
# ✅ BON - Log détaillé
logger.info("🎯 Démarrage de la prédiction")
logger.info(f"✅ Résultat: {result}")
logger.error(f"❌ Erreur: {e}", exc_info=True)

# ❌ MAUVAIS - Pas de logging
# (rien)
```

### 4. Type hints
```python
# ✅ BON - Types explicites
def predict(self, data: DataXXX) -> ResultatXXX:
    model: Optional[Any] = self.repo.get_model()

# ❌ MAUVAIS - Pas de types
def predict(self, data):
    model = self.repo.get_model()
```

---

## 🔄 Workflow complet

```
1. Entraîner le modèle
   ↓
2. Sauvegarder dans saved_ia/
   ↓
3. Créer les schémas (DTOs)
   ↓
4. Créer le Repository
   ↓
5. Créer le Service
   ↓
6. Mettre à jour la Factory
   ↓
7. Mettre à jour le Manager
   ↓
8. Ajouter la configuration
   ↓
9. Tester
   ↓
10. Documenter dans EXAMPLES.md
```

---

## 📚 Ressources

### Architecture similaire en C#
```csharp
// Pattern similaire en C#
public interface IModelService<TData, TResult>
    where TData : DataAI
    where TResult : ResultatAI
{
    Task<TResult> PredictAsync(TData data);
    bool IsAvailable();
}

public class RecommendationService 
    : IModelService<DataRecommendation, ResultatRecommendation>
{
    private readonly IModelRepository _repository;
    
    public RecommendationService(IModelRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<ResultatRecommendation> PredictAsync(
        DataRecommendation data
    )
    {
        // Implementation
    }
    
    public bool IsAvailable() => _repository.IsLoaded;
}
```

---

## 💡 Cas d'usage supplémentaires

### Ajouter un modèle de détection d'anomalies
```python
# DataAnomaly + ResultatAnomaly + AnomalyService
```

### Ajouter un système d'évaluation de photos
```python
# DataPhotoQuality + ResultatPhotoQuality + PhotoQualityService
```

### Ajouter une estimation de durée de vente
```python
# DataTimeToSell + ResultatTimeToSell + TimeToSellService
```

---

**L'architecture est conçue pour l'extensibilité ! 🚀**
