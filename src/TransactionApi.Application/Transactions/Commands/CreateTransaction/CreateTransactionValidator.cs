using FluentValidation;

namespace TransactionApi.Application.Transactions.Commands.CreateTransaction;

public class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    private static readonly string[] SupportedCurrencies = ["BRL", "USD", "EUR"];

    public CreateTransactionValidator()
    {
        RuleFor(x => x.SourceAccountId)
            .NotEmpty().WithMessage("Source account is required.")
            .MaximumLength(50);

        RuleFor(x => x.DestinationAccountId)
            .NotEmpty().WithMessage("Destination account is required.")
            .MaximumLength(50)
            .NotEqual(x => x.SourceAccountId)
                .WithMessage("Source and destination accounts must be different.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.")
            .LessThanOrEqualTo(1_000_000).WithMessage("Amount exceeds the allowed limit.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.")
            .Must(c => SupportedCurrencies.Contains(c.ToUpperInvariant()))
                .WithMessage("Currency not supported. Allowed: BRL, USD, EUR.");

        RuleFor(x => x.Description)
            .MaximumLength(255).When(x => x.Description != null);
    }
}
