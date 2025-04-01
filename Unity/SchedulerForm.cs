using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace HeatBot
{
    public partial class SchedulerForm : Form
    {
        private readonly List<TimeSlot> schedule;
        private const string ScheduleFile = "planningConfig.json";

        public SchedulerForm()
        {
            InitializeComponent();
            schedule = LoadSchedule();
            InitializeSchedulerUI();
        }

        private void InitializeSchedulerUI()
        {
            this.Text = "Bot Planning";
            this.Size = new Size(350, 600);
            this.BackColor = Color.Black;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10),
                BackColor = Color.Black
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 90));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 10));

            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Arial", 10, FontStyle.Regular),
                BackColor = Color.DarkSlateGray,
                ForeColor = Color.White,
            };
            listView.Columns.Add("Hour");
            listView.Columns.Add("Active");

            for (int i = 0; i < 24; i++)
            {
                var timeSlot = schedule.Find(slot => slot.Hour == i) ?? new TimeSlot(i, true);
                var item = new ListViewItem(i.ToString());
                item.SubItems.Add(timeSlot.IsActive ? "True" : "False");
                SetItemColor(item, timeSlot.IsActive);
                listView.Items.Add(item);
            }

            listView.ItemActivate += (s, e) => ToggleTimeSlot(listView);

            listView.SizeChanged += (s, e) =>
            {
                int columnWidth = listView.Width / listView.Columns.Count;
                foreach (ColumnHeader column in listView.Columns)
                {
                    column.Width = columnWidth - 4;
                }
            };

            var saveButton = new Button
            {
                Text = "Save Schedule",
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.DarkGray,
                ForeColor = Color.Black
            };
            saveButton.Click += (s, e) => SaveSchedule(listView);

            layout.Controls.Add(listView, 0, 0);
            layout.Controls.Add(saveButton, 0, 1);

            this.Controls.Add(layout);
        }

        private void SetItemColor(ListViewItem item, bool isActive)
        {
            item.BackColor = isActive ? Color.Green : Color.DarkRed;
            item.ForeColor = Color.White;
        }

        private void ToggleTimeSlot(ListView listView)
        {
            if (listView.SelectedItems.Count > 0)
            {
                var selectedItem = listView.SelectedItems[0];
                bool isActive = selectedItem.SubItems[1].Text == "True";

                selectedItem.SubItems[1].Text = isActive ? "False" : "True";
                SetItemColor(selectedItem, !isActive);
            }
        }

        private void SaveSchedule(ListView listView)
        {
            schedule.Clear();
            foreach (ListViewItem item in listView.Items)
            {
                int hour = int.Parse(item.Text);
                bool isActive = item.SubItems[1].Text == "True";
                schedule.Add(new TimeSlot(hour, isActive));
            }

            File.WriteAllText(ScheduleFile, JsonSerializer.Serialize(schedule, new JsonSerializerOptions { WriteIndented = true }));
            MessageBox.Show("Schedule saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private List<TimeSlot> LoadSchedule()
        {
            if (File.Exists(ScheduleFile))
            {
                string content = File.ReadAllText(ScheduleFile);
                return JsonSerializer.Deserialize<List<TimeSlot>>(content) ?? new List<TimeSlot>();
            }

            var defaultSchedule = new List<TimeSlot>();
            for (int i = 0; i < 24; i++)
            {
                defaultSchedule.Add(new TimeSlot(i, true));
            }
            return defaultSchedule;
        }
    }

    public class TimeSlot
    {
        public int Hour { get; set; }
        public bool IsActive { get; set; }

        public TimeSlot(int hour, bool isActive)
        {
            Hour = hour;
            IsActive = isActive;
        }
    }
}