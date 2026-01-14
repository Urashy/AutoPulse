"""
Gestionnaire centralisé des repositories (Unit of Work pattern)
"""
import logging
from typing import Dict
from config.settings import MODEL_PATHS
from .price_repository import PriceModelRepository
from .adjustment_repository import AdjustmentModelRepository
from .vision_repository import VisionModelRepository

logger = logging.getLogger(__name__)


class ModelRepositoryManager:
    """
    Gestionnaire centralisé de tous les repositories de modèles
    Implémente un pattern similaire au Unit of Work
    """
    
    def __init__(self):
        self.price_repo = PriceModelRepository()
        self.adjustment_repo = AdjustmentModelRepository()
        self.vision_repo = VisionModelRepository()
    
    def load_all_models(self) -> None:
        """Charge tous les modèles au démarrage de l'application"""
        logger.info("=" * 70)
        logger.info("🚀 CHARGEMENT DES MODÈLES IA")
        logger.info("=" * 70)
        
        # Chargement du modèle de prix
        self.price_repo.load(MODEL_PATHS['price_model'])
        
        # Chargement du modèle d'ajustement
        self.adjustment_repo.load(MODEL_PATHS['adjustment_model'])
        
        # Chargement du modèle de vision
        self.vision_repo.load(
            MODEL_PATHS['vision_model'],
            MODEL_PATHS['vision_metadata']
        )
        
        # Résumé du chargement
        self._log_summary()
    
    def reload_all_models(self) -> Dict[str, bool]:
        """
        Recharge tous les modèles
        
        Returns:
            Dictionnaire avec le statut de rechargement de chaque modèle
        """
        logger.info("🔄 RECHARGEMENT DES MODÈLES")
        
        results = {
            'price_model': self.price_repo.reload(),
            'adjustment_model': self.adjustment_repo.reload(),
            'vision_model': self.vision_repo.reload()
        }
        
        self._log_summary()
        return results
    
    def get_models_status(self) -> Dict[str, str]:
        """
        Retourne le statut de tous les modèles
        
        Returns:
            Dictionnaire avec le statut de chaque modèle
        """
        return {
            'price_model': self.price_repo.get_status(),
            'adjustment_model': self.adjustment_repo.get_status(),
            'vision_model': self.vision_repo.get_status()
        }
    
    def _log_summary(self) -> None:
        """Affiche un résumé du statut des modèles"""
        logger.info("=" * 70)
        logger.info("📋 RÉSUMÉ DES MODÈLES:")
        
        for model_name, status in self.get_models_status().items():
            emoji = "✅" if "loaded" in status else "❌"
            logger.info(f"   {emoji} {model_name}: {status}")
        
        logger.info("=" * 70)
