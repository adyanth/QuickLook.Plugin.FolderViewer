// Copyright © 2020 Paddy Xu, Frank Becker
// 
// This file is part of QuickLook program.
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using QuickLook.Common.Annotations;
using QuickLook.Common.ExtensionMethods;

namespace QuickLook.Plugin.FolderViewer
{
    /// <summary>
    ///     Interaction logic for FolderInfoPanel.xaml
    /// </summary>
    public partial class FolderInfoPanel : UserControl, IDisposable, INotifyPropertyChanged
    {
        private readonly Dictionary<string, FileEntry> _fileEntries = new Dictionary<string, FileEntry>();
        private bool _disposed;
        private double _loadPercent;
        private bool _stop;

        public FolderInfoPanel(string path)
        {
            InitializeComponent();

            // design-time only
            Resources.MergedDictionaries.Clear();

            BeginLoadDirectory(path);
        }
        public bool Stop
        {
            set => _stop = value;
            get => _stop;
        }

        public double LoadPercent
        {
            get => _loadPercent;
            private set
            {
                if (value == _loadPercent) return;
                _loadPercent = value;
                OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _disposed = true;

            fileListView.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void BeginLoadDirectory(string path)
        {
            new Task(() =>
            {
                var root = new FileEntry(Path.GetDirectoryName(path), true);
                _fileEntries.Add(path, root);

                LoadItemsFromFolder(path, ref _stop,
                    out var totalDirsL, out var totalFilesL, out var totalSizeL);

                Dispatcher.Invoke(() =>
                {
                    if (_disposed)
                        return;

                    fileListView.SetDataContext(_fileEntries[path].Children.Keys);
                    totalSize.Content =
                        $"Total size: {totalSizeL.ToPrettySize(2)}";
                    numFolders.Content =
                        $"Folders: {totalDirsL}";
                    numFiles.Content = 
                        $"Files: {totalFilesL}";
                });

                LoadPercent = 100d;
            }).Start();
        }

        // based on InfoPanel.FileHelper.CountFolder
        public void LoadItemsFromFolder(string root, ref bool stop, out long totalDirs, out long totalFiles,
    out long totalSize)
        {
            totalDirs = totalFiles = totalSize = 0L;

            var stack = new Stack<DirectoryInfo>();
            stack.Push(new DirectoryInfo(root));

            do
            {
                if (stop)
                    break;

                var pos = stack.Pop();

                try
                {
                    _fileEntries.TryGetValue(pos.FullName, out var fileParent);

                    // process files in current directory
                    foreach (var file in pos.EnumerateFiles())
                    {
                        totalFiles++;
                        totalSize += file.Length;

                        _fileEntries.Add(file.FullName, new FileEntry(file.Name, false, fileParent)
                        {
                            Size = (ulong)file.Length,
                            ModifiedDate = file.LastWriteTime,
                            FullPath = file.FullName
                        }); ;

                    }

                    // then push all sub-directories
                    foreach (var dir in pos.EnumerateDirectories())
                    {
                        totalDirs++;
                        stack.Push(dir);

                        _fileEntries.TryGetValue(GetDirectoryName(dir.FullName), out var parent);

                        var afe = new FileEntry(dir.Name, true, parent)
                        {
                            FullPath = dir.FullName
                        };
                        _fileEntries.Add(dir.FullName, afe);
                    }
                }
                catch (Exception)
                {
                    totalDirs++;
                    //pos = stack.Pop();
                }
            } while (stack.Count != 0);
        }

        private string GetDirectoryName(string path)
        {
            var d = Path.GetDirectoryName(path);

            return d ?? "";
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}