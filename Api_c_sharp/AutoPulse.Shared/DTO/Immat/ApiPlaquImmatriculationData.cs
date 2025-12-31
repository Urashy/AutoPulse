namespace AutoPulse.Shared.DTO.Immat;

public class ApiPlaquImmatriculationData
{
    public string? Erreur { get; set; }
        public string? Immat { get; set; }
        public string? Pays { get; set; }
        public string? Marque { get; set; }
        public string? Modele { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("date1erCir_us")]
        public string? Date1erCirUs { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("date1erCir_fr")]
        public string? Date1erCirFr { get; set; }
        
        public string? Co2 { get; set; }
        public string? Energie { get; set; }
        public string? EnergieNGC { get; set; }
        public string? GenreVCG { get; set; }
        public string? GenreVCGNGC { get; set; }
        public string? PuisFisc { get; set; }
        public string? CarrosserieCG { get; set; }
        public string? PuisFiscReelKW { get; set; }
        public string? PuisFiscReelCH { get; set; }
        public string? Collection { get; set; }
        public string? Date30 { get; set; }
        public string? Vin { get; set; }
        public string? Variante { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("boite_vitesse")]
        public string? BoiteVitesse { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("code_boite_vitesse")]
        public string? CodeBoiteVitesse { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("nr_passagers")]
        public string? NrPassagers { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("nb_portes")]
        public string? NbPortes { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("type_mine")]
        public string? TypeMine { get; set; }
        
        public string? Cnit { get; set; }
        public string? Couleur { get; set; }
        public string? Poids { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("capacite_litres")]
        public string? Ccm { get; set; }
        public string? Cylindres { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("sra_id")]
        public string? SraId { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("sra_group")]
        public string? SraGroup { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("sra_commercial")]
        public string? SraCommercial { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("numero_serie")]
        public string? NumeroSerie { get; set; }
        
        public string? Ptac { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("logo_marque")]
        public string? LogoMarque { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("k_type")]
        public string? KType { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("tecdoc_manuid")]
        public string? TecdocManuid { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("tecdoc_modelid")]
        public string? TecdocModelid { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("tecdoc_carid")]
        public string? TecdocCarid { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("code_moteur")]
        public string? CodeMoteur { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("codes_platforme")]
        public string? CodesPlatforme { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("nbr_req_restants")]
        public int? NbrReqRestants { get; set; }
}