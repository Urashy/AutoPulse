"""
Service abstrait de base pour les prédictions IA
Pattern: Abstract Factory / Strategy
"""
from abc import ABC, abstractmethod
from schemas.dto import DataAI, ResultatAI


class IModelService(ABC):
    """Interface abstraite pour tous les services de prédiction"""
    
    @abstractmethod
    def predict(self, data: DataAI) -> ResultatAI:
        """
        Effectue une prédiction
        
        Args:
            data: Données d'entrée (polymorphe)
            
        Returns:
            Résultat de la prédiction (polymorphe)
        """
        pass
    
    @abstractmethod
    def is_available(self) -> bool:
        """
        Vérifie si le service est disponible (modèle chargé)
        
        Returns:
            True si le service est opérationnel
        """
        pass
