namespace SrcToTextConverter.Model
{
    class SourceFileModel
    {
        public SourceFileModel(string fullFileName, bool isChecked=true)
        {
            FullFileName = fullFileName;
            IsChecked = isChecked;
        }

        public bool IsChecked { get; set; }
        public string FullFileName { get; set; }
    }
}
