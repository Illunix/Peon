namespace Peon.CLI.Models
{
    public class GetCommandOptions
    {
        public string Model { get; set; }
        public string ModelsDir { get; set; }
        public string ModelsList { get; set; }
        public string BuildConfig { get; set; }
        public string Path { get; set; }
        public bool Verbose { get; set; }
        public string LogFile { get; set; }
    }
}