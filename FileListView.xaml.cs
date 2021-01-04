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
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace QuickLook.Plugin.FolderViewer
{
    /// <summary>
    ///     Interaction logic for FileListView.xaml
    /// </summary>
    public partial class FileListView : UserControl, IDisposable
    {
        public FileListView()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void SetDataContext(object context)
        {
            treeGrid.DataContext = context;

            treeView.LayoutUpdated += (sender, e) =>
            {
                // return when empty
                if (treeView.Items.Count == 0)
                    return;

                // return when there are more than one root nodes
                if (treeView.Items.Count > 1)
                    return;

                var root = (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromItem(treeView.Items[0]);
                if (root == null)
                    return;

                root.IsExpanded = true;
            };
        }

        private void OnItemMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            if (sender is TreeViewItem item)
            {
                if (!item.IsSelected)
                {
                    return;
                }
                var fullPath = (item.DataContext as FileEntry).FullPath;
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                {
                    OpenWithDefaultProgram(fullPath);
                }
            }
        }

        public static void OpenWithDefaultProgram(string path)
        {
            Process fileopener = new Process();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = $"\"{path}\"";
            fileopener.Start();
        }
    }
}