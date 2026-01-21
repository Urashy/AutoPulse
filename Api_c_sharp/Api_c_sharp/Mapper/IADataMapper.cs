namespace Api_c_sharp.Mapper;

public static class IADataMapper
    {
        /// <summary>
        /// Convertit un type de boîte de vitesse français en anglais
        /// </summary>
        public static string TranslateGearBoxType(string frenchValue)
        {
            if (string.IsNullOrWhiteSpace(frenchValue))
                return "Manual";

            var key = frenchValue.Trim().ToLower();
            
            return key switch
            {
                "manuelle" => "Manual",
                "automatique" => "Automatic",
                "séquentielle" => "Tiptronic",
                "robotisée" => "Tiptronic",
                "variateur" => "Variator",
                "manual" => "Manual",
                "automatic" => "Automatic",
                "tiptronic" => "Tiptronic",
                "variator" => "Variator",
                _ => "Manual"
            };
        }

        /// <summary>
        /// Convertit un type de motricité français en anglais
        /// </summary>
        public static string TranslateDriveWheels(string frenchValue)
        {
            if (string.IsNullOrWhiteSpace(frenchValue))
                return "Front";

            var key = frenchValue.Trim().ToLower();
            
            return key switch
            {
                "traction" => "Front",
                "avant" => "Front",
                "propulsion" => "Rear",
                "arrière" => "Rear",
                "4x4" => "4x4",
                "intégrale" => "4x4",
                "4wd" => "4x4",
                "awd" => "4x4",
                "front" => "Front",
                "rear" => "Rear",
                _ => "Front"
            };
        }

        /// <summary>
        /// Convertit un type de carburant français en anglais
        /// </summary>
        public static string TranslateFuelType(string frenchValue)
        {
            if (string.IsNullOrWhiteSpace(frenchValue))
                return "Petrol";

            var key = frenchValue.Trim().ToLower();
            
            return key switch
            {
                "essence" => "Petrol",
                "diesel" => "Diesel",
                "hybride" => "Hybrid",
                "électrique" => "Hybrid",
                "gpl" => "LPG",
                "gnv" => "CNG",
                "petrol" => "Petrol",
                "lpg" => "LPG",
                "cng" => "CNG",
                _ => "Petrol"
            };
        }

        /// <summary>
        /// Convertit une catégorie de véhicule française en anglais
        /// </summary>
        public static string TranslateCategory(string frenchValue)
        {
            if (string.IsNullOrWhiteSpace(frenchValue))
                return "Sedan";

            var key = frenchValue.Trim().ToLower();
            
            return key switch
            {
                "sport" => "Coupe",
                "sportive" => "Coupe",
                "berline" => "Sedan",
                "suv" => "Jeep",
                "4x4" => "Jeep",
                "citadine" => "Hatchback",
                "compacte" => "Hatchback",
                "break" => "Goods wagon",
                "familiale" => "Goods wagon",
                "monospace" => "Microbus",
                "utilitaire" => "Microbus",
                "coupé" => "Coupe",
                "cabriolet" => "Coupe",
                "roadster" => "Coupe",
                "sedan" => "Sedan",
                "jeep" => "Jeep",
                "hatchback" => "Hatchback",
                "coupe" => "Coupe",
                "goods wagon" => "Goods wagon",
                "microbus" => "Microbus",
                _ => "Sedan"
            };
        }

        /// <summary>
        /// Convertit une couleur française en anglais
        /// </summary>
        public static string TranslateColor(string frenchValue)
        {
            if (string.IsNullOrWhiteSpace(frenchValue))
                return "Black";

            var key = frenchValue.Trim().ToLower();
            
            return key switch
            {
                "noir" => "Black",
                "blanc" => "White",
                "gris" => "Grey",
                "argent" => "Silver",
                "rouge" => "Red",
                "bleu" => "Blue",
                "vert" => "Green",
                "jaune" => "Yellow",
                "orange" => "Orange",
                "marron" => "Brown",
                "beige" => "Beige",
                "or" => "Gold",
                "violet" => "Purple",
                "rose" => "Pink",
                "gris foncé" => "Grey",
                "gris clair" => "Grey",
                "bleu foncé" => "Blue",
                "bleu clair" => "Blue",
                "vert foncé" => "Green",
                "rouge foncé" => "Red",
                "black" => "Black",
                "white" => "White",
                "grey" => "Grey",
                "silver" => "Silver",
                "red" => "Red",
                "blue" => "Blue",
                "green" => "Green",
                "yellow" => "Yellow",
                "brown" => "Brown",
                "gold" => "Gold",
                _ => "Black"
            };
        }
    }