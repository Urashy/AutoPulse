"""
Script de diagnostic rapide pour vérifier vos modèles

Usage:
    python diagnose_models.py
"""
import sys
from pathlib import Path


def check_file(path: str, name: str) -> bool:
    """Vérifie si un fichier existe"""
    p = Path(path)
    if p.exists():
        size_mb = p.stat().st_size / (1024 * 1024)
        print(f"   ✅ {name}: {size_mb:.2f} MB")
        return True
    else:
        print(f"   ❌ {name}: MANQUANT")
        return False


def check_vision_checkpoint(path: str):
    """Vérifie le format du checkpoint vision"""
    try:
        import torch
        checkpoint = torch.load(path, map_location='cpu')
        
        has_model = 'model' in checkpoint
        has_model_state_dict = 'model_state_dict' in checkpoint
        
        if has_model or has_model_state_dict:
            format_name = []
            if has_model:
                format_name.append("'model'")
            if has_model_state_dict:
                format_name.append("'model_state_dict'")
            
            print(f"      Format: {' + '.join(format_name)} ✅")
            
            # Vérifier métadonnées
            if 'metadata' in checkpoint:
                metadata = checkpoint['metadata']
                num_classes = metadata.get('num_classes', 'N/A')
                print(f"      Classes: {num_classes}")
            else:
                print(f"      ⚠️  Pas de métadonnées dans le checkpoint")
        else:
            print(f"      ❌ Format invalide (ni 'model' ni 'model_state_dict')")
            print(f"      Clés: {list(checkpoint.keys())}")
            
    except Exception as e:
        print(f"      ❌ Erreur: {e}")


def check_adjustment_model(path: str):
    """Vérifie le format du modèle d'ajustement"""
    try:
        import joblib
        model = joblib.load(path)
        
        required = ['ml_model', 'scaler', 'tfidf', 'svd', 'semantic_vocab']
        missing = [k for k in required if k not in model]
        
        if not missing:
            print(f"      Format: Complet ✅")
            print(f"      Composants: {', '.join(required)}")
        else:
            print(f"      ❌ Clés manquantes: {missing}")
            
    except Exception as e:
        print(f"      ❌ Erreur: {e}")


def main():
    print("="*70)
    print("🔍 DIAGNOSTIC DES MODÈLES")
    print("="*70)
    
    base_dir = Path('saved_ia')
    
    print(f"\n📁 Dossier: {base_dir.absolute()}")
    
    if not base_dir.exists():
        print(f"\n❌ Le dossier {base_dir} n'existe pas!")
        print(f"   Créez-le avec: mkdir {base_dir}")
        sys.exit(1)
    
    print("\n📋 Fichiers requis:")
    
    # 1. Modèle de prix
    price_path = base_dir / 'rf_price_model.joblib'
    has_price = check_file(str(price_path), "Prix (Random Forest)")
    
    # 2. Modèle d'ajustement
    adj_path = base_dir / 'adjustment_model_api.pkl'
    has_adj = check_file(str(adj_path), "Ajustement (ML)")
    if has_adj:
        check_adjustment_model(str(adj_path))
    
    # 3. Modèle vision
    vision_paths = [
        base_dir / 'car_classifier_best.pth',
        base_dir / 'best.pth'
    ]
    
    has_vision = False
    for vision_path in vision_paths:
        if vision_path.exists():
            has_vision = True
            size_mb = vision_path.stat().st_size / (1024 * 1024)
            print(f"   ✅ Vision (CNN): {size_mb:.2f} MB ({vision_path.name})")
            check_vision_checkpoint(str(vision_path))
            break
    
    if not has_vision:
        print(f"   ❌ Vision (CNN): MANQUANT")
        print(f"      Cherché: {[p.name for p in vision_paths]}")
    
    # 4. Métadonnées vision
    meta_path = base_dir / 'car_metadata.json'
    has_meta = check_file(str(meta_path), "Métadonnées vision")
    
    if has_meta:
        try:
            import json
            with open(meta_path) as f:
                meta = json.load(f)
            
            num_classes = meta.get('num_classes', 'N/A')
            has_idx_to_model = 'idx_to_model' in meta
            has_make_model = 'make_model_dict' in meta
            
            print(f"      Classes: {num_classes}")
            print(f"      idx_to_model: {'✅' if has_idx_to_model else '❌'}")
            print(f"      make_model_dict: {'✅' if has_make_model else '❌'}")
        except Exception as e:
            print(f"      ❌ Erreur lecture: {e}")
    
    # Résumé
    print("\n" + "="*70)
    print("📊 RÉSUMÉ:")
    
    all_ok = has_price and has_adj and has_vision and has_meta
    
    if all_ok:
        print("✅ Tous les modèles sont présents!")
        print("\n💡 Prochaine étape:")
        print("   python main.py")
    else:
        print("⚠️  Certains modèles sont manquants:")
        
        if not has_price:
            print("   ❌ rf_price_model.joblib")
        if not has_adj:
            print("   ❌ adjustment_model_api.pkl")
            print("      → Lancez: python retrain_adjustment_model.py")
        if not has_vision:
            print("   ❌ car_classifier_best.pth (ou best.pth)")
        if not has_meta:
            print("   ❌ car_metadata.json")
        
        print("\n💡 Consultez TROUBLESHOOTING.md pour plus d'aide")
    
    print("="*70)


if __name__ == "__main__":
    main()
