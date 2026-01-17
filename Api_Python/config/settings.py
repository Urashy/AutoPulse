"""
Configuration centralisée de l'application
"""
from pathlib import Path
from typing import Dict
import logging
import os

# Configuration des chemins de base
BASE_DIR = Path(__file__).parent.parent

# --- DÉTECTION INTELLIGENTE DES MODÈLES ---
# On liste les endroits probables où peuvent être les modèles
chemins_possibles = [
    Path("/home/site/models"),  # Chemin standard Azure (même si tu ne vois pas /home)
    Path("/site/models"),       # Chemin si tu es à la racine système
    BASE_DIR.parent / "models", # Au cas où ils seraient juste à côté du projet
    BASE_DIR / "saved_ia"       # Fallback : dossier local (Développement)
]

MODELS_DIR = BASE_DIR / "saved_ia" # Valeur par défaut pour éviter un crash immédiat

for chemin in chemins_possibles:
    if chemin.exists() and chemin.is_dir():
        # On vérifie s'il contient au moins un fichier modèle pour être sûr
        if list(chemin.glob("*.joblib")) or list(chemin.glob("*.pth")):
            MODELS_DIR = chemin
            print(f"🚀 MODÈLES TROUVÉS : Utilisation du dossier {MODELS_DIR}")
            break

# Chemins des modèles
MODEL_PATHS: Dict[str, str] = {
    'price_model': str(MODELS_DIR / 'rf_price_model.joblib'),
    'adjustment_model': str(MODELS_DIR / 'adjustment_model_api.pkl'),
    'vision_model': str(MODELS_DIR / 'car_classifier_best.pth'),
    'vision_metadata': str(MODELS_DIR / 'car_metadata.json')
}
# ------------------------------------------

# Configuration de l'API
# ... (Le reste du fichier ne change pas)