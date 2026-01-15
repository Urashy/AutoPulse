"""
Architectures des modèles de deep learning
"""
# import torch.nn as nn
from torchvision.models import efficientnet_v2_m, EfficientNet_V2_M_Weights


class CarClassifier(nn.Module):
    """
    Modèle de classification de véhicules basé sur EfficientNetV2
    """
    
    def __init__(self, num_classes: int):
        super(CarClassifier, self).__init__()
        
        # Backbone pré-entraîné
        weights = EfficientNet_V2_M_Weights.IMAGENET1K_V1
        self.backbone = efficientnet_v2_m(weights=weights)
        
        # Classifier personnalisé
        in_features = self.backbone.classifier[1].in_features
        self.backbone.classifier = nn.Sequential(
            nn.Dropout(p=0.3, inplace=True),
            nn.Linear(in_features, 1024),
            nn.ReLU(inplace=True),
            nn.BatchNorm1d(1024),
            nn.Dropout(p=0.3, inplace=True),
            nn.Linear(1024, 512),
            nn.ReLU(inplace=True),
            nn.BatchNorm1d(512),
            nn.Dropout(p=0.2, inplace=True),
            nn.Linear(512, num_classes)
        )
    
    def forward(self, x):
        return self.backbone(x)
