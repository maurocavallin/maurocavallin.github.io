namespace PreviewCleaner;

public partial class ConnectionForm : Form
{
    public string ConnectionString { get; private set; } = string.Empty;

    private TextBox txtServer = null!;
    private TextBox txtDatabase = null!;
    private CheckBox chkWindowsAuth = null!;
    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private Button btnTest = null!;
    private Button btnOk = null!;
    private Button btnCancel = null!;
    private Label lblStatus = null!;

    public ConnectionForm(string existingConnectionString = "")
    {
        InitializeConnectionForm();
        if (!string.IsNullOrEmpty(existingConnectionString))
            ParseConnectionString(existingConnectionString);
    }

    private void InitializeConnectionForm()
    {
        Text = "Configurazione connessione";
        Size = new Size(420, 320);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 8,
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        txtServer = new TextBox { Dock = DockStyle.Fill };
        txtDatabase = new TextBox { Dock = DockStyle.Fill };
        chkWindowsAuth = new CheckBox { Text = "Autenticazione Windows", Dock = DockStyle.Fill, Checked = true };
        txtUsername = new TextBox { Dock = DockStyle.Fill, Enabled = false };
        txtPassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true, Enabled = false };
        lblStatus = new Label { Dock = DockStyle.Fill, ForeColor = Color.Gray, Text = "" };
        btnTest = new Button { Text = "Testa connessione", Dock = DockStyle.Fill };
        btnOk = new Button { Text = "OK", Dock = DockStyle.Fill, DialogResult = DialogResult.OK };
        btnCancel = new Button { Text = "Annulla", Dock = DockStyle.Fill, DialogResult = DialogResult.Cancel };

        AddRow(layout, 0, "Server:", txtServer);
        AddRow(layout, 1, "Database:", txtDatabase);
        layout.Controls.Add(new Label(), 0, 2);
        layout.Controls.Add(chkWindowsAuth, 1, 2);
        AddRow(layout, 3, "Utente:", txtUsername);
        AddRow(layout, 4, "Password:", txtPassword);
        layout.Controls.Add(lblStatus, 0, 5);
        layout.SetColumnSpan(lblStatus, 2);

        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        btnPanel.Controls.Add(btnCancel);
        btnPanel.Controls.Add(btnOk);
        btnPanel.Controls.Add(btnTest);
        layout.Controls.Add(btnPanel, 0, 7);
        layout.SetColumnSpan(btnPanel, 2);

        Controls.Add(layout);
        AcceptButton = btnOk;
        CancelButton = btnCancel;

        chkWindowsAuth.CheckedChanged += (_, _) =>
        {
            txtUsername.Enabled = !chkWindowsAuth.Checked;
            txtPassword.Enabled = !chkWindowsAuth.Checked;
        };

        btnTest.Click += async (_, _) => await TestConnectionAsync();

        btnOk.Click += (_, _) =>
        {
            ConnectionString = BuildConnectionString();
        };
    }

    private static void AddRow(TableLayoutPanel layout, int row, string label, Control control)
    {
        layout.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private async Task TestConnectionAsync()
    {
        lblStatus.ForeColor = Color.Gray;
        lblStatus.Text = "Test in corso...";
        btnTest.Enabled = false;
        try
        {
            var svc = new DatabaseService(BuildConnectionString());
            await svc.TestConnectionAsync();
            lblStatus.ForeColor = Color.Green;
            lblStatus.Text = "Connessione riuscita!";
        }
        catch (Exception ex)
        {
            lblStatus.ForeColor = Color.Red;
            lblStatus.Text = $"Errore: {ex.Message}";
        }
        finally
        {
            btnTest.Enabled = true;
        }
    }

    private string BuildConnectionString()
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
        {
            DataSource = txtServer.Text.Trim(),
            InitialCatalog = txtDatabase.Text.Trim(),
            TrustServerCertificate = true
        };
        if (chkWindowsAuth.Checked)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = txtUsername.Text.Trim();
            builder.Password = txtPassword.Text;
        }
        return builder.ConnectionString;
    }

    private void ParseConnectionString(string cs)
    {
        try
        {
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(cs);
            txtServer.Text = builder.DataSource;
            txtDatabase.Text = builder.InitialCatalog;
            if (builder.IntegratedSecurity)
            {
                chkWindowsAuth.Checked = true;
            }
            else
            {
                chkWindowsAuth.Checked = false;
                txtUsername.Text = builder.UserID;
                txtPassword.Text = builder.Password;
            }
        }
        catch { }
    }
}
