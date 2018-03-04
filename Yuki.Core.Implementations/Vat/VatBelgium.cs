using Yuki.Core.Interfaces.Vat;

namespace Yuki.Core.Implementations
{
    public class VatBelgium : IVat
    {
        private string country;
        public string Country => country;
        private double vatPercentage;
        public double VatPercentage { get => vatPercentage; set => vatPercentage = value; }

        public double ApplyVatTo(double import)
        {
            return import + import * (vatPercentage / 100);
        }

        public void Init()
        {
            country = "Belgium";
            vatPercentage = 6.5;
        }
    }
}
