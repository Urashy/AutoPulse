"""
Service de prédiction de prix
"""
import logging
import pandas as pd
import numpy as np
from typing import List
from .base_service import IModelService
from schemas.dto import DataPrediction, ResultatPrediction, InfluencingFactor
from repositories.repository_manager import ModelRepositoryManager
from config.settings import DEFAULT_CURRENCY, DEFAULT_CONFIDENCE_SCORE

logger = logging.getLogger(__name__)


class PricePredictionService(IModelService):
    """Service pour prédire le prix d'un véhicule"""
    
    def __init__(self, repo_manager: ModelRepositoryManager):
        self.repo_manager = repo_manager
    
    def is_available(self) -> bool:
        """Vérifie si le modèle de prix est disponible"""
        return self.repo_manager.price_repo.get_model() is not None
    
    def predict(self, data: DataPrediction) -> ResultatPrediction:
        """
        Prédit le prix d'un véhicule
        
        Args:
            data: Caractéristiques du véhicule
            
        Returns:
            Prédiction du prix avec les facteurs d'influence
        """
        model = self.repo_manager.price_repo.get_model()
        
        if model is None:
            return ResultatPrediction(
                success=False,
                error="Modèle de prix non disponible"
            )
        
        try:
            # ÉTAPE 1: Récupérer les features attendues par le modèle
            expected_features = model.feature_names_
            logger.info(f"🔍 Features attendues par le modèle: {expected_features}")
            
            # ÉTAPE 2: Préparation des données AVEC feature engineering
            df = self._prepare_dataframe(data)
            
            # ÉTAPE 3: Vérifier et ajuster les colonnes pour correspondre au modèle
            current_columns = df.columns.tolist()
            logger.info(f"🔍 Colonnes actuelles: {current_columns}")
            
            # Ajouter les colonnes manquantes avec des valeurs par défaut
            for col in expected_features:
                if col not in df.columns:
                    logger.warning(f"⚠️ Colonne manquante '{col}', ajout avec valeur par défaut")
                    # Déterminer si la colonne est catégorielle ou numérique
                    # En se basant sur les noms de colonnes typiques
                    categorical_cols = [
                        "Manufacturer", "Model", "Category", "Fuel type", 
                        "Gear box type", "Drive wheels", "Wheel", "Color",
                        "Age_Category", "Usage_Category"
                    ]
                    
                    if col in categorical_cols:
                        df[col] = "Unknown"
                    else:
                        # Pour les colonnes numériques, mettre 0 ou une valeur par défaut
                        default_numeric = {
                            "Turbo": 0,
                            "Levy": 0,
                            "Prod. year": 2015,
                            "Leather interior": 0,
                            "Engine volume": 2.0,
                            "Mileage": 100000,
                            "Cylinders": 4,
                            "Doors": 4,
                            "Airbags": 4,
                            "Car_Age": 10,
                            "Mileage_per_Year": 10000,
                            "Is_Premium": 0,
                            "Engine_Efficiency": 0.5
                        }
                        df[col] = default_numeric.get(col, 0)
            
            # Supprimer les colonnes qui ne sont pas attendues
            for col in current_columns:
                if col not in expected_features:
                    logger.warning(f"⚠️ Colonne non attendue '{col}', suppression")
                    df = df.drop(columns=[col])
            
            # Réordonner les colonnes dans l'ordre attendu par le modèle
            df = df[expected_features]
            logger.info(f"✅ DataFrame réordonné selon le modèle")
            
            # Nettoyage des valeurs manquantes
            df.replace(["-", "?", "None", "none", "null", "NaN", "nan", ""], np.nan, inplace=True)
            
            # IMPORTANT: Définir les cat_features manuellement
            # CatBoost chargé via joblib perd cette information
            # Ces colonnes DOIVENT correspondre exactement à celles définies lors de l'entraînement
            
            # Essayer d'abord de récupérer les cat_features du modèle
            try:
                cat_feature_indices = model.get_cat_feature_indices()
                cat_features = cat_feature_indices
                categorical_columns = [expected_features[i] for i in cat_feature_indices]
                logger.info(f"✅ Features catégorielles récupérées du modèle: {categorical_columns}")
            except:
                # Sinon, définir manuellement
                categorical_columns = [
                    "Manufacturer", "Model", "Category", "Fuel type", 
                    "Gear box type", "Drive wheels", "Doors", "Wheel", "Color",
                    "Age_Category", "Usage_Category"
                ]
                # Filtrer pour ne garder que celles qui existent dans expected_features
                categorical_columns = [col for col in categorical_columns if col in expected_features]
                cat_features = [i for i, col in enumerate(expected_features) if col in categorical_columns]
                logger.warning(f"⚠️ Features catégorielles définies manuellement: {categorical_columns}")
            
            logger.debug(f"🔍 Colonnes du DataFrame: {df.columns.tolist()}")
            logger.debug(f"🔍 Features catégorielles (indices): {cat_features}")
            logger.debug(f"🔍 Features catégorielles (noms): {[df.columns[i] for i in cat_features]}")
            
            # D'abord, gérer toutes les colonnes numériques qui ne sont PAS catégorielles
            all_columns = df.columns.tolist()
            cat_column_names = [all_columns[idx] for idx in cat_features]
            
            numeric_columns = [
                "Prod. year", "Leather interior", "Engine volume", "Mileage",
                "Cylinders", "Airbags", "Car_Age", "Mileage_per_Year",
                "Is_Premium", "Engine_Efficiency"
            ]
            
            # Traiter les colonnes numériques (sauf si elles sont catégorielles)
            for col in numeric_columns:
                if col in df.columns and col not in cat_column_names:
                    # Remplacer les NaN par des valeurs par défaut
                    if df[col].dtype == object:
                        df[col] = pd.to_numeric(df[col], errors='coerce')
                    
                    if df[col].isna().any():
                        default_values = {
                            "Airbags": 4,
                            "Cylinders": 4,
                            "Leather interior": 0,
                            "Is_Premium": 0,
                            "Prod. year": 2015,
                            "Engine volume": 2.0,
                            "Mileage": 100000,
                            "Car_Age": 10,
                            "Mileage_per_Year": 10000,
                            "Engine_Efficiency": 0.5
                        }
                        df[col] = df[col].fillna(default_values.get(col, 0))
            
            # Ensuite, convertir TOUTES les colonnes catégorielles en string
            # (y compris Doors si elle est catégorielle)
            for idx in cat_features:
                col_name = df.columns[idx]
                logger.debug(f"🔄 Conversion de '{col_name}' en string (valeur: {df[col_name].iloc[0]})")
                # Remplacer d'abord les NaN
                if df[col_name].isna().any():
                    df[col_name] = df[col_name].fillna("Unknown")
                # Convertir en string
                df[col_name] = df[col_name].astype(str)
            
            logger.debug(f"📋 Types finaux du DataFrame:")
            for col in df.columns:
                logger.debug(f"  - {col}: {df[col].dtype} = {df[col].iloc[0]}")
            
            # Prédiction avec spécification explicite des cat_features
            from catboost import Pool
            pool = Pool(data=df, cat_features=cat_features)
            predicted_price_log = model.predict(pool)[0]
            predicted_price = np.expm1(predicted_price_log)  # Transformation inverse du log1p
            
            # Extraction des facteurs d'influence
            top_factors = self._extract_feature_importance(model, df)
            
            logger.info(f"✅ Prix prédit: {predicted_price:.2f}€")
            
            return ResultatPrediction(
                success=True,
                predicted_price=float(predicted_price),
                currency=DEFAULT_CURRENCY,
                confidence_score=DEFAULT_CONFIDENCE_SCORE,
                top_influencing_factors=top_factors
            )
        
        except Exception as e:
            logger.error(f"❌ Erreur lors de la prédiction de prix: {e}", exc_info=True)
            return ResultatPrediction(
                success=False,
                error=str(e)
            )
    
    def _prepare_dataframe(self, data: DataPrediction) -> pd.DataFrame:
        """
        Prépare un DataFrame avec feature engineering
        IMPORTANT: Calcule automatiquement les features engineerées
        """
        # ===== EXTRACTION ET NETTOYAGE =====
        prod_year = data.prod_year if data.prod_year else 2015
        
        # Nettoyer le kilométrage
        mileage_str = str(data.mileage) if data.mileage else "100000"
        mileage = float(mileage_str.replace('km', '').replace(',', '').replace(' ', '').strip())
        
        cylinders = data.cylinders if data.cylinders else 4.0
        engine_volume = data.engine_volume if data.engine_volume else 2.0
        manufacturer = (data.manufacturer.upper() or "TOYOTA").strip()
        airbags = data.airbags if data.airbags else 4
        
        # ===== FEATURE ENGINEERING =====
        current_year = 2025
        
        # 1. Car_Age (âge de la voiture)
        car_age = current_year - prod_year
        car_age = max(0, min(car_age, 50))  # Borner entre 0 et 50 ans
        
        # 2. Mileage_per_Year (kilométrage moyen par an)
        mileage_per_year = mileage / (car_age + 1)
        
        # 3. Age_Category (catégorie d'âge)
        if car_age <= 3:
            age_category = "New"
        elif car_age <= 7:
            age_category = "Recent"
        elif car_age <= 15:
            age_category = "Used"
        else:
            age_category = "Old"
        
        # 4. Usage_Category (catégorie d'usage)
        if mileage_per_year <= 10000:
            usage_category = "Low"
        elif mileage_per_year <= 20000:
            usage_category = "Medium"
        elif mileage_per_year <= 50000:
            usage_category = "High"
        else:
            usage_category = "VeryHigh"
        
        # 5. Is_Premium (marque premium)
        premium_brands = ["LEXUS", "MERCEDES-BENZ", "MERCEDES", "BMW", "AUDI", "PORSCHE", "LAND", "LAMBORGHINI", "MASERATI", "JAGUAR", "ROLLS-ROYCE", "BENTLEY", "FERRARI", "BUGATTI"]
        is_premium = 1 if any(brand in manufacturer.upper() for brand in premium_brands) else 0
        
        # 6. Engine_Efficiency (efficacité moteur)
        engine_efficiency = engine_volume / (cylinders + 0.1)
        
        # Extraire doors comme numérique (pas de string)
        doors_str = str(data.doors or "4").replace('-', '').strip()
        try:
            doors = float(doors_str)
        except:
            doors = 4.0
        
        # ===== CRÉATION DU DATAFRAME COMPLET =====
        return pd.DataFrame([{
            # Features originales du dataset
            "Levy": 0,  # Taxe (souvent absente, mettre 0 par défaut)
            "Manufacturer": manufacturer,
            "Model": (data.model or "Unknown").strip(),
            "Prod. year": prod_year,
            "Category": (data.category or "Sedan").strip(),
            "Leather interior": 1 if data.leather_interior == "Yes" else 0,
            "Fuel type": (data.fuel_type or "Petrol").strip(),
            "Engine volume": engine_volume,
            "Mileage": mileage,
            "Cylinders": cylinders,
            "Gear box type": (data.gear_box_type or "Manual").strip(),
            "Drive wheels": (data.drive_wheels or "Front").strip(),
            "Doors": doors,
            "Wheel": (data.wheel or "Left wheel").strip(),
            "Color": (data.color or "Black").strip(),
            "Airbags": airbags,
            
            # Feature manquante détectée
            "Turbo": 0,  # Par défaut, pas de turbo
            
            # Features engineerées (calculées automatiquement)
            "Car_Age": car_age,
            "Mileage_per_Year": mileage_per_year,
            "Age_Category": age_category,
            "Usage_Category": usage_category,
            "Is_Premium": is_premium,
            "Engine_Efficiency": engine_efficiency
        }])
    
    def _extract_feature_importance(
        self,
        model,
        df: pd.DataFrame
    ) -> List[InfluencingFactor]:
        """
        Extrait les facteurs d'influence les plus importants
        
        Args:
            model: Modèle CatBoost
            df: DataFrame des features
            
        Returns:
            Liste des 5 facteurs les plus importants
        """
        try:
            # Pour CatBoost, récupérer directement les feature importances
            feature_names = df.columns.tolist()
            importances = model.get_feature_importance()
            
            # Top 5 features
            sorted_idx = np.argsort(importances)[::-1][:5]
            
            return [
                InfluencingFactor(
                    feature=feature_names[i],
                    importance=float(importances[i])
                )
                for i in sorted_idx
            ]
        
        except Exception as e:
            logger.warning(f"Impossible d'extraire les importances: {e}")
            return []