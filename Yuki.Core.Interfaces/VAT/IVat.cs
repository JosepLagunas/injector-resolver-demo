namespace Yuki.Core.Interfaces.Vat
{
    public interface IVat : IDataComponent
    {
        string Country { get; }
        double VatPercentage { get; set; }
        double ApplyVatTo(double import);
    }
}
