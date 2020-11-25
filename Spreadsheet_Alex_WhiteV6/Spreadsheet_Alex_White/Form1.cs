// Alex White
// <copyright file="Form1.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Spreadsheet_Alex_White
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.IO;
    using Cpts321;

    public partial class Form1 : Form
    {
        private Spreadsheet sheet;

        public event PropertyChangedEventHandler PropertyChanged;

        static class Constants
        {
            public const int numberOfColumns = 26;         // currently accepts (1 to inf)
            public const int numberOfRows = 50;
        }

            public Form1()
        {
            InitializeComponent();
            this.sheet = new Spreadsheet(50, 26);
            InitializeEventHandlers();

            // Data grid properties
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.RowHeadersWidth = 50;

            // Sets the initial form
            string[] columns = new string[26] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            for (int index = 0; index < columns.Length; index++)
            {
                this.dataGridView1.Columns.Add(columns[index], columns[index]);
            }

            for (int index = 0; index < 50; index++)
            {
                this.dataGridView1.Rows.Add();
                this.dataGridView1.Rows[index].HeaderCell.Value = (index + 1).ToString();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void InitializeEventHandlers()
        {
            this.dataGridView1.CellBeginEdit += new DataGridViewCellCancelEventHandler(dataGridView1_CellBeginEdit);
            this.dataGridView1.CellEndEdit += new DataGridViewCellEventHandler(dataGridView1_CellEndEdit);
            this.sheet.PropertyChanged += new PropertyChangedEventHandler(OnCellPropertyChanged);
        }

        // Cell begin edit
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = this.sheet.GetCell(e.RowIndex, e.ColumnIndex).Text;
        }

        // Cell end edit
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            Cell editedCell = this.sheet.GetCell(e.RowIndex, e.ColumnIndex); 
            if (dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            {
                editedCell.Text = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            }
            else
            {
                editedCell.Text = string.Empty;
            }

            dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = editedCell.Value;
        }

        // Cell begin edit
        private void dataGridView1_BGColor(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            this.dataGridView1.Rows[((Cell)sender).RowIndex].Cells[((Cell)sender).ColumnIndex].Style.BackColor = this.UIntToColor(((Cell)sender).BGColor);
        }

        private void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Cell tempCell = sender as Cell;
            switch (e.PropertyName)
            {
                case "Value":
                    this.dataGridView1.Rows[tempCell.RowIndex].Cells[tempCell.ColumnIndex].Value = tempCell.Value;
                    if (this.sheet.Error)
                    {
                        MessageBox.Show(this.sheet.ErrorMessage, "Error: Formula does not work", MessageBoxButtons.OK);
                        this.sheet.Error = false;
                        this.sheet.ErrorMessage = string.Empty;
                    }

                    break;
                case "BGColor":
                    this.dataGridView1.Rows[((Cell)sender).RowIndex].Cells[((Cell)sender).ColumnIndex].Style.BackColor = this.UIntToColor(((Cell)sender).BGColor);
                    if (this.sheet.Error)
                    {
                        MessageBox.Show(this.sheet.ErrorMessage, "Error: Color does not work", MessageBoxButtons.OK);
                        this.sheet.Error = false;
                        this.sheet.ErrorMessage = string.Empty;
                    }

                    break;

                default:
                    break;
            }
        }

        // Converts a color to a uint
        private uint ColorToUInt(Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B << 0));
        }
        
        // Converts a unit to color
        private Color UIntToColor(uint color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }

        // Removes all data from cells on the spreadsheet
        private void EraseSheet()
        {
            for (int i = 0; i < Constants.numberOfRows; ++i)
            {
                for (int j = 0; j < Constants.numberOfColumns; ++j)
                {
                    sheet.GetCell(i, j).Text = "";
                    sheet.GetCell(i, j).BGColor = 0xFFFFFFFF;
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.sheet.Demo();
        }
        
        // Redo cell background color button
        private void RedoCellTextChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ICmd temp = this.sheet.PopRedo();
            this.sheet.AddUndo(temp.RunICmd());
            undoChangingCellBackgroundColorToolStripMenuItem.Enabled = true;            
        }

        // Undo cell text change button
        private void UndoChangingCellBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ICmd temp = this.sheet.PopUndo();
            this.sheet.AddRedo(temp.RunICmd());
            redoCellTextChangeToolStripMenuItem.Enabled = true;
        }

        private void ChangeCellBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog myDialog = new ColorDialog();
            MultiCmd allCommands = new MultiCmd();
            if (myDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
                {
                    uint argb = ColorToUInt(myDialog.Color);
                    Cell tempCell = this.sheet.GetCell(cell.RowIndex, cell.ColumnIndex);
                    ICmd temp = new oldCellBG(tempCell, tempCell.BGColor);
                    this.sheet.AddUndo(temp);
                    allCommands.newCommand(temp);
                    this.sheet.GetCell(cell.RowIndex, cell.ColumnIndex).BGColor = argb;
                }

                this.sheet.AddUndo(allCommands);
            }        
        }

        // Edit dropdown menu
        private void EditToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (this.sheet.IsUndoStackEmpty())
            {
                undoChangingCellBackgroundColorToolStripMenuItem.Enabled = false;
            }
            else
            {
                undoChangingCellBackgroundColorToolStripMenuItem.Enabled = true;
            }

            if (this.sheet.IsRedoStackEmpty())
            {
                redoCellTextChangeToolStripMenuItem.Enabled = false;
            }
            else
            {
                redoCellTextChangeToolStripMenuItem.Enabled = true;
            }
        }

        // Load Click
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog loadFileDialog = new OpenFileDialog
            {
                Title = "Select a file to be read and displayed in the spreadsheet.",
                Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*",
                FilterIndex = 2
            };

            // User chose where to open the file from and it was a correct file type
            if (loadFileDialog.ShowDialog() == DialogResult.OK)
            {                
                // Sheet is cleared first to remove merge errors
                EraseSheet();

                // Loads in new spreadsheet file
                using (FileStream sr = new FileStream(loadFileDialog.FileName, FileMode.Open))
                {
                    sheet.Load(sr);
                }
            }
        }

        // Save Click
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                // Window header
                Title = "Save the spreadsheet to a file.",

                // Save options
                Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*",
                FilterIndex = 2
            };

            // User chose where to save file and what format to put it in
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (FileStream save = new FileStream(saveFileDialog1.FileName, FileMode.Create)) 
                {
                    try
                    {
                        sheet.Save(save);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: File was not able to be saved. " + ex.Message);
                    }
                }
            }
        }
        
        // Clear Click
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EraseSheet();
        }
    }
}