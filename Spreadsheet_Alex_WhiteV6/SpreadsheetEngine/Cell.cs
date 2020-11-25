namespace Cpts321
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;

    public interface ICmd
    {
        ICmd RunICmd();
    }

    public abstract class Cell : INotifyPropertyChanged
    {
        // Class variables
        private string text;
        private string value;
        private uint bgColor;
        private int rowIndex;
        private int columnIndex;
        private readonly string name = "";

        // Property event handler
        public event PropertyChangedEventHandler PropertyChanged;

        // Cell constructor
        public Cell()
        {
        }

        public Cell(int newRowIndex, int newColumnIndex)
        {
            this.rowIndex = newRowIndex;
            this.columnIndex = newColumnIndex;
            this.bgColor = 0xFFFFFFFF;
        }

        public Cell(int newRowIndex, int newColumnIndex, string newName)
        {
            this.rowIndex = newRowIndex;
            this.columnIndex = newColumnIndex;
            this.name = newName;
        }

        // Getter/Setter for row index
        public int RowIndex
        {
            get
            {
                return this.rowIndex;
            }

            protected set
            {
                this.rowIndex = value;
            }
        }

        // Getter/Setter for column index
        public int ColumnIndex
        {
            get
            {
                return this.columnIndex;
            }

            protected set
            {
                this.columnIndex = value;
            }
        }

        // Getter/Setter for text
        public string Text
        {
            get => this.text;
            set
            {
                if (this.text == value)
                {
                    return;
                }

                this.text = value;
                this.OnPropertyChanged("Text");
            }
        }               

        // Returns the cell hash code for use in the hash table
        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

        // Getter/Setter for value
        public string Value
        {
            get => this.value;
            protected internal set
            {
                if (this.value == value)
                {
                    return;
                }

                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        // Getter/Setter for BGColor
        public uint BGColor
        {
            get => this.bgColor;
            set
            {
                if (this.bgColor == value)
                {
                    return;
                }

                this.bgColor = value;
                this.OnPropertyChanged("BGColor");
            }
        }

        // Cell property changed event
        protected void OnPropertyChanged(string s)
        {
            // Make sure the property is not null
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(s));
            }
        }

        // Writes xml to the output file
        public void WriteXml(XmlWriter outputXml)
        {
            outputXml.WriteStartElement("cell");
            outputXml.WriteElementString("cellname", this.name);
            outputXml.WriteElementString("celltext", this.Text);
            outputXml.WriteElementString("bgcolor", this.bgColor.ToString());
            outputXml.WriteEndElement();
        }
    }

    // Undo and redo for cell text
    public class RestoreText : ICmd
    {
        private Cell cell;
        private string text;


        // Undo/redo for cell text
        public RestoreText(Cell c, string txt)
        {
            cell = c;
            text = txt;
        }

        public ICmd RunICmd()
        {
            var inverse = new RestoreText(cell, cell.Value);
            cell.Value = text;
            return inverse;
        }
    }

    // Undo/Redo for background color
    public class oldCellBG : ICmd
    {
        private Cell cell;
        private uint bgColor;

        public oldCellBG(Cell newCell, uint newBGColor)
        {
            this.cell = newCell;
            this.bgColor = newBGColor;
        }

        public ICmd RunICmd()
        {
            var inverse = new oldCellBG(cell, cell.BGColor);
            cell.BGColor = bgColor;
            return inverse;
        }
    }

    public class MultiCmd : ICmd
    {
        private List<ICmd> commandsList;

        public MultiCmd()
        {
            this.commandsList = new List<ICmd>();
        }

        // Adds a command to the command list
        public void newCommand(ICmd com)
        {
            this.commandsList.Add(com);
        }

        // Executes the ICmd and returns the reverse
        public ICmd RunICmd()
        {
            var reverse = new MultiCmd();
            for (int index = commandsList.Count - 1; index >= 0; index--)
            {
                reverse.commandsList.Add(commandsList[index].RunICmd());
            }

            return reverse;
        }
    }
}