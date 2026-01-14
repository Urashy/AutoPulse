"""
Repository pour le modèle de prédiction de prix
"""
import joblib
import logging
from typing import Optional
from .base_repository import ModelRepositoryBase
from catboost import CatBoostRegressor

logger = logging.getLogger(__name__)


class PriceModelRepository(ModelRepositoryBase):
    """Repository pour le modèle Random Forest de prédiction de prix"""
    
    def load(self, path: str) -> bool:
        """Charge le modèle de prix depuis un fichier joblib"""
        try:
            if not self._check_file_exists(path):
                return False
            
            logger.info(f"📊 Chargement du modèle de prix: {path}")
            self._model = joblib.load(path)
            self._path = path
            self._status = "loaded"
            logger.info("✅ Modèle de prix chargé avec succès")
            return True
            
        except Exception as e:
            logger.error(f"❌ Erreur lors du chargement du modèle de prix: {e}")
            self._model = None
            self._status = f"error: {str(e)}"
            return False
    
    def get_model(self):
        """Retourne le pipeline scikit-learn"""
        return self._model
