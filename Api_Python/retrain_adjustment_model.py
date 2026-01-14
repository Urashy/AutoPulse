"""
Script pour réentraîner et sauvegarder le modèle d'ajustement
au format compatible avec l'API

Usage:
    python retrain_adjustment_model.py
    
Output:
    saved_ia/adjustment_model_api.pkl (compatible avec l'API)
"""
import numpy as np
from sklearn.ensemble import GradientBoostingRegressor
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.preprocessing import StandardScaler
from sklearn.decomposition import TruncatedSVD
import joblib
from typing import List


def train_adjustment_model():
    """Entraîne le modèle avec les mêmes données que dans votre code original"""
    
    print("="*70)
    print("🎓 ENTRAÎNEMENT DU MODÈLE D'AJUSTEMENT")
    print("="*70)
    
    # Dataset d'entraînement (même que votre démo)
    train_descriptions = [
        "Petite rayure sur le pare-choc avant, bon état général",
        "Voiture accidentée, aile cabossée, peinture à refaire complètement",
        "Moteur refait à neuf, entretien à jour, intérieur impeccable",
        "Traces de rouille sous les portes, quelques chocs légers",
        "Véhicule de collection, jamais accidenté, état neuf",
        "Moteur HS, boîte de vitesse défectueuse, véhicule non roulant",
        "Léger choc à l'arrière, pneus neufs, révision complète récente",
        "Pare-choc à changer, optiques rayés, intérieur usé",
        "Véhicule pour pièces, ne démarre plus, épave",
        "Excellent état, entretien suivi, garantie constructeur valide",
        "Accident grave, châssis tordu, structure compromise",
        "Quelques micro-rayures superficielles, rien de grave",
        "Rouille importante sur les ailes, carrosserie endommagée",
        "Comme neuf, aucun défaut visible, état impeccable",
        "Plusieurs défauts cosmétiques, mais mécanique OK",
        "Véhicule totalement détruit, structure compromise",
        "Entretien régulier effectué, parfait état de marche",
        "Usure normale pour l'âge, quelques traces d'utilisation",
        "Boîte de vitesse cassée, ne passe plus les rapports",
        "État impeccable, révision complète récente, garantie",
        "Pas mal de rayures et bosses, état moyen",
        "Moteur excellent, jamais de problème mécanique",
        "Carrosserie très abîmée, nombreux chocs",
        "Véhicule bien entretenu, aucun accident, parfait",
        "Rouille perforante, chassis endommagé sérieusement"
    ]
    
    train_coefficients = [
        0.15, 0.75, 0.05, 0.45, 0.00, 1.00, 0.18, 0.60, 1.00, 0.02,
        0.95, 0.10, 0.70, 0.00, 0.35, 1.00, 0.05, 0.22, 0.90, 0.02,
        0.50, 0.08, 0.65, 0.03, 0.85
    ]
    
    print(f"📊 Nombre d'exemples: {len(train_descriptions)}")
    
    # === 1. TF-IDF Vectorizer ===
    print("\n📝 Étape 1: TF-IDF Vectorizer")
    tfidf = TfidfVectorizer(
        max_features=100,
        ngram_range=(1, 3),
        analyzer='word',
        lowercase=True,
        strip_accents='unicode'
    )
    tfidf_features = tfidf.fit_transform(train_descriptions)
    print(f"   ✅ Features TF-IDF: {tfidf_features.shape}")
    
    # === 2. SVD ===
    print("\n🔢 Étape 2: SVD (réduction dimensionnalité)")
    svd = TruncatedSVD(n_components=20, random_state=42)
    svd.fit(tfidf_features)
    print(f"   ✅ Composantes SVD: {svd.n_components}")
    
    # === 3. Vocabulaire sémantique ===
    print("\n📚 Étape 3: Vocabulaire sémantique")
    semantic_vocab = {
        'critique': {
            'keywords': [
                'non roulant', 'moteur hs', 'pièces', 'épave', 'détruit',
                'inutilisable', 'boîte hs', 'châssis tordu', 'mort', 'cassé',
                'ne démarre', 'hors service', 'grippé'
            ],
            'weight': 1.0
        },
        'grave': {
            'keywords': [
                'accidenté', 'accident', 'rouille importante', 'choc violent',
                'structure touchée', 'endommagé', 'fissure', 'corrosion',
                'cabossé', 'tordu', 'déformé'
            ],
            'weight': 0.7
        },
        'modere': {
            'keywords': [
                'rayure', 'usure', 'réparer', 'changer', 'abîmé',
                'traces', 'déchiré', 'optique', 'peinture', 'bosses'
            ],
            'weight': 0.4
        },
        'mineur': {
            'keywords': [
                'petite', 'légère', 'micro', 'quelques', 'trace',
                'petit impact', 'léger'
            ],
            'weight': 0.15
        },
        'positif': {
            'keywords': [
                'neuf', 'excellent', 'impeccable', 'parfait', 'garantie',
                'entretien', 'révision', 'refait', 'jamais accidenté',
                'comme neuf', 'bon état'
            ],
            'weight': -0.3
        }
    }
    print(f"   ✅ Catégories: {len(semantic_vocab)}")
    
    # === 4. Extraction features pour entraînement ===
    print("\n🔨 Étape 4: Extraction des features")
    X = extract_all_features(train_descriptions, tfidf, svd, semantic_vocab)
    print(f"   ✅ Shape: {X.shape}")
    
    # === 5. Normalisation ===
    print("\n⚖️  Étape 5: Normalisation")
    scaler = StandardScaler()
    X_scaled = scaler.fit_transform(X)
    
    # === 6. Entraînement du modèle ===
    print("\n🤖 Étape 6: Entraînement Gradient Boosting")
    ml_model = GradientBoostingRegressor(
        n_estimators=200,
        learning_rate=0.1,
        max_depth=5,
        min_samples_split=5,
        min_samples_leaf=2,
        random_state=42,
        subsample=0.8
    )
    
    y = np.array(train_coefficients)
    ml_model.fit(X_scaled, y)
    
    # Évaluation
    y_pred = ml_model.predict(X_scaled)
    mse = np.mean((y - y_pred) ** 2)
    mae = np.mean(np.abs(y - y_pred))
    r2 = 1 - (np.sum((y - y_pred) ** 2) / np.sum((y - np.mean(y)) ** 2))
    
    print(f"\n📈 Métriques:")
    print(f"   MSE: {mse:.4f}")
    print(f"   MAE: {mae:.4f}")
    print(f"   R²:  {r2:.4f}")
    
    # === 7. Sauvegarde au format API ===
    print("\n💾 Étape 7: Sauvegarde au format API")
    
    model_dict = {
        'ml_model': ml_model,
        'scaler': scaler,
        'tfidf': tfidf,
        'svd': svd,
        'semantic_vocab': semantic_vocab
    }
    
    output_path = 'saved_ia/adjustment_model_api.pkl'
    joblib.dump(model_dict, output_path, compress=3)
    
    print(f"   ✅ Modèle sauvegardé: {output_path}")
    
    # === 8. Test de chargement ===
    print("\n🧪 Étape 8: Test de chargement")
    loaded = joblib.load(output_path)
    
    required_keys = ['ml_model', 'scaler', 'tfidf', 'svd', 'semantic_vocab']
    for key in required_keys:
        assert key in loaded, f"Clé manquante: {key}"
        print(f"   ✅ {key}: OK")
    
    # Test de prédiction
    test_desc = "Petite rayure sur le pare-choc"
    test_features = extract_all_features([test_desc], tfidf, svd, semantic_vocab)
    test_scaled = scaler.transform(test_features)
    test_pred = ml_model.predict(test_scaled)[0]
    
    print(f"\n🎯 Test de prédiction:")
    print(f"   Description: '{test_desc}'")
    print(f"   Coefficient: {test_pred:.4f}")
    
    print("\n" + "="*70)
    print("✅ SUCCÈS - Modèle prêt pour l'API!")
    print("="*70)
    print("\n💡 Prochaine étape:")
    print("   python main.py")


