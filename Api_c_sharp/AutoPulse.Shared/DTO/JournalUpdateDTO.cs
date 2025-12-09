namespace AutoPulse.Shared.DTO
{
    public class JournalUpdateDTO
    {
        public int IdJournal { get; set; }
        public DateTime DateJournal { get; set; }
        public string ContenuJournal { get; set; } = string.Empty;
        public int IdTypeJournal { get; set; }
        public int IdCompte { get; set; }
    }
}
