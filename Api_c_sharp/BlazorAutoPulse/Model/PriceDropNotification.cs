namespace BlazorAutoPulse.Model;

public class PriceDropNotification
{
    public int IdAnnonce { get; set; }
    public string AnnonceLibelle { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public decimal Reduction { get; set; }
}