# Preview Cleaner

Applicazione WinForms (.NET 10) per la pulizia delle immagini di anteprima nel repository.

## Funzionalità

- Navigazione gerarchica: **Classificazioni (livello 1)** → **Classificazioni (livello 2)** → **Documenti** → **Revisioni**
- Aggiunta revisioni alla lista di elaborazione tramite tre pulsanti:
  - *Aggiungi revisioni gruppo* (da Classificazione liv.1 con figli)
  - *Aggiungi revisioni sottogruppo* (da Classificazione liv.2)
  - *Aggiungi revisioni documento* (da un singolo Documento)
- Visualizzazione delle revisioni selezionate in una DataGrid
- Esecuzione del comando di pulizia:
  ```sql
  UPDATE [RepositoryImageUser] SET PreviewImage = NULL WHERE IDRevisione = <id>
  ```
- Limite di 100 revisioni per esecuzione

## Regole di esclusione

Le revisioni vengono **automaticamente escluse** (mai aggiunte alla lista né elaborate) se:
- `IsGlobal = true` **E** la colonna `Descrizione` contiene la sottostringa `GLO` (case insensitive)

## Configurazione connessione

Al primo avvio, selezionare **Connessione > Impostazioni** e inserire:
- Server SQL Server
- Nome database
- Autenticazione Windows oppure SQL Server (utente/password)

La stringa di connessione viene salvata in `%APPDATA%\PreviewCleaner\connection.txt`.

## Requisiti

- Windows (WinForms)
- .NET 10 Runtime
- SQL Server con le tabelle: `Classificazioni`, `Documenti`, `DocumentiRevisioni`, `RepositoryImageUser`

## Build

```bash
cd PreviewCleaner
dotnet build
dotnet run --project PreviewCleaner
```
