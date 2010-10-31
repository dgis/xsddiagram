//    XSDDiagram - A XML Schema Definition file viewer
//    Copyright (C) 2006  Regis COSNIER
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace XSDDiagram
{
	public class Diagram
	{
		private Hashtable hashtableElementsByName = new Hashtable();
		protected List<DiagramBase> rootElements = new List<DiagramBase>();

		private Size size = new Size(100, 100);
		private Size padding = new Size(10, 10);
		private Rectangle boundingBox = new Rectangle();
		private float scale = 1.0f;
		private DiagramBase.Alignement alignement = DiagramBase.Alignement.Center;
		private bool showBoundingBox = false;

		private Font font;
		private Font smallFont;

		public Size Size { get { return this.size; } set { this.size = value; } }
		public Size Padding { get { return this.padding; } set { this.padding = value; } }
		public Rectangle BoundingBox { get { return this.boundingBox; } }
		public float Scale { get { return this.scale; } set { this.scale = value; } }
		public DiagramBase.Alignement Alignement { get { return this.alignement; } set { this.alignement = value; } }
		public bool ShowBoundingBox { get { return this.showBoundingBox; } set { this.showBoundingBox = value; } }

		public Font Font { get { return this.font; } set { this.font = value; } }
		public Font SmallFont { get { return this.smallFont; } set { this.smallFont = value; } }

		public Hashtable ElementsByName { get { return this.hashtableElementsByName; } set { this.hashtableElementsByName = value; } }
		public List<DiagramBase> RootElements { get { return this.rootElements; } }


		public delegate void RequestAnyElementEventHandler(DiagramBase diagramElement, out XMLSchema.element element, out string nameSpace);
		public event RequestAnyElementEventHandler RequestAnyElement;

		public DiagramBase Add(XMLSchema.openAttrs childElement, string nameSpace)
		{
			if (childElement is XMLSchema.element)
				return AddElement(childElement as XMLSchema.element, nameSpace);
			else if (childElement is XMLSchema.group)
				return AddCompositors(childElement as XMLSchema.group, nameSpace);
			else if (childElement is XMLSchema.complexType)
				return AddComplexType(childElement as XMLSchema.complexType, nameSpace);
			return null;
		}

		public DiagramBase AddElement(XMLSchema.element childElement, string nameSpace)
		{
			return AddElement(null, childElement, nameSpace);
		}

		public DiagramBase AddElement(DiagramBase parentDiagramElement, XMLSchema.element childElement, string nameSpace)
		{
			if (childElement != null)
			{
				DiagramBase childDiagramElement = new DiagramBase();

				XMLSchema.element referenceElement = null;
				if (childElement.@ref != null)
				{
					if (!childElement.@ref.IsEmpty)
					{
						childDiagramElement.IsReference = true;

						XSDObject objectReferred = this.hashtableElementsByName[childElement.@ref.Namespace + ":element:" + childElement.@ref.Name] as XSDObject;
						if (objectReferred != null)
						{
							XMLSchema.element elementReferred = objectReferred.Tag as XMLSchema.element;
							if (elementReferred != null)
							{
								referenceElement = childElement;
								childElement = elementReferred;
							}
						}
						else
							childElement.name = childElement.@ref.Name;
					}
				}

				childDiagramElement.Diagram = this;
				childDiagramElement.TabSchema = childElement;
				childDiagramElement.Name = childElement.name != null ? childElement.name : "";
				childDiagramElement.NameSpace = nameSpace;
				childDiagramElement.Type = DiagramBase.TypeEnum.element;
				try { childDiagramElement.MinOccurrence = int.Parse(referenceElement != null ? referenceElement.minOccurs : childElement.minOccurs); }
				catch { childDiagramElement.MinOccurrence = -1; }
				try { childDiagramElement.MaxOccurrence = int.Parse(referenceElement != null ? referenceElement.maxOccurs : childElement.maxOccurs); }
				catch { childDiagramElement.MaxOccurrence = -1; }

				bool hasChildren;
				bool isSimpleType;
				GetChildrenInfo(childElement, out hasChildren, out isSimpleType);
				childDiagramElement.HasChildElements = hasChildren;
				childDiagramElement.IsSimpleContent = isSimpleType;

				if (parentDiagramElement == null)
					this.rootElements.Add(childDiagramElement);
				else
				{
					childDiagramElement.Parent = parentDiagramElement;
					parentDiagramElement.ChildElements.Add(childDiagramElement);
				}

				if (childElement.@abstract)
				{
					string abstractElementFullName = childDiagramElement.FullName;
					foreach(XSDObject xsdObject in this.hashtableElementsByName.Values)
					{
						if (xsdObject != null && xsdObject.Tag is XMLSchema.element)
						{
							XMLSchema.element element = xsdObject.Tag as XMLSchema.element;
							if (element.substitutionGroup != null)
							{
								string elementFullName = element.substitutionGroup.Namespace + ":element:" + element.substitutionGroup.Name;
								if (elementFullName == abstractElementFullName)
								{
									DiagramBase diagramBase = AddElement(parentDiagramElement, element, xsdObject.NameSpace);
									if (diagramBase != null)
										diagramBase.InheritFrom = childDiagramElement;
								}
							}
						}
					}
				}

				return childDiagramElement;
			}
			return null;
		}

		public DiagramBase AddComplexType(XMLSchema.complexType childElement, string nameSpace)
		{
			return AddComplexType(null, childElement, false, nameSpace);
		}

		public DiagramBase AddComplexType(DiagramBase parentDiagramElement, XMLSchema.complexType childElement, string nameSpace)
		{
			return AddComplexType(parentDiagramElement, childElement, false, nameSpace);
		}

		public DiagramBase AddComplexType(DiagramBase parentDiagramElement, XMLSchema.complexType childElement, bool isReference, string nameSpace)
		{
			if (childElement != null)
			{
				DiagramBase childDiagramElement = new DiagramBase();
				childDiagramElement.Diagram = this;
				childDiagramElement.TabSchema = childElement;
				childDiagramElement.Name = childElement.name != null ? childElement.name : "";
				childDiagramElement.NameSpace = nameSpace;
				childDiagramElement.Type = DiagramBase.TypeEnum.type;
				childDiagramElement.MinOccurrence = 1;
				childDiagramElement.MaxOccurrence = 1;
				childDiagramElement.IsReference = isReference;
				childDiagramElement.IsSimpleContent = false;
				childDiagramElement.HasChildElements = false;

				if (childElement.Items != null)
				{
					for (int i = 0; i < childElement.Items.Length; i++)
					{
						if (childElement.Items[i] is XMLSchema.group ||
							childElement.Items[i] is XMLSchema.complexType ||
							childElement.Items[i] is XMLSchema.complexContent)
						{
							childDiagramElement.HasChildElements = true;
							break;
						}
					}
				}

				if (parentDiagramElement == null)
					this.rootElements.Add(childDiagramElement);
				else
				{
					childDiagramElement.Parent = parentDiagramElement;
					parentDiagramElement.ChildElements.Add(childDiagramElement);
				}

				return childDiagramElement;
			}
			return null;
		}

		public DiagramBase AddAny(DiagramBase parentDiagramElement, XMLSchema.any childElement, string nameSpace)
		{
			if (childElement != null)
			{
				DiagramBase childDiagramElement = new DiagramBase();
				childDiagramElement.Diagram = this;
				childDiagramElement.TabSchema = childElement;
				childDiagramElement.Name = "any  " + childElement.@namespace;
				childDiagramElement.NameSpace = nameSpace;
				childDiagramElement.Type = DiagramBase.TypeEnum.group;  //DiagramBase.TypeEnum.element;
				try { childDiagramElement.MinOccurrence = int.Parse(childElement.minOccurs); }
				catch { childDiagramElement.MinOccurrence = -1; }
				try { childDiagramElement.MaxOccurrence = int.Parse(childElement.maxOccurs); }
				catch { childDiagramElement.MaxOccurrence = -1; }
				childDiagramElement.IsReference = false;
				childDiagramElement.IsSimpleContent = false;
				childDiagramElement.HasChildElements = false; // true;

				if (parentDiagramElement == null)
					this.rootElements.Add(childDiagramElement);
				else
				{
					childDiagramElement.Parent = parentDiagramElement;
					parentDiagramElement.ChildElements.Add(childDiagramElement);
				}

				return childDiagramElement;
			}
			return null;
		}

		public DiagramBase AddCompositors(XMLSchema.group childElement, string nameSpace)
		{
			return AddCompositors(null, childElement, DiagramBase.GroupTypeEnum.group, nameSpace);
		}

		public DiagramBase AddCompositors(DiagramBase parentDiagramElement, XMLSchema.group childElement, string nameSpace)
		{
			return AddCompositors(parentDiagramElement, childElement, DiagramBase.GroupTypeEnum.group, nameSpace);
		}

		public DiagramBase AddCompositors(DiagramBase parentDiagramElement, XMLSchema.group childGroup, DiagramBase.GroupTypeEnum type, string nameSpace)
		{
			if (childGroup != null)
			{
				DiagramBase childDiagramGroup = new DiagramBase();
				childDiagramGroup.Type = DiagramBase.TypeEnum.group;
				if (childGroup.@ref != null)
				{
					childDiagramGroup.IsReference = true;
					childDiagramGroup.Name = childGroup.@ref.Name != null ? childGroup.@ref.Name : "";
					childDiagramGroup.NameSpace = childGroup.@ref.Namespace != null ? childGroup.@ref.Namespace : "";
					XMLSchema.group group = (this.hashtableElementsByName[childDiagramGroup.FullName] as XSDObject).Tag as XMLSchema.group;
					if (group != null)
						childGroup = group;
				}
				else if (type == DiagramBase.GroupTypeEnum.group)
				{
					childDiagramGroup.Name = childGroup.name != null ? childGroup.name : "";
					childDiagramGroup.NameSpace = nameSpace;
				}
				else
				{
					childDiagramGroup.NameSpace = nameSpace;
				}

				childDiagramGroup.Diagram = this;
				childDiagramGroup.TabSchema = childGroup;
				try { childDiagramGroup.MinOccurrence = int.Parse(childGroup.minOccurs); }
				catch { childDiagramGroup.MinOccurrence = -1; }
				try { childDiagramGroup.MaxOccurrence = int.Parse(childGroup.maxOccurs); }
				catch { childDiagramGroup.MaxOccurrence = -1; }
				childDiagramGroup.HasChildElements = true;
				childDiagramGroup.GroupType = type;

				if (parentDiagramElement == null)
					this.rootElements.Add(childDiagramGroup);
				else
				{
					childDiagramGroup.Parent = parentDiagramElement;
					parentDiagramElement.ChildElements.Add(childDiagramGroup);
				}

				return childDiagramGroup;
			}
			return null;
		}

		public void Remove(DiagramBase element)
		{
			if (element.Parent == null)
				this.rootElements.Remove(element);
			else
			{
				element.Parent.ChildElements.Remove(element);
				if (element.Parent.ChildElements.Count == 0)
					element.Parent.ShowChildElements = false;
			}
		}

		public void RemoveAll()
		{
			this.rootElements.Clear();
		}

		private void GetChildrenInfo(XMLSchema.complexType complexTypeElement, out bool hasChildren, out bool isSimpleType)
		{
			bool hasSimpleContent = false;
			if (complexTypeElement.Items != null)
			{
				for (int i = 0; i < complexTypeElement.Items.Length; i++)
				{
					if (complexTypeElement.Items[i] is XMLSchema.group ||
						complexTypeElement.Items[i] is XMLSchema.complexType ||
						complexTypeElement.Items[i] is XMLSchema.complexContent)
					{
						hasChildren = true;
						isSimpleType = complexTypeElement.mixed;
						if (complexTypeElement.Items[i] is XMLSchema.complexContent)
						{
							hasChildren = false;
							XMLSchema.complexContent complexContent = complexTypeElement.Items[i] as XMLSchema.complexContent;
							if (complexContent.Item is XMLSchema.extensionType)
							{
								XMLSchema.extensionType extensionType = complexContent.Item as XMLSchema.extensionType;
								if (extensionType.all != null || extensionType.group != null || extensionType.choice != null || extensionType.sequence != null)
									hasChildren = true;
								else if (extensionType.@base != null)
								{
									XSDObject xsdObject = this.hashtableElementsByName[extensionType.@base.Namespace + ":type:" + extensionType.@base.Name] as XSDObject;
									if (xsdObject != null)
									{
										if (xsdObject.Tag is XMLSchema.complexType)
										{
											GetChildrenInfo(xsdObject.Tag as XMLSchema.complexType, out hasChildren, out isSimpleType);
										}
									}
								}
							}
						}
						return;
					}
					else if (complexTypeElement.Items[i] is XMLSchema.simpleContent)
					{
						hasSimpleContent = true;
					}
				}
			}
			hasChildren = false;
			isSimpleType = (hasSimpleContent ? true : complexTypeElement.mixed);
		}

		private void GetChildrenInfo(XMLSchema.element childElement, out bool hasChildren, out bool isSimpleType)
		{
			if (childElement.Item is XMLSchema.complexType)
			{
				XMLSchema.complexType complexTypeElement = childElement.Item as XMLSchema.complexType;
				GetChildrenInfo(complexTypeElement, out hasChildren, out isSimpleType);
				return;
				//if (complexTypeElement.Items != null)
				//{
				//    for (int i = 0; i < complexTypeElement.Items.Length; i++)
				//    {
				//        if (complexTypeElement.Items[i] is XMLSchema.group ||
				//            complexTypeElement.Items[i] is XMLSchema.complexType ||
				//            complexTypeElement.Items[i] is XMLSchema.complexContent)
				//        {
				//            return true;
				//        }
				//    }
				//}
			}
			else if (childElement.type != null)
			{
				if (this.hashtableElementsByName.ContainsKey(childElement.type.Namespace + ":type:" + childElement.type.Name))
				{
					XMLSchema.annotated annotated = (this.hashtableElementsByName[childElement.type.Namespace + ":type:" + childElement.type.Name] as XSDObject).Tag as XMLSchema.annotated;
					if (annotated is XMLSchema.simpleType)
					{
						hasChildren = false;
						isSimpleType = true;
					}
					else
						GetChildrenInfo(annotated as XMLSchema.complexType, out hasChildren, out isSimpleType);
					return;
				}
			}
			hasChildren = false;
			isSimpleType = true;
		}

		private void ExpandAnnotated(DiagramBase parentDiagramElement, XMLSchema.annotated annotated, string nameSpace)
		{
			if (annotated is XMLSchema.element)
			{
				AddElement(parentDiagramElement, annotated as XMLSchema.element, nameSpace);
				parentDiagramElement.ShowChildElements = true;
			}
			else if (annotated is XMLSchema.group)
			{
				AddCompositors(parentDiagramElement, annotated as XMLSchema.group, nameSpace);
				parentDiagramElement.ShowChildElements = true;
			}
			else if (annotated is XMLSchema.complexType)
			{
				ExpandComplexType(parentDiagramElement, annotated as XMLSchema.complexType);
				parentDiagramElement.ShowChildElements = true;
			}
		}

		private void ExpandComplexType(DiagramBase parentDiagramElement, XMLSchema.complexType complexTypeElement)
		{
			if (complexTypeElement.Items != null)
			{
				for (int i = 0; i < complexTypeElement.Items.Length; i++)
				{
					if (complexTypeElement.Items[i] is XMLSchema.group)
					{
						XMLSchema.group group = complexTypeElement.Items[i] as XMLSchema.group;
						DiagramBase diagramCompositors = AddCompositors(parentDiagramElement, group, (DiagramBase.GroupTypeEnum)Enum.Parse(typeof(DiagramBase.GroupTypeEnum), complexTypeElement.ItemsElementName[i].ToString()), parentDiagramElement.NameSpace);
						parentDiagramElement.ShowChildElements = true;
						if (diagramCompositors != null)
							ExpandChildren(diagramCompositors);
					}
					else if (complexTypeElement.Items[i] is XMLSchema.complexContent)
					{
						XMLSchema.complexContent complexContent = complexTypeElement.Items[i] as XMLSchema.complexContent;
						if (complexContent.Item is XMLSchema.extensionType)
						{
							XMLSchema.extensionType extensionType = complexContent.Item as XMLSchema.extensionType;

							XSDObject xsdObject = this.hashtableElementsByName[extensionType.@base.Namespace + ":type:" + extensionType.@base.Name] as XSDObject;
							if (xsdObject != null)
							{
								XMLSchema.annotated annotated = xsdObject.Tag as XMLSchema.annotated;
								ExpandAnnotated(parentDiagramElement, annotated, extensionType.@base.Namespace);
							}

							XMLSchema.group group = extensionType.group as XMLSchema.group;
							if (group != null)
							{
								DiagramBase diagramCompositors = AddCompositors(parentDiagramElement, group, DiagramBase.GroupTypeEnum.group, extensionType.@base.Namespace);
								parentDiagramElement.ShowChildElements = true;
								if (diagramCompositors != null)
									ExpandChildren(diagramCompositors);
							}

							XMLSchema.group groupSequence = extensionType.sequence as XMLSchema.group;
							if (groupSequence != null)
							{
								DiagramBase diagramCompositors = AddCompositors(parentDiagramElement, groupSequence, DiagramBase.GroupTypeEnum.sequence, extensionType.@base.Namespace);
								parentDiagramElement.ShowChildElements = true;
								if (diagramCompositors != null)
									ExpandChildren(diagramCompositors);
							}

							XMLSchema.group groupChoice = extensionType.choice as XMLSchema.group;
							if (groupChoice != null)
							{
								DiagramBase diagramCompositors = AddCompositors(parentDiagramElement, groupChoice, DiagramBase.GroupTypeEnum.choice, extensionType.@base.Namespace);
								parentDiagramElement.ShowChildElements = true;
								if (diagramCompositors != null)
									ExpandChildren(diagramCompositors);
							}
						}
						else if (complexContent.Item is XMLSchema.restrictionType)
						{
							XMLSchema.restrictionType restrictionType = complexContent.Item as XMLSchema.restrictionType;
							XSDObject xsdObject = this.hashtableElementsByName[restrictionType.@base.Namespace + ":type:" + restrictionType.@base.Name] as XSDObject;
							if(xsdObject != null)
							{
								XMLSchema.annotated annotated = xsdObject.Tag as XMLSchema.annotated;
								ExpandAnnotated(parentDiagramElement, annotated, restrictionType.@base.Namespace);
							}
						}
					}
				}
			}
		}

		public void ExpandChildren(DiagramBase parentDiagramElement)
		{
			if (parentDiagramElement.Type == DiagramBase.TypeEnum.element || parentDiagramElement.Type == DiagramBase.TypeEnum.type)
			{
				DiagramBase diagramElement = parentDiagramElement;
				if (diagramElement.TabSchema is XMLSchema.element)
				{
					XMLSchema.element element = diagramElement.TabSchema as XMLSchema.element;

					if (element.Item is XMLSchema.complexType)
					{
						XMLSchema.complexType complexTypeElement = element.Item as XMLSchema.complexType;
						ExpandComplexType(diagramElement, complexTypeElement);
					}
					else if (element.type != null)
					{
						XMLSchema.annotated annotated = (this.hashtableElementsByName[element.type.Namespace + ":type:" + element.type.Name] as XSDObject).Tag as XMLSchema.annotated;
						ExpandAnnotated(diagramElement, annotated, element.type.Namespace);
					}
				}
				else if (diagramElement.TabSchema is XMLSchema.any)
				{
					//XMLSchema.any any = diagramElement.TabSchema as XMLSchema.any;

					if (RequestAnyElement != null)
					{
						XMLSchema.element requestElement;
						string requestNameSpace;
						RequestAnyElement(diagramElement, out requestElement, out requestNameSpace);
						if(requestElement != null)
						{
							AddElement(diagramElement, requestElement, requestNameSpace);
							diagramElement.ShowChildElements = true;
						}
					}
				}
				else if (diagramElement.TabSchema is XMLSchema.complexType)
				{
					XMLSchema.complexType complexTypeElement = diagramElement.TabSchema as XMLSchema.complexType;
					ExpandComplexType(diagramElement, complexTypeElement);
				}
			}
			else if (parentDiagramElement.Type == DiagramBase.TypeEnum.group)
			{
				DiagramBase diagramCompositors = parentDiagramElement;
				XMLSchema.group group = diagramCompositors.TabSchema as XMLSchema.group;

				if (group.Items != null)
				{
					for (int i = 0; i < group.Items.Length; i++)
					{
						switch (group.ItemsElementName[i])
						{
							case XMLSchema.ItemsChoiceType2.element:
								if (group.Items[i] is XMLSchema.element)
									AddElement(diagramCompositors, group.Items[i] as XMLSchema.element, diagramCompositors.NameSpace);
								break;
							case XMLSchema.ItemsChoiceType2.any:
								if (group.Items[i] is XMLSchema.any)
									AddAny(diagramCompositors, group.Items[i] as XMLSchema.any, diagramCompositors.NameSpace);
								break;
							case XMLSchema.ItemsChoiceType2.group:
								if (group.Items[i] is XMLSchema.group)
									AddCompositors(diagramCompositors, group.Items[i] as XMLSchema.group, DiagramBase.GroupTypeEnum.group, diagramCompositors.NameSpace);
								break;
							case XMLSchema.ItemsChoiceType2.all:
								if (group.Items[i] is XMLSchema.group)
									AddCompositors(diagramCompositors, group.Items[i] as XMLSchema.group, DiagramBase.GroupTypeEnum.all, diagramCompositors.NameSpace);
								break;
							case XMLSchema.ItemsChoiceType2.choice:
								if (group.Items[i] is XMLSchema.group)
									AddCompositors(diagramCompositors, group.Items[i] as XMLSchema.group, DiagramBase.GroupTypeEnum.choice, diagramCompositors.NameSpace);
								break;
							case XMLSchema.ItemsChoiceType2.sequence:
								if (group.Items[i] is XMLSchema.group)
									AddCompositors(diagramCompositors, group.Items[i] as XMLSchema.group, DiagramBase.GroupTypeEnum.sequence, diagramCompositors.NameSpace);
								break;
						}
					}
					parentDiagramElement.ShowChildElements = true;
				}
			}
		}

		private void ExpandOneLevel(DiagramBase parentDiagramBase)
		{
			foreach (DiagramBase diagramBase in parentDiagramBase.ChildElements)
			{
				ExpandOneLevel(diagramBase);
				if (diagramBase.HasChildElements && diagramBase.ChildElements.Count == 0)
					ExpandChildren(diagramBase);
			}
		}

		public void ExpandOneLevel()
		{
			foreach (DiagramBase diagramBase in this.rootElements)
			{
				ExpandOneLevel(diagramBase);
				if (diagramBase.HasChildElements && diagramBase.ChildElements.Count == 0)
					ExpandChildren(diagramBase);
			}
		}

		public void Clear()
		{
			this.rootElements.Clear();
		}

		public void Layout(Graphics g)
		{
 			string fontName = "Arial"; // "Verdana"; // "Arial";
			this.font = new Font(fontName, 10.0f * (float)Math.Pow(this.scale, 2.0), FontStyle.Bold, GraphicsUnit.Pixel);
			this.smallFont = new Font(fontName, 9.0f * (float)Math.Pow(this.scale, 2.0), GraphicsUnit.Pixel);

			foreach (DiagramBase element in this.rootElements)
				element.GenerateMeasure(g);

			this.boundingBox = new Rectangle(0, 0, 100, 0);

			int currentY = this.padding.Height;
			foreach (DiagramBase element in this.rootElements)
			{
				Rectangle elementBoundingBox = element.BoundingBox;
				elementBoundingBox.X = this.padding.Width;
				elementBoundingBox.Y = currentY;
				element.BoundingBox = elementBoundingBox;
				element.GenerateLocation();
				currentY += element.BoundingBox.Height;

				this.boundingBox = Rectangle.Union(this.boundingBox, element.BoundingBox);
			}
		}

		public void Paint(Graphics g)
		{
			Paint(g, null);
		}

		public void Paint(Graphics g, Rectangle? clipRectangle)
		{
			if (clipRectangle.HasValue)
			{
				Rectangle cr = new Rectangle(clipRectangle.Value.Location, clipRectangle.Value.Size);
				cr.X = (int)((float)cr.X / this.scale);
				cr.Y = (int)((float)cr.Y / this.scale);
				cr.Width = (int)((float)cr.Width / this.scale);
				cr.Height = (int)((float)cr.Height / this.scale);

				foreach (DiagramBase element in this.rootElements)
				{
					if (element.BoundingBox.IntersectsWith(cr))
						element.Paint(g, cr);
				}
			}
			else
			{
				foreach (DiagramBase element in this.rootElements)
					element.Paint(g, null);
			}
		}

		public string ToSVG() //Graphics g)
		{
			string result = @"<?xml version=""1.0"" standalone=""no""?>

<!DOCTYPE svg PUBLIC ""-//W3C//DTD SVG 1.1//EN""
""http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd"">

<svg width=""100%"" height=""100%"" version=""1.1""
xmlns=""http://www.w3.org/2000/svg"">
";
			foreach (DiagramBase element in this.rootElements)
				result += element.ToSVG(); //g);

			result += @"</svg>";
			return result;
		}

		public void HitTest(Point point, out DiagramBase element, out DiagramBase.HitTestRegion region)
		{
			element = null;
			region = DiagramBase.HitTestRegion.None;

			foreach (DiagramBase childElement in this.rootElements)
			{
				DiagramBase resultElement;
				DiagramBase.HitTestRegion resultRegion;
				childElement.HitTest(point, out resultElement, out resultRegion);
				if (resultRegion != DiagramBase.HitTestRegion.None)
				{
					element = resultElement;
					region = resultRegion;
					break;
				}
			}
		}

		public int ScaleInt(int integer) { return (int)(integer * this.Scale); }
		public Point ScalePoint(Point point)
		{
			return new Point((int)Math.Round(point.X * this.Scale), (int)Math.Round(point.Y * this.Scale));
		}
		public Size ScaleSize(Size point)
		{
			return new Size((int)Math.Round(point.Width * this.Scale), (int)Math.Round(point.Height * this.Scale));
		}
		public Rectangle ScaleRectangle(Rectangle rectangle)
		{
			return new Rectangle((int)Math.Round(rectangle.X * this.Scale), (int)Math.Round(rectangle.Y * this.Scale),
				(int)Math.Round(rectangle.Width * this.Scale), (int)Math.Round(rectangle.Height * this.Scale));
		}



        public delegate bool AlerteDelegate(string title, string message);
        public string SaveToImage(string outputFilename, Graphics g1, AlerteDelegate alerteDelegate)
        {
            string extension = Path.GetExtension(outputFilename).ToLower();
            if (string.IsNullOrEmpty(extension)) { extension = ".svg"; outputFilename += extension; }
            if (extension.CompareTo(".emf") == 0)
            {
                float scaleSave = this.Scale;
                try
                {
                    this.Scale = 1.0f;
                    this.Layout(g1);
                    IntPtr hdc = g1.GetHdc();
                    Metafile metafile = new Metafile(outputFilename, hdc);
                    Graphics g2 = Graphics.FromImage(metafile);
                    g2.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    this.Layout(g2);
                    this.Paint(g2);
                    g1.ReleaseHdc(hdc);
                    metafile.Dispose();
                    g2.Dispose();
                }
                finally
                {
                    this.Scale = scaleSave;
                    this.Layout(g1);
                }
            }
            else if (extension.CompareTo(".png") == 0)
            {
                Rectangle bbox = this.ScaleRectangle(this.BoundingBox);
                bool bypassAlert = true;
                if (alerteDelegate != null && (bbox.Width > 10000 || bbox.Height > 10000))
                    bypassAlert = alerteDelegate("Huge image generation", string.Format("Are you agree to generate a {0}x{1} image?", bbox.Width, bbox.Height));
                if (bypassAlert)
                {
                    Bitmap bitmap = new Bitmap(bbox.Width, bbox.Height);
                    Graphics graphics = Graphics.FromImage((Image)bitmap);
                    graphics.FillRectangle(Brushes.White, 0, 0, bbox.Width, bbox.Height);
                    this.Paint(graphics);
                    bitmap.Save(outputFilename);
                }
            }
            else //if (extension.CompareTo(".svg") == 0)
            {
                float scaleSave = this.Scale;
                try
                {
                    this.Scale = 1.0f;
                    this.Layout(g1);
                    string svgFileContent = this.ToSVG();
                    using (StreamWriter sw = new StreamWriter(outputFilename))
                    {
                        sw.WriteLine(svgFileContent);
                        sw.Close();
                    }
                }
                finally
                {
                    this.Scale = scaleSave;
                    this.Layout(g1);
                }
            }
            g1.Dispose();
            return outputFilename;
        }
	}

	public class DiagramBase
	{
		public enum HitTestRegion { None, BoundingBox, Element, ChildExpandButton }
		public enum Alignement { Near, Center, Far }
		public enum TypeEnum { element, type, group }
		public enum GroupTypeEnum { group, sequence, choice, all }

		protected Diagram diagram = null;
		protected DiagramBase parent = null;
		protected DiagramBase inheritFrom = null;
		protected string name = "";
		protected string nameSpace = "";
		protected TypeEnum type = TypeEnum.element;
		protected GroupTypeEnum groupType;
		protected bool isReference;
		protected bool isSimpleContent;
		protected int minOccurrence = -1;
		protected int maxOccurrence = -1;
		protected bool hasChildElements = false;
		protected bool showChildElements = false;
		protected Point location = new Point(0, 0);
		protected Size size = new Size(50, 25);
		protected Size margin = new Size(10, 5);
		protected Size padding = new Size(10, 15);
		protected Rectangle elementBox = new Rectangle();
		protected Rectangle childExpandButtonBox = new Rectangle();
		protected Rectangle boundingBox = new Rectangle();
		protected int childExpandButtonSize = 10;
		protected int depth = 0;
		protected List<DiagramBase> childElements = new List<DiagramBase>();

		protected XMLSchema.openAttrs tabSchema;

		public Diagram Diagram { get { return this.diagram; } set { this.diagram = value; } }
		public DiagramBase Parent { get { return this.parent; } set { this.parent = value; } }
		public DiagramBase InheritFrom { get { return this.inheritFrom; } set { this.inheritFrom = value; } }
		public string Name { get { return this.name; } set { this.name = value; } }
		public string NameSpace { get { return this.nameSpace; } set { this.nameSpace = value; } }
		public TypeEnum Type { get { return this.type; } set { this.type = value; } }
		public GroupTypeEnum GroupType { get { return this.groupType; } set { this.groupType = value; } }
		public bool IsReference { get { return this.isReference; } set { this.isReference = value; } }
		public bool IsSimpleContent { get { return this.isSimpleContent; } set { this.isSimpleContent = value; } }
		public int MinOccurrence { get { return this.minOccurrence; } set { this.minOccurrence = value; } }
		public int MaxOccurrence { get { return this.maxOccurrence; } set { this.maxOccurrence = value; } }
		public bool HasChildElements { get { return this.hasChildElements; } set { this.hasChildElements = value; } }
		public bool ShowChildElements { get { return this.showChildElements; } set { this.showChildElements = value; } }
		public string FullName { get { return this.nameSpace + ':' + this.type + ':' + this.name; } }
		public Font Font { get { return this.diagram.Font; } }
		public Font SmallFont { get { return this.diagram.SmallFont; } }
		public Point Location { get { return this.location; } set { this.location = value; } }
		public Size Size { get { return this.size; } set { this.size = value; } }
		public Size Margin { get { return this.margin; } set { this.margin = value; } }
		public Size Padding { get { return this.padding; } set { this.padding = value; } }
		public Rectangle ElementBox { get { return this.elementBox; } set { this.elementBox = value; } }
		public Rectangle ChildExpandButtonBox { get { return this.childExpandButtonBox; } set { this.childExpandButtonBox = value; } }
		public Rectangle BoundingBox { get { return this.boundingBox; } set { this.boundingBox = value; } }
		public int ChildExpandButtonSize { get { return childExpandButtonSize; } set { childExpandButtonSize = value; } }
		public int Depth { get { return this.depth; } set { this.depth = value; } }

		public List<DiagramBase> ChildElements { get { return this.childElements; } }

		public XMLSchema.openAttrs TabSchema { get { return this.tabSchema; } set { this.tabSchema = value; } }

		public DiagramBase()
		{
		}

		public virtual void GenerateMeasure(Graphics g)
		{
			if (this.parent != null)
				this.depth = this.parent.Depth + 1;

			if (this.type == DiagramBase.TypeEnum.group)
			{
				this.size = new Size(40, 20);
			}
			//else
			//    this.size = new Size(50, 25);

			if (this.name.Length > 0)
			{
				SizeF sizeF = g.MeasureString(this.name, this.Font);
				//MONOFIX this.size = sizeF.ToSize();
				this.size = new Size((int)sizeF.Width, (int)sizeF.Height);
				this.size = this.size + new Size(2 * Margin.Width + (this.hasChildElements ? this.ChildExpandButtonSize : 0), 2 * Margin.Height);
			}

			int childBoundingBoxHeight = 0;
			int childBoundingBoxWidth = 0;
			if (this.showChildElements)
			{
				foreach (DiagramBase element in this.childElements)
				{
					//MONOFIX GenerateMeasure not supported???
					element.GenerateMeasure(g);
					childBoundingBoxWidth = Math.Max(childBoundingBoxWidth, element.BoundingBox.Size.Width);
					childBoundingBoxHeight += element.BoundingBox.Size.Height;
				}
			}
			this.boundingBox.Width = this.size.Width + 2 * this.padding.Width + childBoundingBoxWidth;
			this.boundingBox.Height = Math.Max(this.size.Height + 2 * this.padding.Height, childBoundingBoxHeight);


			this.elementBox = new Rectangle(new Point(0, 0), this.size - new Size(this.hasChildElements ? this.childExpandButtonSize / 2 : 0, 0));

			if (this.hasChildElements)
			{
				this.childExpandButtonBox.X = this.elementBox.Width - this.childExpandButtonSize / 2;
				this.childExpandButtonBox.Y = (this.elementBox.Height - this.childExpandButtonSize) / 2;
				this.childExpandButtonBox.Width = this.childExpandButtonSize;
				this.childExpandButtonBox.Height = this.childExpandButtonSize;
			}
		}

		public virtual void GenerateLocation()
		{
			this.location.X = this.boundingBox.X + this.padding.Width;

			switch (this.diagram.Alignement)
			{
				case Alignement.Center:
					this.location.Y = this.boundingBox.Y +
						(this.boundingBox.Height - this.size.Height) / 2;
					break;
				case Alignement.Near:
					if (this.type == DiagramBase.TypeEnum.group && this.parent != null && this.parent.ChildElements.Count == 1)
						this.location.Y = this.parent.Location.Y + (this.parent.elementBox.Height - this.elementBox.Height) / 2;
					else
						this.location.Y = this.boundingBox.Y + this.padding.Height;
					break;
				case Alignement.Far:
					if (this.type == DiagramBase.TypeEnum.group && this.parent != null && this.parent.ChildElements.Count == 1)
						this.location.Y = this.parent.Location.Y + (this.parent.elementBox.Height - this.elementBox.Height) / 2;
					else
						this.location.Y = this.boundingBox.Y +
							this.boundingBox.Height - this.size.Height - this.padding.Height;
					break;
			}

			if (this.showChildElements)
			{
				int childrenHeight = 0;
				foreach (DiagramBase element in this.childElements)
					childrenHeight += element.BoundingBox.Height;

				int childrenX = this.boundingBox.X + 2 * this.padding.Width + this.Size.Width;
				int childrenY = this.boundingBox.Y + Math.Max(0, (this.boundingBox.Height - childrenHeight) / 2);

				foreach (DiagramBase element in this.childElements)
				{
					Rectangle elementBoundingBox = element.BoundingBox;
					elementBoundingBox.X = childrenX;
					elementBoundingBox.Y = childrenY;
					element.BoundingBox = elementBoundingBox;
					element.GenerateLocation();
					childrenY += element.BoundingBox.Height;
				}
			}

			if (this.hasChildElements)
				this.childExpandButtonBox.Offset(this.location);
			this.elementBox.Offset(this.location);
		}

		protected int ScaleInt(int integer) { return this.diagram.ScaleInt(integer); }
		protected Point ScalePoint(Point point) { return this.diagram.ScalePoint(point); }
		protected Size ScaleSize(Size point) { return this.diagram.ScaleSize(point); }
		protected Rectangle ScaleRectangle(Rectangle rectangle) { return this.diagram.ScaleRectangle(rectangle); }

		public void HitTest(Point point, out DiagramBase element, out HitTestRegion region)
		{
			element = null;
			if (ScaleRectangle(this.boundingBox).Contains(point))
			{
				element = this;
				if (ScaleRectangle(new Rectangle(this.location, this.size)).Contains(point))
				{
					if (this.hasChildElements && ScaleRectangle(this.childExpandButtonBox).Contains(point))
						region = HitTestRegion.ChildExpandButton;
					else
						region = HitTestRegion.Element;
				}
				else
				{
					region = HitTestRegion.BoundingBox;
					if (this.showChildElements)
					{
						foreach (DiagramBase childElement in this.childElements)
						{
							DiagramBase resultElement;
							HitTestRegion resultRegion;
							childElement.HitTest(point, out resultElement, out resultRegion);
							if (resultRegion != HitTestRegion.None)
							{
								element = resultElement;
								region = resultRegion;
								break;
							}
						}
					}
				}
			}
			else
				region = HitTestRegion.None;
		}

		public virtual void Paint(Graphics g)
		{
			Paint(g, null);
		}

		public virtual void Paint(Graphics g, Rectangle? clipRectangle)
		{
			//System.Diagnostics.Trace.WriteLine("DiagramElement.Paint\n\tName: " + this.name);

			SolidBrush background = new SolidBrush(Color.White);
			SolidBrush foreground = new SolidBrush(Color.Black);
			Pen foregroundPen = new Pen(foreground);
			float[] dashPattern = new float[] { Math.Max(2f, ScaleInt(5)), Math.Max(1f, ScaleInt(2)) };

			//if (this.isReference && this.diagram.ShowBoundingBox)
			if (this.diagram.ShowBoundingBox)
			{
				int color = 255 - depth * 8;
				g.FillRectangle(new SolidBrush(Color.FromArgb(color, color, color)), ScaleRectangle(this.boundingBox));
				g.DrawRectangle(foregroundPen, ScaleRectangle(this.boundingBox));
			}

			// Draw the children
			if (this.showChildElements)
			{
				if (clipRectangle.HasValue)
				{
					foreach (DiagramBase element in this.childElements)
						if (element.BoundingBox.IntersectsWith(clipRectangle.Value))
							element.Paint(g, clipRectangle);
				}
				else
				{
					foreach (DiagramBase element in this.childElements)
						element.Paint(g);
				}
			}

			Rectangle scaledElementBox = ScaleRectangle(this.elementBox);

			// Draw the children lines
            if (this.showChildElements)
            {
                Pen foregroundInheritPen = new Pen(foreground);
                foregroundInheritPen.StartCap = LineCap.Round;
                foregroundInheritPen.EndCap = LineCap.Round;

                if (this.childElements.Count == 1)
                {
                    int parentMidleY = ScaleInt(this.location.Y + this.size.Height / 2);
                    g.DrawLine(foregroundInheritPen, ScaleInt(this.location.X + this.size.Width), parentMidleY, ScaleInt(this.childElements[0].Location.X), parentMidleY);
                }
                else if (this.childElements.Count > 1)
                {
                    DiagramBase firstElement = this.childElements[0];
                    DiagramBase lastElement = this.childElements[this.childElements.Count - 1];
                    int verticalLine = ScaleInt(firstElement.BoundingBox.Left);
                    foreach (DiagramBase element in this.childElements)
                    {
                        if (element.InheritFrom == null)
                        {
                            int currentMidleY = ScaleInt(element.Location.Y + element.Size.Height / 2);
                            g.DrawLine(foregroundInheritPen, verticalLine, currentMidleY, ScaleInt(element.Location.X), currentMidleY);
                        }
                    }
                    int parentMidleY = ScaleInt(this.location.Y + this.size.Height / 2);
                    int firstMidleY = ScaleInt(firstElement.Location.Y + firstElement.Size.Height / 2);
                    firstMidleY = Math.Min(firstMidleY, parentMidleY);
                    int lastMidleY = ScaleInt(lastElement.Location.Y + lastElement.Size.Height / 2);
                    lastMidleY = Math.Max(lastMidleY, parentMidleY);
                    g.DrawLine(foregroundInheritPen, verticalLine, firstMidleY, verticalLine, lastMidleY);
                    g.DrawLine(foregroundInheritPen, ScaleInt(this.location.X + this.size.Width), parentMidleY, verticalLine, parentMidleY);
                }
            }


			// Draw the inheritor line
			if (this.inheritFrom != null)
			{
				Pen foregroundInheritPen = new Pen(foreground);
				foregroundInheritPen.DashStyle = DashStyle.Dash;
				foregroundInheritPen.DashPattern = dashPattern;

				Point p1 = new Point(ScaleInt(this.inheritFrom.Location.X - 5), ScaleInt(this.inheritFrom.Location.Y + this.inheritFrom.Size.Height + 5));
				Point p2 = new Point(ScaleInt(this.location.X - 5), ScaleInt(this.location.Y - 5));
				g.DrawLine(foregroundInheritPen, p1, p2);
				g.DrawLine(foregroundInheritPen, p2, new Point(ScaleInt(this.location.X), ScaleInt(this.location.Y)));

				Point targetPoint = new Point(ScaleInt(this.inheritFrom.Location.X), ScaleInt(this.inheritFrom.Location.Y + this.inheritFrom.Size.Height));
				g.DrawLine(foregroundInheritPen, targetPoint, p1);

				Point[] pathPoint = new Point[4];
				pathPoint[0] = targetPoint;
				pathPoint[1] = targetPoint; pathPoint[1].Y += ScaleInt(5);
				pathPoint[2] = targetPoint; pathPoint[2].X -= ScaleInt(5);
				pathPoint[3] = targetPoint;

				GraphicsPath path = new GraphicsPath();
				path.StartFigure();
				path.AddPolygon(pathPoint);
				path.CloseFigure();
				
				Pen foregroundBoxPen = new Pen(foreground);
				g.FillPath(background, path);
				g.DrawPath(foregroundBoxPen, path);
			}

			switch (this.type)
			{
				case TypeEnum.element:
					{
						// Draw the main shape following the min/max occurences
						Pen foregroundBoxPen = new Pen(foreground);
						if (this.minOccurrence == 0)
						{
							foregroundBoxPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
							foregroundBoxPen.DashPattern = dashPattern;
						}
						if (this.maxOccurrence == 1)
						{
							g.FillRectangle(background, scaledElementBox);
							g.DrawRectangle(foregroundBoxPen, scaledElementBox);
						}
						else
						{
							Rectangle elementBoxShifted = scaledElementBox;
							elementBoxShifted.Offset(ScalePoint(new Point(3, 3)));
							g.FillRectangle(background, elementBoxShifted);
							g.DrawRectangle(foregroundBoxPen, elementBoxShifted);
							g.FillRectangle(background, scaledElementBox);
							g.DrawRectangle(foregroundBoxPen, scaledElementBox);
						}
					}
					break;

				case TypeEnum.type:
					{
						// Draw the main shape following the min/max occurences
						int bevel = (int)(scaledElementBox.Height * 0.30);
						Point[] pathPoint = new Point[6];
						pathPoint[0] = pathPoint[5] = scaledElementBox.Location;
						pathPoint[1] = scaledElementBox.Location; pathPoint[1].X = scaledElementBox.Right;
						pathPoint[2] = scaledElementBox.Location + scaledElementBox.Size;
						pathPoint[3] = scaledElementBox.Location; pathPoint[3].Y = scaledElementBox.Bottom; pathPoint[4] = pathPoint[3];
						pathPoint[0].X += bevel;
						pathPoint[3].X += bevel;
						pathPoint[4].Y -= bevel;
						pathPoint[5].Y += bevel;

						GraphicsPath path = new GraphicsPath();
						path.StartFigure();
						path.AddPolygon(pathPoint);
						path.CloseFigure();

						Point[] pathPointShifted = new Point[6];
						Size scaledShiftedBevel = ScaleSize(new Size(3, 3));
						for (int i = 0; i < pathPoint.Length; i++)
							pathPointShifted[i] = pathPoint[i] + scaledShiftedBevel;

						GraphicsPath pathShifted = new GraphicsPath();
						pathShifted.StartFigure();
						pathShifted.AddPolygon(pathPointShifted);
						pathShifted.CloseFigure();

						Pen foregroundBoxPen = new Pen(foreground);
						if (this.minOccurrence == 0)
						{
							foregroundBoxPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
							foregroundBoxPen.DashPattern = dashPattern;
						}
						if (this.maxOccurrence == 1)
						{
							g.FillPath(background, path);
							g.DrawPath(foregroundBoxPen, path);
						}
						else
						{
							Rectangle elementBoxShifted = scaledElementBox;
							elementBoxShifted.Offset(ScalePoint(new Point(3, 3)));
							g.FillPath(background, pathShifted);
							g.DrawPath(foregroundBoxPen, pathShifted);
							g.FillPath(background, path);
							g.DrawPath(foregroundBoxPen, path);
						}
					}
					break;

				case TypeEnum.group:
					{
						// Draw the main shape following the min/max occurences
						int bevel = (int)(scaledElementBox.Height * 0.30);
						Point[] pathPoint = new Point[8];
						pathPoint[0] = pathPoint[7] = scaledElementBox.Location;
						pathPoint[1] = scaledElementBox.Location; pathPoint[1].X = scaledElementBox.Right; pathPoint[2] = pathPoint[1];
						pathPoint[3] = pathPoint[4] = scaledElementBox.Location + scaledElementBox.Size;
						pathPoint[5] = scaledElementBox.Location; pathPoint[5].Y = scaledElementBox.Bottom; pathPoint[6] = pathPoint[5];
						pathPoint[0].X += bevel;
						pathPoint[1].X -= bevel;
						pathPoint[2].Y += bevel;
						pathPoint[3].Y -= bevel;
						pathPoint[4].X -= bevel;
						pathPoint[5].X += bevel;
						pathPoint[6].Y -= bevel;
						pathPoint[7].Y += bevel;

						GraphicsPath path = new GraphicsPath();
						path.StartFigure();
						path.AddPolygon(pathPoint);
						path.CloseFigure();

						Point[] pathPointShifted = new Point[8];
						Size scaledShiftedBevel = ScaleSize(new Size(3, 3));
						for (int i = 0; i < pathPoint.Length; i++)
							pathPointShifted[i] = pathPoint[i] + scaledShiftedBevel;

						GraphicsPath pathShifted = new GraphicsPath();
						pathShifted.StartFigure();
						pathShifted.AddPolygon(pathPointShifted);
						pathShifted.CloseFigure();

						Pen foregroundBoxPen = new Pen(foreground);
						if (this.minOccurrence == 0)
						{
							foregroundBoxPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
							foregroundBoxPen.DashPattern = dashPattern;
						}
						if (this.maxOccurrence == 1)
						{
							g.FillPath(background, path);
							g.DrawPath(foregroundBoxPen, path);
						}
						else
						{
							Rectangle elementBoxShifted = scaledElementBox;
							elementBoxShifted.Offset(ScalePoint(new Point(3, 3)));
							g.FillPath(background, pathShifted);
							g.DrawPath(foregroundBoxPen, pathShifted);
							g.FillPath(background, path);
							g.DrawPath(foregroundBoxPen, path);
						}

						// Draw the group type
						//Pen foregroundPointPen = new Pen(foreground, 4.0f);
						switch (this.groupType)
						{
							case GroupTypeEnum.sequence:
								{
									Point p0 = this.Location + new Size(0, this.elementBox.Height / 2);
									Point p1 = p0 + new Size(3, 0);
									Point p2 = p1 + new Size(this.elementBox.Width - 6, 0);
									g.DrawLine(foregroundPen, ScalePoint(p1), ScalePoint(p2));
									Point point2 = p0 + new Size(this.elementBox.Width / 2, 0);
									Point point1 = point2 + new Size(-5, 0);
									Point point3 = point2 + new Size(+5, 0);
									Size pointSize = new Size(4, 4);
									Size pointSize2 = new Size(pointSize.Width / 2, pointSize.Height / 2);
									point1 -= pointSize2;
									point2 -= pointSize2;
									point3 -= pointSize2;
									pointSize = ScaleSize(pointSize);
									g.FillEllipse(foreground, new Rectangle(ScalePoint(point1), pointSize));
									g.FillEllipse(foreground, new Rectangle(ScalePoint(point2), pointSize));
									g.FillEllipse(foreground, new Rectangle(ScalePoint(point3), pointSize));

									//Point p0 = this.Location + new Size(0, this.elementBox.Height / 2);
									//Point point0 = p0 + new Size(3, 0);
									//Point point2 = p0 + new Size(this.elementBox.Width / 2, 0);
									//Point point1 = point2 + new Size(-5, 0);
									//Point point3 = point2 + new Size(+5, 0);
									//Point point4 = point0 + new Size(this.elementBox.Width - 6, 0);

									//Pen foregroundBallPen = new Pen(foreground);
									//foregroundBallPen.EndCap = LineCap.RoundAnchor;
									////foregroundBallPen.ScaleTransform(1.0f / this.diagram.Scale, 1.0f / this.diagram.Scale);
									//foregroundBallPen.ScaleTransform(this.diagram.Scale, this.diagram.Scale);

									//g.DrawLine(foregroundBallPen, ScalePoint(point0), ScalePoint(point1));
									//g.DrawLine(foregroundBallPen, ScalePoint(point1), ScalePoint(point2));
									//g.DrawLine(foregroundBallPen, ScalePoint(point2), ScalePoint(point3));
									//foregroundBallPen.EndCap = LineCap.Flat;
									//g.DrawLine(foregroundBallPen, ScalePoint(point3), ScalePoint(point4));
								}
								break;
							case GroupTypeEnum.choice:
								{
									int yMiddle = this.elementBox.Y + this.elementBox.Height / 2;
									int yUp = yMiddle - 4;
									int yDown = yMiddle + 4;
									int xMiddle = this.elementBox.X + this.elementBox.Width / 2;
									int xLeft2 = xMiddle - 4;
									int xLeft1 = xLeft2 - 4;
									int xLeft0 = xLeft1 - 4;
									int xRight0 = xMiddle + 4;
									int xRight1 = xRight0 + 4;
									int xRight2 = xRight1 + 4;

									Point point1 = new Point(xMiddle, yUp);
									Point point2 = new Point(xMiddle, yMiddle);
									Point point3 = new Point(xMiddle, yDown);
									Size pointSize = new Size(4, 4);
									Size pointSize2 = new Size(pointSize.Width / 2, pointSize.Height / 2);
									point1 -= pointSize2;
									point2 -= pointSize2;
									point3 -= pointSize2;
									pointSize = ScaleSize(pointSize);
									g.DrawLine(foregroundPen, ScalePoint(new Point(xLeft0, yMiddle)), ScalePoint(new Point(xLeft1, yMiddle)));
									g.DrawLine(foregroundPen, ScalePoint(new Point(xLeft1, yMiddle)), ScalePoint(new Point(xLeft2, yUp)));
									g.DrawLine(foregroundPen, ScalePoint(new Point(xRight0, yUp)), ScalePoint(new Point(xRight1, yUp)));
									g.DrawLine(foregroundPen, ScalePoint(new Point(xRight0, yMiddle)), ScalePoint(new Point(xRight2, yMiddle)));
									g.DrawLine(foregroundPen, ScalePoint(new Point(xRight0, yDown)), ScalePoint(new Point(xRight1, yDown)));
									g.DrawLine(foregroundPen, ScalePoint(new Point(xRight1, yUp)), ScalePoint(new Point(xRight1, yDown)));
									g.FillEllipse(foreground, new Rectangle(ScalePoint(point1), pointSize));
									g.FillEllipse(foreground, new Rectangle(ScalePoint(point2), pointSize));
									g.FillEllipse(foreground, new Rectangle(ScalePoint(point3), pointSize));
								}
								break;
							case GroupTypeEnum.all:
								{
									int yMiddle = this.elementBox.Y + this.elementBox.Height / 2;
									int yUp = yMiddle - 4;
									int yDown = yMiddle + 4;
									int xMiddle = this.elementBox.X + this.elementBox.Width / 2;
									int xLeft2 = xMiddle - 4;
									int xLeft1 = xLeft2 - 4;
									int xLeft0 = xLeft1 - 4;
									int xRight0 = xMiddle + 4;
									int xRight1 = xRight0 + 4;
									int xRight2 = xRight1 + 4;

									Point point1 = new Point(xMiddle, yUp);
									Point point2 = new Point(xMiddle, yMiddle);
									Point point3 = new Point(xMiddle, yDown);
									Size pointSize = new Size(4, 4);
									Size pointSize2 = new Size(pointSize.Width / 2, pointSize.Height / 2);
									point1 -= pointSize2;
									point2 -= pointSize2;
									point3 -= pointSize2;
									pointSize = ScaleSize(pointSize);
									g.DrawLine(foregroundPen, ScalePoint(new Point(xLeft2, yUp)), ScalePoint(new Point(xLeft1, yUp)));
									g.DrawLine(foregroundPen, ScalePoint(new Point(xLeft2, yMiddle)), ScalePoint(new Point(xLeft0, yMiddle)));
									g.DrawLine(foregroundPen, ScalePoint(new Point(xLeft2, yDown)), ScalePoint(new Point(xLeft1, yDown)));
									g.DrawLine(foregroundPen, ScalePoint(new Point(xLeft1, yUp)), ScalePoint(new Point(xLeft1, yDown)));

									g.DrawLine(foregroundPen, ScalePoint(new Point(xRight0, yUp)), ScalePoint(new Point(xRight1, yUp)));
									g.DrawLine(foregroundPen, ScalePoint(new Point(xRight0, yMiddle)), ScalePoint(new Point(xRight2, yMiddle)));
									g.DrawLine(foregroundPen, ScalePoint(new Point(xRight0, yDown)), ScalePoint(new Point(xRight1, yDown)));
									g.DrawLine(foregroundPen, ScalePoint(new Point(xRight1, yUp)), ScalePoint(new Point(xRight1, yDown)));
									g.FillEllipse(foreground, new Rectangle(ScalePoint(point1), pointSize));
									g.FillEllipse(foreground, new Rectangle(ScalePoint(point2), pointSize));
									g.FillEllipse(foreground, new Rectangle(ScalePoint(point3), pointSize));
								}
								break;
						}
						break;
					}
			}

			// Draw text
			if (this.name.Length > 0)
			{
				StringFormat stringFormatText = new StringFormat();
				stringFormatText.Alignment = StringAlignment.Center;
				stringFormatText.LineAlignment = StringAlignment.Center;
				stringFormatText.FormatFlags |= StringFormatFlags.NoClip; //MONOFIX
				g.DrawString(this.name, this.Font, foreground, new RectangleF(scaledElementBox.X, scaledElementBox.Y, scaledElementBox.Width, scaledElementBox.Height), stringFormatText);
			}

			// Draw occurences small text
			if (this.maxOccurrence > 1 || this.maxOccurrence == -1)
			{
				StringFormat stringFormatOccurences = new StringFormat();
				stringFormatOccurences.Alignment = StringAlignment.Far;
				stringFormatOccurences.LineAlignment = StringAlignment.Center;
				stringFormatOccurences.FormatFlags |= StringFormatFlags.NoClip; //MONOFIX
				string occurences = string.Format("{0}..", this.minOccurrence) + (this.maxOccurrence == -1 ? "∞" : string.Format("{0}", this.maxOccurrence));
				PointF pointOccurences = new PointF();
				pointOccurences.X = this.Diagram.Scale * (this.Location.X + this.Size.Width - 10);
				pointOccurences.Y = this.Diagram.Scale * (this.Location.Y + this.Size.Height + 10);
				g.DrawString(occurences, this.SmallFont, foreground, pointOccurences, stringFormatOccurences);
			}

			// Draw type
			if (this.isSimpleContent)
			{
				Point currentPoint = scaledElementBox.Location + new Size(2, 2);
				g.DrawLine(foregroundPen, currentPoint, currentPoint + new Size(ScaleInt(8), 0));
				currentPoint += new Size(0, 2);
				g.DrawLine(foregroundPen, currentPoint, currentPoint + new Size(ScaleInt(6), 0));
				currentPoint += new Size(0, 2);
				g.DrawLine(foregroundPen, currentPoint, currentPoint + new Size(ScaleInt(6), 0));
				currentPoint += new Size(0, 2);
				g.DrawLine(foregroundPen, currentPoint, currentPoint + new Size(ScaleInt(6), 0));
			}

			// Draw reference arrow
			if (this.isReference)
			{
				Pen arrowPen = new Pen(foreground, this.Diagram.Scale * 2.0f);
				//arrowPen.EndCap = LineCap.ArrowAnchor;
				//Point basePoint = new Point(this.elementBox.Left + 2, this.elementBox.Bottom - 2);
				//g.DrawLine(arrowPen, ScalePoint(basePoint), ScalePoint(basePoint + new Size(4, -4)));
				Point basePoint = new Point(this.elementBox.Left + 1, this.elementBox.Bottom - 1);
				Point targetPoint = basePoint + new Size(3, -3);
				basePoint = ScalePoint(basePoint);
				targetPoint = ScalePoint(targetPoint);
				g.DrawLine(arrowPen, basePoint, targetPoint);

				Point[] pathPoint = new Point[5];
				pathPoint[0] = targetPoint;
				pathPoint[1] = targetPoint; pathPoint[1].X += ScaleInt(2); pathPoint[1].Y += ScaleInt(2);
				pathPoint[2] = targetPoint; pathPoint[2].X += ScaleInt(3); pathPoint[2].Y -= ScaleInt(3);
				pathPoint[3] = targetPoint; pathPoint[3].X -= ScaleInt(2); pathPoint[3].Y -= ScaleInt(2);
				pathPoint[4] = targetPoint;

				GraphicsPath path = new GraphicsPath();
				path.StartFigure();
				path.AddPolygon(pathPoint);
				path.CloseFigure();
				g.FillPath(foreground, path);
			}

			// Draw children expand box
			if (this.hasChildElements)
			{
				Rectangle scaledChildExpandButtonBox = ScaleRectangle(this.childExpandButtonBox);
				g.FillRectangle(background, scaledChildExpandButtonBox);
				g.DrawRectangle(foregroundPen, scaledChildExpandButtonBox);

				Point middle = new Point(scaledChildExpandButtonBox.Width / 2, scaledChildExpandButtonBox.Height / 2);
				int borderPadding = Math.Max(2, ScaleInt(2));

				Point p1 = scaledChildExpandButtonBox.Location + new Size(borderPadding, middle.Y);
				Point p2 = new Point(scaledChildExpandButtonBox.Right - borderPadding, p1.Y);
				g.DrawLine(foregroundPen, p1, p2);
				if (!this.showChildElements)
				{
					p1 = scaledChildExpandButtonBox.Location + new Size(middle.X, borderPadding);
					p2 = new Point(p1.X, scaledChildExpandButtonBox.Bottom - borderPadding);
					g.DrawLine(foregroundPen, p1, p2);
				}
			}
		}

		// pen = "stroke:rgb(99,99,99);stroke-width:2"

		void SVGLine(StringBuilder result, string pen, Point pt1, Point pt2) { SVGLine(result, pen, pt1.X, pt1.Y, pt2.X, pt2.Y); }
		void SVGLine(StringBuilder result, string pen, int x1, int y1, int x2, int y2)
		{
			result.AppendFormat("<line x1=\"{0}\" y1=\"{1}\" x2=\"{2}\" y2=\"{3}\" style=\"{4}\"/>\n", x1, y1, x2, y2, pen);
		}

		void SVGRectangle(StringBuilder result, string pen, Rectangle rect)
		{
			result.AppendFormat("<rect x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{3}\" style=\"{4}\"/>\n", rect.X, rect.Y, rect.Width, rect.Height, pen);
		}

		void SVGEllipse(StringBuilder result, string brush, Rectangle rect)
		{
			result.AppendFormat("<ellipse cx=\"{0}\" cy=\"{1}\" rx=\"{2}\" ry=\"{3}\" style=\"{4}\"/>\n", rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width / 2, rect.Height / 2, brush);
		}

		void SVGPath(StringBuilder result, string style, string drawCommand)
		{
			result.AppendFormat("<path d=\"{0}\" style=\"{1}\"/>\n", drawCommand, style);
		}

		void SVGText(StringBuilder result, string text, string style, Point point)
		{
			result.AppendFormat("<text x=\"{0}\" y=\"{1}\" style=\"{2}\">{3}</text>\n", point.X, point.Y, style, text);
		}
		void SVGText(StringBuilder result, string text, string style, Rectangle rect)
		{
			result.AppendFormat("<text x=\"{0}\" y=\"{1}\" style=\"{2}\">{3}</text>\n", rect.X + rect.Width / 2.0, rect.Y + rect.Height / 2.0, style, text);
		}

		private string SVGPolygonToDrawCommand(Point[] pathPoint)
		{
			StringBuilder result = new StringBuilder();
			for (int i = 0; i < pathPoint.Length; i++)
			{
				result.AppendFormat("{0}{1} {2} ", i == 0 ? 'M' : 'L', pathPoint[i].X, pathPoint[i].Y);
			}
			result.Append('Z');
			return result.ToString();
		}

		public virtual string ToSVG() //Graphics g)
		{
			StringBuilder result = new StringBuilder("");

			string backgroundBrush = "fill:rgb(255,255,255)";
			string foregroundColor = "rgb(0,0,0)";
			string foregroundBrush = "fill:" + foregroundColor;
			string foregroundPen = "stroke:" + foregroundColor + ";stroke-width:1";
			string foregroundRoundPen = foregroundPen + ";stroke-linecap:round";
			string dashed = "stroke-dasharray:4,1";

			//if (this.diagram.ShowBoundingBox)
			//{
			//    int color = 255 - depth * 8;
			//    g.FillRectangle(new SolidBrush(Color.FromArgb(color, color, color)), ScaleRectangle(this.boundingBox));
			//    g.DrawRectangle(foregroundPen, ScaleRectangle(this.boundingBox));
			//}

			// Draw the children
			if (this.showChildElements)
			{
				foreach (DiagramBase element in this.childElements)
					result.Append(element.ToSVG());
			}

			Rectangle scaledElementBox = ScaleRectangle(this.elementBox);

			// Draw the children lines
			if (this.showChildElements)
			{
				if (this.childElements.Count == 1)
				{
					int parentMidleY = ScaleInt(this.location.Y + this.size.Height / 2);
					SVGLine(result, foregroundRoundPen, ScaleInt(this.location.X + this.size.Width), parentMidleY, ScaleInt(this.childElements[0].Location.X), parentMidleY);
				}
				else if (this.childElements.Count > 1)
				{
					DiagramBase firstElement = this.childElements[0];
					DiagramBase lastElement = this.childElements[this.childElements.Count - 1];
					int verticalLine = ScaleInt(firstElement.BoundingBox.Left);
					foreach (DiagramBase element in this.childElements)
					{
						if (element.InheritFrom == null)
						{
							int currentMidleY = ScaleInt(element.Location.Y + element.Size.Height / 2);
							SVGLine(result, foregroundRoundPen, verticalLine, currentMidleY, ScaleInt(element.Location.X), currentMidleY);
						}
					}
					int parentMidleY = ScaleInt(this.location.Y + this.size.Height / 2);
					int firstMidleY = ScaleInt(firstElement.Location.Y + firstElement.Size.Height / 2);
					firstMidleY = Math.Min(firstMidleY, parentMidleY);
					int lastMidleY = ScaleInt(lastElement.Location.Y + lastElement.Size.Height / 2);
					lastMidleY = Math.Max(lastMidleY, parentMidleY);
					SVGLine(result, foregroundRoundPen, verticalLine, firstMidleY, verticalLine, lastMidleY);
					SVGLine(result, foregroundRoundPen, ScaleInt(this.location.X + this.size.Width), parentMidleY, verticalLine, parentMidleY);
				}
			}

			// Draw the inheritor line
			if (this.inheritFrom != null)
			{
				string foregroundInheritPen = foregroundPen + ";" + dashed;

				Point p1 = new Point(ScaleInt(this.inheritFrom.Location.X - 5), ScaleInt(this.inheritFrom.Location.Y + this.inheritFrom.Size.Height + 5));
				Point p2 = new Point(ScaleInt(this.location.X - 5), ScaleInt(this.location.Y - 5));
				SVGLine(result, foregroundInheritPen, p1, p2);
				SVGLine(result, foregroundInheritPen, p2, new Point(ScaleInt(this.location.X), ScaleInt(this.location.Y)));

				Point targetPoint = new Point(ScaleInt(this.inheritFrom.Location.X - 3), ScaleInt(this.inheritFrom.Location.Y + this.inheritFrom.Size.Height + 3));
				SVGLine(result, foregroundInheritPen, targetPoint, p1);
				Point[] pathPoint = new Point[5];
				pathPoint[0] = targetPoint;
				pathPoint[1] = targetPoint; pathPoint[1].X += ScaleInt(2); pathPoint[1].Y += ScaleInt(2);
				pathPoint[2] = targetPoint; pathPoint[2].X += ScaleInt(3); pathPoint[2].Y -= ScaleInt(3);
				pathPoint[3] = targetPoint; pathPoint[3].X -= ScaleInt(2); pathPoint[3].Y -= ScaleInt(2);
				pathPoint[4] = targetPoint;

				string path = SVGPolygonToDrawCommand(pathPoint);
				SVGPath(result, backgroundBrush + ";" + foregroundPen, path);
			}

			switch (this.type)
			{
				case TypeEnum.element:
					{
						// Draw the main shape following the min/max occurences
						string foregroundBoxPen = foregroundPen;

						if (this.minOccurrence == 0)
						{
							foregroundBoxPen += ";" + dashed;
						}
						if (this.maxOccurrence == 1)
						{
							SVGRectangle(result, backgroundBrush + ";" + foregroundBoxPen, scaledElementBox);
						}
						else
						{
							Rectangle elementBoxShifted = scaledElementBox;
							elementBoxShifted.Offset(ScalePoint(new Point(3, 3)));
							SVGRectangle(result, backgroundBrush + ";" + foregroundBoxPen, elementBoxShifted);
							SVGRectangle(result, backgroundBrush + ";" + foregroundBoxPen, scaledElementBox);
						}
					}
					break;

				case TypeEnum.type:
					{
						// Draw the main shape following the min/max occurences
						int bevel = (int)(scaledElementBox.Height * 0.30);
						Point[] pathPoint = new Point[6];
						pathPoint[0] = pathPoint[5] = scaledElementBox.Location;
						pathPoint[1] = scaledElementBox.Location; pathPoint[1].X = scaledElementBox.Right;
						pathPoint[2] = scaledElementBox.Location + scaledElementBox.Size;
						pathPoint[3] = scaledElementBox.Location; pathPoint[3].Y = scaledElementBox.Bottom; pathPoint[4] = pathPoint[3];
						pathPoint[0].X += bevel;
						pathPoint[3].X += bevel;
						pathPoint[4].Y -= bevel;
						pathPoint[5].Y += bevel;

						string path = SVGPolygonToDrawCommand(pathPoint);

						Point[] pathPointShifted = new Point[6];
						Size scaledShiftedBevel = ScaleSize(new Size(3, 3));
						for (int i = 0; i < pathPoint.Length; i++)
							pathPointShifted[i] = pathPoint[i] + scaledShiftedBevel;

						string pathShifted = SVGPolygonToDrawCommand(pathPointShifted);

						string foregroundBoxPen = foregroundPen;
						if (this.minOccurrence == 0)
						{
							foregroundBoxPen += ";" + dashed;
						}
						if (this.maxOccurrence == 1)
						{
							SVGPath(result, backgroundBrush + ";" + foregroundBoxPen, path);
						}
						else
						{
							Rectangle elementBoxShifted = scaledElementBox;
							elementBoxShifted.Offset(ScalePoint(new Point(3, 3)));
							SVGPath(result, backgroundBrush + ";" + foregroundBoxPen, pathShifted);
							SVGPath(result, backgroundBrush + ";" + foregroundBoxPen, path);
						}
					}
					break;

				case TypeEnum.group:
					{
						// Draw the main shape following the min/max occurences
						int bevel = (int)(scaledElementBox.Height * 0.30);
						Point[] pathPoint = new Point[8];
						pathPoint[0] = pathPoint[7] = scaledElementBox.Location;
						pathPoint[1] = scaledElementBox.Location; pathPoint[1].X = scaledElementBox.Right; pathPoint[2] = pathPoint[1];
						pathPoint[3] = pathPoint[4] = scaledElementBox.Location + scaledElementBox.Size;
						pathPoint[5] = scaledElementBox.Location; pathPoint[5].Y = scaledElementBox.Bottom; pathPoint[6] = pathPoint[5];
						pathPoint[0].X += bevel;
						pathPoint[1].X -= bevel;
						pathPoint[2].Y += bevel;
						pathPoint[3].Y -= bevel;
						pathPoint[4].X -= bevel;
						pathPoint[5].X += bevel;
						pathPoint[6].Y -= bevel;
						pathPoint[7].Y += bevel;

						string path = SVGPolygonToDrawCommand(pathPoint);

						Point[] pathPointShifted = new Point[8];
						Size scaledShiftedBevel = ScaleSize(new Size(3, 3));
						for (int i = 0; i < pathPoint.Length; i++)
							pathPointShifted[i] = pathPoint[i] + scaledShiftedBevel;

						string pathShifted = SVGPolygonToDrawCommand(pathPointShifted);


						string foregroundBoxPen = foregroundPen;
						if (this.minOccurrence == 0)
						{
							foregroundBoxPen += ";" + dashed;
						}
						if (this.maxOccurrence == 1)
						{
							SVGPath(result, backgroundBrush + ";" + foregroundBoxPen, path);
						}
						else
						{
							SVGPath(result, backgroundBrush + ";" + foregroundBoxPen, pathShifted);
							SVGPath(result, backgroundBrush + ";" + foregroundBoxPen, path);
						}

						// Draw the group type
						switch (this.groupType)
						{
							case GroupTypeEnum.sequence:
								{
									Point p0 = this.Location + new Size(0, this.elementBox.Height / 2);
									Point p1 = p0 + new Size(3, 0);
									Point p2 = p1 + new Size(this.elementBox.Width - 6, 0);
									SVGLine(result, foregroundPen, ScalePoint(p1), ScalePoint(p2));
									Point point2 = p0 + new Size(this.elementBox.Width / 2, 0);
									Point point1 = point2 + new Size(-5, 0);
									Point point3 = point2 + new Size(+5, 0);
									Size pointSize = new Size(4, 4);
									Size pointSize2 = new Size(pointSize.Width / 2, pointSize.Height / 2);
									point1 -= pointSize2;
									point2 -= pointSize2;
									point3 -= pointSize2;
									pointSize = ScaleSize(pointSize);
									SVGEllipse(result, foregroundColor, new Rectangle(ScalePoint(point1), pointSize));
									SVGEllipse(result, foregroundColor, new Rectangle(ScalePoint(point2), pointSize));
									SVGEllipse(result, foregroundColor, new Rectangle(ScalePoint(point3), pointSize));

									//Point p0 = this.Location + new Size(0, this.elementBox.Height / 2);
									//Point point0 = p0 + new Size(3, 0);
									//Point point2 = p0 + new Size(this.elementBox.Width / 2, 0);
									//Point point1 = point2 + new Size(-5, 0);
									//Point point3 = point2 + new Size(+5, 0);
									//Point point4 = point0 + new Size(this.elementBox.Width - 6, 0);

									//Pen foregroundBallPen = new Pen(foreground);
									//foregroundBallPen.EndCap = LineCap.RoundAnchor;
									////foregroundBallPen.ScaleTransform(1.0f / this.diagram.Scale, 1.0f / this.diagram.Scale);
									//foregroundBallPen.ScaleTransform(this.diagram.Scale, this.diagram.Scale);

									//SVGDrawLine(result, foregroundBallPen, ScalePoint(point0), ScalePoint(point1));
									//SVGDrawLine(result, foregroundBallPen, ScalePoint(point1), ScalePoint(point2));
									//SVGDrawLine(result, foregroundBallPen, ScalePoint(point2), ScalePoint(point3));
									//foregroundBallPen.EndCap = LineCap.Flat;
									//SVGDrawLine(result, foregroundBallPen, ScalePoint(point3), ScalePoint(point4));
								}
								break;
							case GroupTypeEnum.choice:
								{
									int yMiddle = this.elementBox.Y + this.elementBox.Height / 2;
									int yUp = yMiddle - 4;
									int yDown = yMiddle + 4;
									int xMiddle = this.elementBox.X + this.elementBox.Width / 2;
									int xLeft2 = xMiddle - 4;
									int xLeft1 = xLeft2 - 4;
									int xLeft0 = xLeft1 - 4;
									int xRight0 = xMiddle + 4;
									int xRight1 = xRight0 + 4;
									int xRight2 = xRight1 + 4;

									Point point1 = new Point(xMiddle, yUp);
									Point point2 = new Point(xMiddle, yMiddle);
									Point point3 = new Point(xMiddle, yDown);
									Size pointSize = new Size(4, 4);
									Size pointSize2 = new Size(pointSize.Width / 2, pointSize.Height / 2);
									point1 -= pointSize2;
									point2 -= pointSize2;
									point3 -= pointSize2;
									pointSize = ScaleSize(pointSize);
									SVGLine(result, foregroundPen, ScalePoint(new Point(xLeft0, yMiddle)), ScalePoint(new Point(xLeft1, yMiddle)));
									SVGLine(result, foregroundPen, ScalePoint(new Point(xLeft1, yMiddle)), ScalePoint(new Point(xLeft2, yUp)));
									SVGLine(result, foregroundPen, ScalePoint(new Point(xRight0, yUp)), ScalePoint(new Point(xRight1, yUp)));
									SVGLine(result, foregroundPen, ScalePoint(new Point(xRight0, yMiddle)), ScalePoint(new Point(xRight2, yMiddle)));
									SVGLine(result, foregroundPen, ScalePoint(new Point(xRight0, yDown)), ScalePoint(new Point(xRight1, yDown)));
									SVGLine(result, foregroundPen, ScalePoint(new Point(xRight1, yUp)), ScalePoint(new Point(xRight1, yDown)));
									SVGEllipse(result, foregroundColor, new Rectangle(ScalePoint(point1), pointSize));
									SVGEllipse(result, foregroundColor, new Rectangle(ScalePoint(point2), pointSize));
									SVGEllipse(result, foregroundColor, new Rectangle(ScalePoint(point3), pointSize));
								}
								break;
							case GroupTypeEnum.all:
								{
									int yMiddle = this.elementBox.Y + this.elementBox.Height / 2;
									int yUp = yMiddle - 4;
									int yDown = yMiddle + 4;
									int xMiddle = this.elementBox.X + this.elementBox.Width / 2;
									int xLeft2 = xMiddle - 4;
									int xLeft1 = xLeft2 - 4;
									int xLeft0 = xLeft1 - 4;
									int xRight0 = xMiddle + 4;
									int xRight1 = xRight0 + 4;
									int xRight2 = xRight1 + 4;

									Point point1 = new Point(xMiddle, yUp);
									Point point2 = new Point(xMiddle, yMiddle);
									Point point3 = new Point(xMiddle, yDown);
									Size pointSize = new Size(4, 4);
									Size pointSize2 = new Size(pointSize.Width / 2, pointSize.Height / 2);
									point1 -= pointSize2;
									point2 -= pointSize2;
									point3 -= pointSize2;
									pointSize = ScaleSize(pointSize);
									SVGLine(result, foregroundPen, ScalePoint(new Point(xLeft2, yUp)), ScalePoint(new Point(xLeft1, yUp)));
									SVGLine(result, foregroundPen, ScalePoint(new Point(xLeft2, yMiddle)), ScalePoint(new Point(xLeft0, yMiddle)));
									SVGLine(result, foregroundPen, ScalePoint(new Point(xLeft2, yDown)), ScalePoint(new Point(xLeft1, yDown)));
									SVGLine(result, foregroundPen, ScalePoint(new Point(xLeft1, yUp)), ScalePoint(new Point(xLeft1, yDown)));

									SVGLine(result, foregroundPen, ScalePoint(new Point(xRight0, yUp)), ScalePoint(new Point(xRight1, yUp)));
									SVGLine(result, foregroundPen, ScalePoint(new Point(xRight0, yMiddle)), ScalePoint(new Point(xRight2, yMiddle)));
									SVGLine(result, foregroundPen, ScalePoint(new Point(xRight0, yDown)), ScalePoint(new Point(xRight1, yDown)));
									SVGLine(result, foregroundPen, ScalePoint(new Point(xRight1, yUp)), ScalePoint(new Point(xRight1, yDown)));
									SVGEllipse(result, foregroundColor, new Rectangle(ScalePoint(point1), pointSize));
									SVGEllipse(result, foregroundColor, new Rectangle(ScalePoint(point2), pointSize));
									SVGEllipse(result, foregroundColor, new Rectangle(ScalePoint(point3), pointSize));
								}
								break;
						}
						break;
					}
			}

			float fontScale = 0.8f;

			// Draw text
			if (this.name.Length > 0)
			{
                string style = string.Format("font-family:{0};font-size:{1}pt;fill:{2};font-weight:bold;text-anchor:middle;dominant-baseline:central", this.Font.Name, this.Font.Size * fontScale, foregroundColor);
				SVGText(result, this.name, style, new Rectangle(scaledElementBox.X, scaledElementBox.Y, scaledElementBox.Width, scaledElementBox.Height));
			}

			// Draw occurences small text
			if (this.maxOccurrence > 1 || this.maxOccurrence == -1)
			{
				string occurences = string.Format("{0}..", this.minOccurrence) + (this.maxOccurrence == -1 ? "∞" : string.Format("{0}", this.maxOccurrence));
				PointF pointOccurences = new PointF();
				pointOccurences.X = this.Diagram.Scale * (this.Location.X + this.Size.Width - 10);
				pointOccurences.Y = this.Diagram.Scale * (this.Location.Y + this.Size.Height + 10);
				string style = string.Format("font-family:{0};font-size:{1}pt;fill:{2};text-anchor:end;dominant-baseline:central", this.SmallFont.Name, this.SmallFont.Size * fontScale, foregroundColor);
				SVGText(result, occurences, style, new Point((int)pointOccurences.X, (int)pointOccurences.Y));
			}

			// Draw type
			if (this.isSimpleContent)
			{
				Point currentPoint = scaledElementBox.Location + new Size(2, 2);
				SVGLine(result, foregroundPen, currentPoint, currentPoint + new Size(ScaleInt(8), 0));
				currentPoint += new Size(0, 2);
				SVGLine(result, foregroundPen, currentPoint, currentPoint + new Size(ScaleInt(6), 0));
				currentPoint += new Size(0, 2);
				SVGLine(result, foregroundPen, currentPoint, currentPoint + new Size(ScaleInt(6), 0));
				currentPoint += new Size(0, 2);
				SVGLine(result, foregroundPen, currentPoint, currentPoint + new Size(ScaleInt(6), 0));
			}

			// Draw reference arrow
			if (this.isReference)
			{
				string arrowPen = string.Format("stroke:{0};stroke-width:{1}", foregroundColor, this.Diagram.Scale * 2.0f);
				Point basePoint = new Point(this.elementBox.Left + 1, this.elementBox.Bottom - 1);
				Point targetPoint = basePoint + new Size(3, -3);
				basePoint = ScalePoint(basePoint);
				targetPoint = ScalePoint(targetPoint);
				SVGLine(result, arrowPen, basePoint, targetPoint);

				Point[] pathPoint = new Point[5];
				pathPoint[0] = targetPoint;
				pathPoint[1] = targetPoint; pathPoint[1].X += ScaleInt(2); pathPoint[1].Y += ScaleInt(2);
				pathPoint[2] = targetPoint; pathPoint[2].X += ScaleInt(3); pathPoint[2].Y -= ScaleInt(3);
				pathPoint[3] = targetPoint; pathPoint[3].X -= ScaleInt(2); pathPoint[3].Y -= ScaleInt(2);
				pathPoint[4] = targetPoint;

                string path = SVGPolygonToDrawCommand(pathPoint);
				SVGPath(result, foregroundBrush, path);
			}

			// Draw children expand box
			if (this.hasChildElements)
			{
				Rectangle scaledChildExpandButtonBox = ScaleRectangle(this.childExpandButtonBox);
				SVGRectangle(result, backgroundBrush + ";" + foregroundPen, scaledChildExpandButtonBox);

				Point middle = new Point(scaledChildExpandButtonBox.Width / 2, scaledChildExpandButtonBox.Height / 2);
				int borderPadding = Math.Max(2, ScaleInt(2));

				Point p1 = scaledChildExpandButtonBox.Location + new Size(borderPadding, middle.Y);
				Point p2 = new Point(scaledChildExpandButtonBox.Right - borderPadding, p1.Y);
				SVGLine(result, foregroundPen, p1, p2);
				if (!this.showChildElements)
				{
					p1 = scaledChildExpandButtonBox.Location + new Size(middle.X, borderPadding);
					p2 = new Point(p1.X, scaledChildExpandButtonBox.Bottom - borderPadding);
					SVGLine(result, foregroundPen, p1, p2);
				}
			}

			return result.ToString();
		}
	}
}
