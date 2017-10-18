namespace DirectiveTest.ScreenKeyBorad.Libs
{
    public class ContentBuffer
    {
        public ContentBuffer() { }

        public int SelectionStart { get; set; } = 0;

        public int SelectionLength { get; set; } = 0;

        public string Content { get; set; } = null;
    }
}
