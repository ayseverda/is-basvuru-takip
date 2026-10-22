namespace is_basvuru_takip.Dtos
{
    public class IceAktarSonucDto
    {
        public int Eklenen {  get; set; }
        public int Atlanan { get; set; }
        public int Hatali { get; set; }
        public List<string> Hatalar { get; set; } = new();
    }
}
