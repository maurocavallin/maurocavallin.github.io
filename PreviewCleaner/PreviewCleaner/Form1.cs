namespace PreviewCleaner;

public partial class Form1 : Form
{
    private const int MaxRevisioni = 100;

    private DatabaseService? _db;
    private string _connectionString = string.Empty;

    // Left panel controls
    private ListBox lstClassificazioniL1 = null!;
    private ListBox lstClassificazioniL2 = null!;
    private ListBox lstDocumenti = null!;
    private Button btnAddDaL1 = null!;
    private Button btnAddDaL2 = null!;
    private Button btnAddDaDoc = null!;

    // Right panel controls
    private DataGridView dgvRevisioni = null!;
    private Button btnEsegui = null!;
    private Button btnRimuovi = null!;
    private Button btnSvuota = null!;
    private Label lblContatore = null!;
    private Label lblStatus = null!;

    // Menu
    private MenuStrip menuStrip = null!;

    // Backing list for the grid
    private readonly List<Revisione> _revisioniSelezionate = new();

    public Form1()
    {
        InitializeComponent();
        BuildUI();
        LoadConnectionString();
    }

    private void BuildUI()
    {
        Text = "Preview Cleaner";
        MinimumSize = new Size(900, 550);
        Size = new Size(1200, 700);
        StartPosition = FormStartPosition.CenterScreen;

        // Menu
        menuStrip = new MenuStrip();
        var mnuConnessione = new ToolStripMenuItem("Connessione");
        var mnuImpostazioni = new ToolStripMenuItem("Impostazioni...", null, (_, _) => ConfiguraConnessione());
        mnuConnessione.DropDownItems.Add(mnuImpostazioni);
        menuStrip.Items.Add(mnuConnessione);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;

        // Main splitter
        var splitMain = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 450,
            Panel1MinSize = 350,
            Panel2MinSize = 350
        };
        Controls.Add(splitMain);
        splitMain.BringToFront();

