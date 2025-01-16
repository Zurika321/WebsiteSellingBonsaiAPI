using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebsiteSellingBonsaiAPI.Models;
using System.ComponentModel;

namespace WebsiteSellingBonsaiAPI.DTOS.View
{
    public class BonsaiDTO : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên Bonsai không được để trống.")]
        [MinLength(1, ErrorMessage = "Tên Bonsai phải có ít nhất 1 ký tự.")]
        [MaxLength(20, ErrorMessage = "Tên Bonsai không đc quá 20 ký tự")]
        [DisplayName("Bonsai Name")]
        public string BonsaiName { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Description không đc quá 500 ký tự")]
        public string Description { get; set; }
        [MaxLength(500, ErrorMessage = "FengShuiMeaning không đc quá 500 ký tự")]
        [DisplayName("Feng Shui Meaning")]
        public string FengShuiMeaning { get; set; }

        [Range(10, 200, ErrorMessage = "Kích thước phải từ 10 đến 200.")]
        public int Size { get; set; }

        [Range(1, 100, ErrorMessage = "Tuổi cây phải nằm trong khoảng từ 1 đến 100.")]
        [DisplayName("Year Old")]
        public int YearOld { get; set; }

        [Range(0, 200, ErrorMessage = "Giá trị chỉ trong khoản 0-200.")]
        [DisplayName("Min Life")]
        public int MinLife { get; set; }

        [Range(0, 200, ErrorMessage = "Giá trị chỉ trong khoản 0-200.")]
        [DisplayName("Max Life")]
        [CustomValidation(typeof(Bonsai), nameof(ValidateLifeSpan))]
        public int MaxLife { get; set; }

        [Range(100, 100000, ErrorMessage = "Giá phải nằm trong khoảng từ 100.000 đến 100,000.000")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "Số lượng không được âm hoặc lớn hơn 100.")]
        public int Quantity { get; set; }

        public IFormFile? Image { get; set; }
        public string? ImageOld { get; set; }
        public double? nopwr { get; set; } = 0;
        public double? rates { get; set; } = 0;

        public int TypeId { get; set; }
        [ForeignKey("TypeId")]
        public BonsaiType? Type { get; set; }

        public int StyleId { get; set; }
        [ForeignKey("StyleId")]
        public Style? Style { get; set; }

        public int GeneralMeaningId { get; set; }
        [ForeignKey("GeneralMeaningId")]
        public GeneralMeaning? GeneralMeaning { get; set; }

        [DisplayName("Lượt yêu thích")]
        public int? CountFav { get; set; }
        public bool? IsFav { get; set; } = false;
        public virtual ICollection<Favourite>? Favourites { get; set; }
        public static ValidationResult? ValidateLifeSpan(object? value, ValidationContext context)
        {
            var instance = (Bonsai)context.ObjectInstance;
            if (instance.MinLife > instance.MaxLife)
            {
                return new ValidationResult("MinLife không được lớn hơn MaxLife.");
            }
            return ValidationResult.Success;
        }
    }
}
