using PdfSharp.Fonts;

namespace proyecto3.Frontend.Services
{
    public class FuentePdf : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo("Proyecto3Unicode");
        }

        public byte[] GetFont(string faceName)
        {
            using Stream? recurso = typeof(FuentePdf).Assembly.GetManifestResourceStream("Proyecto3Unicode.ttf");
            if (recurso == null)
            {
                throw new InvalidOperationException("No se encontró la fuente incluida para los reportes PDF.");
            }
            using MemoryStream memoria = new MemoryStream();
            recurso.CopyTo(memoria);
            return memoria.ToArray();
        }
    }
}
