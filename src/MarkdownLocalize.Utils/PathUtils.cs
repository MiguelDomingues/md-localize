namespace MarkdownLocalize.Utils
{

    public static class PathUtils
    {
        public static string SimplifyRelativePath(string path)
        {
            path = path.Replace("\\", "/"); // normalize path separator
            Stack<string> items = new Stack<string>();
            IEnumerable<string> parts = path.Split('/');
            foreach (string part in parts)
            {
                if (part == "." && items.Count > 0)
                    continue;

                if (part == ".." && items.Count > 0 && items.Peek() != "..")
                {
                    items.Pop();
                    continue;
                }

                items.Push(part);
            }

            return String.Join('/', items.Reverse());
        }

        public static string GetRelativePath(string relativeTo, string path, bool appendFilename)
        {
            if (relativeTo == null || relativeTo == "")
                return null;
            string relativeToDirectory = Path.GetDirectoryName(relativeTo);
            if (relativeToDirectory == "")
                relativeToDirectory = "./";
            string pathDirectory = Path.GetDirectoryName(path);
            if (pathDirectory == "")
                pathDirectory = "./";
            string newPath = Path.GetRelativePath(relativeToDirectory, pathDirectory);
            if (appendFilename)
                newPath = Path.Combine(newPath, Path.GetFileName(path)).Replace("\\", "/");
            return newPath;
        }
    }
}