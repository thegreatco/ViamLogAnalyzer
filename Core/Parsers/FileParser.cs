namespace Core.Parsers
{
    public class FileParser(string fileName)
    {
        public async Task<ParseResults> Parse()
        {
            await using var fs = File.OpenRead(fileName);
            await using var bs = new BufferedStream(fs);
            using var sr = new StreamReader(bs);
            var results = new ParseResults();
            while (!sr.EndOfStream)
            {
                var line = await sr.ReadLineAsync();
                if (line == null)
                    continue;
                
                if (!line.StartsWith("2"))
                    continue;
                
                if (line.Contains("StdOut") || line.Contains("StdErr"))
                {
                    var nextLine = await sr.ReadLineAsync();
                    results.AddLogEntry(line, nextLine);
                }
                else
                {
                    results.AddLogEntry(line, null);
                }

            }
            return results;
        }
    }
}
