"""
Contrôleur principal pour les prédictions IA
Similaire au pattern Controller en C# MVC/API
"""
import logging
from typing import Union
from fastapi import HTTPException
from schemas.dto import (
    PredictRequest,
    ResultatCNN,
    ResultatPrediction,
    ResultatAjustement,
    HealthResponse,
    ReloadResponse
)
from services.service_factory import ModelServiceFactory
from repositories.repository_manager import ModelRepositoryManager

logger = logging.getLogger(__name__)


class PredictionController:
    """
    Contrôleur pour gérer les prédictions IA
    Point d'entrée pour toutes les requêtes de prédiction
    """
    
    def __init__(
        self,
        repo_manager: ModelRepositoryManager,
        service_factory: ModelServiceFactory
    ):
        self.repo_manager = repo_manager
        self.service_factory = service_factory
    
    def predict(
        self,
        request: PredictRequest
    ) -> Union[ResultatCNN, ResultatPrediction, ResultatAjustement]:
        """
        Point d'entrée polymorphe pour toutes les prédictions
        
        Args:
            request: Requête polymorphe contenant le type et les données
            
        Returns:
            Résultat polymorphe selon le type de requête
            
        Raises:
            HTTPException: Si le type est inconnu ou si une erreur survient
        """
        logger.info(f"📨 Requête reçue - Type: {request.type}")
        
        try:
            # Récupération du service approprié via la factory
            service = self.service_factory.get_service(request.type)
            
            if service is None:
                raise HTTPException(
                    status_code=400,
                    detail=f"Type de prédiction inconnu: {request.type}"
                )
            
            # Vérification de la disponibilité du service
            if not service.is_available():
                raise HTTPException(
                    status_code=503,
                    detail=f"Service '{request.type}' non disponible (modèle non chargé)"
                )
            
            # Exécution de la prédiction
            result = service.predict(request.data)
            
            # Log du résultat
            if result.success:
                logger.info(f"✅ Prédiction réussie - Type: {request.type}")
            else:
                logger.warning(f"⚠️ Échec de la prédiction - Type: {request.type}, Erreur: {result.error}")
            
            return result
        
        except HTTPException:
            raise
        
        except Exception as e:
            logger.error(f"❌ Erreur inattendue lors de la prédiction: {e}", exc_info=True)
            raise HTTPException(
                status_code=500,
                detail=f"Erreur interne: {str(e)}"
            )
    
    def get_health(self) -> HealthResponse:
        """
        Retourne l'état de santé de l'API et de ses modèles
        
        Returns:
            État de santé avec statuts des modèles
        """
        models_status = self.repo_manager.get_models_status()
        
        # Déterminer si l'API est en bonne santé
        all_loaded = all("loaded" in status for status in models_status.values())
        status = "healthy" if all_loaded else "degraded"
        
        return HealthResponse(
            status=status,
            models=models_status
        )
    
    def reload_models(self) -> ReloadResponse:
        """
        Recharge tous les modèles IA
        
        Returns:
            Résultat du rechargement avec statuts détaillés
        """
        logger.info("🔄 Demande de rechargement des modèles")
        
        try:
            # Rechargement de tous les modèles
            reload_results = self.repo_manager.reload_all_models()
            
            # Vider le cache des services pour forcer la recréation
            self.service_factory.clear_cache()
            
            # Vérifier si tous les modèles ont été rechargés avec succès
            all_success = all(reload_results.values())
            
            # Récupérer les statuts actuels
            models_status = self.repo_manager.get_models_status()
            
            message = (
                "✅ Tous les modèles ont été rechargés avec succès"
                if all_success
                else "⚠️ Certains modèles n'ont pas pu être rechargés"
            )
            
            logger.info(message)
            
            return ReloadResponse(
                success=all_success,
                message=message,
                models_status=models_status
            )
        
        except Exception as e:
            logger.error(f"❌ Erreur lors du rechargement des modèles: {e}")
            return ReloadResponse(
                success=False,
                message=f"Erreur lors du rechargement: {str(e)}",
                models_status=self.repo_manager.get_models_status()
            )
