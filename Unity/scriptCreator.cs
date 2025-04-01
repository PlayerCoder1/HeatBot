using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HeatBot
{
    public partial class scriptCreator : Form
    {
        private WebView2 webView;
        private TextBox jsonEditor;
        private Button btnToggleClicks;
        private Button btnToggleBank;
        private Button btnActionLeft;
        private Button btnActionRight;
        private Button btnActionUp;
        private Button btnActionDown;
        private Button btnActionZaap;
        private Button btnActionBanking;
        private Button btnExportJson;
        private FlowLayoutPanel actionPanel;

        private string currentMode = null;
        private string currentAction = "left";
        private readonly string[] zaapLocations =
        {
            "Bord de la forêt maléfique",
            "Village d'Amakna",
            "Port de Madrestam",
            "Montagne des Craqueleurs",
            "Plaine des Scarafeuilles",
            "Château d'Amakna",
            "Coin des Bouftous",
            "Tainéla",
            "Cité d'Astrub",
            "Sufokia",
            "Rivage sufokien",
            "Immaculé",
            "La Cuirasse",
            "Foire du Trool",
            "Route des Roulottes",
            "Terres Désacrées",
            "Village des Éleveurs",
            "Plaines Rocheuses",
            "Massif de Cania",
            "Lac de Cania",
            "Routes Rocailleuses",
            "Champs de Cania",
            "Villages des Dopeuls",
            "Plaines des Porkass",
            "Village des Kanigs",
            "La BVourgade",
            "Village cotier",
            "Villade de Pandala"
        };

        private readonly string[] bankingLocations =
        {
            "astrub",
            "amakna",
            "sufokia"
        };

        public scriptCreator()
        {
            InitializeComponent();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            try
            {
                webView = new WebView2
                {
                    Dock = DockStyle.Fill
                };

                jsonEditor = new TextBox
                {
                    Dock = DockStyle.Right,
                    Width = 250,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new System.Drawing.Font("Consolas", 10),
                    Text = "{\n  \"clicks\": [],\n  \"bank\": []\n}"
                };

                btnToggleClicks = new Button
                {
                    Text = "Toggle Clicks",
                    Dock = DockStyle.Top,
                    Height = 40
                };
                btnToggleClicks.Click += BtnToggleClicks_Click;

                btnToggleBank = new Button
                {
                    Text = "Toggle Bank",
                    Dock = DockStyle.Top,
                    Height = 40
                };
                btnToggleBank.Click += BtnToggleBank_Click;

                btnExportJson = new Button
                {
                    Text = "Export Script",
                    Dock = DockStyle.Top,
                    Height = 40
                };
                btnExportJson.Click += BtnExportJson_Click;

                btnActionLeft = CreateActionButton("Left");
                btnActionRight = CreateActionButton("Right");
                btnActionUp = CreateActionButton("Up");
                btnActionDown = CreateActionButton("Down");
                btnActionZaap = CreateZaapButton();
                btnActionBanking = CreateBankingButton();

                actionPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    Height = 50,
                    FlowDirection = FlowDirection.LeftToRight,
                    Padding = new Padding(5)
                };
                actionPanel.Controls.Add(btnActionLeft);
                actionPanel.Controls.Add(btnActionRight);
                actionPanel.Controls.Add(btnActionUp);
                actionPanel.Controls.Add(btnActionDown);
                actionPanel.Controls.Add(btnActionZaap);
                actionPanel.Controls.Add(btnActionBanking);

                this.Controls.Add(webView);
                this.Controls.Add(actionPanel);
                this.Controls.Add(btnExportJson);
                this.Controls.Add(btnToggleBank);
                this.Controls.Add(btnToggleClicks);
                this.Controls.Add(jsonEditor);

                InitializeWebViewAsync();
            }
            catch (Exception ex)
            {

            }
        }

        private void BtnExportJson_Click(object sender, EventArgs e)
        {
            try
            {

                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    saveFileDialog.DefaultExt = "json";
                    saveFileDialog.FileName = "HeatBot_Script.json";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {

                        System.IO.File.WriteAllText(saveFileDialog.FileName, jsonEditor.Text);
                        MessageBox.Show("JSON exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export JSON: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void InitializeWebViewAsync()
        {
            try
            {

                var environment = await CoreWebView2Environment.CreateAsync(
                    null,
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TrajetMakerWebViewLogs"));

                await webView.EnsureCoreWebView2Async(environment);
                webView.Source = new Uri("https://dofus-map.com");

                webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            }
            catch (Exception ex)
            {

            }
        }

        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {

            try
            {
                await webView.CoreWebView2.ExecuteScriptAsync(@"
        document.addEventListener('click', function(event) {
            var mapDiv = document.getElementById('mapCoordinates');
            if (mapDiv) {
                var coordsText = mapDiv.textContent.trim();
                if (coordsText) {
                    window.chrome.webview.postMessage(coordsText);
                }
            }
        });
    ");
            }
            catch (Exception ex)
            {

            }
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {

                string coordsText = e.WebMessageAsJson.Trim('\"');

                var parts = coordsText.Split(',');
                int x = int.Parse(parts[0].Trim());
                int y = int.Parse(parts[1].Trim());

                if (currentAction == "zaap")
                {

                    SynchronizationContext.Current.Post((_) =>
                    {
                        AddZaapAction(x, y);
                    }, null);
                }
                else if (currentAction == "banking")
                {

                    SynchronizationContext.Current.Post((_) =>
                    {
                        AddBankingActions(x, y);
                    }, null);
                }
                else
                {
                    AddCoordinatesToCurrentMode(x, y);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void BtnToggleClicks_Click(object sender, EventArgs e)
        {
            ToggleMode("clicks");
        }

        private void BtnToggleBank_Click(object sender, EventArgs e)
        {
            ToggleMode("bank");
        }

        private void ToggleMode(string mode)
        {
            currentMode = (currentMode == mode) ? null : mode;

            btnToggleClicks.Text = (currentMode == "clicks") ? "Clicks (Active)" : "Toggle Clicks";
            btnToggleBank.Text = (currentMode == "bank") ? "Bank (Active)" : "Toggle Bank";
        }

        private void AddCoordinatesToCurrentMode(int x, int y)
        {
            try
            {

                if (currentMode == null)
                {

                    return;
                }

                var json = JObject.Parse(jsonEditor.Text);

                var newEntry = new JObject
                {
                    ["x"] = x,
                    ["y"] = y,
                    ["action"] = currentAction
                };

                if (json[currentMode] is JArray sectionArray)
                {
                    sectionArray.Add(newEntry);
                }
                else
                {
                    json[currentMode] = new JArray { newEntry };
                }

                jsonEditor.Text = json.ToString();

            }
            catch (Exception ex)
            {

            }
        }

        private void AddZaapAction(int x, int y)
        {
            var location = ShowZaapLocationSelection();
            if (string.IsNullOrEmpty(location)) return;

            try
            {
                var json = JObject.Parse(jsonEditor.Text);

                var zaapEntry = new JObject
                {
                    ["x"] = x,
                    ["y"] = y,
                    ["action"] = "zaap",
                    ["location"] = location
                };

                if (json[currentMode] is JArray sectionArray)
                {
                    sectionArray.Add(zaapEntry);
                }
                else
                {
                    json[currentMode] = new JArray { zaapEntry };
                }

                jsonEditor.Text = json.ToString();

            }
            catch (Exception ex)
            {

            }
        }

        private void AddBankingActions(int x, int y)
        {
            try
            {

                var location = ShowBankingLocationSelection();
                if (string.IsNullOrEmpty(location))
                {

                    return;
                }

                var json = JObject.Parse(jsonEditor.Text);

                var bankEntry1 = new JObject
                {
                    ["x"] = x,
                    ["y"] = y,
                    ["action"] = "bank",
                    ["bankStyle"] = $"bank.{location}"
                };

                var bankEntry2 = new JObject
                {
                    ["x"] = x,
                    ["y"] = y,
                    ["action"] = "bank",
                    ["bankStyle"] = $"npc.bank.{location}"
                };

                if (json[currentMode] is JArray sectionArray)
                {
                    sectionArray.Add(bankEntry1);
                    sectionArray.Add(bankEntry2);
                }
                else
                {
                    json[currentMode] = new JArray { bankEntry1, bankEntry2 };
                }

                jsonEditor.Text = json.ToString();

            }
            catch (Exception ex)
            {

            }
        }

        private string ShowZaapLocationSelection()
        {
            try
            {

                using (var form = new Form())
                {
                    form.Text = "Select Zaap Location";
                    form.Size = new System.Drawing.Size(300, 400);
                    form.StartPosition = FormStartPosition.CenterParent;

                    if (zaapLocations == null || zaapLocations.Length == 0)
                    {

                        MessageBox.Show("No Zaap locations available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }

                    foreach (var location in zaapLocations)
                    {

                    }

                    var listBox = new ListBox
                    {
                        Dock = DockStyle.Fill,
                        DataSource = zaapLocations
                    };

                    listBox.SelectedIndexChanged += (sender, e) =>
                    {

                    };

                    form.Controls.Add(listBox);

                    var btnOk = new Button
                    {
                        Text = "OK",
                        Dock = DockStyle.Bottom,
                        DialogResult = DialogResult.OK
                    };
                    form.Controls.Add(btnOk);

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        string selectedLocation = listBox.SelectedItem?.ToString();

                        return selectedLocation;
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }
        private string ShowBankingLocationSelection()
        {
            try
            {

                using (var form = new Form())
                {
                    form.Text = "Select Banking Location";
                    form.Size = new System.Drawing.Size(300, 200);
                    form.StartPosition = FormStartPosition.CenterParent;

                    if (bankingLocations == null || bankingLocations.Length == 0)
                    {

                        MessageBox.Show("No banking locations available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }

                    foreach (var location in bankingLocations)
                    {

                    }

                    var listBox = new ListBox
                    {
                        Dock = DockStyle.Fill,
                        DataSource = bankingLocations
                    };

                    listBox.SelectedIndexChanged += (sender, e) =>
                    {

                    };

                    form.Controls.Add(listBox);

                    var btnOk = new Button
                    {
                        Text = "OK",
                        Dock = DockStyle.Bottom,
                        DialogResult = DialogResult.OK
                    };
                    form.Controls.Add(btnOk);

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        string selectedLocation = listBox.SelectedItem?.ToString();

                        return selectedLocation;
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }
        private Button CreateBankingButton()
        {
            var button = new Button
            {
                Text = "Banking",
                Width = 80,
                Height = 40
            };

            button.Click += (sender, e) =>
            {
                currentAction = "banking";

                btnActionLeft.Text = "Left";
                btnActionRight.Text = "Right";
                btnActionUp.Text = "Up";
                btnActionDown.Text = "Down";
                btnActionZaap.Text = "Zaap";
                btnActionBanking.Text = "Banking (Active)";
            };

            return button;
        }

        private Button CreateActionButton(string action)
        {
            var button = new Button
            {
                Text = action,
                Width = 80,
                Height = 40
            };

            button.Click += (sender, e) =>
            {
                currentAction = action.ToLower();

                btnActionLeft.Text = "Left";
                btnActionRight.Text = "Right";
                btnActionUp.Text = "Up";
                btnActionDown.Text = "Down";
                btnActionZaap.Text = "Zaap";
                btnActionBanking.Text = "Banking";

                switch (action.ToLower())
                {
                    case "left": btnActionLeft.Text = "Left (Active)"; break;
                    case "right": btnActionRight.Text = "Right (Active)"; break;
                    case "up": btnActionUp.Text = "Up (Active)"; break;
                    case "down": btnActionDown.Text = "Down (Active)"; break;
                }
            };

            return button;
        }

        private Button CreateZaapButton()
        {
            var button = new Button
            {
                Text = "Zaap",
                Width = 80,
                Height = 40
            };

            button.Click += (sender, e) =>
            {
                currentAction = "zaap";

                btnActionLeft.Text = "Left";
                btnActionRight.Text = "Right";
                btnActionUp.Text = "Up";
                btnActionDown.Text = "Down";
                btnActionZaap.Text = "Zaap (Active)";
                btnActionBanking.Text = "Banking";
            };

            return button;
        }
    }
}