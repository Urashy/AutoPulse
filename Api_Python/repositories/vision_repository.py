"""
Repository pour le modèle de reconnaissance visuelle
"""
import torch
import json
import logging
from pathlib import Path
from typing import Optional, Dict
import torchvision.transforms as transforms
from .base_repository import ModelRepositoryBase
from models.architectures import CarClassifier
from config.settings import VISION_IMAGE_SIZE, VISION_NORMALIZE_MEAN, VISION_NORMALIZE_STD

logger = logging.getLogger(__name__)


class VisionModelRepository(ModelRepositoryBase):
    """Repository pour le modèle CNN de reconnaissance de véhicules"""
    
    def __init__(self):
        super().__init__()
        self._metadata: Optional[Dict] = None
        self._transform: Optional[transforms.Compose] = None
        self._device: Optional[torch.device] = None
        self._num_classes: int = 0
    
    def load(self, model_path: str, metadata_path: Optional[str] = None) -> bool:
        """
        Charge le modèle de vision et ses métadonnées
        
        Args:
            model_path: Chemin vers le fichier .pth du modèle
            metadata_path: Chemin vers le fichier JSON des métadonnées (optionnel)
        """
        try:
            if not self._check_file_exists(model_path):
                return False
            
            logger.info(f"👁️ Chargement du modèle vision: {model_path}")
            
            # Détection du device (GPU/CPU)
            self._device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
            
            # Chargement du checkpoint
            checkpoint = torch.load(model_path, map_location=self._device)
            
            # Chargement des métadonnées
            if metadata_path and Path(metadata_path).exists():
                with open(metadata_path, 'r') as f:
                    self._metadata = json.load(f)
            else:
                self._metadata = checkpoint.get('metadata', {})
            
            # Détermination du nombre de classes
            self._num_classes = self._metadata.get(
                'num_classes',
                len(self._metadata.get('idx_to_model', {}))
            )
            
            # Création du modèle
            model = CarClassifier(self._num_classes)
            
            # Chargement du state_dict - Support de deux formats
            if 'model_state_dict' in checkpoint:
                # Ancien format
                logger.info("   → Format: model_state_dict")
                model.load_state_dict(checkpoint['model_state_dict'])
            elif 'model' in checkpoint:
                # Nouveau format
                logger.info("   → Format: model")
                model.load_state_dict(checkpoint['model'])
            else:
                raise ValueError("Le checkpoint ne contient ni 'model_state_dict' ni 'model'")
            
            model.to(self._device)
            model.eval()
            
            # Configuration des transformations d'image
            self._transform = transforms.Compose([
                transforms.Resize(VISION_IMAGE_SIZE),
                transforms.ToTensor(),
                transforms.Normalize(
                    mean=VISION_NORMALIZE_MEAN,
                    std=VISION_NORMALIZE_STD
                )
            ])
            
            self._model = model
            self._path = model_path
            self._status = f"loaded ({self._device}, {self._num_classes} classes)"
            
            logger.info(f"✅ Modèle vision chargé ({self._device}, {self._num_classes} classes)")
            return True
            
        except Exception as e:
            logger.error(f"❌ Erreur lors du chargement du modèle vision: {e}")
            
            # Message d'aide selon l'erreur
            if "'model_state_dict'" in str(e):
                logger.error("💡 Le checkpoint semble utiliser un format différent.")
                logger.error("   Vérifiez que le checkpoint contient 'model' ou 'model_state_dict'")
            elif "num_classes" in str(e) or "idx_to_model" in str(e):
                logger.error("💡 Problème avec les métadonnées du modèle.")
                logger.error("   Assurez-vous que car_metadata.json est présent et valide.")
            
            self._model = None
            self._status = f"error: {str(e)}"
            return False
    
    def get_model(self):
        """Retourne le modèle PyTorch"""
        return self._model
    
    def get_metadata(self) -> Optional[Dict]:
        """Retourne les métadonnées du modèle"""
        return self._metadata
    
    def get_transform(self):
        """Retourne les transformations d'image"""
        return self._transform
    
    def get_device(self):
        """Retourne le device (CPU/GPU)"""
        return self._device
    
    def get_num_classes(self) -> int:
        """Retourne le nombre de classes"""
        return self._num_classes
