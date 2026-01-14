"""
Service de reconnaissance visuelle de véhicules
"""
import logging
import torch
import base64
from io import BytesIO
from PIL import Image
from typing import List
from .base_service import IModelService
from schemas.dto import DataCNN, ResultatCNN, TopPrediction
from repositories.repository_manager import ModelRepositoryManager

logger = logging.getLogger(__name__)


class VisionRecognitionService(IModelService):
    """Service pour identifier un véhicule depuis une image"""
    
    def __init__(self, repo_manager: ModelRepositoryManager):
        self.repo_manager = repo_manager
    
    def is_available(self) -> bool:
        """Vérifie si le modèle de vision est disponible"""
        return self.repo_manager.vision_repo.get_model() is not None
    
    def predict(self, data: DataCNN) -> ResultatCNN:
        """
        Identifie un véhicule depuis une image encodée en base64
        
        Args:
            data: Image encodée en base64
            
        Returns:
            Résultat avec marque, modèle et top prédictions
        """
        repo = self.repo_manager.vision_repo
        model = repo.get_model()
        
        if model is None:
            return ResultatCNN(
                success=False,
                error="Modèle de vision non disponible"
            )
        
        try:
            # Décodage de l'image
            image = self._decode_image(data.image_base64)
            
            # Préparation de l'image
            transform = repo.get_transform()
            device = repo.get_device()
            img_tensor = transform(image).unsqueeze(0).to(device)
            
            # Prédiction
            with torch.no_grad():
                outputs = model(img_tensor)
                probabilities = torch.softmax(outputs, dim=1)
                confidence, predicted_idx = probabilities.max(1)
                confidence = confidence.item()
                predicted_idx = predicted_idx.item()
            
            # Récupération des métadonnées
            metadata = repo.get_metadata()
            idx_to_model = metadata.get('idx_to_model', {})
            make_model_dict = metadata.get('make_model_dict', {})
            
            # Vérification de l'index
            if str(predicted_idx) not in idx_to_model:
                return ResultatCNN(
                    success=False,
                    error=f"Index {predicted_idx} non trouvé dans les métadonnées"
                )
            
            # Extraction marque et modèle
            manufacturer, model_name, full_name = self._extract_vehicle_info(
                predicted_idx,
                idx_to_model,
                make_model_dict
            )
            
            # Top 5 prédictions
            top5_predictions = self._get_top_predictions(
                probabilities[0],
                idx_to_model,
                make_model_dict
            )
            
            logger.info(f"✅ Véhicule identifié: {full_name} ({confidence*100:.2f}%)")
            
            return ResultatCNN(
                success=True,
                manufacturer=manufacturer,
                model=model_name,
                full_name=full_name,
                confidence=f"{confidence * 100:.2f}%",
                confidence_score=float(confidence),
                image_size=f"{image.size[0]}x{image.size[1]}",
                top_predictions=top5_predictions
            )
        
        except Exception as e:
            logger.error(f"❌ Erreur lors de la reconnaissance visuelle: {e}")
            return ResultatCNN(
                success=False,
                error=str(e)
            )
    
    def _decode_image(self, image_base64: str) -> Image.Image:
        """
        Décode une image base64
        
        Args:
            image_base64: Image encodée en base64
            
        Returns:
            Image PIL
        """
        image_bytes = base64.b64decode(image_base64)
        image = Image.open(BytesIO(image_bytes)).convert('RGB')
        return image
    
    def _extract_vehicle_info(
        self,
        predicted_idx: int,
        idx_to_model: dict,
        make_model_dict: dict
    ) -> tuple:
        """
        Extrait les informations du véhicule
        
        Args:
            predicted_idx: Index prédit
            idx_to_model: Mapping index -> label
            make_model_dict: Mapping label -> nom complet
            
        Returns:
            Tuple (manufacturer, model_name, full_name)
        """
        model_label = idx_to_model[str(predicted_idx)]
        full_name = make_model_dict.get(
            str(model_label),
            f"Vehicle_Class_{model_label}"
        )
        
        parts = full_name.split(' ', 1)
        manufacturer = parts[0] if len(parts) > 0 else "Unknown"
        model_name = parts[1] if len(parts) > 1 else "Unknown"
        
        return manufacturer, model_name, full_name
    
    def _get_top_predictions(
        self,
        probabilities: torch.Tensor,
        idx_to_model: dict,
        make_model_dict: dict,
        top_k: int = 5
    ) -> List[TopPrediction]:
        """
        Récupère les top K prédictions
        
        Args:
            probabilities: Tensor de probabilités
            idx_to_model: Mapping index -> label
            make_model_dict: Mapping label -> nom complet
            top_k: Nombre de prédictions à retourner
            
        Returns:
            Liste des top prédictions
        """
        top_k = min(top_k, probabilities.shape[0])
        top_prob, top_idx = torch.topk(probabilities, top_k)
        
        predictions = []
        for prob, idx in zip(top_prob, top_idx):
            idx_val = idx.item()
            prob_val = prob.item()
            
            if str(idx_val) in idx_to_model:
                label = idx_to_model[str(idx_val)]
                name = make_model_dict.get(str(label), f"Class_{label}")
                parts = name.split(' ', 1)
                
                predictions.append(TopPrediction(
                    manufacturer=parts[0] if len(parts) > 0 else "Unknown",
                    model=parts[1] if len(parts) > 1 else "Unknown",
                    confidence=f"{prob_val * 100:.2f}%"
                ))
        
        return predictions
