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

    }
}