using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Threading;
using Microsoft.Win32;

namespace CopyFiles
{
    class ViewModel : ViewModelBase
    {
        private readonly Progress<double> _progress;
        private int progress;
        private string _sourcePath = String.Empty;
        private string _destinationPath = String.Empty;


        public ViewModel()
        {
            _progress = new Progress<double>();
            _progress.ProgressChanged += (s, e) =>
            {
                var val = (int)(e * 100);
                Progress = val == 100 ? 0 : val;
            };
        }

        #region Properties
        public int Progress
        {
            get => progress;
            private set
            {
                progress = value;
                OnPropertyChanged();
            }
        }
        public string SourcePath
        {
            get => _sourcePath;
            set
            {
                _sourcePath = value;
                OnPropertyChanged();
            }
        }
        public string DestinationPath
        {
            get => _destinationPath;
            set
            {
                _destinationPath = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        private RelayCommand _copyCommand;
        private RelayCommand _setsourcePath;
        private RelayCommand _setDestinationPath;

        public RelayCommand CopyCommand
        {
            get
            {
                return _copyCommand ??
                       (_copyCommand = new RelayCommand
                           (
                           obj => new Thread(StartCopy).Start(),
                           obj => !String.IsNullOrWhiteSpace(SourcePath) && !String.IsNullOrWhiteSpace(DestinationPath)
                           )
                        );
            }
        }
        public RelayCommand SetSourcePathCommand
        {
            get
            {
                return _setsourcePath ??
                       (_setsourcePath = new RelayCommand
                           (
                               obj =>
                               {
                                   var ofd = new OpenFileDialog();
                                   if (ofd.ShowDialog() == true)
                                       SourcePath = ofd.FileName;
                               }
                           )
                       );
            }
        }
        public RelayCommand SetDestPathCommand
        {
            get
            {
                return _setDestinationPath ??
                       (_setDestinationPath = new RelayCommand
                           (
                               obj =>
                               {
                                   var ofd = new SaveFileDialog();
                                   if (ofd.ShowDialog() == true)
                                       DestinationPath = ofd.FileName;
                               }
                            )
                       );
            }
        }

        #endregion

        #region Methods

        private void StartCopy()
        {
            if (File.Exists(SourcePath))
            {
                if (File.Exists(DestinationPath))
                    File.Delete(DestinationPath);

                new Thread(() => Copy(_progress)).Start();
            }
        }
        private void Copy(IProgress<double> localprogress)
        {
            using (var sourceStream = new FileStream(SourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
            using (var destinationStream = new FileStream(DestinationPath, FileMode.Append, FileAccess.Write, FileShare.Write, 4096))
            {
                byte[] res = new byte[4096];

                while (sourceStream.Position <= sourceStream.Length - 1)
                {
                    var readlength = sourceStream.Position + 4096 <= sourceStream.Length - 1
                        ? 4096
                        : sourceStream.Length - sourceStream.Position;

                    sourceStream.Read(res, 0, (int)readlength);
                    destinationStream.Write(res, 0, (int)readlength);
                    localprogress.Report((double)sourceStream.Position / sourceStream.Length);
                }
            }
        }

        #endregion
    }
}
