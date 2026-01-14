"""
Service d'ajustement de prix basé sur la description
"""
import logging
import numpy as np
import re
from typing import Dict, Tuple
from collections import Counter
from .base_service import IModelService
from schemas.dto import DataAjustement, ResultatAjustement
from repositories.repository_manager import ModelRepositoryManager

logger = logging.getLogger(__name__)


class PriceAdjustmentService(IModelService):
    """Service pour ajuster le prix selon la description du véhicule"""
    
    def __init__(self, repo_manager: ModelRepositoryManager):
        self.repo_manager = repo_manager
    
    def is_available(self) -> bool:
        """Vérifie si le modèle d'ajustement est disponible"""
        return self.repo_manager.adjustment_repo.get_model() is not None
    
    def predict(self, data: DataAjustement) -> ResultatAjustement:
        """
        Ajuste le prix selon la description
        
        Args:
            data: Prix de base et description
            
        Returns:
            Prix ajusté avec détails de la réduction
        """
        model = self.repo_manager.adjustment_repo.get_model()
        
        if model is None:
            return ResultatAjustement(
                success=False,
                error="Modèle d'ajustement non disponible"
            )
        
        try:
            logger.info(f"🔍 Analyse: '{data.description[:50]}...'")
            
            # Extraction des features
            features = self._extract_features(data.description, model)
            X = np.array([list(features.values())])
            
            # Prédiction du coefficient
            X_scaled = model['scaler'].transform(X)
            coefficient = float(model['ml_model'].predict(X_scaled)[0])
            coefficient = np.clip(coefficient, 0.0, 1.0)
            
            # Détermination de la catégorie
            category, max_reduction = self._determine_category(coefficient, features)
            
            # Ajustement selon le prix
            price_factor = self._calculate_price_factor(data.base_price)
            coefficient_final = np.clip(coefficient * price_factor, 0.0, 1.0)
            
            # Calculs finaux
            reduction_percent = max_reduction * coefficient_final * 100
            reduction_amount = data.base_price * (reduction_percent / 100)
            adjusted_price = data.base_price - reduction_amount
            
            logger.info(f"✅ {category} (-{reduction_percent:.2f}%)")
            
            return ResultatAjustement(
                success=True,
                base_price=data.base_price,
                adjusted_price=adjusted_price,
                reduction_amount=reduction_amount,
                reduction_percent=reduction_percent,
                quality_coefficient=coefficient_final,
                category=category,
                description_analyzed=data.description
            )
        
        except Exception as e:
            logger.error(f"❌ Erreur lors de l'ajustement: {e}")
            return ResultatAjustement(
                success=False,
                error=str(e)
            )
    
    def _extract_features(self, description: str, model: dict) -> dict:
        """
        Extraction des features depuis la description
        
        Args:
            description: Texte de description du véhicule
            model: Dictionnaire contenant les composants du modèle
            
        Returns:
            Dictionnaire de features
        """
        features = {}
        desc_lower = description.lower()
        
        # Features TF-IDF + SVD
        tfidf_vec = model['tfidf'].transform([description])
        svd_features = model['svd'].transform(tfidf_vec)[0]
        for i, val in enumerate(svd_features):
            features[f'svd_{i}'] = val
        
        # Features sémantiques
        for category, info in model['semantic_vocab'].items():
            score = 0.0
            matches = 0
            for keyword in info['keywords']:
                if keyword in desc_lower:
                    score += info['weight']
                    matches += 1
            features[f'semantic_{category}_score'] = score
            features[f'semantic_{category}_matches'] = matches
        
        # Features linguistiques
        features.update(self._extract_linguistic_features(description))
        
        # Features de combinaisons critiques
        features['critical_combo_score'] = self._calculate_critical_combos(desc_lower)
        
        # Mots répétés
        words = desc_lower.split()
        word_freq = Counter(words)
        features['repeated_words'] = sum(1 for count in word_freq.values() if count > 1)
        
        return features
    
    def _extract_linguistic_features(self, description: str) -> dict:
        """Extrait les features linguistiques de base"""
        desc_lower = description.lower()
        words = desc_lower.split()
        
        return {
            'length': len(description),
            'word_count': len(words),
            'avg_word_length': np.mean([len(w) for w in words]) if words else 0,
            'exclamation_count': description.count('!'),
            'question_count': description.count('?'),
            'comma_count': description.count(','),
            'has_numbers': 1.0 if re.search(r'\d', description) else 0.0,
            'negation_count': sum(
                1 for neg in ['pas', 'jamais', 'aucun', 'sans', 'non']
                if neg in desc_lower
            ),
            'intensity': sum(
                1 for intens in ['très', 'extrêmement', 'vraiment', 'beaucoup']
                if intens in desc_lower
            )
        }
    
    def _calculate_critical_combos(self, desc_lower: str) -> float:
        """Calcule le score des combinaisons de mots critiques"""
        critical_combos = [
            ('moteur', 'hs'), ('moteur', 'mort'), ('boîte', 'hs'),
            ('non', 'roulant'), ('pour', 'pièces'), ('châssis', 'tordu')
        ]
        
        combo_score = 0
        for word1, word2 in critical_combos:
            if word1 in desc_lower and word2 in desc_lower:
                if abs(desc_lower.find(word1) - desc_lower.find(word2)) < 50:
                    combo_score += 1
        
        return combo_score
    
    def _calculate_price_factor(self, base_price: float) -> float:
        """Calcule le facteur d'ajustement selon le prix"""
        if base_price > 50000:
            return 1.3
        elif base_price < 10000:
            return 0.7
        else:
            return 1.0
    
    def _determine_category(
        self,
        coefficient: float,
        features: dict
    ) -> Tuple[str, float]:
        """
        Détermine la catégorie et la réduction maximale
        
        Args:
            coefficient: Coefficient prédit par le modèle
            features: Features extraites
            
        Returns:
            Tuple (catégorie, réduction_max)
        """
        critique_score = features.get('semantic_critique_score', 0)
        grave_score = features.get('semantic_grave_score', 0)
        modere_score = features.get('semantic_modere_score', 0)
        combo_score = features.get('critical_combo_score', 0)
        
        critique_matches = features.get('semantic_critique_matches', 0)
        grave_matches = features.get('semantic_grave_matches', 0)
        modere_matches = features.get('semantic_modere_matches', 0)
        positif_matches = features.get('semantic_positif_matches', 0)
        
        # Logique de catégorisation
        if combo_score >= 1 or critique_matches >= 1 or coefficient > 0.85:
            return "🔴 CRITIQUE - Véhicule non roulant/épave", 0.50
        
        elif grave_matches >= 2 or grave_score > 0.5 or coefficient > 0.65:
            return "🟠 GRAVE - Défauts structurels", 0.20
        
        elif modere_matches >= 2 or modere_score > 0.3 or coefficient > 0.35:
            return "🟡 MODÉRÉ - Plusieurs défauts", 0.10
        
        elif coefficient > 0.10 and positif_matches < 2:
            return "🟢 BON - Défauts mineurs", 0.10
        
        elif positif_matches >= 2:
            return "🟢 BON - Bon entretien", 0.10
        
        else:
            return "✅ EXCELLENT - État impeccable", 0.10
