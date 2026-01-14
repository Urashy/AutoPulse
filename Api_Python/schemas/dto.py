"""
Schémas Pydantic pour les requêtes et réponses
"""
from pydantic import BaseModel, Field
from typing import Optional, List, Union, Literal


# ============================================================================
# SCHÉMAS DE REQUÊTES (DTOs Input)
# ============================================================================

class DataAI(BaseModel):
    """Classe de base pour tous les types de données"""
    type: str


class DataCNN(DataAI):
    """Données pour la reconnaissance visuelle"""
    type: Literal["cnn"] = "cnn"
    image_base64: str = Field(..., description="Image encodée en base64")


class DataPrediction(DataAI):
    """Données pour la prédiction de prix"""
    type: Literal["prediction"] = "prediction"
    manufacturer: Optional[str] = None
    model: Optional[str] = None
    prod_year: Optional[int] = None
    category: Optional[str] = None
    leather_interior: Optional[str] = None
    fuel_type: Optional[str] = None
    engine_volume: Optional[float] = None
    mileage: Optional[str] = None
    cylinders: Optional[float] = None
    gear_box_type: Optional[str] = None
    drive_wheels: Optional[str] = None
    doors: Optional[str] = None
    wheel: Optional[str] = None
    color: Optional[str] = None
    airbags: Optional[int] = None
    levy: Optional[float] = None


class DataAjustement(DataAI):
    """Données pour l'ajustement de prix"""
    type: Literal["ajustement"] = "ajustement"
    base_price: float = Field(..., gt=0)
    description: str = Field(..., min_length=1)


class PredictRequest(BaseModel):
    """Requête polymorphe pour la prédiction"""
    type: str
    data: Union[DataCNN, DataPrediction, DataAjustement]


# ============================================================================
# SCHÉMAS DE RÉPONSES (DTOs Output)
# ============================================================================

class ResultatAI(BaseModel):
    """Classe de base pour tous les résultats"""
    type: str
    success: bool
    error: Optional[str] = None


class TopPrediction(BaseModel):
    """Prédiction Top-N pour la vision"""
    manufacturer: str
    model: str
    confidence: str


class ResultatCNN(ResultatAI):
    """Résultat de reconnaissance visuelle"""
    type: Literal["cnn"] = "cnn"
    manufacturer: Optional[str] = None
    model: Optional[str] = None
    full_name: Optional[str] = None
    confidence: Optional[str] = None
    confidence_score: Optional[float] = None
    image_size: Optional[str] = None
    top_predictions: Optional[List[TopPrediction]] = None


class InfluencingFactor(BaseModel):
    """Facteur d'influence pour la prédiction de prix"""
    feature: str
    importance: float


class ResultatPrediction(ResultatAI):
    """Résultat de prédiction de prix"""
    type: Literal["prediction"] = "prediction"
    predicted_price: Optional[float] = None
    currency: Optional[str] = None
    confidence_score: Optional[float] = None
    top_influencing_factors: Optional[List[InfluencingFactor]] = None


class ResultatAjustement(ResultatAI):
    """Résultat d'ajustement de prix"""
    type: Literal["ajustement"] = "ajustement"
    base_price: Optional[float] = None
    adjusted_price: Optional[float] = None
    reduction_amount: Optional[float] = None
    reduction_percent: Optional[float] = None
    quality_coefficient: Optional[float] = None
    category: Optional[str] = None
    description_analyzed: Optional[str] = None


class HealthResponse(BaseModel):
    """Réponse du endpoint health"""
    status: str
    models: dict


class ReloadResponse(BaseModel):
    """Réponse du endpoint de rechargement"""
    success: bool
    message: str
    models_status: dict
