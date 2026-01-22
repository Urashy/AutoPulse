"""
Dataset INTELLIGENT - Apprend les concepts, pas les phrases exactes
Version finale v3.3
"""

from vehicle_adjuster import SmartPriceAdjuster

# Dataset MINIMAL et GÉNÉRALISTE
# Le modèle apprend les CONCEPTS, pas les phrases exactes

descriptions_smart = [
    # === CRITIQUES (coefficient > 0.85) ===
    # Concept: Non-roulant
    "Moteur HS",
    "Boîte morte",
    "Ne démarre plus",
    "Pour pièces",
    "Épave",
    
    # Concept: Cumul de problèmes majeurs
    "Embrayage mort et moteur usé",
    "Mal entretenue plusieurs problèmes",
    
    # === GRAVES (0.65-0.85) ===
    # Concept: Problème mécanique majeur unique
    "Embrayage mort",
    "Moteur qui fume beaucoup",
    "Turbo HS",
    "Accidentée grave",
    "Rouille importante châssis",
    
    # === MODÉRÉS (0.30-0.60) ===
    # Concept: Plusieurs défauts mineurs ou un défaut moyen
    "Plusieurs rayures",
    "Pas mal de bosses",
    "Aucun entretien",  # Seul = modéré
    "Optiques à changer peinture abîmée",
    "Usure importante",
    
    # === MINEURS (0.10-0.30) ===
    # Concept: Défauts superficiels légers
    "Légère rayure",
    "Petite bosse",
    "Micro défauts",
    "Traces d'usure",
    
    # === EXCELLENTS (< 0.10) ===
    # Concept: Parfait état
    "Parfait état",
    "Comme neuve",
    "État impeccable",
    "Jamais accidentée entretien suivi",
]

coefficients_smart = [
    # CRITIQUES
    0.95, 0.96, 0.97, 0.98, 1.00,
    0.92, 0.90,
    
    # GRAVES
    0.72, 0.75, 0.70, 0.80, 0.78,
    
    # MODÉRÉS
    0.50, 0.52, 0.45, 0.48, 0.55,
    
    # MINEURS
    0.20, 0.18, 0.15, 0.22,
    
    # EXCELLENTS
    0.02, 0.01, 0.01, 0.03,
]

def train_smart_model():
    """Entraîner avec dataset intelligent"""
    print("="*70)
    print("🎓 ENTRAÎNEMENT INTELLIGENT - Généralisation ML")
    print("="*70)
    print(f"\n📊 {len(descriptions_smart)} exemples (MINIMAL)")
    print(f"🧠 Le modèle va APPRENDRE à généraliser, pas mémoriser")
    
    adjuster = SmartPriceAdjuster()
    
    # Vocabulaire optimisé (gardé du v3.2)
    adjuster.feature_extractor.semantic_vocab = {
        'critique': {
            'keywords': [
                'non roulant', 'ne roule plus', 'roule plus', 'roule pas',
                'moteur hs', 'boîte hs', 'hs',
                'moteur mort', 'boîte morte', 'mort',
                'pièces', 'épave', 'détruit',
                'ne démarre', 'hors service',
                'plusieurs problèmes', 'mal entretenue'
            ],
            'weight': 1.0
        },
        'grave': {
            'keywords': [
                'accidenté', 'accident grave',
                'rouille importante', 'châssis',
                'fume', 'qui fume',
                'embrayage mort', 'turbo', 'alternateur',
                'moteur usé', 'problème sérieux'
            ],
            'weight': 0.7
        },
        'modere': {
            'keywords': [
                'rayure', 'rayures', 'usure', 'abîmé',
                'bosses', 'bosse', 'pas mal', 'plusieurs',
                'optique', 'peinture',
                'aucun entretien', 'pas entretenue'
            ],
            'weight': 0.4
        },
        'mineur': {
            'keywords': [
                'légère', 'léger', 'petite', 'petit',
                'micro', 'trace', 'traces'
            ],
            'weight': 0.15
        },
        'positif': {
            'keywords': [
                'neuf', 'neuve', 'excellent', 'impeccable',
                'parfait', 'parfaite', 'comme neuf',
                'entretien suivi', 'jamais accidenté',
                'bon état', 'roule bien'
            ],
            'weight': -0.3
        }
    }
    
    # Entraîner
    adjuster.train(descriptions_smart, coefficients_smart, verbose=True)
    
    # Sauvegarder
    adjuster.save_model("smart_adjuster_v33_intelligent.pkl")
    print("\n💾 Modèle intelligent sauvegardé!")
    
    return adjuster

