using FluentValidation;
using System;
using DTOLayer.WebApiDTO.ProductDTO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.ValidationRules.ProductValidator
{
    public class UpdateProductDTOValidator : AbstractValidator<UpdateProductDTO>
    {
        public UpdateProductDTOValidator()
        {
            // ProductId için doğrulama
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Ürün ID'si boş olamaz.")
                .GreaterThan(0).WithMessage("Geçersiz bir Ürün ID'si giriniz.");

            // Name için doğrulama kuralları
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ürün adı boş olamaz.")
                .MaximumLength(30).WithMessage("Ürün adı en fazla 30 karakter olabilir.");

            // Author için doğrulama kuralları
            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Yazar adı boş olamaz.")
                .MaximumLength(50).WithMessage("Yazar adı en fazla 50 karakter olabilir.");

            // Description için doğrulama kuralları
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Açıklama boş olamaz.")
                .MaximumLength(600).WithMessage("Açıklama en fazla 600 karakter olabilir.");

            // ImageUrl için doğrulama kuralları
            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("Görsel URL'si boş olamaz.")
                .MaximumLength(200).WithMessage("Görsel URL'si en fazla 200 karakter olabilir.")
                .Matches(@"^(http(s?):)([/|.|\w|\s|-])*\.(?:jpg|gif|png|jpeg)$")
                .WithMessage("Geçersiz görsel URL formatı. Desteklenen uzantılar: jpg, gif, png, jpeg.");

            // Price için doğrulama kuralları
            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("Fiyat boş olamaz.")
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

            // CategoryId için doğrulama kuralları
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Kategori ID'si boş olamaz.")
                .GreaterThan(0).WithMessage("Geçerli bir Kategori ID'si giriniz.");
        }
    }
}
