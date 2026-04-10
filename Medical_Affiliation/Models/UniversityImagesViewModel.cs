namespace Medical_Affiliation.Models
{
    public class UniversityImagesViewModel
    {
        public int ImgId { get; set; }
        public string ImgName { get; set; }
        public IFormFile ImgFile { get; set; }
        public byte[] ImgByteArr { get; set; }
    }
}
