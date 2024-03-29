﻿// Copyright © 2020 Paddy Xu, Frank Becker
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
using System.IO;
using System.Windows;
using QuickLook.Common.Plugin;

namespace QuickLook.Plugin.FolderViewer
{
    public class Plugin : IViewer
    {
        private FolderInfoPanel _panel;
        private bool _isDirectory;

        public int Priority => -5;

        public void Init()
        {
        }

        public bool CanHandle(string path)
        {
            if (Path.GetPathRoot(path) == path)
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
            context.Title = $"{Path.GetFileName(path)}";

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