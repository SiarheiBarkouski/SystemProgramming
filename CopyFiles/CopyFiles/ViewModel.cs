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
        private readonly Progress<int> _progress;
        private string _sourcePath = String.Empty;
        private string _destinationPath = String.Empty;
        private double _progress1;
        private double _progress2;
        private int _totalProgress;

        public ViewModel()
        {
            _progress = new Progress<int>();
            _progress.ProgressChanged += (s, e) => { Progress = e; };
        }

        #region Properties
        public int Progress
        {
            get => _totalProgress;
            private set
            {
                _totalProgress = value;
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
                           obj => new Thread(Copy).Start(),
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
        private List<List<byte>> ToChunks(List<byte> items, int chunkSize)
        {
            var result = new List<List<byte>>();
            var chunk = new List<byte>(chunkSize);
            result.Add(chunk);
            foreach (var item in items)
            {
                chunk.Add(item);
                if (chunk.Count == chunkSize)
                {
                    chunk = new List<byte>(chunkSize);
                    result.Add(chunk);
                }
            }
            return result;
        }

        private void Copy()
        {
            if (File.Exists(SourcePath))
            {
                if (File.Exists(DestinationPath))
                    File.Delete(DestinationPath);

                var sourceBytesRaw = File.ReadAllBytes(SourcePath);
                var sourceBytes = ToChunks(sourceBytesRaw.ToList(), 4096);

                new Thread(() => CopyBytes(sourceBytes.Take(sourceBytes.Count / 2).ToList(), 0, sourceBytesRaw.Length,
                    1, _progress)).Start();
                new Thread(() => CopyBytes(sourceBytes.Skip(sourceBytes.Count / 2).ToList(), sourceBytes.Count / 2 * 4096,
                    sourceBytesRaw.Length, 2, _progress)).Start();

            }
        }

        private void CopyBytes(List<List<byte>> array, int startPosition, int totalLength, int threadnumber, IProgress<int> progress)
        {
            using (var destinationStream = new FileStream(DestinationPath, FileMode.Append, FileAccess.Write, FileShare.Write, 4096))
            {
                destinationStream.Position = startPosition;
                foreach (var item in array)
                {
                    var length = item.Count();
                    destinationStream.Write(item.ToArray(), 0, length);
                    switch (threadnumber)
                    {
                        case 1: _progress1 = ((double)destinationStream.Position - startPosition) / totalLength; break;
                        case 2: _progress2 = ((double)destinationStream.Position - startPosition) / totalLength; break;
                    }
                    progress.Report((int)((_progress1 + _progress2) * 100));
                }
            }
        }
        #endregion
    }
}
