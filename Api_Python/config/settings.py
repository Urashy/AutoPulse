"""
Configuration centralisée de l'application
"""
from pathlib import Path
from typing import Dict
import logging
import os

# ==============================================================================
# 1. CONFIGURATION DES CHEMINS ET DÉTECTION INTELLIGENTE
# ==============================================================================

# Chemin de base du projet
BASE_DIR = Path(__file__).parent.parent

# Détection intelligente du dossier des modèles
# On liste les endroits probables : Azure, Racine système, Local
chemins_possibles = [
    Path("/home/site/models"),  # Chemin standard Azure App Service
    Path("/site/models"),       # Variante Azure
    BASE_DIR.parent / "models", # Dossier parent
    BASE_DIR / "saved_ia"       # Fallback : Développement local
]

# Valeur par défaut pour éviter un crash si rien n'est trouvé tout de suite
MODELS_DIR = BASE_DIR / "saved_ia" 

# Boucle de recherche du bon dossier
for chemin in chemins_possibles:
    if chemin.exists() and chemin.is_dir():
        # On vérifie s'il contient au moins un fichier modèle (.joblib ou .pth)
        if list(chemin.glob("*.joblib")) or list(chemin.glob("*.pth")):
            MODELS_DIR = chemin
            print(f"🚀 MODÈLES TROUVÉS : Utilisation du dossier {MODELS_DIR}")
            break

# Définition des chemins exacts des fichiers modèles
MODEL_PATHS: Dict[str, str] = {
    'price_model': str(MODELS_DIR / 'rf_price_model.joblib'),
    'adjustment_model': str(MODELS_DIR / 'adjustment_model_api.pkl'),
    'vision_model': str(MODELS_DIR / 'car_classifier_best.pth'),
    'vision_metadata': str(MODELS_DIR / 'car_metadata.json')
}

API_TITLE = "API IA Automobile"
API_VERSION = "4.0.0"
API_DESCRIPTION = "Architecture propre avec pattern Repository"

# Configuration du logging
LOGGING_CONFIG = {
    'level': logging.INFO,
    'format': '%(asctime)s - %(name)s - %(levelname)s - %(message)s'
}

# Configuration des modèles de vision
VISION_IMAGE_SIZE = (384, 384)
VISION_NORMALIZE_MEAN = [0.485, 0.456, 0.406]
VISION_NORMALIZE_STD = [0.229, 0.224, 0.225]

# Limites et contraintes
MAX_IMAGE_SIZE_MB = 10
DEFAULT_CURRENCY = "€"
DEFAULT_CONFIDENCE_SCORE = 0.85