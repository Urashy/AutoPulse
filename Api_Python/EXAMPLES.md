# Exemples d'utilisation - API IA Automobile

## 🚀 Démarrage rapide

### Lancement de l'API
```bash
# Installation
pip install -r requirements.txt

# Lancement
python main.py
```

L'API sera disponible sur: http://localhost:8000

## 📡 Exemples de requêtes

### 1. Health Check

**cURL:**
```bash
curl http://localhost:8000/health
```

**Python:**
```python
import requests

response = requests.get("http://localhost:8000/health")
print(response.json())
```

**Réponse:**
```json
{
  "status": "healthy",
  "models": {
    "price_model": "loaded",
    "adjustment_model": "loaded",
    "vision_model": "loaded (cuda, 196 classes)"
  }
}
```

---

### 2. Prédiction de prix (Prediction)

**cURL:**
```bash
curl -X POST http://localhost:8000/predict \
  -H "Content-Type: application/json" \
  -d '{
    "type": "prediction",
    "data": {
      "type": "prediction",
      "manufacturer": "Toyota",
      "model": "Camry",
      "prod_year": 2020,
      "category": "Sedan",
      "leather_interior": "Yes",
      "fuel_type": "Petrol",
      "engine_volume": 2.5,
      "mileage": "50000",
      "cylinders": 4.0,
      "gear_box_type": "Automatic",
      "drive_wheels": "Front",
      "doors": "04-May",
      "wheel": "Left wheel",
      "color": "Silver",
      "airbags": 12,
      "levy": 0
    }
  }'
```

**Python:**
```python
import requests

data = {
    "type": "prediction",
    "data": {
        "type": "prediction",
        "manufacturer": "Toyota",
        "model": "Camry",
        "prod_year": 2020,
        "category": "Sedan",
        "leather_interior": "Yes",
        "fuel_type": "Petrol",
        "engine_volume": 2.5,
        "mileage": "50000",
        "cylinders": 4.0,
        "gear_box_type": "Automatic",
        "drive_wheels": "Front",
        "doors": "04-May",
        "wheel": "Left wheel",
        "color": "Silver",
        "airbags": 12,
        "levy": 0
    }
}

response = requests.post(
    "http://localhost:8000/predict",
    json=data
)

result = response.json()
print(f"Prix prédit: {result['predicted_price']}€")
print(f"Confiance: {result['confidence_score']}")
```

**Réponse:**
```json
{
  "type": "prediction",
  "success": true,
  "error": null,
  "predicted_price": 18500.50,
  "currency": "€",
  "confidence_score": 0.85,
  "top_influencing_factors": [
    {
      "feature": "Prod. year",
      "importance": 0.234
    },
    {
      "feature": "Engine volume",
      "importance": 0.187
    },
    {
      "feature": "Mileage",
      "importance": 0.156
    }
  ]
}
```

---

### 3. Ajustement de prix (Ajustement)

**cURL:**
```bash
curl -X POST http://localhost:8000/predict \
  -H "Content-Type: application/json" \
  -d '{
    "type": "ajustement",
    "data": {
      "type": "ajustement",
      "base_price": 15000,
      "description": "Voiture en excellent état, entretien régulier chez le concessionnaire, carnet à jour, pneus neufs. Petit accroc sur le pare-choc avant."
    }
  }'
```

**Python:**
```python
import requests

data = {
    "type": "ajustement",
    "data": {
        "type": "ajustement",
        "base_price": 15000,
        "description": "Voiture en excellent état, entretien régulier"
    }
}

response = requests.post(
    "http://localhost:8000/predict",
    json=data
)

result = response.json()
print(f"Prix de base: {result['base_price']}€")
print(f"Prix ajusté: {result['adjusted_price']}€")
print(f"Réduction: {result['reduction_percent']}%")
print(f"Catégorie: {result['category']}")
```

**Réponse:**
```json
{
  "type": "ajustement",
  "success": true,
  "error": null,
  "base_price": 15000,
  "adjusted_price": 14550.0,
  "reduction_amount": 450.0,
  "reduction_percent": 3.0,
  "quality_coefficient": 0.15,
  "category": "🟢 BON - Défauts mineurs",
  "description_analyzed": "Voiture en excellent état..."
}
```

**Exemples de descriptions:**

