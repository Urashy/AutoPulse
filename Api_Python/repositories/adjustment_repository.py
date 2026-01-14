"""
Repository pour le modèle d'ajustement de prix
"""
import joblib
import logging
import pickle
import warnings
from typing import Optional, Dict
from .base_repository import ModelRepositoryBase

# Importer sklearn pour éviter les problèmes de dépendances
try:
    import sklearn
    import sklearn.preprocessing
    import sklearn.feature_extraction
    import sklearn.decomposition
    import sklearn.linear_model
    import sklearn.ensemble
except ImportError:
    warnings.warn("Certains modules sklearn ne sont pas disponibles")

logger = logging.getLogger(__name__)


class AdjustmentModelRepository(ModelRepositoryBase):
    """Repository pour le modèle d'ajustement de prix basé sur la description"""
    
    def load(self, path: str) -> bool:
        """Charge le modèle d'ajustement depuis un fichier pickle"""
        try:
            if not self._check_file_exists(path):
                return False
            
            logger.info(f"🤖 Chargement du modèle d'ajustement: {path}")
            
            # Tentative de chargement avec pickle (plus compatible)
            try:
                import pickle
                with open(path, 'rb') as f:
                    model_dict = pickle.load(f)
                logger.info("   → Chargé avec pickle")
            except Exception as e1:
                logger.warning(f"   → Pickle échoué: {e1}, essai avec joblib...")
                try:
                    model_dict = joblib.load(path)
                    logger.info("   → Chargé avec joblib")
                except Exception as e2:
                    raise Exception(f"Échec pickle et joblib: {e1} / {e2}")
            
            # Vérification de l'intégrité
            required_keys = ['ml_model', 'scaler', 'tfidf', 'svd', 'semantic_vocab']
            for key in required_keys:
                if key not in model_dict:
                    raise ValueError(f"Clé manquante dans le modèle: {key}")
            
            self._model = model_dict
            self._path = path
            self._status = "loaded"
            logger.info("✅ Modèle d'ajustement chargé avec succès")
            return True
            
        except Exception as e:
            logger.error(f"❌ Erreur lors du chargement du modèle d'ajustement: {e}")
            logger.error("💡 Conseil: Essayez de reconvertir le modèle avec fix_adjustment_model.py")
            self._model = None
            self._status = f"error: {str(e)}"
            return False
    
    def get_model(self) -> Optional[Dict]:
        """Retourne le dictionnaire contenant les composants du modèle"""
        return self._model