def extract_all_features(descriptions: List[str], tfidf, svd, semantic_vocab) -> np.ndarray:
    """Extrait toutes les features pour une liste de descriptions"""
    import re
    from collections import Counter
    
    all_features = []
    
    for description in descriptions:
        features = {}
        desc_lower = description.lower()
        
        # === TF-IDF + SVD ===
        tfidf_vec = tfidf.transform([description])
        svd_features = svd.transform(tfidf_vec)[0]
        for i, val in enumerate(svd_features):
            features[f'svd_{i}'] = val
        
        # === Scoring sémantique ===
        for category, info in semantic_vocab.items():
            score = 0.0
            matches = 0
            for keyword in info['keywords']:
                if keyword in desc_lower:
                    score += info['weight']
                    matches += 1
            features[f'semantic_{category}_score'] = score
            features[f'semantic_{category}_matches'] = matches
        
        # === Features linguistiques ===
        words = desc_lower.split()
        features['length'] = len(description)
        features['word_count'] = len(words)
        features['avg_word_length'] = np.mean([len(w) for w in words]) if words else 0
        features['exclamation_count'] = description.count('!')
        features['question_count'] = description.count('?')
        features['comma_count'] = description.count(',')
        features['has_numbers'] = 1.0 if re.search(r'\d', description) else 0.0
        
        negation_patterns = ['pas', 'jamais', 'aucun', 'sans', 'non']
        features['negation_count'] = sum(1 for neg in negation_patterns if neg in desc_lower)
        
        intensifiers = ['très', 'extrêmement', 'vraiment', 'beaucoup']
        features['intensity'] = sum(1 for intens in intensifiers if intens in desc_lower)
        
        # === Combinaisons critiques ===
        critical_combos = [
            ('moteur', 'hs'), ('moteur', 'mort'), ('boîte', 'hs'),
            ('non', 'roulant'), ('pour', 'pièces'), ('châssis', 'tordu')
        ]
        combo_score = 0
        for word1, word2 in critical_combos:
            if word1 in desc_lower and word2 in desc_lower:
                idx1 = desc_lower.find(word1)
                idx2 = desc_lower.find(word2)
                if abs(idx1 - idx2) < 50:
                    combo_score += 1
        features['critical_combo_score'] = combo_score
        
        # === Fréquence des mots ===
        word_freq = Counter(words)
        features['repeated_words'] = sum(1 for count in word_freq.values() if count > 1)
        
        all_features.append(list(features.values()))
    
    return np.array(all_features)


if __name__ == "__main__":
    train_adjustment_model()
