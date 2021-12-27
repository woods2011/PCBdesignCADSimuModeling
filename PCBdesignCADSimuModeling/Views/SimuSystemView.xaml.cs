﻿using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PcbDesignCADSimuModeling.Views
{
    public partial class SimuSystemView : UserControl
    {
        public SimuSystemView()
        {
            InitializeComponent();
        }
        
        private void TextBox_KeyEnterUpdate(object sender, KeyEventArgs e)  
        {
            if (e.Key != Key.Enter) return;
                
            var tBox = (TextBox)sender;
            var prop = TextBox.TextProperty;
            BindingOperations.GetBindingExpression(tBox, prop)?.UpdateSource();
        }
    }
}