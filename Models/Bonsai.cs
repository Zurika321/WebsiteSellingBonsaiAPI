using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellingBonsaiAPI.Models
{
    [Table("Bonsais")]
    public class Bonsai : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên Bonsai không được để trống.")]
        [MinLength(1, ErrorMessage = "Tên Bonsai phải có ít nhất 1 ký tự.")]
        public string BonsaiName { get; set; } = string.Empty;

        public string Description { get; set; }

        public string FengShuiMeaning { get; set; }

        [Range(10, int.MaxValue, ErrorMessage = "Kích thước phải lớn hơn hoặc bằng 10.")]
        public int Size { get; set; }

        [Range(1, 100, ErrorMessage = "Tuổi cây phải nằm trong khoảng từ 1 đến 100.")]
        public int YearOld { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Giá trị không được âm.")]
        public int MinLife { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Giá trị không được âm.")]
        [CustomValidation(typeof(Bonsai), nameof(ValidateLifeSpan))]
        public int MaxLife { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Giá không được âm.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm.")]
        public int Quantity { get; set; }

        public string? Image { get; set; }

        public double? NOPWR { get; set; } = 0;

        public int? Rates { get; set; } = 0;

        public int TypeId { get; set; }

        [ForeignKey("TypeId")]
        public virtual BonsaiType? Type { get; set; }

        public int StyleId { get; set; }

        [ForeignKey("StyleId")]
        public virtual Style? Style { get; set; }

        public int GeneralMeaningId { get; set; }

        [ForeignKey("GeneralMeaningId")]
        public virtual GeneralMeaning? GeneralMeaning { get; set; }
        public virtual ICollection<Favourite> Favourites { get; set; }

        public static ValidationResult? ValidateLifeSpan(object? value, ValidationContext context)
        {
            var instance = context.ObjectInstance;
            var minLifeProperty = context.ObjectType.GetProperty("MinLife");
            var maxLifeProperty = context.ObjectType.GetProperty("MaxLife");

            if (minLifeProperty != null && maxLifeProperty != null)
            {
                var minLife = (int?)minLifeProperty.GetValue(instance);
                var maxLife = (int?)maxLifeProperty.GetValue(instance);

                if (minLife.HasValue && maxLife.HasValue && minLife > maxLife)
                {
                    return new ValidationResult("MinLife không được lớn hơn MaxLife.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
