namespace Akka.Configuration.Hocon
{
    internal class HoconPath
    {
        private readonly string _pathStr;
        private string[] _pathElements;

        public HoconPath(string pathStr)
        {
            _pathStr = pathStr;
        }

        public string[] AsArray()
        {
            return _pathElements ?? (_pathElements = Parse(_pathStr));
        }

        private static string[] Parse(string input)
        {
            var rv = DotSeparatedListParser.Parse(input).ToArray();
            return rv;
        }
    }

}