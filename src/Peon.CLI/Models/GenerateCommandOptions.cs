namespace Peon.CLI.Models
{
    public class GenerateCommandOptions
    {
        public string Listfile { get; set; }
        public string Model { get; set; }
        public string ModelsDir { get; set; }
        public string ModelsList { get; set; }
        public bool Verbose { get; set; }
        public string LogFile { get; set; }
    }
}