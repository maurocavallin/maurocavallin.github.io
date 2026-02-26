namespace PreviewCleaner;

public class Classificazione
{
    public int IDClassificazione { get; set; }
    public int? IDClassificazionePadre { get; set; }
    public int? Ordine { get; set; }
    public string Descrizione { get; set; } = string.Empty;

    public override string ToString() => Descrizione;
}

public class Documento
{
    public int IDDocumento { get; set; }
    public string Descrizione { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public int IDClassificazione { get; set; }

    public override string ToString() => Descrizione;
}

public class Revisione
{
    public int IDRevisione { get; set; }
    public int IDDocumento { get; set; }
    public string NumeroRevisione { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string? Descrizione { get; set; }
    public bool IsGlobal { get; set; }
    public string DocumentoDescrizione { get; set; } = string.Empty;

    public bool IsEsclusa =>
        IsGlobal && (Descrizione?.Contains("GLO", StringComparison.OrdinalIgnoreCase) == true);

    public override string ToString() => $"{NumeroRevisione} - {Descrizione}";
}
