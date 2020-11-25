namespace Cpts321
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    public class Spreadsheet
    {
        // Class variables
        private static string[] columnHeaders;
        private static string columnLetters = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
        private int rows, columns;
        private SpreadsheetCell[,] cellArray;              
        private bool error = false;
        private string errorText = string.Empty;

        // Undo/Redo stacks
        private Stack<ICmd> undoStack = new Stack<ICmd>();
        private Stack<ICmd> redoStack = new Stack<ICmd>();

        // Dictionary stores cell dependencies
        private Dictionary<Cell, HashSet<Cell>> dependencies;

        // Default constructor
        public Spreadsheet(int newRows, int newColumns)
        {
            this.rows = newRows;
            this.columns = newColumns;
            this.cellArray = new SpreadsheetCell[this.rows, this.columns];
            this.dependencies = new Dictionary<Cell, HashSet<Cell>>();
            columnHeaders = CreateColumnHeaders(this.columns);
            for (int rowIndex = 0; rowIndex < this.rows; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < this.columns; columnIndex++)
                {
                    this.cellArray[rowIndex, columnIndex] = new SpreadsheetCell(rowIndex, columnIndex, columnHeaders[columnIndex] + (rowIndex + 1).ToString());
                    this.cellArray[rowIndex, columnIndex].PropertyChanged += new PropertyChangedEventHandler(this.OnCellPropertyChanged);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Error
        {
            get
            {
                return this.error;
            }

            set
            {
                this.error = value;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return this.errorText;
            }

            set
            {
                this.errorText = value;
            }
        }

        public static string[] CreateColumnHeaders(int numberOfColumns)
        {
            string[] alphabet = columnLetters.Split(new char[] { ',' });
            string[] columnHeaders = new string[numberOfColumns];
            for (int i = 0; i < numberOfColumns; ++i)
            {
                string header = string.Empty;
                                
                for (int j = 0; j <= i / alphabet.Length; j++)
                {
                    header += alphabet[i % alphabet.Length];
                }

                columnHeaders.SetValue(header, i);
            }

            return columnHeaders;
        }

        public void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Cell c = sender as Cell;
            switch (e.PropertyName)
            {
                case "Text":
                    try
                    {
                        string tempCell = ((Cell)sender).Value;
                        ICmd temp = new RestoreText((Cell)sender, tempCell);
                        this.AddUndo(temp);
                        this.RemoveDependencies(c);
                        this.DetermineCellValue(c);
                    }
                    catch (Exception ex)
                    {
                        error = true;
                        errorText = ex.Message;
                    }

                    this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("Text"));    // Pass along event to whoever uses this class
                    break;
                case "Value":
                    try
                    {
                        this.CircularReferenceCheck(c);
                    }
                    catch (Exception ex)
                    {
                        error = true;
                        errorText = ex.Message;
                    }

                    this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("Value"));
                    break;
                case "BGColor":
                    try
                    {
                        //uint tempCell = ((Cell)sender).BGColor;
                        //ICmd temp = new oldCellBG((Cell)sender, tempCell);
                        //this.AddUndo(temp);
                        this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("BGColor"));
                    }
                    catch (Exception ex)
                    {
                        error = true;
                        errorText = ex.Message;
                    }

                    //this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("BGColor"));    // Pass along event to whoever uses this class
                    break;
                default:
                    break;
            }
        }

        // Return cell at given index
        public Cell GetCell(int row, int column)
        {
            if (row <= this.rows && row >= 0 && column <= this.columns && column >= 0)
            {
                return this.cellArray[row, column];
            }
            else
            {
                return null;
            }
        }

        public int GetRows()
        {
            return this.rows;
        }

        public int GetColumns()
        {
            return this.columns;
        }

        public void Demo()
        {
            this.cellArray[5, 5].Value = "20";
            this.cellArray[5, 6].Value = "30";
            for (int i = 0; i < 50; i++)
            {
            }
        }

        /*Adds an undo command to the undoStack*/
        public void AddUndo(ICmd inverse)
        {
            this.undoStack.Push(inverse);
        }

        public ICmd PopUndo()
        {
            return this.undoStack.Pop();
        }

        /*Adds a redo command to the redoStack, only called when undo button is clicked*/
        public void AddRedo(ICmd inverse)
        {
            this.redoStack.Push(inverse);
        }

        public ICmd PopRedo()
        {
            return this.redoStack.Pop();
        }

        /*check if undo stack is empty*/
        public bool IsUndoStackEmpty()
        {
            if (this.undoStack.Count == 0)
            {
                return true;
            }

            return false;
        }

        /*check if redo stack is empty*/
        public bool IsRedoStackEmpty()
        {
            if (this.redoStack.Count == 0)
            {
                return true;
            }

            return false;
        }

        private void RemoveDependencies(Cell cell)
        {
            if (this.dependencies.ContainsKey(cell))
            {
                this.dependencies[cell].Clear();
            }

            this.dependencies.Remove(cell);
        }

        private bool ReferenceCycles(Cell root, Cell cell)
        {
            bool check = true;
            if (!this.dependencies.ContainsKey(cell))
            {
                return true;
            }                                   
            
            foreach (Cell currentCell in this.dependencies[cell])
            {
                if (ReferenceEquals(currentCell, root))
                {
                    return false;
                }

                check = check && this.ReferenceCycles(root, currentCell);
            }

            return check;
        }

        // Checks the spreadsheet for circular references and dependencies and throws an error if a circular reference is found
        private void CircularReferenceCheck(Cell cell)
        {
            if (this.ReferenceCycles(cell, cell))
            {
                foreach (Cell key in this.dependencies.Keys)
                {
                    if (this.dependencies[key].Contains(cell))
                    {
                        this.DetermineCellValue(key);
                    }
                }
            }
            else
            {
                //throw new Exception("Error: Circular reference");
            }
        }

        // Evaluates the cell expression and throws an error if the equation does not work
        private void DetermineCellValue(Cell cell)
        {
            if (cell.Text.Length == 0)
            {
                cell.Value = string.Empty;
            }
            else if (cell.Text.StartsWith("="))
            {
                if (cell.Text.Length > 1)
                {
                    try
                    {
                        this.EvaluateFormula(cell);
                    }
                    catch
                    {
                        // The equation was not formatted properly, so error is printed on the cell
                        cell.Value = "Error: Wrong equation format";
                        throw;
                    }
                }
                else
                {
                    throw new ArgumentException("Error: Improper equation");
                }
            }
            else
            {
                cell.Value = cell.Text;
            }
        }

        // Evaluates the cell formula
        private void EvaluateFormula(Cell cell)
        {
            try
            {
                ExpressionTree expTree = new ExpressionTree(cell.Text.Substring(1));
                foreach (string cellName in expTree.VariablesInExpression)
                {
                    int[] indices = this.ReferenceIndexes(cellName);
                    Cell cellReliesOnThisGuy = this.GetCell(indices[0], indices[1]);
                    if (!this.dependencies.ContainsKey(cell))
                    {
                        this.dependencies.Add(cell, new HashSet<Cell>());
                    }

                    this.dependencies[cell].Add(cellReliesOnThisGuy);
                    bool success = double.TryParse(cellReliesOnThisGuy.Value, out double result);
                    if (success)
                    {
                        expTree.SetVariable(cellName, result);
                    }
                    else
                    {
                        expTree.SetVariable(cellName, 0.0);
                    }
                }

                cell.Value = expTree.Evaluate().ToString();
            }
            catch
            {
                throw;
            }
        }
        
        // Finds the indexes that are referenced in the spreadsheet
        private int[] ReferenceIndexes(string reference)
        {
            int[] indexes = new int[this.cellArray.Rank];
            int index = 0, repeatedLetters = 0;
            char currentCharacter = char.ToUpper(reference[0]);
            for (index = 0; index < reference.Length; ++index)
            {
                if (currentCharacter != reference[index])
                {
                    break;
                }

                // Increments when a repeated letter is found
                repeatedLetters++;
            }

            indexes[0] = int.Parse(reference.Substring(index));
            indexes[0] -= 1;
            string[] string_alphabet = columnLetters.Split(new char[] { ',' });
            string alphabet = string.Join(string.Empty, string_alphabet, 0, string_alphabet.Length);
            indexes[1] = (alphabet.Length * --repeatedLetters) + alphabet.IndexOf(currentCharacter);
            return indexes;
        }

        // Checks for valid formula
        private bool CheckValidFormulaVariables(string[] variables)
        {
            bool formula = true;
            foreach (string text in variables)
            {
                // ASCII conversion
                int column = text[0] - 'A';
                if (column > 25 || column < 0)
                {
                    formula = false;
                }

                int row;
                if (int.TryParse((text.Substring(1)), out row))
                {
                    if ((row - 1) < 0 || (row - 1) > 50)
                    {
                        formula = false;
                    }
                }
                else
                {
                    formula = false;
                }

                if (!formula)
                {
                    break;
                }
            }

            return formula;
        }

        // Chekcks for circular dependency
        private bool SelfReference(Cell self, string[] variables)
        {
            bool reference = true;
            Cell temp;
            foreach (string s in variables)
            {
                int col = s[0] - 'A';
                int row;
                if (int.TryParse(s.Substring(1), out row))
                {
                    reference = true;
                    temp = this.GetCell(row - 1, col);
                    if (temp.RowIndex != temp.RowIndex || temp.ColumnIndex != self.ColumnIndex)
                    {
                        reference = false;
                    }

                    if (reference)
                    {
                        break;
                    }
                }
            }

            return reference;
        }


        public void Load(Stream infile)
        {
            // XmlWriter object initialization for reading input
            XDocument xmlReader = XDocument.Load(infile);

            // Traverses all "cell" elements in the xml
            foreach (XElement tag in xmlReader.Root.Elements("cell"))
            {
                // Pulls cell information by its location
                int[] location = ReferenceIndexes(tag.Element("cellname").Value);
                Cell cell = GetCell(location[0], location[1]);
                cell.Text = tag.Element("celltext").Value;
                cell.BGColor = Convert.ToUInt32(tag.Element("bgcolor").Value);
            }
        }

        // Returns true if cell is empty, false otherwise
        public bool IsEmpty(Cell cell)
        {
            uint emptyColor = 0xFFFFFFFF;
            if (cell.Text == "" || cell.Value == "" || cell.BGColor == emptyColor)
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        public void Save(Stream outfile)
        {
            // XmlWriter object initialization for writing output
            XmlWriter outputWriter = XmlWriter.Create(outfile);

            // Create the starting tag/element.
            outputWriter.WriteStartElement("spreadsheet");

            // Traverses all cell in cellArray and adds non-empty cells to the spreadsheet
            foreach (Cell cell in cellArray)
            {
                if (IsEmpty(cell) == false)
                {
                    // Adds the cell to the xml output
                    cell.WriteXml(outputWriter);
                }
            }

            // Close xml file
            outputWriter.WriteEndElement();
            outputWriter.Close();
        }
    }

    public class SpreadsheetCell : Cell
    {
        private int rowIndex;
        private int columnIndex;

        public SpreadsheetCell(int rowIndex, int columnIndex, string name) : base(rowIndex, columnIndex, name) { }

        // This constructor sets SpreadsheetCell indexes
        public SpreadsheetCell(int newRowIndex, int newColumnIndex)
        {
            this.rowIndex = newRowIndex;
            this.columnIndex = newColumnIndex;
        }
    }
}