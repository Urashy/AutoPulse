"""
Script pour convertir les formats de checkpoint PyTorch

Convertit entre les deux formats :
- Ancien : {'model_state_dict': ..., 'metadata': ...}
- Nouveau : {'model': ..., 'metadata': ...}

Usage:
    python convert_vision_checkpoint.py input.pth output.pth [--to-new|--to-old]
"""
# import torch
import sys
import argparse
from pathlib import Path


def convert_checkpoint(input_path: str, output_path: str, to_format: str = 'new'):
    """
    Convertit un checkpoint d'un format à l'autre
    
    Args:
        input_path: Chemin du checkpoint source
        output_path: Chemin du checkpoint destination
        to_format: 'new' (vers format 'model') ou 'old' (vers format 'model_state_dict')
    """
    print(f"🔄 Conversion de checkpoint PyTorch")
    print(f"   Source: {input_path}")
    print(f"   Destination: {output_path}")
    print(f"   Format cible: {to_format}")
    print()
    
    if not Path(input_path).exists():
        print(f"❌ Erreur: Fichier non trouvé: {input_path}")
        sys.exit(1)
    
    # Charger le checkpoint
    print("📥 Chargement du checkpoint...")
    # checkpoint = torch.load(input_path, map_location='cpu')
    
    # Détecter le format actuel
    has_model = 'model' in checkpoint
    has_model_state_dict = 'model_state_dict' in checkpoint
    
    if has_model and has_model_state_dict:
        print("⚠️  Le checkpoint contient les deux formats!")
        current_format = 'both'
    elif has_model:
        current_format = 'new'
        print("   Format actuel: NOUVEAU (clé 'model')")
    elif has_model_state_dict:
        current_format = 'old'
        print("   Format actuel: ANCIEN (clé 'model_state_dict')")
    else:
        print("❌ Erreur: Aucun state_dict trouvé dans le checkpoint!")
        print("   Clés disponibles:", list(checkpoint.keys()))
        sys.exit(1)
    
    # Afficher les autres clés
    other_keys = [k for k in checkpoint.keys() if k not in ['model', 'model_state_dict']]
    if other_keys:
        print(f"   Autres clés: {other_keys}")
    
    # Créer le nouveau checkpoint
    new_checkpoint = {}
    
    if to_format == 'new':
        # Conversion vers nouveau format ('model')
        if has_model:
            print("\n✅ Déjà au bon format!")
            if not has_model_state_dict:
                print("   Aucune conversion nécessaire.")
                return
            else:
                print("   Suppression de 'model_state_dict' redondant...")
                state_dict = checkpoint['model']
        else:
            print("\n🔄 Conversion: model_state_dict → model")
            state_dict = checkpoint['model_state_dict']
        
        new_checkpoint['model'] = state_dict
        
    else:  # to_format == 'old'
        # Conversion vers ancien format ('model_state_dict')
        if has_model_state_dict:
            print("\n✅ Déjà au bon format!")
            if not has_model:
                print("   Aucune conversion nécessaire.")
                return
            else:
                print("   Suppression de 'model' redondant...")
                state_dict = checkpoint['model_state_dict']
        else:
            print("\n🔄 Conversion: model → model_state_dict")
            state_dict = checkpoint['model']
        
        new_checkpoint['model_state_dict'] = state_dict
    
    # Copier les autres clés
    for key in other_keys:
        new_checkpoint[key] = checkpoint[key]
    
    # Sauvegarder
    print(f"\n💾 Sauvegarde de {output_path}...")
    torch.save(new_checkpoint, output_path)
    
    # Vérification
    print("\n✅ Vérification du fichier converti...")
    # verify = torch.load(output_path, map_location='cpu')
    
    if to_format == 'new':
        assert 'model' in verify, "Erreur: 'model' manquant!"
        print("   ✅ 'model' présent")
    else:
        assert 'model_state_dict' in verify, "Erreur: 'model_state_dict' manquant!"
        print("   ✅ 'model_state_dict' présent")
    
    # Taille
    size_mb = Path(output_path).stat().st_size / (1024 * 1024)
    print(f"   Taille: {size_mb:.2f} MB")
    
    print("\n🎉 Conversion réussie!")
    print(f"\n💡 Vous pouvez maintenant utiliser: {output_path}")


def main():
    parser = argparse.ArgumentParser(
        description="Convertir les formats de checkpoint PyTorch"
    )
    parser.add_argument(
        'input',
        help="Chemin du checkpoint source"
    )
    parser.add_argument(
        'output',
        help="Chemin du checkpoint destination"
    )
    parser.add_argument(
        '--to-new',
        action='store_true',
        help="Convertir vers nouveau format (clé 'model')"
    )
    parser.add_argument(
        '--to-old',
        action='store_true',
        help="Convertir vers ancien format (clé 'model_state_dict')"
    )
    
    args = parser.parse_args()
    
    # Déterminer le format cible
    if args.to_old:
        to_format = 'old'
    else:
        to_format = 'new'  # Par défaut
    
    convert_checkpoint(args.input, args.output, to_format)


if __name__ == "__main__":
    if len(sys.argv) == 1:
        print("Usage: python convert_vision_checkpoint.py input.pth output.pth [--to-new|--to-old]")
        print()
        print("Exemples:")
        print("  # Vers nouveau format (par défaut)")
        print("  python convert_vision_checkpoint.py old_model.pth new_model.pth")
        print()
        print("  # Vers ancien format")
        print("  python convert_vision_checkpoint.py new_model.pth old_model.pth --to-old")
        sys.exit(0)
    
    main()
