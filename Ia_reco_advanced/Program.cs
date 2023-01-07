using IA_reco;

namespace Ia_reco_advanced
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Form1 myform = new Form1();
            Application.Run(myform);

        }    
    }
}