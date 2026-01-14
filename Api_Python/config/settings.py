"""
Configuration centralisée de l'application
"""
from pathlib import Path
from typing import Dict
import logging

# Configuration des chemins
BASE_DIR = Path(__file__).parent.parent
MODELS_DIR = BASE_DIR / "saved_ia"

# Chemins des modèles
MODEL_PATHS: Dict[str, str] = {
    'price_model': str(MODELS_DIR / 'rf_price_model.joblib'),
    'adjustment_model': str(MODELS_DIR / 'adjustment_model_api.pkl'),
    'vision_model': str(MODELS_DIR / 'car_classifier_best.pth'),  # ou 'best.pth'
    'vision_metadata': str(MODELS_DIR / 'car_metadata.json')
}

# Configuration de l'API
#
API_TITLE = "API IA Automobile"
API_VERSION = "4.0.0"
API_DESCRIPTION = "Architecture propre avec pattern Repository"

# Configuration du logging
LOGGING_CONFIG = {
    'level': logging.INFO,
    'format': '%(asctime)s - %(name)s - %(levelname)s - %(message)s'
}

# Configuration des modèles
VISION_IMAGE_SIZE = (384, 384)
VISION_NORMALIZE_MEAN = [0.485, 0.456, 0.406]
VISION_NORMALIZE_STD = [0.229, 0.224, 0.225]

# Limites et contraintes
MAX_IMAGE_SIZE_MB = 10
DEFAULT_CURRENCY = "€"
DEFAULT_CONFIDENCE_SCORE = 0.85
