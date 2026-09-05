using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATS.MiniForm {
    public partial class FormSetDataDrid : Form {
        public FormSetDataDrid(fMain main_) {
            InitializeComponent();
            main = main_;
        }

        private static FormSetDataDrid instance; // Static instance of the form
        private fMain main;

        public static FormSetDataDrid GetInstance(fMain main_) {
            if (instance == null || instance.IsDisposed)
                instance = new FormSetDataDrid(main_);
            return instance;
        }

        private void FormSetDataDrid_Load(object sender, EventArgs e) {
            try
            {
                tb_step.Text = main.setupPay.read_text(Config.LengthStep, Config.FileName);
                tb_spec.Text = main.setupPay.read_text(Config.LengthSpec, Config.FileName);
                tb_measure.Text = main.setupPay.read_text(Config.LengthMeasure, Config.FileName);
                tb_result.Text = main.setupPay.read_text(Config.LengthResult, Config.FileName);

                int stepValue, specValue, measureValue, resultValue;
                if (int.TryParse(tb_step.Text, out stepValue))
                    hsb_step.Value = stepValue;
                if (int.TryParse(tb_spec.Text, out specValue))
                    hsb_spec.Value = specValue;
                if (int.TryParse(tb_measure.Text, out measureValue))
                    hsb_measure.Value = measureValue;
                if (int.TryParse(tb_result.Text, out resultValue))
                    hsb_result.Value = resultValue;
            } catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., display an error message)
                MessageBox.Show("An error occurred while loading the data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tb_step_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Suppress the Enter key to prevent its default behavior

                string enteredText = tb_step.Text.Trim();
                if (!string.IsNullOrEmpty(enteredText))
                {
                    int stepValue;
                    if (int.TryParse(enteredText, out stepValue))
                    {
                        // Additional validation if needed
                        if (stepValue >= hsb_step.Minimum && stepValue <= hsb_step.Maximum)
                        {
                            hsb_step.Value = stepValue;
                            for (int i = 1; i <= main.configTester.numHead; i++)
                            {
                                DataGridSetting(main.dataGridViews[i - 1], "STEP", stepValue);
                            }
                            main.setupPay.write_text(Config.LengthStep, stepValue.ToString(), Config.FileName);
                        }
                        else
                        {
                            MessageBox.Show("Entered value is out of range.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Optionally, you can reset the textbox value or perform other error handling actions
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid numeric input.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // Optionally, you can reset the textbox value or perform other error handling actions
                    }
                }
            }
        }

        private void tb_spec_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Suppress the Enter key to prevent its default behavior

                string enteredText = tb_spec.Text.Trim();
                if (!string.IsNullOrEmpty(enteredText))
                {
                    int value;
                    if (int.TryParse(enteredText, out value))
                    {
                        // Additional validation if needed
                        if (value >= hsb_spec.Minimum && value <= hsb_spec.Maximum)
                        {
                            hsb_spec.Value = value;
                            for (int i = 1; i <= main.configTester.numHead; i++)
                            {
                                DataGridSetting(main.dataGridViews[i - 1], "SPEC", value);
                            }
                            main.setupPay.write_text(Config.LengthSpec, value.ToString(), Config.FileName);
                        }
                        else
                        {
                            MessageBox.Show("Entered value is out of range.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Optionally, you can reset the textbox value or perform other error handling actions
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid numeric input.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // Optionally, you can reset the textbox value or perform other error handling actions
                    }
                }
            }
        }

        private void tb_measure_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Suppress the Enter key to prevent its default behavior

                string enteredText = tb_measure.Text.Trim();
                if (!string.IsNullOrEmpty(enteredText))
                {
                    int value;
                    if (int.TryParse(enteredText, out value))
                    {
                        // Additional validation if needed
                        if (value >= hsb_measure.Minimum && value <= hsb_measure.Maximum)
                        {
                            hsb_measure.Value = value;
                            for (int i = 1; i <= main.configTester.numHead; i++)
                            {
                                DataGridSetting(main.dataGridViews[i - 1], "MEASURE", value);
                            }
                            main.setupPay.write_text(Config.LengthMeasure, value.ToString(), Config.FileName);
                        }
                        else
                        {
                            MessageBox.Show("Entered value is out of range.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Optionally, you can reset the textbox value or perform other error handling actions
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid numeric input.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // Optionally, you can reset the textbox value or perform other error handling actions
                    }
                }
            }
        }

        private void tb_result_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Suppress the Enter key to prevent its default behavior

                string enteredText = tb_result.Text.Trim();
                if (!string.IsNullOrEmpty(enteredText))
                {
                    int value;
                    if (int.TryParse(enteredText, out value))
                    {
                        // Additional validation if needed
                        if (value >= hsb_result.Minimum && value <= hsb_result.Maximum)
                        {
                            hsb_result.Value = value;
                            for (int i = 1; i <= main.configTester.numHead; i++)
                            {
                                DataGridSetting(main.dataGridViews[i - 1], "RESULT", value);
                            }
                            main.setupPay.write_text(Config.LengthResult, value.ToString(), Config.FileName);
                        }
                        else
                        {
                            MessageBox.Show("Entered value is out of range.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Optionally, you can reset the textbox value or perform other error handling actions
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid numeric input.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // Optionally, you can reset the textbox value or perform other error handling actions
                    }
                }
            }
        }

        private void hsb_step_Scroll(object sender, ScrollEventArgs e) {
            try
            {
                int newValue = hsb_step.Value;
                tb_step.Text = newValue.ToString();
                for (int i = 1; i <= main.configTester.numHead; i++)
                {
                    DataGridSetting(main.dataGridViews[i - 1], "STEP", newValue);
                }
                main.setupPay.write_text(Config.LengthStep, newValue.ToString(), Config.FileName);
            } catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., display an error message)
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void hsb_spec_Scroll(object sender, ScrollEventArgs e) {
            try
            {
                int newValue = hsb_spec.Value;
                tb_spec.Text = newValue.ToString();
                for (int i = 1; i <= main.configTester.numHead; i++)
                {
                    DataGridSetting(main.dataGridViews[i - 1], "SPEC", newValue);
                }
                main.setupPay.write_text(Config.LengthSpec, newValue.ToString(), Config.FileName);
            } catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., display an error message)
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void hsb_measure_Scroll(object sender, ScrollEventArgs e) {
            try
            {
                int newValue = hsb_measure.Value;
                tb_measure.Text = newValue.ToString();
                for (int i = 1; i <= main.configTester.numHead; i++)
                {
                    DataGridSetting(main.dataGridViews[i - 1], "MEASURE", newValue);
                }
                main.setupPay.write_text(Config.LengthMeasure, newValue.ToString(), Config.FileName);
            } catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., display an error message)
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void hsb_result_Scroll(object sender, ScrollEventArgs e) {
            try
            {
                int newValue = hsb_result.Value;
                tb_result.Text = newValue.ToString();
                for (int i = 1; i <= main.configTester.numHead; i++)
                {
                    DataGridSetting(main.dataGridViews[i - 1], "RESULT", newValue);
                }
                main.setupPay.write_text(Config.LengthResult, newValue.ToString(), Config.FileName);
            } catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., display an error message)
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public class Config {
            public const string FileName = "dataGrid_config";
            public const string LengthStep = "Length Step";
            public const string LengthSpec = "Length Spec";
            public const string LengthMeasure = "Length Measure";
            public const string LengthResult = "Length Result";
        }

        private void DataGridSetting(DataGridView dataGridView, string headGrid, int value) {
            DataGridViewColumn column = dataGridView.Columns
                    .Cast<DataGridViewColumn>()
                    .FirstOrDefault(c => c.HeaderText.Equals(headGrid, StringComparison.OrdinalIgnoreCase));

            if (column != null)
            {
                column.Width = value;
            }
        }
    }
}