        // ---- LEFT PANEL ----
        var splitLeft = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 220,
            Panel1MinSize = 150,
            Panel2MinSize = 120
        };
        splitMain.Panel1.Controls.Add(splitLeft);

        // Top of left: two side-by-side classification lists
        var splitClassif = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 50,
        };
        splitClassif.Panel1.Controls.Add(BuildClassifPanel(out lstClassificazioniL1, out btnAddDaL1,
            "Classificazioni (liv.1)", "Aggiungi revisioni gruppo"));
        splitClassif.Panel2.Controls.Add(BuildClassifPanel(out lstClassificazioniL2, out btnAddDaL2,
            "Classificazioni (liv.2)", "Aggiungi revisioni sottogruppo"));
        splitLeft.Panel1.Controls.Add(splitClassif);

        // Bottom of left: documents list
        splitLeft.Panel2.Controls.Add(BuildDocPanel(out lstDocumenti, out btnAddDaDoc));

        // ---- RIGHT PANEL ----
        splitMain.Panel2.Controls.Add(BuildRightPanel());

        // Wire events
        lstClassificazioniL1.SelectedIndexChanged += async (_, _) => await CaricaClassificazioniL2Async();
        lstClassificazioniL2.SelectedIndexChanged += async (_, _) => await CaricaDocumentiAsync();
        lstDocumenti.SelectedIndexChanged += (_, _) => { /* could preview count */ };

        btnAddDaL1.Click += async (_, _) => await AggiungiRevisioniAsync(TipoSelezione.Gruppo);
        btnAddDaL2.Click += async (_, _) => await AggiungiRevisioniAsync(TipoSelezione.Sottogruppo);
        btnAddDaDoc.Click += async (_, _) => await AggiungiRevisioniAsync(TipoSelezione.Documento);
        btnEsegui.Click += async (_, _) => await EseguiPuliziaAsync();
        btnRimuovi.Click += (_, _) => RimuoviSelezionate();
        btnSvuota.Click += (_, _) => SvuotaLista();
    }

    private static Panel BuildClassifPanel(out ListBox listBox, out Button button, string title, string btnText)
    {
        listBox = new ListBox
        {
            Dock = DockStyle.Fill,
            SelectionMode = SelectionMode.One,
            IntegralHeight = false
        };
        button = new Button
        {
            Text = btnText,
            Dock = DockStyle.Bottom,
            Height = 30
        };
        var lbl = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 22,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
        };
        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(listBox);
        panel.Controls.Add(button);
        panel.Controls.Add(lbl);
        return panel;
    }

    private static Panel BuildDocPanel(out ListBox listBox, out Button button)
    {
        listBox = new ListBox
        {
            Dock = DockStyle.Fill,
            SelectionMode = SelectionMode.One,
            IntegralHeight = false
        };
        button = new Button
        {
            Text = "Aggiungi revisioni documento",
            Dock = DockStyle.Bottom,
            Height = 30
        };
        var lbl = new Label
        {
            Text = "Documenti",
            Dock = DockStyle.Top,
            Height = 22,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
        };
        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(listBox);
        panel.Controls.Add(button);
        panel.Controls.Add(lbl);
        return panel;
    }

    private Panel BuildRightPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill };

        var lblTitle = new Label
        {
            Text = "Revisioni selezionate",
            Dock = DockStyle.Top,
            Height = 22,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
        };

        lblContatore = new Label
        {
            Text = "0 / 100 revisioni",
            Dock = DockStyle.Top,
            Height = 20,
            ForeColor = Color.DarkSlateGray
        };

        dgvRevisioni = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false
        };

        dgvRevisioni.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "IDRevisione", HeaderText = "ID", Width = 60, AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
            new DataGridViewTextBoxColumn { Name = "NumeroRevisione", HeaderText = "Revisione" },
            new DataGridViewTextBoxColumn { Name = "Documento", HeaderText = "Documento" },
            new DataGridViewTextBoxColumn { Name = "Descrizione", HeaderText = "Descrizione" },
            new DataGridViewTextBoxColumn { Name = "Data", HeaderText = "Data", Width = 110, AutoSizeMode = DataGridViewAutoSizeColumnMode.None }
        );

        var bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 80
        };

        btnEsegui = new Button
        {
            Text = "Esegui pulizia preview",
            Location = new Point(6, 6),
            Size = new Size(160, 34),
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };

        btnRimuovi = new Button
        {
            Text = "Rimuovi selezionate",
            Location = new Point(174, 6),
            Size = new Size(140, 34)
        };

        btnSvuota = new Button
        {
            Text = "Svuota lista",
            Location = new Point(322, 6),
            Size = new Size(110, 34)
        };

        lblStatus = new Label
        {
            Location = new Point(6, 46),
            Size = new Size(600, 22),
            ForeColor = Color.DarkSlateGray
        };

        bottomPanel.Controls.AddRange([btnEsegui, btnRimuovi, btnSvuota, lblStatus]);

        panel.Controls.Add(dgvRevisioni);
        panel.Controls.Add(bottomPanel);
        panel.Controls.Add(lblContatore);
        panel.Controls.Add(lblTitle);
        return panel;
    }

    // ---- Data loading ----

    private async Task CaricaClassificazioniL1Async()
    {
        if (_db == null) return;
        try
        {
            lstClassificazioniL1.Items.Clear();
            lstClassificazioniL2.Items.Clear();
            lstDocumenti.Items.Clear();
            var items = await _db.GetClassificazioniRootAsync();
            foreach (var c in items)
                lstClassificazioniL1.Items.Add(c);
            SetStatus("Classificazioni caricate.");
        }
        catch (Exception ex)
        {
            SetStatus($"Errore caricamento classificazioni: {ex.Message}", true);
        }
    }

    private async Task CaricaClassificazioniL2Async()
    {
        if (_db == null || lstClassificazioniL1.SelectedItem is not Classificazione c1) return;
        try
        {
            lstClassificazioniL2.Items.Clear();
            lstDocumenti.Items.Clear();
            var items = await _db.GetClassificazioniByPadreAsync(c1.IDClassificazione);
            foreach (var c in items)
                lstClassificazioniL2.Items.Add(c);
        }
        catch (Exception ex)
        {
            SetStatus($"Errore caricamento sottoclassificazioni: {ex.Message}", true);
        }
    }

    private async Task CaricaDocumentiAsync()
    {
        if (_db == null || lstClassificazioniL2.SelectedItem is not Classificazione c2) return;
        try
        {
            lstDocumenti.Items.Clear();
            var items = await _db.GetDocumentiByClassificazioneAsync(c2.IDClassificazione);
            foreach (var d in items)
                lstDocumenti.Items.Add(d);
        }
        catch (Exception ex)
        {
            SetStatus($"Errore caricamento documenti: {ex.Message}", true);
        }
    }

    // ---- Add to selection ----

    private enum TipoSelezione { Gruppo, Sottogruppo, Documento }

    private async Task AggiungiRevisioniAsync(TipoSelezione tipo)
    {
        if (_db == null)
        {
            MessageBox.Show("Configurare prima la connessione al database (menu Connessione > Impostazioni).",
                "Nessuna connessione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        List<Revisione> revisioni;
        try
        {
            revisioni = tipo switch
            {
                TipoSelezione.Gruppo when lstClassificazioniL1.SelectedItem is Classificazione c1 =>
                    await _db.GetRevisioniByClassificazionePadreAsync(c1.IDClassificazione),
                TipoSelezione.Sottogruppo when lstClassificazioniL2.SelectedItem is Classificazione c2 =>
                    await _db.GetRevisioniByClassificazioneAsync(c2.IDClassificazione),
                TipoSelezione.Documento when lstDocumenti.SelectedItem is Documento doc =>
                    await _db.GetRevisioniByDocumentoAsync(doc.IDDocumento),
                _ => new List<Revisione>()
            };
        }
        catch (Exception ex)
        {
            SetStatus($"Errore recupero revisioni: {ex.Message}", true);
            return;
        }

        var escluse = 0;
        var duplicate = 0;
        var aggiunte = 0;
        var overflow = 0;

        foreach (var r in revisioni)
        {
            if (r.IsEsclusa)
            {
                escluse++;
                continue;
            }
            if (_revisioniSelezionate.Any(x => x.IDRevisione == r.IDRevisione))
            {
                duplicate++;
                continue;
            }
            if (_revisioniSelezionate.Count >= MaxRevisioni)
            {
                overflow++;
                continue;
            }
            _revisioniSelezionate.Add(r);
            aggiunte++;
        }

        AggiornaDatiGriglia();

        var msg = $"Aggiunte {aggiunte} revisioni.";
        if (escluse > 0) msg += $" Escluse {escluse} (IsGlobal+GLO).";
        if (duplicate > 0) msg += $" Già presenti: {duplicate}.";
        if (overflow > 0) msg += $" Limite 100 raggiunto, {overflow} non aggiunte.";
        SetStatus(msg);
    }

    private void AggiornaDatiGriglia()
    {
        dgvRevisioni.Rows.Clear();
        foreach (var r in _revisioniSelezionate)
        {
            dgvRevisioni.Rows.Add(
                r.IDRevisione,
                r.NumeroRevisione,
                r.DocumentoDescrizione,
                r.Descrizione ?? string.Empty,
                r.Data.ToString("dd/MM/yyyy HH:mm")
            );
        }
        AggiornaContatore();
    }

    private void AggiornaContatore()
    {
        var count = _revisioniSelezionate.Count;
        lblContatore.Text = $"{count} / {MaxRevisioni} revisioni";
        lblContatore.ForeColor = count >= MaxRevisioni ? Color.Red : Color.DarkSlateGray;
        btnEsegui.Enabled = count > 0;
    }

    // ---- List management ----

    private void RimuoviSelezionate()
    {
        var selectedIds = dgvRevisioni.SelectedRows
            .Cast<DataGridViewRow>()
            .Select(r => r.Cells["IDRevisione"].Value is int id ? id : 0)
            .Where(id => id != 0)
            .ToHashSet();

        _revisioniSelezionate.RemoveAll(r => selectedIds.Contains(r.IDRevisione));
        AggiornaDatiGriglia();
        SetStatus($"Rimosse {selectedIds.Count} revisioni.");
    }

    private void SvuotaLista()
    {
        if (_revisioniSelezionate.Count == 0) return;
        if (MessageBox.Show("Svuotare la lista delle revisioni selezionate?",
                "Conferma", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        _revisioniSelezionate.Clear();
        AggiornaDatiGriglia();
        SetStatus("Lista svuotata.");
    }

    // ---- Execute cleanup ----

    private async Task EseguiPuliziaAsync()
    {
        if (_revisioniSelezionate.Count == 0) return;

        var conferma = MessageBox.Show(
            $"Eseguire la pulizia delle preview per {_revisioniSelezionate.Count} revisioni?\n\n" +
            "Verrà eseguito:\nUPDATE [RepositoryImageUser] SET PreviewImage = NULL WHERE IDRevisione = ...",
            "Conferma esecuzione",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (conferma != DialogResult.Yes) return;

        btnEsegui.Enabled = false;
        SetStatus("Esecuzione in corso...");

        try
        {
            var ids = _revisioniSelezionate.Select(r => r.IDRevisione).ToList();
            var affected = await _db!.PulisciPreviewAsync(ids);
            SetStatus($"Completato. Righe aggiornate: {affected}.");
            _revisioniSelezionate.Clear();
            AggiornaDatiGriglia();
        }
        catch (Exception ex)
        {
            SetStatus($"Errore durante l'esecuzione: {ex.Message}", true);
            btnEsegui.Enabled = _revisioniSelezionate.Count > 0;
        }
    }

    // ---- Connection ----

    private void ConfiguraConnessione()
    {
        using var dlg = new ConnectionForm(_connectionString);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        _connectionString = dlg.ConnectionString;
        _db = new DatabaseService(_connectionString);
        SaveConnectionString(_connectionString);
        SetStatus("Connessione configurata. Caricamento dati...");
        _ = CaricaClassificazioniL1Async();
    }

    private void LoadConnectionString()
    {
        try
        {
            var configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PreviewCleaner", "connection.txt");

            if (File.Exists(configPath))
            {
                _connectionString = File.ReadAllText(configPath).Trim();
                _db = new DatabaseService(_connectionString);
                _ = CaricaClassificazioniL1Async();
            }
            else
            {
                SetStatus("Configurare la connessione al database (menu Connessione > Impostazioni).");
            }
        }
        catch
        {
            SetStatus("Configurare la connessione al database (menu Connessione > Impostazioni).");
        }
    }

    private static void SaveConnectionString(string cs)
    {
        try
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PreviewCleaner");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "connection.txt"), cs);
        }
        catch { }
    }

    private void SetStatus(string message, bool isError = false)
    {
        lblStatus.Text = message;
        lblStatus.ForeColor = isError ? Color.Red : Color.DarkSlateGray;
    }
}

