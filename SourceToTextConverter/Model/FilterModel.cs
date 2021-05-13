using System.Text;

namespace SrcToTextConverter.Model
{
    public class FilterModel
    {
        public FilterModel(string name, string[] extensions, int codePage)
        {
            Name = name;
            Extensions = extensions;
            CodePage = codePage;
        }

        public string Name { get; set; }
        public string[] Extensions { get; set; }       
        public int CodePage { get; set; }

        public override string ToString()
        {
            StringBuilder filterDescription = new StringBuilder();

            filterDescription.Append($"{Name} (CP{GetEncoding().CodePage} - {GetEncoding().EncodingName}): ");
            
            for (int i = 0; i < Extensions.Length; i++)
            {
                filterDescription.Append($"{Extensions[i]} ");
            }
                
            return filterDescription.ToString();
        }

        public Encoding GetEncoding() => Encoding.GetEncoding(CodePage);
    }
}
