﻿using Domain;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using UI.Properties;

namespace UI
{
    public partial class Publisher : UserControl
    {
        private int _frequency = 1;
        private IPublisher _publisher;
        private int _messageCount;

        public event EventHandler? Deleted;

        public Publisher(IPublisher publisher)
        {
            _publisher = publisher;
            InitializeComponent();
        }

        private void Publisher_Load(object sender, EventArgs e)
        {
            SetOptions();
        }

        private void SetOptions()
        {
            ResourceSet? resources = Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true);

            if (resources == null)
            {
                return;
            }

            List<string> options = [];

            foreach (DictionaryEntry entry in resources)
            {
                string key = (string)entry.Key;

                if (key.StartsWith("template-"))
                {
                    string templateName = key.Substring("template-".Length);

                    options.Add(templateName);
                }
            }

            options.Sort();
            comboBox.Items.AddRange(options.ToArray());
            comboBox.SelectedIndex = 0;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy)
            {
                return;
            }
            richTextBox.AppendText($"[{DateTime.Now.ToLocalTime()}] Starting...\n");
            timer.Start();
            string message = GetMessage();
            backgroundWorker.RunWorkerAsync(message);
        }

        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string message = (string)e.Argument!;

            while (!backgroundWorker.CancellationPending)
            {
                int interval = 1000 / _frequency;
                Thread.Sleep(interval);
                _publisher.Publish(message).Wait();
                backgroundWorker.ReportProgress(0);
            }

            e.Cancel = true;
        }

        private string GetMessage()
        {
            string templateName = (string)comboBox.SelectedItem!;
            string key = $"template-{templateName}";
            return Resources.ResourceManager.GetString(key)!;
        }

        private void backgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            _messageCount++;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            richTextBox.AppendText($"[{DateTime.Now.ToLocalTime()}] Messages sent: {_messageCount}\n");
            _messageCount = 0;
        }

        private void buttonEnd_Click(object sender, EventArgs e)
        {
            if (backgroundWorker.WorkerSupportsCancellation)
            {
                backgroundWorker.CancelAsync();
            }

            timer.Stop();
            richTextBox.ResetText();
        }

        private void buttonErase_Click(object sender, EventArgs e)
        {
            if (Deleted != null)
            {
                Deleted(this, EventArgs.Empty);
            }
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            _frequency = trackBar.Value;
        }

        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            string[] lines = richTextBox.Text.Split('\n');

            if (lines.Length > 6)
            {
                richTextBox.Text = string.Join("\n", lines.Skip(lines.Length - 6));
            }
        }
    }
}
