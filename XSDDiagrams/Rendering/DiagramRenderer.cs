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

namespace XSDDiagram.Rendering
{
    public abstract class DiagramRenderer : IDisposable
    {
        #region Constructors and Destructor

        protected DiagramRenderer()
        {   
        }

        ~DiagramRenderer()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public Properties

        public abstract string Name
        {
            get;
        }

        #endregion

        #region Public Methods

        public abstract void BeginItemsRender();

        public abstract void Render(Diagram diagram);
        public abstract void Render(DiagramItem item);

        public abstract void EndItemsRender();

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion
    }
}
