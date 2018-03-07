using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Threading;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace CopyFiles
{
    class ViewModel : ViewModelBase
    {
        private readonly Progress<long> _progress;
        private int progress;
        private long _currentlyCopied;
        private long _totalFilesSize;
        private string _sourcePath = String.Empty;
        private string _destinationPath = String.Empty;
        private string _folderToCreate = String.Empty;
        private readonly CancellationTokenSource _cancelTokenSource;


        public ViewModel()
        {
            _cancelTokenSource = new CancellationTokenSource();
            _progress = new Progress<long>();
            _progress.ProgressChanged += (s, e) =>
            {
                _currentlyCopied += e;
                Progress = (int)((double)_currentlyCopied / _totalFilesSize * 100);
            };
        }

        #region Properties

        public int Progress
        {
            get => progress;
            private set
            {
                progress = value;
                if (progress == 100)
                {
                    progress = 0;
                    _currentlyCopied = 0;
                }
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
        private RelayCommand _cancelCommand;
        private RelayCommand _setsourceFile;
        private RelayCommand _setsourceFolder;
        private RelayCommand _setDestinationPath;

        public RelayCommand CopyCommand
        {
            get
            {
                return _copyCommand ??
                       (_copyCommand = new RelayCommand
                           (
                               obj => StartCopy(),
                               obj => !String.IsNullOrWhiteSpace(SourcePath) &&
                                      !String.IsNullOrWhiteSpace(DestinationPath)
                           )
                       );
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ??
                       (_cancelCommand = new RelayCommand
                           (
                               obj => { _cancelTokenSource.Cancel(); }
                           )
                       );
            }
        }

        public RelayCommand SetSourceFileCommand
        {
            get
            {
                return _setsourceFile ??
                       (_setsourceFile = new RelayCommand
                           (
                               obj =>
                               {
                                   var ofd =
                                       new OpenFileDialog
                                       {
                                           Multiselect = true,
                                           InitialDirectory = @"D:\Скачки\Kingdom Come Deliverance [qoob RePack RT]"
                                       };
                                   if (ofd.ShowDialog() == true)
                                   {
                                       SourcePath = String.Empty;
                                       foreach (var path in ofd.FileNames)
                                           SourcePath += $"{path}{Environment.NewLine}";
                                   }
                               }
                           )
                       );
            }
        }

        public RelayCommand SetSourceFolderCommand
        {
            get
            {
                return _setsourceFolder ??
                       (_setsourceFolder = new RelayCommand
                           (
                               obj =>
                               {
                                   var fbd =
                                       new FolderBrowserDialog
                                       {
                                           SelectedPath =
                                               @"h:\Other\Музончик\Brick Bazuka - Дискография\(2011) Парадокс EP"
                                       };
                                   if (fbd.ShowDialog() == DialogResult.OK)
                                   {
                                       _folderToCreate = new DirectoryInfo(fbd.SelectedPath).Name;
                                       SourcePath = fbd.SelectedPath;
                                   }
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
                                   var fbd = new FolderBrowserDialog { SelectedPath = @"D:\Games\temp" };
                                   if (fbd.ShowDialog() == DialogResult.OK)
                                       DestinationPath = fbd.SelectedPath;

                               }
                           )
                       );
            }
        }

        #endregion

        #region Methods

        private void StartCopy()
        {
            var initialPath = String.Empty;
            string[] files = null;
            
            try
            {
                Task.Run(() =>
                {
                    if (Directory.Exists(SourcePath))
                    {
                        initialPath = $"{DestinationPath}\\{_folderToCreate}";

                        if (!Directory.Exists(initialPath))
                            Directory.CreateDirectory(initialPath);

                        files = Directory.GetFiles(SourcePath);
                    }

                    if (files == null)
                        files = SourcePath.Replace("\r", "").Split('\n');

                    if (String.IsNullOrEmpty(initialPath))
                        initialPath = DestinationPath;

                    _totalFilesSize = files.Sum(x => !string.IsNullOrWhiteSpace(x) ? new FileInfo(x).Length : 0);
                    foreach (var line in files)
                    {
                        if (File.Exists(line))
                        {
                            var destination = $"{initialPath}\\{Path.GetFileName(line)}";

                            if (File.Exists(destination))
                                File.Delete(destination);

                            var task = new Task(() => Copy(_cancelTokenSource.Token, _progress, line, destination), _cancelTokenSource.Token);
                            task.Start();
                        }
                    }
                }, _cancelTokenSource.Token);
            }
            catch (AggregateException ex)
            {
                if (Directory.Exists(initialPath))
                    Directory.Delete(initialPath);
                else if (files != null)
                    foreach (var line in files)
                    {
                        if (File.Exists($"{initialPath}\\{Path.GetFileName(line)}"))
                            File.Delete($"{initialPath}\\{Path.GetFileName(line)}");
                    }
            }
        }

        private void Copy(CancellationToken ct, IProgress<long> localprogress, string sourcePath, string destPath)
        {
            try
            {
                using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
                using (var destinationStream = new FileStream(destPath, FileMode.Append, FileAccess.Write, FileShare.Write, 4096))
                {
                    var buffer = new byte[4096];
                    while (sourceStream.Position <= sourceStream.Length - 1)
                    {
                        ct.ThrowIfCancellationRequested();

                        var readlength = sourceStream.Position + 4096 <= sourceStream.Length - 1
                            ? 4096
                            : sourceStream.Length - sourceStream.Position;

                        sourceStream.Read(buffer, 0, (int)readlength);
                        destinationStream.Write(buffer, 0, (int)readlength);
                        localprogress.Report(readlength);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
        #endregion
    }
}