```python
# État critique
description_critique = "Moteur HS, boîte de vitesse cassée, châssis tordu"
# → Réduction: 40-50%

# État grave
description_grave = "Gros accident, carrosserie très abîmée, embrayage à refaire"
# → Réduction: 15-20%

# État modéré
description_modere = "Quelques rayures, klaxon ne fonctionne pas, rétroviseur cassé"
# → Réduction: 5-10%

# Bon état
description_bon = "Entretien régulier, carnet à jour, pneus neufs"
# → Réduction: 0-5%
```

---

### 4. Reconnaissance visuelle (CNN)

**Python avec image locale:**
```python
import requests
import base64

# Charger et encoder l'image
with open("voiture.jpg", "rb") as image_file:
    image_base64 = base64.b64encode(image_file.read()).decode('utf-8')

data = {
    "type": "cnn",
    "data": {
        "type": "cnn",
        "image_base64": image_base64
    }
}

response = requests.post(
    "http://localhost:8000/predict",
    json=data
)

result = response.json()
print(f"Véhicule: {result['manufacturer']} {result['model']}")
print(f"Confiance: {result['confidence']}")
print("\nTop 5 prédictions:")
for pred in result['top_predictions']:
    print(f"  - {pred['manufacturer']} {pred['model']}: {pred['confidence']}")
```

**Réponse:**
```json
{
  "type": "cnn",
  "success": true,
  "error": null,
  "manufacturer": "BMW",
  "model": "X5",
  "full_name": "BMW X5",
  "confidence": "92.45%",
  "confidence_score": 0.9245,
  "image_size": "1920x1080",
  "top_predictions": [
    {
      "manufacturer": "BMW",
      "model": "X5",
      "confidence": "92.45%"
    },
    {
      "manufacturer": "BMW",
      "model": "X3",
      "confidence": "4.23%"
    },
    {
      "manufacturer": "Audi",
      "model": "Q7",
      "confidence": "1.87%"
    }
  ]
}
```

---

### 5. Rechargement des modèles

**cURL:**
```bash
curl -X POST http://localhost:8000/reload
```

**Python:**
```python
import requests

response = requests.post("http://localhost:8000/reload")
result = response.json()

print(f"Succès: {result['success']}")
print(f"Message: {result['message']}")
print("\nStatut des modèles:")
for model, status in result['models_status'].items():
    print(f"  - {model}: {status}")
```

**Réponse:**
```json
{
  "success": true,
  "message": "✅ Tous les modèles ont été rechargés avec succès",
  "models_status": {
    "price_model": "loaded",
    "adjustment_model": "loaded",
    "vision_model": "loaded (cuda, 196 classes)"
  }
}
```

---

## 🔧 Scripts complets d'utilisation

### Script Python complet
```python
#!/usr/bin/env python3
"""
Script de test complet de l'API IA Automobile
"""
import requests
import base64
import json
from pathlib import Path

BASE_URL = "http://localhost:8000"

class APIClient:
    def __init__(self, base_url: str):
        self.base_url = base_url
    
    def health_check(self):
        """Vérifie l'état de santé de l'API"""
        response = requests.get(f"{self.base_url}/health")
        return response.json()
    
    def predict_price(self, vehicle_data: dict):
        """Prédit le prix d'un véhicule"""
        payload = {
            "type": "prediction",
            "data": {
                "type": "prediction",
                **vehicle_data
            }
        }
        response = requests.post(f"{self.base_url}/predict", json=payload)
        return response.json()
    
    def adjust_price(self, base_price: float, description: str):
        """Ajuste le prix selon la description"""
        payload = {
            "type": "ajustement",
            "data": {
                "type": "ajustement",
                "base_price": base_price,
                "description": description
            }
        }
        response = requests.post(f"{self.base_url}/predict", json=payload)
        return response.json()
    
    def recognize_vehicle(self, image_path: str):
        """Reconnaît un véhicule depuis une image"""
        with open(image_path, "rb") as f:
            image_b64 = base64.b64encode(f.read()).decode()
        
        payload = {
            "type": "cnn",
            "data": {
                "type": "cnn",
                "image_base64": image_b64
            }
        }
        response = requests.post(f"{self.base_url}/predict", json=payload)
        return response.json()
    
    def reload_models(self):
        """Recharge tous les modèles"""
        response = requests.post(f"{self.base_url}/reload")
        return response.json()

# Utilisation
if __name__ == "__main__":
    client = APIClient(BASE_URL)
    
    # 1. Health check
    print("🏥 Health Check...")
    health = client.health_check()
    print(json.dumps(health, indent=2))
    
    # 2. Prédiction de prix
    print("\n💰 Prédiction de prix...")
    vehicle = {
        "manufacturer": "Toyota",
        "model": "Camry",
        "prod_year": 2020,
        "category": "Sedan",
        "fuel_type": "Petrol",
        "engine_volume": 2.5,
        "mileage": "50000"
    }
    price_result = client.predict_price(vehicle)
    print(f"Prix prédit: {price_result['predicted_price']}€")
    
    # 3. Ajustement de prix
    print("\n🔧 Ajustement de prix...")
    adj_result = client.adjust_price(
        15000,
        "Excellent état, entretien régulier"
    )
    print(f"Prix ajusté: {adj_result['adjusted_price']}€")
    print(f"Catégorie: {adj_result['category']}")
    
    # 4. Reconnaissance (si image disponible)
    # vision_result = client.recognize_vehicle("test_car.jpg")
    # print(f"Véhicule: {vision_result['full_name']}")
```

