namespace Api_c_sharp.DTO
{
    public class JournalDTO
    {
        public int IdJournal { get; set; }
        public DateTime DateJournal { get; set; }
        public string ContenuJournal { get; set; } = string.Empty;
        public int IdTypeJournal { get; set; }
        public int IdCompte { get; set; }
    }
}
