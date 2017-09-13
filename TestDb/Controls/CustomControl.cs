using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TestDb.Controls
{
    public class CustomControl : Control
    {
        public CustomControl()
        {
            this.DefaultStyleKey = typeof(CustomControl);
        }

        public object Content1
        {
            get { return GetValue(Content1Property); }
            set { SetValue(Content1Property, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Content1Property =
            DependencyProperty.Register("Content1", typeof(object), typeof(CustomControl), null);

        public object Content2
        {
            get { return GetValue(Content2Property); }
            set { SetValue(Content2Property, value); }
        }

        // Using a DependencyProperty as the backing store for Content2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Content2Property =
            DependencyProperty.Register("Content2", typeof(object), typeof(CustomControl), null);


        public object Content3
        {
            get { return GetValue(Content3Property); }
            set { SetValue(Content3Property, value); }
        }

        // Using a DependencyProperty as the backing store for Content3.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Content3Property =
            DependencyProperty.Register("Content3", typeof(object), typeof(CustomControl), null);
    }
}
