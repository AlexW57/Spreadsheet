namespace NUnit.Tests1
{

    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void RunTests()
        {
            TestCreateSpreadsheet();
            TestSpreadsheetRows();
            TestSpreadsheetColumns();
            TestCellCreation();
            TestExpressionTree();
            TestReferencesToOtherCells();
            TestLoad();
            TestSave();
        }


        // Tests that spreadsheet is created and not null
        public void TestCreateSpreadsheet()
        {
            Assert.IsNotNull(new Cpts321.Spreadsheet(50, 26));
        }

        // Tests the number of rows in spreadsheet
        public void TestSpreadsheetRows()
        {
            Assert.IsTrue(new Cpts321.Spreadsheet(50, 26).GetRows() == 50);
            Assert.IsFalse(new Cpts321.Spreadsheet(50, 26).GetRows() == 26);
        }

        // Tests the number of columns in spreadsheet
        public void TestSpreadsheetColumns()
        {
            Assert.IsTrue(new Cpts321.Spreadsheet(50, 26).GetColumns() == 26);
            Assert.IsFalse(new Cpts321.Spreadsheet(50, 26).GetColumns() == 50);
        }

        public void TestCellCreation()
        {
            // Standard case
            Assert.IsNotNull(new Cpts321.Spreadsheet(50, 26).GetCell(1, 1));

            // Lowerbound case
            Assert.IsNotNull(new Cpts321.Spreadsheet(50, 26).GetCell(0, 0));

            // Upperbound case
            Assert.IsNotNull(new Cpts321.Spreadsheet(50, 26).GetCell(49, 25));

            // Out of bounds case
            Assert.IsNull(new Cpts321.Spreadsheet(50, 26).GetCell(51, 27));
        }
                
        public void TestExpressionTree()
        {
            // Tests addittion within the expression tree cell
            Cpts321.ExpressionTree sheet1 = new Cpts321.ExpressionTree("5+5");
            Assert.AreEqual(sheet1.Evaluate(), "10");

            // Tests subtraction within the expression tree cell
            Cpts321.ExpressionTree sheet2 = new Cpts321.ExpressionTree("5-5");
            Assert.AreEqual(sheet1.Evaluate(), "0");

            // Tests multiplication within the expression tree cell
            Cpts321.ExpressionTree sheet3 = new Cpts321.ExpressionTree("5*5");
            Assert.AreEqual(sheet1.Evaluate(), "25");

            // Tests division within the expression tree cell
            Cpts321.ExpressionTree sheet4 = new Cpts321.ExpressionTree("10/5");
            Assert.AreEqual(sheet1.Evaluate(), "2");

            // Tests mixed operators within the expression tree cell
            Cpts321.ExpressionTree sheet5 = new Cpts321.ExpressionTree("10*2+5");
            Assert.AreEqual(sheet5.Evaluate(), "25");

            // Tests mixed operators within the expression tree cell
            Cpts321.ExpressionTree sheet6 = new Cpts321.ExpressionTree("10/2+5");
            Assert.AreEqual(sheet5.Evaluate(), "10");

            // Tests mixed operators with parenthesis within the expression tree cell
            Cpts321.ExpressionTree sheet7 = new Cpts321.ExpressionTree("10*(2+5)");
            Assert.AreEqual(sheet5.Evaluate(), "70");

            // Tests mixed operators with parenthesis within the expression tree cell
            Cpts321.ExpressionTree sheet8 = new Cpts321.ExpressionTree("(12*5)/(10*2)");
            Assert.AreEqual(sheet5.Evaluate(), "3");
        }

        public void TestReferencesToOtherCells()
        {
            // Tests adding cells together
            Cpts321.ExpressionTree A1 = new Cpts321.ExpressionTree("5");
            Cpts321.ExpressionTree A2 = new Cpts321.ExpressionTree("A1+5");
            Assert.AreEqual(A2.Evaluate(), "10");

            // Tests subtracting cells
            Cpts321.ExpressionTree A3 = new Cpts321.ExpressionTree("5");
            Cpts321.ExpressionTree A4 = new Cpts321.ExpressionTree("10-A1");
            Assert.AreEqual(A4.Evaluate(), "5");

            // Tests multuplying cells
            Cpts321.ExpressionTree A5 = new Cpts321.ExpressionTree("5");
            Cpts321.ExpressionTree A6 = new Cpts321.ExpressionTree("5*A1");
            Assert.AreEqual(A6.Evaluate(), "25");

            // Tests dividing cells
            Cpts321.ExpressionTree A7 = new Cpts321.ExpressionTree("5");
            Cpts321.ExpressionTree A8 = new Cpts321.ExpressionTree("10/A1");
            Assert.AreEqual(A8.Evaluate(), "2");
        }             

        // Tests spreadsheet cell variable bgColor
        public void TestBGColor()
        {
            Cpts321.SpreadsheetCell a = new Cpts321.SpreadsheetCell(0,0);

            // Tests that the cell initialized properly
            Assert.IsNotNull(a.BGColor);

            // Tests that bgColor was set to white by default
            Assert.AreEqual(a.BGColor, 0xFFFFFFFF);

            // Tests that bgColor is mutable and setter function works properly
            a.BGColor = 112;
            Assert.AreEqual(a.BGColor, 112);
        }

        // Tests load function
        public void TestLoad()
        {
            Cpts321.Spreadsheet sheet = new Cpts321.Spreadsheet(50, 26);
            Cpts321.Cell tempCell = (sheet.GetCell(1, 1));
            Assert.IsTrue(sheet.IsEmpty(tempCell));
            tempCell.Text = "15";
            Assert.IsFalse(sheet.IsEmpty(tempCell));
        }

        // Tests save function
        public void TestSave()
        {
            Cpts321.Spreadsheet sheet = new Cpts321.Spreadsheet(50, 26);
            Cpts321.Cell tempCell = (sheet.GetCell(1, 1));
            Assert.IsTrue(sheet.IsEmpty(tempCell));
            tempCell.Text = "15";
            Assert.IsFalse(sheet.IsEmpty(tempCell));
        }
    }
}
