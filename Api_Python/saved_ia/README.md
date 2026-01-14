# Dossier pour les modèles entraînés

Placer les fichiers suivants dans ce dossier:

1. **rf_price_model.joblib** - Modèle Random Forest pour la prédiction de prix
2. **adjustment_model_api.pkl** - Modèle ML pour l'ajustement de prix
3. **car_classifier_best.pth** - Modèle CNN EfficientNetV2 pour la reconnaissance
4. **car_metadata.json** - Métadonnées du modèle de vision

## Format attendu

### adjustment_model_api.pkl
Doit contenir un dictionnaire avec les clés:
- `ml_model`: Modèle scikit-learn entraîné
- `scaler`: StandardScaler ou MinMaxScaler
- `tfidf`: TfidfVectorizer
- `svd`: TruncatedSVD
- `semantic_vocab`: Dictionnaire de vocabulaire sémantique

### car_metadata.json
Doit contenir:
- `num_classes`: Nombre de classes
- `idx_to_model`: Mapping index -> label
- `make_model_dict`: Mapping label -> nom complet
