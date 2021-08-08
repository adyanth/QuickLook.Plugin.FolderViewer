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
using System.IO;
using System.Linq;
using System.Windows;
using QuickLook.Common.Plugin;

namespace QuickLook.Plugin.FolderViewer
{
    public class Plugin : IViewer
    {
        private FolderInfoPanel _panel;
        private bool _isDirectory;

        public int Priority => -5;
        private static readonly int FILES_MAX = 300;
        public void Init()
        {
        }

        /// <summary>
        /// A safe way to get all the files in a directory and sub directory without crashing on UnauthorizedException or PathTooLongException
        /// </summary>
        /// <param name="rootPath">Starting directory</param>
        /// <param name="patternMatch">Filename pattern match</param>
        /// <param name="searchOption">Search subdirectories or only top level directory for files</param>
        /// <returns>List of files</returns>
        public static IEnumerable<string> GetDirectoryFiles(string rootPath, string patternMatch, SearchOption searchOption)
        {
            var foundFiles = Enumerable.Empty<string>();

            if (searchOption == SearchOption.AllDirectories)
            {
                try
                {
                    IEnumerable<string> subDirs = Directory.EnumerateDirectories(rootPath);
                    foreach (string dir in subDirs)
                    {
                        if (foundFiles.Count() < FILES_MAX)
                            foundFiles = foundFiles.Concat(GetDirectoryFiles(dir, patternMatch, searchOption)); // Add files in subdirectories recursively to the list
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (PathTooLongException) { }
            }

            try
            {
                if (foundFiles.Count() < FILES_MAX)
                    foundFiles = foundFiles.Concat(Directory.EnumerateFiles(rootPath, patternMatch)); // Add files from the current directory
            }
            catch (UnauthorizedAccessException) { }

            return foundFiles;
        }

        private int FileCount(string path)
        {
            /*DirectoryInfo dirInfo = new DirectoryInfo(path);
            return dirInfo.EnumerateDirectories()
                    .AsParallel()
                    .SelectMany(di => di.EnumerateFiles("*.*", SearchOption.AllDirectories))
                    .Count();*/
            return GetDirectoryFiles(path, "*", SearchOption.AllDirectories).Count();
        }
        public bool CanHandle(string path)
        {
            if (Path.GetPathRoot(path) == path || FileCount(path) > FILES_MAX - 1)
                return false;

            _isDirectory = Directory.Exists(path);
            return _isDirectory;
        }

        public void Prepare(string path, ContextObject context)
        {
            context.PreferredSize = new Size { Width = 800, Height = 400 };
        }

        public void View(string path, ContextObject context)
        {
            _panel = new FolderInfoPanel(path);

            context.ViewerContent = _panel;
            context.Title = $"{Path.GetDirectoryName(path)}";

            context.IsBusy = false;
        }

        public void Cleanup()
        {
            GC.SuppressFinalize(this);

            _panel.Stop = true;
            _panel?.Dispose();
            _panel = null;
        }
    }
}