def test_generalization(adjuster):
    """Tester la GÉNÉRALISATION - phrases jamais vues"""
    print("\n" + "="*70)
    print("🧪 TEST DE GÉNÉRALISATION")
    print("="*70)
    print("Ces phrases N'ÉTAIENT PAS dans l'entraînement !")
    print("-"*70)
    
    # CAS JAMAIS VUS - le modèle doit généraliser
    test_cases = [
        # Variations du cas de ton ami
        ("aucun entretien, embrayage mort, pneus usés", 40000, "🔴", 35, 45),
        ("pas entretenue, embrayage HS, moteur usé", 40000, "🔴", 35, 45),
        ("mal entretenue, plusieurs soucis mécaniques", 40000, "🟠", 15, 25),
        
        # Autres variations
        ("embrayage cassé à changer", 40000, "🟠", 15, 20),
        ("moteur qui fait du bruit bizarre", 40000, "🟠", 15, 20),
        ("nombreuses rayures et bosses", 40000, "🟡", 5, 10),
        ("excellent état jamais de problème", 40000, "✅", 0, 3),
        ("elle démarre plus du tout", 40000, "🔴", 40, 50),
        
        # Cas complexes
        ("turbo mort, pneus usés, pas d'entretien", 40000, "🟠", 18, 28),
        ("micro rayures rien de grave", 40000, "🟢", 1, 5),
    ]
    
    print("\n")
    passed = 0
    for desc, prix, expected_emoji, min_red, max_red in test_cases:
        result = adjuster.predict(desc, prix)
        actual_emoji = result.category.split()[0]
        red = result.reduction_percent
        
        category_ok = actual_emoji == expected_emoji
        reduction_ok = min_red <= red <= max_red
        
        if category_ok and reduction_ok:
            status = "✅"
            passed += 1
        elif category_ok:
            status = "⚠️ "
        else:
            status = "❌"
        
        print(f"{status} \"{desc[:50]}\"")
        print(f"   Attendu: {expected_emoji} ({min_red}-{max_red}%)")
        print(f"   Obtenu:  {result.category} (-{red:.1f}%)")
        print()
    
    print("="*70)
    print(f"📊 Généralisation: {passed}/{len(test_cases)} ({passed/len(test_cases)*100:.0f}%)")
    print("="*70)
    
    return passed / len(test_cases)

def test_specific_case(adjuster):
    """Tester le cas exact de ton ami"""
    print("\n" + "="*70)
    print("🎯 CAS SPÉCIFIQUE - Ton ami")
    print("="*70)
    
    desc = "aucun entretient, temps de chauffe jamais respecté, pneu a prévoir, embrayge mort, trace de cyprine a l'arrière"
    prix = 40000
    
    result = adjuster.predict(desc, prix)
    
    print(f"\n📝 {desc}")
    print(f"\n💰 Prix: {prix:,}€")
    print(f"📊 Catégorie: {result.category}")
    print(f"📉 Réduction: {result.reduction_percent:.2f}% (-{result.reduction_amount:,.0f}€)")
    print(f"💵 Prix final: {result.adjusted_price:,.0f}€")
    
    if result.reduction_percent >= 35:
        print(f"\n✅ EXCELLENT - Réduction correcte (≥35%)")
        return True
    elif result.reduction_percent >= 25:
        print(f"\n⚠️  ACCEPTABLE - Réduction un peu faible (25-35%)")
        return False
    else:
        print(f"\n❌ PROBLÈME - Réduction trop faible (<25%)")
        return False

if __name__ == "__main__":
    print("\n🚀 ENTRAÎNEMENT INTELLIGENT v3.3")
    print("="*70)
    print("\n💡 PHILOSOPHIE:")
    print("   • Dataset MINIMAL (30 exemples)")
    print("   • Apprend les CONCEPTS, pas les phrases")
    print("   • ML fait le travail de GÉNÉRALISATION")
    print("   • Vocabulaire sémantique fait le reste")
    print("="*70)
    
    # Entraîner
    adjuster = train_smart_model()
    
    # Tester la généralisation
    gen_score = test_generalization(adjuster)
    
    # Tester le cas spécifique
    specific_ok = test_specific_case(adjuster)
    
    print("\n" + "="*70)
    print("📊 RÉSULTATS FINAUX")
    print("="*70)
    print(f"🎯 Généralisation: {gen_score*100:.0f}%")
    print(f"✅ Cas spécifique: {'OK' if specific_ok else 'À améliorer'}")
    
    if gen_score >= 0.7 and specific_ok:
        print("\n🎉 MODÈLE PRÊT POUR LA PRODUCTION !")
        print("\n💡 Pour déployer:")
        print("   python convert_model_api.py smart_adjuster_v33_intelligent.pkl adjustment_model_api.pkl")
    else:
        print("\n⚠️  Modèle à affiner - Ajouter quelques exemples ciblés")
    
    print("="*70)
