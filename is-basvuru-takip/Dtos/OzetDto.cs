namespace is_basvuru_takip.Dtos
{
    
        public class SayimDto
        {
            public int Deger { get; set; }
            public int Sayi { get; set; } 
           
        }

        public class OzetDto
        {
            public int Toplam { get; set; }
        public List<SayimDto> DurumaGore { get; set; } = new();
        public List<SayimDto> KaynagaGore { get; set; } = new();
    }
    
}
