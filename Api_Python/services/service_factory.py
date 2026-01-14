"""
Factory pour créer les services appropriés selon le type de requête
Pattern: Abstract Factory
"""
import logging
from typing import Optional
from .base_service import IModelService
from .price_service import PricePredictionService
from .adjustment_service import PriceAdjustmentService
from .vision_service import VisionRecognitionService
from repositories.repository_manager import ModelRepositoryManager

logger = logging.getLogger(__name__)


class ModelServiceFactory:
    """
    Factory pour créer les services de prédiction
    Implémente le pattern Abstract Factory
    """
    
    def __init__(self, repo_manager: ModelRepositoryManager):
        self.repo_manager = repo_manager
        self._services_cache = {}
    
    def get_service(self, service_type: str) -> Optional[IModelService]:
        """
        Retourne le service approprié selon le type
        Les services sont créés une seule fois et mis en cache
        
        Args:
            service_type: Type de service ("cnn", "prediction", "ajustement")
            
        Returns:
            Instance du service ou None si type inconnu
        """
        # Vérifier le cache
        if service_type in self._services_cache:
            return self._services_cache[service_type]
        
        # Créer le service selon le type
        service = self._create_service(service_type)
        
        # Mettre en cache si créé avec succès
        if service is not None:
            self._services_cache[service_type] = service
            logger.info(f"✅ Service '{service_type}' créé et mis en cache")
        
        return service
    
    def _create_service(self, service_type: str) -> Optional[IModelService]:
        """
        Crée une instance du service selon le type
        
        Args:
            service_type: Type de service à créer
            
        Returns:
            Instance du service ou None
        """
        if service_type == "cnn":
            return VisionRecognitionService(self.repo_manager)
        
        elif service_type == "prediction":
            return PricePredictionService(self.repo_manager)
        
        elif service_type == "ajustement":
            return PriceAdjustmentService(self.repo_manager)
        
        else:
            logger.warning(f"⚠️ Type de service inconnu: {service_type}")
            return None
    
    def clear_cache(self):
        """Vide le cache des services (utile après rechargement des modèles)"""
        self._services_cache.clear()
        logger.info("🗑️ Cache des services vidé")
