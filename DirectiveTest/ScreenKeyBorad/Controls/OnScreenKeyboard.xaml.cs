/**
    Copyright(c) Microsoft Open Technologies, Inc.All rights reserved.
   The MIT License(MIT)
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files(the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions :
    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
**/

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DirectiveTest.ScreenKeyBorad.Libs;
using DirectiveTest.ScreenKeyBorad.ViewModels;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace DirectiveTest.ScreenKeyBorad.Controls
{
    public partial class OnScreenKeyBoard : UserControl
    {
        private static ContentBuffer _buffer;
        private static TextBox attachTextBox;

        public static ContentBuffer Buffer => _buffer ?? (_buffer = new ContentBuffer());

        private string _outputstring;
        public string OutputString
        {
            get { return _outputstring; }
            set
            {
                _outputstring = value;
                OnTextBoxTextPropertyChanged(_outputstring);
            }
        }

        private static void OnTextBoxTextPropertyChanged(string val)
        {
            Buffer.Content = val;

            if (attachTextBox != null)
                attachTextBox.Text = Buffer.Content;
        }

        public void Attach(Control control)
        {
            Detach();

            var t = control as TextBox;

           // if (t?.Focus != FocusState.Pointer) return;

            OutputString = t.Text;
            Buffer.Content = t.Text;
            Buffer.SelectionStart = t.SelectionStart;
            Buffer.SelectionLength = t.SelectionLength;

            attachTextBox = t;

            t.SelectionChanged += Target_SelectionChanged;
            t.TextChanged += T_TextChanged;
            SetPosition();

            root.IsOpen = true;
        }

        private void T_TextChanged(object sender, TextChangedEventArgs e)
        {
//            var t = sender as TextBox;
// if (t?.FocusState != FocusState.Pointer) return;

            //            Debug.WriteLine("txt" + t.SelectionStart + "," + t.SelectionLength);
            //            this.OutputString = t.Text;
            //            Buffer.Content = t.Text;
            //            Buffer.SelectionStart = t.SelectionStart;
            //            Buffer.SelectionLength = t.SelectionLength;
        }

        private void SetPosition()
        {
            var w = Application.Current.MainWindow.ActualWidth;//  Window.Current.Bounds.Width;
            var h = Application.Current.MainWindow.ActualHeight;  //Window.Current.Bounds.Height;
            var cw = board.Width;
            var ch = board.Height;

            var frame = (FrameworkElement)Application.Current.MainWindow.Content;


            var p = attachTextBox.TransformToVisual(frame);
            var currentPoint = p.TransformBounds(new Rect(0, 0, 0, 0));

            var txtPos = currentPoint.Y + attachTextBox.ActualHeight;

            //如果虚拟键盘弹出来遮挡住文本框 则将页面整体向上移动
            if (txtPos + ch > h)
            {
                TranslateTransform translate = new TranslateTransform();
                translate.Y = -1 * (txtPos + ch - h);
                frame.RenderTransform = translate;
            }

            //Debug.WriteLine(currentPoint.X + "|" + currentPoint.Y + "," + attachTextBox.Height + "," + attachTextBox.ActualHeight);


            root.VerticalOffset = h - ch;
            root.HorizontalOffset = (w - cw) / 2;
        }

        public void Detach()
        {
            root.IsOpen = false;

            //如果页面移动了 则将页面还原
            var frame = (FrameworkElement)Application.Current.MainWindow.Content;
            TranslateTransform translate = new TranslateTransform();
            translate.X = 0;
            frame.RenderTransform = translate;

            if (null == attachTextBox) return;
            attachTextBox.SelectionChanged -= Target_SelectionChanged;
            attachTextBox.TextChanged -= T_TextChanged;
            attachTextBox = null;
        }

        private void Target_SelectionChanged(object sender, RoutedEventArgs e)
        {

            var t = sender as TextBox;
            if (t != null)
            {
                this.OutputString = t.Text;
                Buffer.Content = t.Text;

                Buffer.SelectionStart = t.SelectionStart;
                Buffer.SelectionLength = t.SelectionLength;
            }

            Debug.WriteLine("change" + t.SelectionStart + "," + t.SelectionLength);
            Debug.WriteLine( FocusManager.GetFocusedElement(Application.Current.MainWindow));
        }

        private OnScreenKeyBoard()
        {
            DataContext = new KeyboardViewModel(this);
            InitializeComponent();
        }

        public static readonly OnScreenKeyBoard Instance = new OnScreenKeyBoard();
    }
}

