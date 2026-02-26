using Microsoft.Data.SqlClient;

namespace PreviewCleaner;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<Classificazione>> GetClassificazioniRootAsync()
    {
        const string sql = @"
            SELECT IDClassificazione, IDClassificazionePadre, Ordine, Descrizione
            FROM dbo.Classificazioni
            WHERE IDClassificazionePadre IS NULL
            ORDER BY Ordine, Descrizione";

        return await QueryClassificazioniAsync(sql);
    }

    public async Task<List<Classificazione>> GetClassificazioniByPadreAsync(int idPadre)
    {
        const string sql = @"
            SELECT IDClassificazione, IDClassificazionePadre, Ordine, Descrizione
            FROM dbo.Classificazioni
            WHERE IDClassificazionePadre = @IDPadre
            ORDER BY Ordine, Descrizione";

        return await QueryClassificazioniAsync(sql, cmd =>
            cmd.Parameters.AddWithValue("@IDPadre", idPadre));
    }

    public async Task<List<Documento>> GetDocumentiByClassificazioneAsync(int idClassificazione)
    {
        const string sql = @"
            SELECT IDDocumento, Descrizione, Data, IDClassificazione
            FROM dbo.Documenti
            WHERE IDClassificazione = @IDClassificazione
            ORDER BY Descrizione";

        var result = new List<Documento>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@IDClassificazione", idClassificazione);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Documento
            {
                IDDocumento = reader.GetInt32(0),
                Descrizione = reader.GetString(1),
                Data = reader.GetDateTime(2),
                IDClassificazione = reader.GetInt32(3)
            });
        }
        return result;
    }

    public async Task<List<Revisione>> GetRevisioniByDocumentoAsync(int idDocumento)
    {
        const string sql = @"
            SELECT r.IDRevisione, r.IDDocumento, r.Revisione, r.Data,
                   r.Descrizione, r.IsGlobal, d.Descrizione AS DocDesc
            FROM dbo.DocumentiRevisioni r
            INNER JOIN dbo.Documenti d ON d.IDDocumento = r.IDDocumento
            WHERE r.IDDocumento = @IDDocumento
            ORDER BY r.Revisione";

        return await QueryRevisioniAsync(sql, cmd =>
            cmd.Parameters.AddWithValue("@IDDocumento", idDocumento));
    }

    public async Task<List<Revisione>> GetRevisioniByClassificazioneAsync(int idClassificazione)
    {
        const string sql = @"
            SELECT r.IDRevisione, r.IDDocumento, r.Revisione, r.Data,
                   r.Descrizione, r.IsGlobal, d.Descrizione AS DocDesc
            FROM dbo.DocumentiRevisioni r
            INNER JOIN dbo.Documenti d ON d.IDDocumento = r.IDDocumento
            WHERE d.IDClassificazione = @IDClassificazione
            ORDER BY d.Descrizione, r.Revisione";

        return await QueryRevisioniAsync(sql, cmd =>
            cmd.Parameters.AddWithValue("@IDClassificazione", idClassificazione));
    }

    public async Task<List<Revisione>> GetRevisioniByClassificazionePadreAsync(int idClassificazione)
    {
        const string sql = @"
            SELECT r.IDRevisione, r.IDDocumento, r.Revisione, r.Data,
                   r.Descrizione, r.IsGlobal, d.Descrizione AS DocDesc
            FROM dbo.DocumentiRevisioni r
            INNER JOIN dbo.Documenti d ON d.IDDocumento = r.IDDocumento
            INNER JOIN dbo.Classificazioni c ON c.IDClassificazione = d.IDClassificazione
            WHERE c.IDClassificazione = @IDClassificazione
               OR c.IDClassificazionePadre = @IDClassificazione
            ORDER BY d.Descrizione, r.Revisione";

        return await QueryRevisioniAsync(sql, cmd =>
            cmd.Parameters.AddWithValue("@IDClassificazione", idClassificazione));
    }

    public async Task<int> PulisciPreviewAsync(IEnumerable<int> idRevisioni)
    {
        var ids = idRevisioni.ToList();
        if (ids.Count == 0) return 0;

        var paramNames = ids.Select((_, i) => $"@id{i}").ToList();
        var inClause = string.Join(",", paramNames);
        var sql = $"UPDATE [RepositoryImageUser] SET PreviewImage = NULL WHERE IDRevisione IN ({inClause})";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        for (var i = 0; i < ids.Count; i++)
            cmd.Parameters.AddWithValue(paramNames[i], ids[i]);
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> TestConnectionAsync()
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        return true;
    }

    private async Task<List<Classificazione>> QueryClassificazioniAsync(
        string sql, Action<SqlCommand>? paramAction = null)
    {
        var result = new List<Classificazione>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        paramAction?.Invoke(cmd);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Classificazione
            {
                IDClassificazione = reader.GetInt32(0),
                IDClassificazionePadre = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                Ordine = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                Descrizione = reader.GetString(3)
            });
        }
        return result;
    }

    private async Task<List<Revisione>> QueryRevisioniAsync(
        string sql, Action<SqlCommand>? paramAction = null)
    {
        var result = new List<Revisione>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        paramAction?.Invoke(cmd);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Revisione
            {
                IDRevisione = reader.GetInt32(0),
                IDDocumento = reader.GetInt32(1),
                NumeroRevisione = reader.GetString(2),
                Data = reader.GetDateTime(3),
                Descrizione = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsGlobal = reader.GetBoolean(5),
                DocumentoDescrizione = reader.GetString(6)
            });
        }
        return result;
    }
}
