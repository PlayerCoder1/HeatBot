using DarkModeForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HeatBot
{
    public class Form1UI
    {
        private readonly Form1 form;
        private DarkModeCS dm;

        public Form1UI(Form1 form)
        {
            this.form = form;
        }

        public void InitializeUI()
        {
            form.Text = "HeatBot - V1.0.6";
            form.Size = new Size(450, 680);
            form.MinimumSize = new Size(450, 515);

            dm = new DarkModeCS(form)
            {
                ColorMode = DarkModeCS.DisplayMode.SystemDefault
            };

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoSize = true,
                Padding = new Padding(5),
            };
            form.Controls.Add(mainPanel);

            AddCoordinateFields(mainPanel);

            AddGroupedResourceCheckboxes(mainPanel);

            AddControlButtons(mainPanel);
        }

        private void AddCoordinateFields(TableLayoutPanel parent)
        {
            var coordinatesPanel = new TableLayoutPanel
            {
                ColumnCount = 4,
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(5),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            parent.Controls.Add(coordinatesPanel);

            coordinatesPanel.Controls.Add(CreateLabel("Set X:"), 0, 0);
            form.txtDefaultX = CreateTextBox("1");
            coordinatesPanel.Controls.Add(form.txtDefaultX, 1, 0);

            coordinatesPanel.Controls.Add(CreateLabel("Set Y:"), 2, 0);
            form.txtDefaultY = CreateTextBox("-4");
            coordinatesPanel.Controls.Add(form.txtDefaultY, 3, 0);

            coordinatesPanel.Controls.Add(CreateLabel("Current X:"), 0, 1);
            form.txtX = CreateTextBox("", true);
            coordinatesPanel.Controls.Add(form.txtX, 1, 1);

            coordinatesPanel.Controls.Add(CreateLabel("Current Y:"), 2, 1);
            form.txtY = CreateTextBox("", true);
            coordinatesPanel.Controls.Add(form.txtY, 3, 1);
        }

        private void AddGroupedResourceCheckboxes(TableLayoutPanel parent)
        {
            var resourcesPanel = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(5),
            };
            parent.Controls.Add(resourcesPanel);

            AddResourceGroup(resourcesPanel, "Paysan", new Dictionary<string, CheckBox>
            {
                { "Blé", form.chkPaysan = new CheckBox() },
                { "Orge", form.chkOrge = new CheckBox() },
                { "Avoine", form.chkAvoine = new CheckBox() },
                { "Houblon", form.chkHoublon = new CheckBox() },
                { "Seigle", form.chkSeigle = new CheckBox() },
                { "Chanvre", form.chkChanvre = new CheckBox() },
                { "Lin", form.chkLin = new CheckBox() },
                { "Frostiz", form.chkFrostiz = new CheckBox() },
                { "Maïs", form.chkMais = new CheckBox() },
                { "Malt", form.chkMalt = new CheckBox() },
                { "Millet", form.chkMillet = new CheckBox() }
            });

            AddResourceGroup(resourcesPanel, "Bucheron", new Dictionary<string, CheckBox>
            {
                { "Frene", form.chkFrene = new CheckBox() },
                { "Charme", form.chkCharme = new CheckBox() },
                { "Chataigne", form.chkChataigne = new CheckBox() },
                { "Erable", form.chkErable = new CheckBox() },
                { "Merisier", form.chkMerisier = new CheckBox() },
                { "Noyer", form.chkNoyer = new CheckBox() },
                { "Orme", form.chkOrme = new CheckBox() },
                { "If", form.chkIf = new CheckBox() },
                { "Bambou", form.chkBambou = new CheckBox() },
                { "Bambou Sacré", form.chkBambouSacre = new CheckBox() },
                { "Bombu", form.chkBombu = new CheckBox() },
                { "Ebène", form.chkEbene = new CheckBox() },
                { "Kaliptus", form.chkKaliptus = new CheckBox() },
                { "Noisetier", form.chkNoisetier = new CheckBox() },
                { "Oliviolet", form.chkOliviolet = new CheckBox() },
                { "Pin", form.chkPin = new CheckBox() },
                { "Tremble", form.chkTremble = new CheckBox() }
            });

            AddResourceGroup(resourcesPanel, "Alchimiste", new Dictionary<string, CheckBox>
            {
                { "Ortie", form.chkOrtie = new CheckBox() },
                { "Sauge", form.chkSauge = new CheckBox() },
                { "Trèfle", form.chkTrefle = new CheckBox() },
                { "Eldeweiss", form.chkEldeweiss = new CheckBox() },
                { "Menthe", form.chkMenthe = new CheckBox() },
                { "Belladone", form.chkBelladone = new CheckBox() },
                { "Mandragore", form.chkMandragore = new CheckBox() },
                { "Pandouille", form.chkPandouille = new CheckBox() },
                { "Perce-neige", form.chkPerceNeige = new CheckBox() },
                { "Orchidée",form.chkOrchider = new CheckBox() },
                { "Ginseng",form.chkGinseng = new CheckBox() },

            });

            AddResourceGroup(resourcesPanel, "Mineur", new Dictionary<string, CheckBox>
            {
                { "Fer", form.chkFer = new CheckBox() },
                { "Etain", form.chkEtain = new CheckBox() },
                { "Bronze", form.chkBronze = new CheckBox() },
                { "Kobalte", form.chkKobalte = new CheckBox() }
            });
        }

        private void AddResourceGroup(Control parent, string groupName, Dictionary<string, CheckBox> resources)
        {
            var groupBox = new GroupBox
            {
                Text = groupName,
                AutoSize = true,
                Padding = new Padding(5),
                Dock = DockStyle.Top,
            };
            parent.Controls.Add(groupBox);

            var checkboxPanel = new TableLayoutPanel
            {
                ColumnCount = 4,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(3),
            };
            groupBox.Controls.Add(checkboxPanel);

            foreach (var resource in resources)
            {
                var checkbox = resource.Value;
                checkbox.Text = resource.Key;
                checkbox.AutoSize = true;
                checkboxPanel.Controls.Add(checkbox);
            }
        }

        private void AddControlButtons(Control parent)
        {
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Padding = new Padding(5),
            };
            parent.Controls.Add(buttonPanel);

            var setCoordinatesButton = new Button
            {
                Text = "Set Coordinates",
                AutoSize = true,
            };
            setCoordinatesButton.Click += (s, e) => form.ResetCoordinates();
            buttonPanel.Controls.Add(setCoordinatesButton);

            var loadScriptButton = new Button
            {
                Text = "Load Script",
                AutoSize = true,
            };
            loadScriptButton.Click += form.LoadAndExecuteScript;
            buttonPanel.Controls.Add(loadScriptButton);

            var stopScriptButton = new Button
            {
                Text = "Stop Script",
                AutoSize = true,
            };
            stopScriptButton.Click += (s, e) => form.StopScript();
            buttonPanel.Controls.Add(stopScriptButton);

            var schedulerButton = new Button
            {
                Text = "Planning",
                AutoSize = true,
            };
            schedulerButton.Click += (s, e) => form.OpenSchedulerForm();
            buttonPanel.Controls.Add(schedulerButton);

            var scriptCreatorButton = new Button
            {
                Text = "Script Creator",
                AutoSize = true,
            };
            scriptCreatorButton.Click += (s, e) => OpenScriptCreatorForm();
            buttonPanel.Controls.Add(scriptCreatorButton);
            var parametersButton = new Button
            {
                Text = "Parameters",
                AutoSize = true,
            };
            parametersButton.Click += (s, e) => OpenParametersForm();
            buttonPanel.Controls.Add(parametersButton);
            var treasureHuntButton = new Button
            {
                Text = "Treasure Hunt",
                AutoSize = true,
            };
            treasureHuntButton.Click += (s, e) => TreasureHunt.StartTreasureHunt();
            buttonPanel.Controls.Add(treasureHuntButton);
        }
        private void OpenParametersForm()
        {
            try
            {
                var podsDetector = new PodsDetector();
                using (var parametersForm = new ParametersForm(podsDetector.PodsRegion))
                {
                    parametersForm.ShowDialog();

                    podsDetector.PodsRegion = parametersForm.SelectedRegion;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Parameters Form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenScriptCreatorForm()
        {
            try
            {

                using (var scriptCreatorForm = new scriptCreator())
                {
                    scriptCreatorForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Script Creator: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Padding = new Padding(2),
            };
        }

        private TextBox CreateTextBox(string text, bool readOnly = false)
        {
            return new TextBox
            {
                Text = text,
                ReadOnly = readOnly,
                Width = 80,
            };
        }
    }
}