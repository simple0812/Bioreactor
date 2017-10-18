using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DirectiveTest.Commands;
using DirectiveTest.ScreenKeyBorad.Controls;

namespace DirectiveTest.ScreenKeyBorad.ViewModels
{
    public class BoolToSolidBrushConverter : IValueConverter
    { 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return new SolidColorBrush(Color.FromArgb(128, 128, 128, 128));
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class KeyboardViewModel : ViewModel
    {
        #region constructor
        public KeyboardViewModel(OnScreenKeyBoard container)
        {
            this.container = container;
            KeyModel.theKeyboardViewModel = this;
        }
        #endregion constructor

        #region Commanding

        #region BackspaceCommand

        public RelayCommand BackspaceCommand
        {
            get { return backspaceCommand ?? (backspaceCommand = new RelayCommand((x) => ExecuteBackspaceCommand())); }
        }

        public void ExecuteBackspaceCommand()
        {
            int currentSelectionStart = OnScreenKeyBoard.Buffer.SelectionStart;
            int currentSelectionLength = OnScreenKeyBoard.Buffer.SelectionLength;

            if (currentSelectionLength != 0)
            {
                container.OutputString = container.OutputString.Remove(currentSelectionStart, currentSelectionLength);
                OnScreenKeyBoard.Buffer.SelectionLength = 0;
            }

            else if (OnScreenKeyBoard.Buffer.SelectionStart > 0)
            {
                container.OutputString = container.OutputString.Remove(currentSelectionStart - 1, 1);
                if (OnScreenKeyBoard.Buffer.SelectionStart > 0)
                {
                    OnScreenKeyBoard.Buffer.SelectionStart--;
                }
            }
        }

        private RelayCommand backspaceCommand;

        #endregion BackspaceCommand

        #region CapsLockCommand

        public RelayCommand CapsLockCommand
        {
            get
            {
                if (capsLockCommand == null)
                {
                    capsLockCommand = new RelayCommand((x) => ExecuteCapsLockCommand());
                }
                return capsLockCommand;
            }
        }

        #region ExecuteCapsLockCommand
        /// <summary>
        /// Execute the CapsLockCommand, which ocurrs when the user clicks on the CAPS button.
        /// </summary>
        public void ExecuteCapsLockCommand()
        {
            IsCapsLock = !IsCapsLock;
        }

        #endregion

        private RelayCommand capsLockCommand;

        #endregion CapsLockCommand

        #region KeyPressedCommand

        public RelayCommand KeyPressedCommand => keyPressedCommand ?? (keyPressedCommand = new RelayCommand(ExecuteKeyPressedCommand));

        #region ExecuteKeyPressedCommand
        /// <summary>
        /// Execute the KeyPressedCommand.
        /// </summary>
        /// <param name="arg">The KeyModel of the key-button that was pressed</param>
        public void ExecuteKeyPressedCommand(object arg)
        {
            //enter键
            try
            {
                if (arg != null && arg.ToString() == "\n")
                {
                    this.container.Visibility = Visibility.Collapsed;
                    container.Detach();
                    return;
                }
                if (container.OutputString != null)
                {
                    int currentSelectionStart = OnScreenKeyBoard.Buffer.SelectionStart;
                    int currentSelectionLength = OnScreenKeyBoard.Buffer.SelectionLength;

                    if (currentSelectionLength != 0)
                    {
                        container.OutputString = container.OutputString.Remove(currentSelectionStart, currentSelectionLength);
                        OnScreenKeyBoard.Buffer.SelectionLength = 0;
                    }
                    container.OutputString = container.OutputString.Insert(currentSelectionStart, (string)arg);
                    OnScreenKeyBoard.Buffer.SelectionStart++;

                    //Return to un-shift mode if currently in shift mode
                    if (IsShiftLock)
                    {
                        IsShiftLock = false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("ExecuteKeyPressedCommand->" + e.Message);
                //
            }

        }
        #endregion

        private RelayCommand keyPressedCommand;

        #endregion KeyPressedCommand

        #region ShiftCommand

        public RelayCommand ShiftCommand
        {
            get { return shiftCommand ?? (shiftCommand = new RelayCommand((x) => ExecuteShiftCommand())); }
        }

        #region ExecuteShiftCommand
        /// <summary>
        /// Execute the ShiftCommand, which ocurrs when the user clicks on the SHIFT key.
        /// </summary>
        public void ExecuteShiftCommand()
        {
            ToggleShiftState();
        }
        #endregion


        private RelayCommand shiftCommand;

        #endregion ShiftCommand

        #endregion Commanding

        #region Methods

        #region NotifyTheIndividualKeys
        /// <summary>
        /// Make the individual key-button view models notify their views that their properties have changed.
        /// </summary>
        public void NotifyTheIndividualKeys(string notificationText)
        {
            if (KB?.KeyAssignments == null) return;
            if (notificationText != null)
            {
                foreach (var keyModel in KB.KeyAssignments)
                {
                    keyModel?.Notify(notificationText);
                }
            }
        }

        #endregion

        #region ToggleShiftState
        /// <summary>
        /// Turn the shift-lock mode off if it's on, and on if it's off.
        /// </summary>
        public void ToggleShiftState()
        {
            IsShiftLock = !IsShiftLock;
        }

        #endregion

        #endregion Methods

        #region Properties

        #region IsCapsLock
        /// <summary>
        /// Get/set whether the VirtualKeyboard is currently is Caps-Lock mode.
        /// </summary>
        public bool IsCapsLock
        {
            get
            {
                return isCapsLock;
            }
            set
            {
                if (value != isCapsLock)
                {
                    isCapsLock = value;
                    if (IsShiftLock)
                    {
                        IsShiftLock = false;
                    }
                    else
                    {
                        NotifyTheIndividualKeys("Text");
                    }
                    Notify("IsCapsLock");
                }
            }
        }
        #endregion

        #region IsShiftLock
        /// <summary>
        /// Get/set whether the VirtualKeyboard is currently is Shift-Lock mode.
        /// </summary>
        public bool IsShiftLock
        {
            get
            {
                return shiftKey.IsInShiftState;
            }
            set
            {
                if (value != shiftKey.IsInShiftState)
                {
                    shiftKey.IsInShiftState = value;
                    NotifyTheIndividualKeys("Text");
                    Notify("IsShiftLock");
                }
            }
        }
        #endregion

        #region KB
        /// <summary>
        /// Get/set the specific subclass of KeyAssignmentSet that is currently attached to this keyboard.
        /// </summary>
        public KeyAssignmentSet KB => _currentKeyboardAssignment ?? (_currentKeyboardAssignment = KeyAssignmentSet.KeyAssignment);

        #endregion

        #region The view-model properties for the individual key-buttons of the keyboard

        public KeyModel TabKey { get; set; } = new KeyModel('\t', '\t');

        public KeyModel EnterKey { get; set; } = new KeyModel('\n', '\n');

        public ShiftKeyViewModel ShiftKey => shiftKey;

        #endregion The view-model properties for the individual key-buttons of the keyboard

        #region TheKeyModels array
        /// <summary>
        /// Get the array of KeyModels from the KB that comprise the view-models for the keyboard key-buttons
        /// </summary>
        public KeyModel[] TheKeyModels => KB?.KeyAssignments;

        #endregion

        #endregion Properties

        #region fields

        private bool isCapsLock;

        /// <summary>
        /// The KeyAssignmentSet that is currently attached to this keyboard.
        /// </summary>
        private KeyAssignmentSet _currentKeyboardAssignment;
        // These are view-models for the individual key-buttons of the keyboard which are not provided by the KeyAssignmentSet:
        private ShiftKeyViewModel shiftKey = new ShiftKeyViewModel();
        private OnScreenKeyBoard container = null;

        #endregion fields
    }
}
