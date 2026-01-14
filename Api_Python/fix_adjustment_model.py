"""
Script de conversion du modèle d'ajustement vers un format compatible
Résout le problème "No module named '_loss'" lors du chargement

Usage:
    python fix_adjustment_model.py input_model.pkl output_model.pkl
"""
import sys
import pickle
import joblib
from pathlib import Path


def convert_model(input_path: str, output_path: str):
    """
    Convertit un modèle vers un format plus compatible
    
    Args:
        input_path: Chemin du modèle original
        output_path: Chemin du modèle converti
    """
    print(f"🔄 Conversion de {input_path} vers {output_path}")
    
    try:
        # Tentative de chargement avec différentes méthodes
        model_dict = None
        
        # Méthode 1: pickle standard
        try:
            print("   → Essai avec pickle...")
            with open(input_path, 'rb') as f:
                model_dict = pickle.load(f)
            print("   ✅ Chargé avec pickle")
        except Exception as e:
            print(f"   ❌ Pickle échoué: {e}")
        
        # Méthode 2: joblib avec allow_pickle
        if model_dict is None:
            try:
                print("   → Essai avec joblib...")
                model_dict = joblib.load(input_path)
                print("   ✅ Chargé avec joblib")
            except Exception as e:
                print(f"   ❌ Joblib échoué: {e}")
                raise Exception("Impossible de charger le modèle")
        
        # Vérification
        required_keys = ['ml_model', 'scaler', 'tfidf', 'svd', 'semantic_vocab']
        for key in required_keys:
            if key not in model_dict:
                raise ValueError(f"Clé manquante: {key}")
        
        print(f"\n📊 Contenu du modèle:")
        print(f"   - ml_model: {type(model_dict['ml_model'])}")
        print(f"   - scaler: {type(model_dict['scaler'])}")
        print(f"   - tfidf: {type(model_dict['tfidf'])}")
        print(f"   - svd: {type(model_dict['svd'])}")
        print(f"   - semantic_vocab: {len(model_dict['semantic_vocab'])} catégories")
        
        # Sauvegarde avec joblib en version compatible
        print(f"\n💾 Sauvegarde en format compatible...")
        joblib.dump(model_dict, output_path, compress=3)
        
        # Vérification
        print(f"\n✅ Vérification du fichier converti...")
        test_model = joblib.load(output_path)
        for key in required_keys:
            assert key in test_model
        
        print(f"\n🎉 Conversion réussie!")
        print(f"   Nouveau fichier: {output_path}")
        print(f"   Taille: {Path(output_path).stat().st_size / 1024:.2f} KB")
        
    except Exception as e:
        print(f"\n❌ Erreur: {e}")
        sys.exit(1)


def main():
    if len(sys.argv) != 3:
        print("Usage: python fix_adjustment_model.py input_model.pkl output_model.pkl")
        print("\nExemple:")
        print("  python fix_adjustment_model.py saved_ia/adjustment_model_api.pkl saved_ia/adjustment_model_fixed.pkl")
        sys.exit(1)
    
    input_path = sys.argv[1]
    output_path = sys.argv[2]
    
    if not Path(input_path).exists():
        print(f"❌ Fichier non trouvé: {input_path}")
        sys.exit(1)
    
    convert_model(input_path, output_path)


if __name__ == "__main__":
    main()
