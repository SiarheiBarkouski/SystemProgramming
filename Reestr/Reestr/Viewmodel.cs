using System;
using System.Windows.Media;
using System.Windows.Forms;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace Reestr
{
    class Viewmodel : ViewmodelBase
    {
        private string _firstName;
        private string _secondName;
        private string _surName;
        private Color _color;
        private string _font;
        private bool _isMale;

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (!value.Equals(_firstName))
                {
                    _firstName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SecondName
        {
            get => _secondName;
            set
            {
                if (!value.Equals(_secondName))
                {
                    _secondName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Surname
        {
            get => _surName;
            set
            {
                if (!value.Equals(_surName))
                {
                    _surName = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsMale
        {
            get => _isMale;
            set
            {
                if (_isMale != value)
                {
                    _isMale = value;
                    OnPropertyChanged();
                }
            }

        }
        public bool IsFemale => !_isMale;

        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Font
        {
            get => _font;
            set
            {
                if (!value.Equals(_font))
                {
                    _font = value;
                    OnPropertyChanged();
                }
            }
        }


        public Viewmodel()
        {
            GetValuesFromRegister();
            Application.Current.Exit += (s, e) => SaveValuesToRegister();
        }

        private void SaveValuesToRegister()
        {
            var hklm = Registry.CurrentUser;
            var hkSoftware = hklm.OpenSubKey("Software", true);
            var hkMine = hkSoftware?.CreateSubKey("Reestr");

            if (!string.IsNullOrEmpty(FirstName))
                hkMine?.SetValue("Firstname", FirstName, RegistryValueKind.String);
            if (!string.IsNullOrEmpty(SecondName))
                hkMine?.SetValue("Secondname", SecondName, RegistryValueKind.String);
            if (!string.IsNullOrEmpty(Surname))
                hkMine?.SetValue("Surname", Surname, RegistryValueKind.String);
            if (!string.IsNullOrEmpty(Font))
                hkMine?.SetValue("Font", Font, RegistryValueKind.String);

            hkMine?.SetValue("IsMale", IsMale.ToString(), RegistryValueKind.String);
            hkMine?.SetValue("A", Color.A, RegistryValueKind.DWord);
            hkMine?.SetValue("R", Color.R, RegistryValueKind.DWord);
            hkMine?.SetValue("G", Color.G, RegistryValueKind.DWord);
            hkMine?.SetValue("B", Color.B, RegistryValueKind.DWord);
        }

        private void GetValuesFromRegister()
        {
            var hklm = Registry.CurrentUser;
            var hkSoftware = hklm.OpenSubKey("Software");
            var hkMine = hkSoftware?.OpenSubKey("Reestr");

            FirstName = (string)hkMine?.GetValue("Firstname");
            SecondName = (string)hkMine?.GetValue("Secondname");
            Surname = (string)hkMine?.GetValue("Surname");
            Font = (string)hkMine?.GetValue("Font");

            IsMale = Convert.ToBoolean(hkMine?.GetValue("IsMale"));

            var a = Convert.ToByte(hkMine?.GetValue("A"));
            var r = Convert.ToByte(hkMine?.GetValue("R"));
            var g = Convert.ToByte(hkMine?.GetValue("G"));
            var b = Convert.ToByte(hkMine?.GetValue("B"));
            Color = Color.FromArgb(a, r, g, b);
        }

        
        private RelayCommand _colorDialog;
        public RelayCommand ShowColorDialog
        {
            get
            {
                return _colorDialog ??
                       (_colorDialog = new RelayCommand(obj =>
                       {
                           var cd = new ColorDialog();
                           if (cd.ShowDialog() == DialogResult.OK)
                               Color = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
                       }));
            }
        }

        private RelayCommand _fontDialog;
        public RelayCommand ShowFontDialog
        {
            get
            {
                return _fontDialog ??
                       (_fontDialog = new RelayCommand(obj =>
                       {
                           var fd = new FontDialog();
                           if (fd.ShowDialog() == DialogResult.OK)
                               Font = fd.Font.Name;
                       }));
            }
        }
    }
}
