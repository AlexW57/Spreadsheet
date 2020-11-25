namespace Cpts321
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ExpressionTree
    {
        // Class variables
        private static Dictionary<string, double> variables = new Dictionary<string, double>();
        private ExpressionNode root = null;      

        // Default constructor for expression tree
        public ExpressionTree()
        {
        }

        public ExpressionTree(string expression)
        {
            this.root = BuildTree(expression);
        }

        // Clears all values from dictionary list
        public void ClearDictionary()
        {
            variables.Clear();
        }

        public void SetVar(string varName, double varValue)
        {
            if (!variables.ContainsKey(varName))
            {
                variables.Add(varName, varValue);
            }
            else
            {
                variables[varName] = varValue;
            }
        }

        // Returns the variables in the variable dictionary
        public string[] GetVars()
        {
            string[] variableList = new string[variables.Count];
            int index = 0;
            foreach (KeyValuePair<string, double> item in variables)
            {
                variableList[index] = item.Key;
                index++;
            }
            return variableList;
        }

        // Returns the keys in the variables dictionary
        public List<string> VariablesInExpression
        {
            get
            {
                return new List<string>(variables.Keys);
            }
        }

        // Adds variable to the variable dictionary
        public void SetVariable(string variableName, double variableValue)
        {
            variables[variableName] = variableValue;
        }

        public double Evaluate()
        {
            return this.Evaluate(this.root);
        }

        private static ExpressionNode BuildTree(string s)
        {
            // Counts number of parenthesis
            int count = 0;

            // Checks if the string is empty
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            // Check for extra parentheses
            if (s[0] == '(')
            {
                count++;
                for (int stringIndex = 1; stringIndex < s.Length; stringIndex++)
                {
                    // Checks for open parenthesis
                    if (s[stringIndex] == '(')
                    {
                        stringIndex++;
                    }
                    // Checks for end parenthesis
                    else if (s[stringIndex] == ')')
                    {
                        count--;
                        // if the counter is 0 check where we are
                        if (count == 0)
                        {
                            if (stringIndex != s.Length - 1)
                            {
                                break;
                            }
                            else
                            {
                                // Removes outer parenthesis and recursively calls itself
                                return BuildTree(s.Substring(1, s.Length - 2));
                            }
                        }
                    }
                }
            }

            // Array of possible operators
            char[] operators = { '+', '-', '*', '/' };
            foreach (char op in operators)
            {
                ExpressionNode tempNode = BuildTreeHelper(s, op);
                if (tempNode != null)
                {
                    return tempNode;
                }
            }

            double newNumber;
            if (double.TryParse(s, out newNumber))
            {
                // Creates and returns a new ConstantNode
                return new ConstantNode()
                {
                    Constant = newNumber
                };
            }
            else
            {
                // Creates and returns a new VariableNode
                VariableNode temp = new VariableNode();
                temp.Variable = s;
                variables[s] = 0;
                return temp;
            }
        }

        // Helper function to build the tree
        private static ExpressionNode BuildTreeHelper(string expression, char op)
        {
            int count = 0;
            // OperatorNodeFactory creates all operator nodes for the tree
            OperatorNodeFactory opFactory = new OperatorNodeFactory();
            for (int stringIndex = expression.Length - 1; stringIndex >= 0; stringIndex--)
            {
                // Lowers counter for open parenthesis
                if (expression[stringIndex] == '(')
                {
                    count--;
                }

                // Increases counter for closed parenthesis
                else if (expression[stringIndex] == ')')
                {
                    count++;
                }

                if (count == 0 && expression[stringIndex] == op)
                {
                    OperatorNode tempNode = opFactory.CreateOperatorNode(expression[stringIndex]);
                    tempNode.Left = BuildTree(expression.Substring(0, stringIndex));
                    tempNode.Right = BuildTree(expression.Substring(stringIndex + 1));
                    return tempNode;
                }
            }

            return null;
        }
        
        private double Evaluate(ExpressionNode node)
        {
            // OperatorNode evaluation
            OperatorNode tempNode = this.root as OperatorNode;
            char onlyOP = tempNode.Operator;
            ConstantNode constantNode = node as ConstantNode;
            if (constantNode != null)
            {
                // Returns constant
                return constantNode.Constant;
            }

            // Variable evaluation
            VariableNode variableNode = node as VariableNode;
            if (variableNode != null)
            {
                // Check if dictionary is empty, if so it returns 0 since the variable hasn't been set
                if (variables.Count() == 0)
                {
                    return 0;
                }
                else
                {
                    // Returns the variable name in the dictionary
                    return variables[variableNode.Variable];
                }
            }

            // Determines arithmetic operation based on operator character
            OperatorNode operatorNode = node as OperatorNode;
            if (operatorNode != null)
            {
                switch (operatorNode.Operator)
                {
                    case '+':
                            return this.Evaluate(operatorNode.Left) + this.Evaluate(operatorNode.Right);
                    case '-':
                        return this.Evaluate(operatorNode.Left) - this.Evaluate(operatorNode.Right);
                    case '*':
                        return this.Evaluate(operatorNode.Left) * this.Evaluate(operatorNode.Right);
                    case '/':
                        return this.Evaluate(operatorNode.Left) / this.Evaluate(operatorNode.Right);
                    default: 
                        throw new NotSupportedException(
                            "Operator " + operatorNode.Operator.ToString() + " not supported.");
                }
            }

            throw new NotSupportedException();
        }
    }

    // Abstract ExpressionNode class which all the other nodes inherit from
    internal abstract class ExpressionNode
    {
        public abstract double Evaluate();
    }
}