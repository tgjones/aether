namespace Aether.Reflection
{
    public class Bssrdf
    {
        public Spectrum SigmaA { get; private set; }
        public Spectrum SigmaPrimeS { get; private set; }
        public float Eta { get; private set; }

        public Bssrdf(Spectrum sigmaA, Spectrum sigmaPrimeS, float eta)
        {
            SigmaA = sigmaA;
            SigmaPrimeS = sigmaPrimeS;
            Eta = eta;
        }
    }
}