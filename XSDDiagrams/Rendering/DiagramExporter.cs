//    XSDDiagram - A XML Schema Definition file viewer
//    Copyright (C) 2006-2011  Regis COSNIER
//
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation; either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program; if not, write to the Free Software
//    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace XSDDiagram.Rendering
{
    public delegate bool DiagramAlertHandler(string title, string message);

    public sealed class DiagramExporter
    {
        #region Private Fields

        private Diagram _diagram;

        #endregion

        #region Constructors and Destructor

        public DiagramExporter(Diagram diagram)
        {
            if (diagram == null)
            {
                throw new ArgumentNullException("diagram",
                    "The diagram parameter cannot be null (or Nothing).");
            }

            _diagram = diagram;
        }

        #endregion

        #region Public Properties

        public Diagram Diagram
        {
            get
            {
                return _diagram;
            }
        }

        #endregion

        #region Public Methods

        public bool Export(string outputFilename, Graphics referenceGraphics, 
            DiagramAlertHandler alerteDelegate)
        {
            string extension = Path.GetExtension(outputFilename).ToLower();
            if (string.IsNullOrEmpty(extension)) 
            { 
                extension = ".svg"; 
                outputFilename += extension; 
            }
            using (FileStream stream = File.Create(outputFilename))
            {
                return Export(stream, extension, referenceGraphics, alerteDelegate);
            }
        }

        public bool Export(Stream stream, string extension, Graphics referenceGraphics, 
            DiagramAlertHandler alerteDelegate)
        {
            bool result = false;

            if (extension.Equals(".emf", StringComparison.OrdinalIgnoreCase))
            {
                float scaleSave = _diagram.Scale;
                try
                {
                    _diagram.Scale = 1.0f;
                    _diagram.Layout(referenceGraphics);
                    
                    IntPtr hdc = referenceGraphics.GetHdc();
                    Metafile metafile      = new Metafile(stream, hdc);
                    Graphics graphics      = Graphics.FromImage(metafile);
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    _diagram.Layout(graphics);
                    DiagramGdiRenderer.Draw(_diagram, graphics);
                    referenceGraphics.ReleaseHdc(hdc);
                    metafile.Dispose();
                    graphics.Dispose();

                    result = true;
                }
                finally
                {
                    _diagram.Scale = scaleSave;
                    _diagram.Layout(referenceGraphics);
                }
            }
            else if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)      ||
                extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                Rectangle bbox = _diagram.ScaleRectangle(_diagram.BoundingBox);
                bool bypassAlert = true;
                if (alerteDelegate != null && (bbox.Width > 10000 || bbox.Height > 10000))
                    bypassAlert = alerteDelegate("Huge image generation", 
                        String.Format("Do you agree to generate a {0}x{1} image?", bbox.Width, bbox.Height));
                if (bypassAlert)
                {
                    Bitmap bitmap     = new Bitmap(bbox.Width, bbox.Height);
                    Graphics graphics = Graphics.FromImage(bitmap);
                    graphics.FillRectangle(Brushes.White, 0, 0, bbox.Width, bbox.Height);
                    DiagramGdiRenderer.Draw(_diagram, graphics);
                    if (extension.CompareTo(".png") == 0)
                        bitmap.Save(stream, ImageFormat.Png);
                    else
                        bitmap.Save(stream, ImageFormat.Jpeg);

                    result = true;
                }
            }
            else //if (extension.CompareTo(".svg") == 0)
            {
                float scaleSave = _diagram.Scale;
                try
                {
                    _diagram.Scale = 1.0f;
                    _diagram.Layout(referenceGraphics);
                    using (StreamWriter sw = new StreamWriter(stream))
                    {
                        using (DiagramSvgRenderer renderer = new DiagramSvgRenderer(sw))
                        {
                            renderer.Render(_diagram);
                        }

                        sw.Close();
                    }
                    result = true;
                }
                finally
                {
                    _diagram.Scale = scaleSave;
                    _diagram.Layout(referenceGraphics);
                }
            }

            return result;
        }

        #endregion
    }
}
