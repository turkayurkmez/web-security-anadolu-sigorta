using FluentValidation;
using SecureBlog.API.DTOs;

namespace SecureBlog.API.Validators;

public class PostValidator : AbstractValidator<CreatePostDto>
{
    // TODO: FluentValidation kuralları burada yazılacak
    public PostValidator()
    {
        RuleFor(x=>x.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz!")
            .MaximumLength(200).WithMessage("En fazla 200 karakter olabilir")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ0-9\s.,!]+$")
            .WithMessage("Başlık yalnızca harf, rakam, boşluk ve . , ! içerebilir");


        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Makale içeriği boş olamaz");


        RuleFor(x => x.AuthorId).GreaterThan(0).WithMessage("Geçerli bir yazar ID girin...");
          

    }
}
