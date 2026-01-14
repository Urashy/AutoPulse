"""
Repository Pattern - Interface pour la gestion des modèles IA
"""
from abc import ABC, abstractmethod
from typing import Any, Dict, Optional
from pathlib import Path
import logging

logger = logging.getLogger(__name__)


class IModelRepository(ABC):
    """Interface du Repository pour les modèles"""
    
    @abstractmethod
    def load(self, path: str) -> bool:
        """Charge un modèle depuis un fichier"""
        pass
    
    @abstractmethod
    def get_model(self) -> Optional[Any]:
        """Retourne le modèle chargé"""
        pass
    
    @abstractmethod
    def get_status(self) -> str:
        """Retourne le statut du modèle"""
        pass
    
    @abstractmethod
    def reload(self) -> bool:
        """Recharge le modèle"""
        pass


class ModelRepositoryBase(IModelRepository):
    """Classe de base pour les repositories de modèles"""
    
    def __init__(self):
        self._model: Optional[Any] = None
        self._status: str = "not_loaded"
        self._path: Optional[str] = None
    
    def get_model(self) -> Optional[Any]:
        return self._model
    
    def get_status(self) -> str:
        return self._status
    
    def reload(self) -> bool:
        """Recharge le modèle depuis le dernier chemin connu"""
        if self._path is None:
            logger.warning("Aucun chemin connu pour recharger le modèle")
            return False
        return self.load(self._path)
    
    def _check_file_exists(self, path: str) -> bool:
        """Vérifie si le fichier existe"""
        if not Path(path).exists():
            logger.error(f"❌ Fichier non trouvé: {path}")
            self._status = "error: file not found"
            return False
        return True