---

## 🌐 Intégration JavaScript (Frontend)

### Vanilla JavaScript
```javascript
const API_URL = 'http://localhost:8000';

// Prédiction de prix
async function predictPrice(vehicleData) {
  const response = await fetch(`${API_URL}/predict`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      type: 'prediction',
      data: {
        type: 'prediction',
        ...vehicleData
      }
    })
  });
  
  return await response.json();
}

// Ajustement de prix
async function adjustPrice(basePrice, description) {
  const response = await fetch(`${API_URL}/predict`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      type: 'ajustement',
      data: {
        type: 'ajustement',
        base_price: basePrice,
        description: description
      }
    })
  });
  
  return await response.json();
}

// Reconnaissance visuelle
async function recognizeVehicle(imageFile) {
  // Convertir en base64
  const base64 = await new Promise((resolve) => {
    const reader = new FileReader();
    reader.onloadend = () => resolve(reader.result.split(',')[1]);
    reader.readAsDataURL(imageFile);
  });
  
  const response = await fetch(`${API_URL}/predict`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      type: 'cnn',
      data: {
        type: 'cnn',
        image_base64: base64
      }
    })
  });
  
  return await response.json();
}

// Utilisation
(async () => {
  const result = await predictPrice({
    manufacturer: 'Toyota',
    model: 'Camry',
    prod_year: 2020,
    fuel_type: 'Petrol'
  });
  
  console.log('Prix prédit:', result.predicted_price);
})();
```

---

## 📱 Intégration dans une application C#

```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AIAutomobileClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:8000";
    
    public AIAutomobileClient()
    {
        _httpClient = new HttpClient();
    }
    
    public async Task<PriceResult> PredictPriceAsync(VehicleData vehicle)
    {
        var payload = new
        {
            type = "prediction",
            data = new
            {
                type = "prediction",
                manufacturer = vehicle.Manufacturer,
                model = vehicle.Model,
                prod_year = vehicle.Year,
                fuel_type = vehicle.FuelType
                // ... autres champs
            }
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        
        var response = await _httpClient.PostAsync(
            $"{BaseUrl}/predict",
            content
        );
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PriceResult>(json);
    }
    
    public async Task<AdjustmentResult> AdjustPriceAsync(
        decimal basePrice,
        string description
    )
    {
        var payload = new
        {
            type = "ajustement",
            data = new
            {
                type = "ajustement",
                base_price = basePrice,
                description = description
            }
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        
        var response = await _httpClient.PostAsync(
            $"{BaseUrl}/predict",
            content
        );
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AdjustmentResult>(json);
    }
}

// Utilisation
var client = new AIAutomobileClient();

var priceResult = await client.PredictPriceAsync(new VehicleData
{
    Manufacturer = "Toyota",
    Model = "Camry",
    Year = 2020
});

Console.WriteLine($"Prix prédit: {priceResult.PredictedPrice}€");
```

---

## 🔐 Authentification (Exemple avec JWT)

Si vous souhaitez ajouter de l'authentification:

```python
# Dans main.py, ajouter:
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from fastapi import Depends, HTTPException

security = HTTPBearer()

async def verify_token(
    credentials: HTTPAuthorizationCredentials = Depends(security)
):
    token = credentials.credentials
    # Vérifier le token JWT
    if not is_valid_token(token):
        raise HTTPException(status_code=401, detail="Token invalide")
    return token

# Protéger un endpoint
@app.post("/predict", dependencies=[Depends(verify_token)])
async def predict(request: PredictRequest):
    return prediction_controller.predict(request)
```

---

**Prêt à utiliser l'API ! 🚀**
