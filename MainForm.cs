using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

// To generate the XMLSchema.cs file:
// > xsd.exe XMLSchema.xsd /classes /l:cs /n:XMLSchema /order

namespace XSDDiagram
{
	public partial class MainForm : Form
	{
		private static XmlSerializer schemaSerializer = new XmlSerializer(typeof(XMLSchema.schema));
		private Diagram diagram = new Diagram();
		//private Hashtable hashtableNamespaceUrlByPrefix = new Hashtable();
		private Hashtable hashtableElementsByName = new Hashtable();
		private Hashtable hashtableAttributesByName = new Hashtable();
		private ArrayList listOfXsdFilename = new ArrayList();
		private Hashtable hashtableTabPageByFilename = new Hashtable();
		private XSDObject firstElement = null;
		//private string currentXsdfileName = "";
		private List<string> loadError = new List<string>();
		private string originalTitle = "";
		private DiagramBase contextualMenuPointedElement = null;

		public MainForm()
		{
			InitializeComponent();

			this.originalTitle = Text;

			this.toolStripComboBoxSchemaElement.Sorted = true;
			this.toolStripComboBoxSchemaElement.Items.Add("");
			this.hashtableElementsByName[""] = null;
			this.hashtableAttributesByName[""] = null;

			this.diagram.RequestAnyElement += new Diagram.RequestAnyElementEventHandler(diagram_RequestAnyElement);
			this.panelDiagram.DiagramControl.ContextMenuStrip = this.contextMenuStripDiagram;
			this.panelDiagram.DiagramControl.MouseWheel += new MouseEventHandler(DiagramControl_MouseWheel);
			this.panelDiagram.DiagramControl.MouseClick += new MouseEventHandler(DiagramControl_MouseClick);
			this.panelDiagram.VirtualSize = new Size(0, 0);
			this.panelDiagram.DiagramControl.Paint += new PaintEventHandler(DiagramControl_Paint);

			schemaSerializer.UnreferencedObject += new UnreferencedObjectEventHandler(schemaSerializer_UnreferencedObject);
			schemaSerializer.UnknownNode += new XmlNodeEventHandler(schemaSerializer_UnknownNode);
			schemaSerializer.UnknownElement += new XmlElementEventHandler(schemaSerializer_UnknownElement);
			schemaSerializer.UnknownAttribute += new XmlAttributeEventHandler(schemaSerializer_UnknownAttribute);

			//this.panelDiagram.DiagramControl.MouseMove += new MouseEventHandler(DiagramControl_MouseMove);
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			this.toolStripComboBoxZoom.SelectedIndex = 8;
			this.toolStripComboBoxAlignement.SelectedIndex = 1;

			string[] options = Environment.GetCommandLineArgs();
			if (options.Length > 1)
				LoadSchema(options[1]);
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "xsd files (*.xsd)|*.xsd|All files (*.*)|*.*";
			openFileDialog.FilterIndex = 1;
			openFileDialog.RestoreDirectory = true;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
				LoadSchema(openFileDialog.FileName);
		}

		private void MainForm_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				LoadSchema(files[0]);
			}
		}

		private void MainForm_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Move;
			else
				e.Effect = DragDropEffects.None;
		}

		private void saveDiagramToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "emf files (*.emf)|*.emf|All files (*.*)|*.*";
			saveFileDialog.FilterIndex = 1;
			saveFileDialog.RestoreDirectory = true;
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
//					Graphics g1 = this.panelDiagram.DiagramControl.CreateGraphics();
//					IntPtr hdc = g1.GetHdc();
//					Metafile metafile = new Metafile(hdc, EmfType.EmfPlusOnly, "...");
//					g1.ReleaseHdc(hdc);
//	
//					Graphics g2 = Graphics.FromImage(metafile);
//					//g2.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
//					this.diagram.Layout(g2);
//					this.diagram.Paint(g2);
//					
//					g2.Dispose();
//					
//		            int enhMetafileHandle = metafile.GetHenhmetafile().ToInt32();
//		            int bufferSize = GetEnhMetaFileBits(enhMetafileHandle, 0, null); // Get required buffer size.
//		            byte[] buffer = new byte[bufferSize]; // Allocate sufficient buffer
//		            if(GetEnhMetaFileBits(enhMetafileHandle, bufferSize, buffer) <= 0) // Get raw metafile data.
//		                throw new SystemException("DoTheTrick.GetEnhMetaFileBits");
//		            FileStream ms = File.Open("C:\\test.emf", FileMode.Create);
//		            ms.Write(buffer, 0, bufferSize);
//		            ms.Close();
//		            mf.Dispose();
//						
//					
//					g1.Dispose();

					
					
					Graphics g1 = this.panelDiagram.DiagramControl.CreateGraphics();
					IntPtr hdc = g1.GetHdc();
					Metafile metafile = new Metafile(saveFileDialog.FileName, hdc);
	
	
					Graphics g2 = Graphics.FromImage(metafile);
					g2.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
					this.diagram.Layout(g2);
					this.diagram.Paint(g2);
					g1.ReleaseHdc(hdc);
					g2.Dispose();
					g1.Dispose();
				}
				catch(Exception ex)
				{
					System.Diagnostics.Trace.WriteLine(ex.ToString());
				}
			}
		}

		private int currentPage = 0;

		private void printDocument_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
		{
			this.currentPage = 0;
		}

		private void printDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
		{
			this.diagram.Layout(e.Graphics);

			Size bbSize = this.diagram.BoundingBox.Size + this.diagram.Padding + this.diagram.Padding;
			Size totalSize = new Size((int)(bbSize.Width * this.diagram.Scale), (int)(bbSize.Height * this.diagram.Scale));

			int columnNumber = 1 + totalSize.Width / e.MarginBounds.Width;
			int rowNumber = 1 + totalSize.Height / e.MarginBounds.Height;
			int pageNumber = columnNumber * rowNumber;

			int row, column = Math.DivRem(currentPage, rowNumber, out row);

			Rectangle clipping = new Rectangle(new Point(column * e.MarginBounds.Width, row * e.MarginBounds.Height),
				new Size((column + 1) * e.MarginBounds.Width, (row + 1) * e.MarginBounds.Height));

			e.Graphics.Clip = new Region(e.MarginBounds);

			//Point virtualPoint = this.panelDiagram.VirtualPoint;
			e.Graphics.TranslateTransform(-(float)(clipping.Left - e.MarginBounds.Left), -(float)(clipping.Top - e.MarginBounds.Top));

			this.diagram.Paint(e.Graphics, clipping);

			if (this.currentPage < pageNumber - 1)
			{
				this.currentPage++;
				e.HasMorePages = true;
			}
		}

		private void printDocument_EndPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
		{

		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutForm aboutForm = new AboutForm();
			aboutForm.ShowDialog(this);
		}

		private void toolStripComboBoxSchemaElement_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.toolStripComboBoxSchemaElement.SelectedItem != null)
			{
				XSDObject xsdObject = this.toolStripComboBoxSchemaElement.SelectedItem as XSDObject;
				if (xsdObject != null)
					SelectSchemaElement(xsdObject);
			}
		}

		private void toolStripButtonAddToDiagram_Click(object sender, EventArgs e)
		{
			if (this.toolStripComboBoxSchemaElement.SelectedItem != null)
			{
				XSDObject xsdObject = this.toolStripComboBoxSchemaElement.SelectedItem as XSDObject;
				if (xsdObject != null)
					this.diagram.Add(xsdObject.Tag, xsdObject.NameSpace);
				UpdateDiagram();
			}
		}

		private void toolStripButtonAddAllToDiagram_Click(object sender, EventArgs e)
		{
			foreach (XSDObject xsdObject in this.hashtableElementsByName.Values)
				if (xsdObject != null)
					this.diagram.Add(xsdObject.Tag, xsdObject.NameSpace);
			UpdateDiagram();
		}

		void DiagramControl_Paint(object sender, PaintEventArgs e)
		{
			Point virtualPoint = this.panelDiagram.VirtualPoint;
			e.Graphics.TranslateTransform(-(float)virtualPoint.X, -(float)virtualPoint.Y);
			this.diagram.Paint(e.Graphics, new Rectangle(virtualPoint, this.panelDiagram.DiagramControl.ClientRectangle.Size));
		}

		private void UpdateDiagram()
		{
			if (this.diagram.RootElements.Count != 0)
			{
				Graphics g = this.panelDiagram.DiagramControl.CreateGraphics();
				this.diagram.Layout(g);
				g.Dispose();
				Size bbSize = this.diagram.BoundingBox.Size + this.diagram.Padding + this.diagram.Padding;
				this.panelDiagram.VirtualSize = new Size((int)(bbSize.Width * this.diagram.Scale), (int)(bbSize.Height * this.diagram.Scale));
			}
			else
				this.panelDiagram.VirtualSize = new Size(0, 0);
		}

		private void UpdateTitle(string filename)
		{
			if (filename.Length > 0)
				Text = this.originalTitle + " - " + filename;
			else
				Text = this.originalTitle;
		}

		private void LoadSchema(string fileName)
		{
			Cursor = Cursors.WaitCursor;

			//this.currentXsdfileName = fileName;
			UpdateTitle(fileName);

			this.diagram.Clear();
			this.panelDiagram.VirtualSize = new Size(0, 0);
			this.panelDiagram.VirtualPoint = new Point(0, 0);
			this.panelDiagram.Clear();
			this.firstElement = null;
			this.hashtableElementsByName.Clear();
			this.hashtableElementsByName[""] = null;
			this.hashtableAttributesByName.Clear();
			this.hashtableAttributesByName[""] = null;
			this.listOfXsdFilename.Clear();
			this.hashtableTabPageByFilename.Clear();
			this.listViewElements.Items.Clear();
			this.toolStripComboBoxSchemaElement.Items.Clear();
			this.toolStripComboBoxSchemaElement.Items.Add("");
			this.propertyGridSchemaObject.SelectedObject = null;

			while (this.tabControlView.TabCount > 1)
				this.tabControlView.TabPages.RemoveAt(1);

			this.loadError.Clear();

			ImportSchema(fileName);

			Cursor = Cursors.Default;

			if (this.loadError.Count > 0)
			{
				ErrorReportForm errorReportForm = new ErrorReportForm();
				errorReportForm.Errors = this.loadError;
				errorReportForm.ShowDialog(this);
			}

			this.diagram.ElementsByName = this.hashtableElementsByName;
			if (this.firstElement != null)
				this.toolStripComboBoxSchemaElement.SelectedItem = this.firstElement;
			else
				this.toolStripComboBoxSchemaElement.SelectedIndex = 0;

			tabControlView_Selected(null, null);

			this.tabControlView.SuspendLayout();
			foreach (string filename in this.listOfXsdFilename)
			{
				WebBrowser webBrowser = new WebBrowser();
				webBrowser.Dock = DockStyle.Fill;
				webBrowser.TabIndex = 0;

				TabPage tabPage = new TabPage(Path.GetFileNameWithoutExtension(filename));
				tabPage.Tag = filename;
				tabPage.ToolTipText = filename;
				tabPage.Controls.Add(webBrowser);
				tabPage.UseVisualStyleBackColor = true;

				this.tabControlView.TabPages.Add(tabPage);
				this.hashtableTabPageByFilename[filename] = tabPage;

			}
			this.tabControlView.ResumeLayout();
		}

		private void ImportSchema(string fileName)
		{
			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				this.loadError.Clear();
				XMLSchema.schema schemaDOM = (XMLSchema.schema)schemaSerializer.Deserialize(fileStream);
				//string prefix = Path.GetFileNameWithoutExtension(fileName);
				//this.hashtableNamespaceUrlByPrefix[schemaDOM.targetNamespace] = prefix;

				this.listOfXsdFilename.Add(fileName);

				ParseSchema(fileName, schemaDOM);
			}
			catch (IOException ex)
			{
				this.loadError.Add(ex.Message);
			}
			catch (NotSupportedException ex)
			{
				this.loadError.Add(ex.Message + " (" + fileName + ")");
			}
			catch (InvalidOperationException ex)
			{
				this.loadError.Add(ex.Message + "\r\n" + ex.InnerException.Message);
			}
			finally
			{
				if (fileStream != null)
					fileStream.Close();
			}
		}

		private void ParseSchema(string fileName, XMLSchema.schema schemaDOM)
		{
			string basePath = Path.GetDirectoryName(fileName);
			if (schemaDOM.Items != null)
			{
				foreach (XMLSchema.openAttrs openAttrs in schemaDOM.Items)
				{
					string loadedFileName = "";
					string schemaLocation = "";

					if (openAttrs is XMLSchema.include)
					{
						XMLSchema.include include = openAttrs as XMLSchema.include;
						if (include.schemaLocation != null)
							schemaLocation = include.schemaLocation;
					}
					else if (openAttrs is XMLSchema.import)
					{
						XMLSchema.import import = openAttrs as XMLSchema.import;
						if (import.schemaLocation != null)
							schemaLocation = import.schemaLocation;
					}

					if (!string.IsNullOrEmpty(schemaLocation))
					{
						loadedFileName = basePath + Path.DirectorySeparatorChar + schemaLocation.Replace('/', Path.DirectorySeparatorChar);

						string url = schemaLocation.Trim();
						if (url.IndexOf("http://") == 0 || url.IndexOf("https://") == 0)
						{
							Uri uri = new Uri(url);
							if (uri.Segments.Length > 0)
							{
								string fileNameToImport = uri.Segments[uri.Segments.Length - 1];
								loadedFileName = basePath + Path.DirectorySeparatorChar + fileNameToImport;
								if (!File.Exists(loadedFileName))
								{
									WebClient webClient = new WebClient();
									//webClient.Credentials = new System.Net.NetworkCredential("username", "password");
									try
									{
										webClient.DownloadFile(uri, loadedFileName);
									}
									catch (WebException)
									{
										this.loadError.Add("Cannot load the dependency: " + uri.ToString());
										loadedFileName = null;
									}
								}
							}
						}
					}

					if (!string.IsNullOrEmpty(loadedFileName))
						ImportSchema(loadedFileName);
				}
			}

			string nameSpace = schemaDOM.targetNamespace;

			if (schemaDOM.Items1 != null)
			{
				foreach (XMLSchema.openAttrs openAttrs in schemaDOM.Items1)
				{
					if (openAttrs is XMLSchema.element)
					{
						XMLSchema.element element = openAttrs as XMLSchema.element;
						XSDObject xsdObject = new XSDObject(fileName, element.name, nameSpace, "element", element);
						this.hashtableElementsByName[xsdObject.FullName] = xsdObject;

						if (this.firstElement == null)
							this.firstElement = xsdObject;

						this.listViewElements.Items.Add(new ListViewItem(new string[] { xsdObject.Name, xsdObject.Type, xsdObject.NameSpace })).Tag = xsdObject;
						this.toolStripComboBoxSchemaElement.Items.Add(xsdObject);
					}
					else if (openAttrs is XMLSchema.group)
					{
						XMLSchema.group group = openAttrs as XMLSchema.group;
						XSDObject xsdObject = new XSDObject(fileName, group.name, nameSpace, "group", group);
						this.hashtableElementsByName[xsdObject.FullName] = xsdObject;

						this.listViewElements.Items.Add(new ListViewItem(new string[] { xsdObject.Name, xsdObject.Type, xsdObject.NameSpace })).Tag = xsdObject;
						this.toolStripComboBoxSchemaElement.Items.Add(xsdObject);
					}
					else if (openAttrs is XMLSchema.simpleType)
					{
						XMLSchema.simpleType simpleType = openAttrs as XMLSchema.simpleType;
						XSDObject xsdObject = new XSDObject(fileName, simpleType.name, nameSpace, "simpleType", simpleType);
						this.hashtableElementsByName[xsdObject.FullName] = xsdObject;

						this.listViewElements.Items.Add(new ListViewItem(new string[] { xsdObject.Name, xsdObject.Type, xsdObject.NameSpace })).Tag = xsdObject;
						this.toolStripComboBoxSchemaElement.Items.Add(xsdObject);
					}
					else if (openAttrs is XMLSchema.complexType)
					{
						XMLSchema.complexType complexType = openAttrs as XMLSchema.complexType;
						XSDObject xsdObject = new XSDObject(fileName, complexType.name, nameSpace, "complexType", complexType);
						this.hashtableElementsByName[xsdObject.FullName] = xsdObject;

						this.listViewElements.Items.Add(new ListViewItem(new string[] { xsdObject.Name, xsdObject.Type, xsdObject.NameSpace })).Tag = xsdObject;
						this.toolStripComboBoxSchemaElement.Items.Add(xsdObject);
					}
					else if (openAttrs is XMLSchema.attribute)
					{
						XMLSchema.attribute attribute = openAttrs as XMLSchema.attribute;
						XSDAttribute xsdAttribute = new XSDAttribute(fileName, attribute.name, nameSpace, "attribute", attribute.@ref != null, attribute.@default, attribute.use.ToString(), attribute);
						this.hashtableAttributesByName[xsdAttribute.FullName] = xsdAttribute;
					}
					else if (openAttrs is XMLSchema.attributeGroup)
					{
						XMLSchema.attributeGroup attributeGroup = openAttrs as XMLSchema.attributeGroup;
						XSDAttributeGroup xsdAttributeGroup = new XSDAttributeGroup(fileName, attributeGroup.name, nameSpace, "attributeGroup", attributeGroup is XMLSchema.attributeGroupRef, attributeGroup);
						this.hashtableAttributesByName[xsdAttributeGroup.FullName] = xsdAttributeGroup;
					}
				}
			}
		}

		void schemaSerializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			this.loadError.Add("Unkonwn attribute (" + e.LineNumber + ", " + e.LinePosition + "): " + e.Attr.Name);
		}

		void schemaSerializer_UnknownElement(object sender, XmlElementEventArgs e)
		{
			this.loadError.Add("Unkonwn element (" + e.LineNumber + ", " + e.LinePosition + "): " + e.Element.Name);
		}

		void schemaSerializer_UnknownNode(object sender, XmlNodeEventArgs e)
		{
			this.loadError.Add("Unkonwn node (" + e.LineNumber + ", " + e.LinePosition + "): " + e.Name);
		}

		void schemaSerializer_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
		{
			this.loadError.Add("Unreferenced object: " + e.UnreferencedId);
		}

		void DiagramControl_MouseClick(object sender, MouseEventArgs e)
		{
			Point location = e.Location;
			location.Offset(this.panelDiagram.VirtualPoint);

			DiagramBase resultElement;
			DiagramBase.HitTestRegion resultRegion;
			this.diagram.HitTest(location, out resultElement, out resultRegion);
			if (resultRegion != DiagramBase.HitTestRegion.None)
			{
				if (resultRegion == DiagramBase.HitTestRegion.ChildExpandButton)
				{
					if (resultElement.HasChildElements)
					{
						if (resultElement.ChildElements.Count == 0)
						{
							this.diagram.ExpandChildren(resultElement);
							resultElement.ShowChildElements = true;
						}
						else
							resultElement.ShowChildElements ^= true;

						UpdateDiagram();
						this.panelDiagram.ScrollTo(this.diagram.ScalePoint(resultElement.Location), true);
					}
				}
				else if (resultRegion == DiagramBase.HitTestRegion.Element)
				{
					if ((ModifierKeys & (Keys.Control | Keys.Shift)) == (Keys.Control | Keys.Shift)) // For Debug
					{
						this.toolStripComboBoxSchemaElement.SelectedItem = "";
						this.propertyGridSchemaObject.SelectedObject = resultElement;
					}
					else
						SelectDiagramElement(resultElement);
				}
				else
					SelectDiagramElement(null);
			}
		}

		private void SelectDiagramElement(DiagramBase element)
		{
			this.textBoxElementPath.Text = "";
			if (element == null)
			{
				this.toolStripComboBoxSchemaElement.SelectedItem = "";
				this.propertyGridSchemaObject.SelectedObject = null;
				this.listViewAttributes.Items.Clear();
				return;
			}

			if (element is DiagramBase)
			{
				if (this.hashtableElementsByName[element.FullName] != null)
					this.toolStripComboBoxSchemaElement.SelectedItem = this.hashtableElementsByName[element.FullName];
				else
					this.toolStripComboBoxSchemaElement.SelectedItem = null;

				SelectSchemaElement(element);

				string path = '/' + element.Name;
				DiagramBase parentElement = element.Parent;
				while (parentElement != null)
				{
					if (parentElement.Type == DiagramBase.TypeEnum.element && !string.IsNullOrEmpty(parentElement.Name))
						path = '/' + parentElement.Name + path;
					parentElement = parentElement.Parent;
				}
				this.textBoxElementPath.Text = path;
			}
			else
			{
				this.toolStripComboBoxSchemaElement.SelectedItem = "";
				SelectSchemaElement(element);
			}
		}

		private void SelectSchemaElement(XSDObject xsdObject)
		{
			SelectSchemaElement(xsdObject.Tag, xsdObject.NameSpace);
		}

		private void SelectSchemaElement(DiagramBase diagramBase)
		{
			SelectSchemaElement(diagramBase.TabSchema, diagramBase.NameSpace);
		}

		private void SelectSchemaElement(XMLSchema.openAttrs openAttrs, string nameSpace)
		{
			this.propertyGridSchemaObject.SelectedObject = openAttrs;
			ShowDocumentation(null);

			XMLSchema.annotated annotated = openAttrs as XMLSchema.annotated;
			if (annotated != null)
			{
				// Element documentation
				if (annotated.annotation != null)
					ShowDocumentation(annotated.annotation);

				// Show the enumeration
				ShowEnumerate(annotated);

				// Attributes enumeration
				List<XSDAttribute> listAttributes = new List<XSDAttribute>();
				if (annotated is XMLSchema.element)
				{
					XMLSchema.element element = annotated as XMLSchema.element;

					if (element.Item is XMLSchema.complexType)
					{
						XMLSchema.complexType complexType = element.Item as XMLSchema.complexType;
						listAttributes.AddRange(ShowAttributes(complexType, nameSpace));
					}
					else if (element.type != null)
					{
						XSDObject xsdObject = this.hashtableElementsByName[QualifiedNameToFullName("type", element.type)] as XSDObject;
						if(xsdObject != null)
						{
							XMLSchema.annotated annotatedElement = xsdObject.Tag as XMLSchema.annotated;
							if (annotatedElement is XMLSchema.complexType)
							{
								XMLSchema.complexType complexType = annotatedElement as XMLSchema.complexType;
								listAttributes.AddRange(ShowAttributes(complexType, nameSpace));
							}
							else
							{
							}
						}
						else
						{
						}
					}
					else
					{
					}
				}
				else if (annotated is XMLSchema.complexType)
				{
					XMLSchema.complexType complexType = annotated as XMLSchema.complexType;
					listAttributes.AddRange(ShowAttributes(complexType, nameSpace));
				}
				else
				{
				}

				this.listViewAttributes.Items.Clear();
				foreach (XSDAttribute attribute in listAttributes)
					this.listViewAttributes.Items.Add(new ListViewItem(new string[]
							{ attribute.Name, attribute.Type, attribute.Use, attribute.DefaultValue })).Tag = attribute;
			}
		}

		private List<XSDAttribute> ShowAttributes(XMLSchema.complexType complexType, string nameSpace)
		{
			List<XSDAttribute> listAttributes = new List<XSDAttribute>();
			ParseComplexTypeAttributes(nameSpace, listAttributes, complexType, false);
			return listAttributes;
		}

		private void ParseComplexTypeAttributes(string nameSpace, List<XSDAttribute> listAttributes, XMLSchema.complexType complexType, bool isRestriction)
		{
			if (complexType.ItemsElementName != null)
			{
				for (int i = 0; i < complexType.ItemsElementName.Length; i++)
				{
					switch (complexType.ItemsElementName[i])
					{
						case XMLSchema.ItemsChoiceType4.attribute:
							{
								XMLSchema.attribute attribute = complexType.Items[i] as XMLSchema.attribute;
								ParseAttribute(nameSpace, listAttributes, attribute, false);
							}
							break;
						case XMLSchema.ItemsChoiceType4.attributeGroup:
							{
								XMLSchema.attributeGroup attributeGroup = complexType.Items[i] as XMLSchema.attributeGroup;
								ParseAttributeGroup(nameSpace, listAttributes, attributeGroup, false);
							}
							break;
						case XMLSchema.ItemsChoiceType4.anyAttribute:
							XMLSchema.wildcard wildcard = complexType.Items[i] as XMLSchema.wildcard;
							XSDAttribute xsdAttribute = new XSDAttribute("", "*", wildcard.@namespace, "", false, null, null, null);
							listAttributes.Add(xsdAttribute);
							break;
						case XMLSchema.ItemsChoiceType4.simpleContent:
						case XMLSchema.ItemsChoiceType4.complexContent:
							XMLSchema.annotated annotatedContent = null;
							if(complexType.Items[i] is XMLSchema.complexContent)
							{
								XMLSchema.complexContent complexContent = complexType.Items[i] as XMLSchema.complexContent;
								annotatedContent = complexContent.Item;
							}
							else if(complexType.Items[i] is XMLSchema.simpleContent)
							{
								XMLSchema.simpleContent simpleContent = complexType.Items[i] as XMLSchema.simpleContent;
								annotatedContent = simpleContent.Item;
							}
							if (annotatedContent is XMLSchema.extensionType)
							{
								XMLSchema.extensionType extensionType = annotatedContent as XMLSchema.extensionType;
								XSDObject xsdExtensionType = this.hashtableElementsByName[QualifiedNameToFullName("type", extensionType.@base)] as XSDObject;
								if (xsdExtensionType != null)
								{
									XMLSchema.annotated annotatedExtension = xsdExtensionType.Tag as XMLSchema.annotated;
									if (annotatedExtension != null)
									{
										if (annotatedExtension is XMLSchema.complexType)
											ParseComplexTypeAttributes(extensionType.@base.Namespace, listAttributes, annotatedExtension as XMLSchema.complexType, false);
									}
								}
								if (extensionType.Items != null)
								{
									foreach (XMLSchema.annotated annotated in extensionType.Items)
									{
										if (annotated is XMLSchema.attribute)
										{
											ParseAttribute(nameSpace, listAttributes, annotated as XMLSchema.attribute, false);
										}
										else if (annotated is XMLSchema.attributeGroup)
										{
											ParseAttributeGroup(nameSpace, listAttributes, annotated as XMLSchema.attributeGroup, false);
										}
									}
								}
							}
							else if (annotatedContent is XMLSchema.restrictionType)
							{
								XMLSchema.restrictionType restrictionType = annotatedContent as XMLSchema.restrictionType;
								XSDObject xsdRestrictionType = this.hashtableElementsByName[QualifiedNameToFullName("type", restrictionType.@base)] as XSDObject;
								if (xsdRestrictionType != null)
								{
									XMLSchema.annotated annotatedRestriction = xsdRestrictionType.Tag as XMLSchema.annotated;
									if (annotatedRestriction != null)
									{
										if (annotatedRestriction is XMLSchema.complexType)
											ParseComplexTypeAttributes(restrictionType.@base.Namespace, listAttributes, annotatedRestriction as XMLSchema.complexType, false);
									}
								}
								if (restrictionType.Items1 != null)
								{
									foreach (XMLSchema.annotated annotated in restrictionType.Items1)
									{
										if (annotated is XMLSchema.attribute)
										{
											ParseAttribute(nameSpace, listAttributes, annotated as XMLSchema.attribute, true);
										}
										else if (annotated is XMLSchema.attributeGroup)
										{
											ParseAttributeGroup(nameSpace, listAttributes, annotated as XMLSchema.attributeGroup, true);
										}
									}
								}
							}
							break;
					}
				}
			}
			else
			{
			}
		}

		private void ParseAttribute(string nameSpace, List<XSDAttribute> listAttributes, XMLSchema.attribute attribute, bool isRestriction)
		{
			bool isReference = false;
			string filename = "";
			string name = attribute.name;
			string type = "";
			if (attribute.@ref != null)
			{
				object o = this.hashtableAttributesByName[QualifiedNameToFullName("attribute", attribute.@ref)];
				if (o is XSDAttribute)
				{
					XSDAttribute xsdAttributeInstance = o as XSDAttribute;
					ParseAttribute(nameSpace, listAttributes, xsdAttributeInstance.Tag, isRestriction);
					return;
				}
				else // Reference not found!
				{
					type = QualifiedNameToAttributeTypeName(attribute.@ref);
					name = attribute.@ref.Name;
					nameSpace = attribute.@ref.Namespace;
					isReference = true;
				}
			}
			else if (attribute.type != null)
			{
				type = QualifiedNameToAttributeTypeName(attribute.type);
				nameSpace = attribute.type.Namespace;
			}
			else if (attribute.simpleType != null)
			{
				XMLSchema.simpleType simpleType = attribute.simpleType as XMLSchema.simpleType;
				if (simpleType.Item is XMLSchema.restriction)
				{
					XMLSchema.restriction restriction = simpleType.Item as XMLSchema.restriction;
					type = QualifiedNameToAttributeTypeName(restriction.@base);
					nameSpace = restriction.@base.Namespace;
				}
				else if (simpleType.Item is XMLSchema.list)
				{
					XMLSchema.list list = simpleType.Item as XMLSchema.list;
					type = QualifiedNameToAttributeTypeName(list.itemType);
					nameSpace = list.itemType.Namespace;
				}
				else
				{
				}
			}
			else
			{

			}
			if (string.IsNullOrEmpty(attribute.name) && string.IsNullOrEmpty(name))
			{
			}
			if (isRestriction)
			{
				if (attribute.use == XMLSchema.attributeUse.prohibited)
				{
					foreach (XSDAttribute xsdAttribute in listAttributes)
					{
						if (xsdAttribute.Name == name)
						{
							//listAttributes.Remove(xsdAttribute);
							xsdAttribute.Use = attribute.use.ToString();
							break;
						}
					}
				}
			}
			else
			{
				XSDAttribute xsdAttribute = new XSDAttribute(filename, name, nameSpace, type, isReference, attribute.@default, attribute.use.ToString(), attribute);
				listAttributes.Insert(0, xsdAttribute);
			}
		}

		private void ParseAttributeGroup(string nameSpace, List<XSDAttribute> listAttributes, XMLSchema.attributeGroup attributeGroup, bool isRestriction)
		{
			if (attributeGroup is XMLSchema.attributeGroupRef && attributeGroup.@ref != null)
			{
				object o = this.hashtableAttributesByName[QualifiedNameToFullName("attributeGroup", attributeGroup.@ref)];
				if (o is XSDAttributeGroup)
				{
					XSDAttributeGroup xsdAttributeGroup = o as XSDAttributeGroup;
					XMLSchema.attributeGroup attributeGroupInstance = xsdAttributeGroup.Tag;

					foreach (XMLSchema.annotated annotated in attributeGroupInstance.Items)
					{
						if (annotated is XMLSchema.attribute)
						{
							ParseAttribute(nameSpace, listAttributes, annotated as XMLSchema.attribute, isRestriction);
						}
						else if (annotated is XMLSchema.attributeGroup)
						{
							ParseAttributeGroup(nameSpace, listAttributes, annotated as XMLSchema.attributeGroup, isRestriction);
						}
					}
				}
			}
			else
			{

			}
		}

		private static string QualifiedNameToFullName(string type, System.Xml.XmlQualifiedName xmlQualifiedName)
		{
			return xmlQualifiedName.Namespace + ':' + type + ':' + xmlQualifiedName.Name;
		}

		private static string QualifiedNameToAttributeTypeName(System.Xml.XmlQualifiedName xmlQualifiedName)
		{
			return xmlQualifiedName.Name + "  : " + xmlQualifiedName.Namespace;
		}

		private void ShowEnumerate(XMLSchema.attribute attribute)
		{
			this.listViewEnumerate.Items.Clear();
			if (attribute != null)
			{
				if (attribute.type != null)
				{
					XSDObject xsdObject = this.hashtableElementsByName[QualifiedNameToFullName("type", attribute.type)] as XSDObject;
					if (xsdObject != null)
					{
						XMLSchema.annotated annotatedElement = xsdObject.Tag as XMLSchema.annotated;
						if (annotatedElement is XMLSchema.simpleType)
							ShowEnumerate(annotatedElement as XMLSchema.simpleType);
					}
				}
				else if (attribute.simpleType != null)
				{
					ShowEnumerate(attribute.simpleType);
				}
			}
		}

		private void ShowEnumerate(XMLSchema.annotated annotated)
		{
			this.listViewEnumerate.Items.Clear();

			if (annotated != null)
			{
				XMLSchema.element element = annotated as XMLSchema.element;
				if (element != null && element.type != null)
				{
					XSDObject xsdObject = this.hashtableElementsByName[QualifiedNameToFullName("type", element.type)] as XSDObject;
					if (xsdObject != null)
					{
						XMLSchema.annotated annotatedElement = xsdObject.Tag as XMLSchema.annotated;
						if (annotatedElement is XMLSchema.simpleType)
							ShowEnumerate(annotatedElement as XMLSchema.simpleType);
					}
				}
			}
		}

		private void ShowEnumerate(XMLSchema.simpleType simpleType)
		{
			if (simpleType != null)
			{
				if (simpleType.Item != null)
				{
					XMLSchema.restriction restriction = simpleType.Item as XMLSchema.restriction;
					if (restriction != null && restriction.ItemsElementName != null)
					{
						for (int i = 0; i < restriction.ItemsElementName.Length; i++)
						{
							if (restriction.ItemsElementName[i] == XMLSchema.ItemsChoiceType.enumeration)
							{
								XMLSchema.facet facet = restriction.Items[i] as XMLSchema.facet;
								if (facet != null)
									this.listViewEnumerate.Items.Add(facet.value);
							}
						}

						if (this.listViewEnumerate.Items.Count != 0)
							this.listViewEnumerate.Columns[0].Width = -1;
					}
				}
			}
		}


		private void ShowDocumentation(XMLSchema.annotation annotation)
		{
			if (annotation == null)
			{
				this.textBoxAnnotation.Text = "";
				this.textBoxAnnotation.Visible = true;
				this.webBrowserDocumentation.Visible = false;

				return;
			}

			foreach (object o in annotation.Items)
			{
				if (o is XMLSchema.documentation)
				{
					XMLSchema.documentation documentation = o as XMLSchema.documentation;
					if (documentation.Any != null && documentation.Any.Length > 0)
					{
						string text = documentation.Any[0].Value;
						text = text.Replace("\n", " ");
						text = text.Replace("\t", " ");
						text = text.Replace("\r", "");
						text = Regex.Replace(text, " +", " ");
						text = text.Trim();

						//text = text.Replace(, " ");

						//text = text.Trim('\n', '\t', '\r', ' ');
						//string[] textLines = text.Split(new char[] { '\n' });
						//for (int i = 0; i < textLines.Length; i++)
						//    textLines[i] = textLines[i].Trim('\n', '\t', '\r', ' ');
						//text = string.Join("\r\n", textLines);
						this.textBoxAnnotation.Text = text;
						this.textBoxAnnotation.Visible = true;
						this.webBrowserDocumentation.Visible = false;
					}
					else if (documentation.source != null)
					{
						this.textBoxAnnotation.Visible = false;
						this.webBrowserDocumentation.Visible = true;
						this.webBrowserDocumentation.Navigate(documentation.source);
					}
					break;
				}
			}
		}

		private void listViewAttributes_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.listViewAttributes.SelectedItems.Count > 0)
			{
				XSDAttribute xsdAttribute = this.listViewAttributes.SelectedItems[0].Tag as XSDAttribute;
				XMLSchema.attribute attribute = xsdAttribute.Tag;
				if (attribute != null && attribute.annotation != null)
					ShowDocumentation(attribute.annotation);
				else
					ShowDocumentation(null);
				ShowEnumerate(attribute);
			}
		}

		private void toolStripComboBoxZoom_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				string zoomString = this.toolStripComboBoxZoom.SelectedItem as string;
				zoomString = zoomString.Replace("%", "");

				float zoom = (float)int.Parse(zoomString) / 100.0f;
				if (zoom >= 0.10 && zoom <= 10)
				{
					//Point virtualCenter = this.panelDiagram.VirtualPoint;
					//virtualCenter.Offset(this.panelDiagram.DiagramControl.Width / 2, this.panelDiagram.DiagramControl.Height / 2);
					//Size oldSize = this.panelDiagram.VirtualSize;
					//this.diagram.Scale = zoom;
					//UpdateDiagram();
					//Size newSize = this.panelDiagram.VirtualSize;
					//virtualCenter.X = (int)((float)newSize.Width / (float)oldSize.Width * (float)virtualCenter.X);
					//virtualCenter.Y = (int)((float)newSize.Height / (float)oldSize.Height * (float)virtualCenter.Y);
					//if (virtualCenter.X > this.diagram.BoundingBox.Right)
					//    virtualCenter.X = this.diagram.BoundingBox.Right;
					//if (virtualCenter.Y > this.diagram.BoundingBox.Bottom)
					//    virtualCenter.Y = this.diagram.BoundingBox.Bottom;
					//this.panelDiagram.ScrollTo(virtualCenter, true);

					Point virtualCenter = this.panelDiagram.VirtualPoint;
					virtualCenter.Offset(this.panelDiagram.DiagramControl.Width / 2, this.panelDiagram.DiagramControl.Height / 2);
					Size oldSize = this.panelDiagram.VirtualSize;
					this.diagram.Scale = zoom;
					UpdateDiagram();
					Size newSize = this.panelDiagram.VirtualSize;
					Point newVirtualCenter = new Point();
					newVirtualCenter.X = (int)((float)newSize.Width / (float)oldSize.Width * (float)virtualCenter.X);
					newVirtualCenter.Y = (int)((float)newSize.Height / (float)oldSize.Height * (float)virtualCenter.Y);
					if (newVirtualCenter.X > this.diagram.BoundingBox.Right)
						newVirtualCenter.X = this.diagram.BoundingBox.Right;
					if (newVirtualCenter.Y > this.diagram.BoundingBox.Bottom)
						newVirtualCenter.Y = this.diagram.BoundingBox.Bottom;
					this.panelDiagram.ScrollTo(newVirtualCenter, true);
				}
			}
			catch { }
		}

		private void toolStripComboBoxZoom_TextChanged(object sender, EventArgs e)
		{
			//try
			//{
			//    string zoomString = this.toolStripComboBoxZoom.SelectedItem as string;
			//    zoomString = zoomString.Replace("%", "");

			//    float zoom = (float)int.Parse(zoomString) / 100.0f;
			//    if (zoom >= 0.10 && zoom <= 10)
			//    {
			//        this.diagram.Scale = zoom;
			//        UpdateDiagram();
			//    }
			//}
			//catch { }
		}

		void DiagramControl_MouseWheel(object sender, MouseEventArgs e)
		{
			if ((ModifierKeys & Keys.Control) == Keys.Control)
			{
				if (e.Delta > 0)
				{
					if (this.toolStripComboBoxZoom.SelectedIndex > 0)
						this.toolStripComboBoxZoom.SelectedIndex--;
				}
				else
				{
					if (this.toolStripComboBoxZoom.SelectedIndex < this.toolStripComboBoxZoom.Items.Count - 1)
						this.toolStripComboBoxZoom.SelectedIndex++;
				}
			}
		}

		private void pageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				this.pageSetupDialog.ShowDialog(this);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				this.printPreviewDialog.ShowDialog(this);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void printToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				this.printDialog.ShowDialog(this);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void toolStripButtonTogglePanel_Click(object sender, EventArgs e)
		{
			this.splitContainerMain.Panel2Collapsed = !this.toolStripButtonTogglePanel.Checked;
		}

		private void contextMenuStripDiagram_Opened(object sender, EventArgs e)
		{
			this.gotoXSDFileToolStripMenuItem.Enabled = false;
			this.removeFromDiagramToolStripMenuItem.Enabled = false;

			Point contextualMenuMousePosition = this.panelDiagram.DiagramControl.PointToClient(MousePosition);
			contextualMenuMousePosition.Offset(this.panelDiagram.VirtualPoint);
			DiagramBase resultElement;
			DiagramBase.HitTestRegion resultRegion;
			this.diagram.HitTest(contextualMenuMousePosition, out resultElement, out resultRegion);
			if (resultRegion != DiagramBase.HitTestRegion.None)
			{
				if (resultRegion == DiagramBase.HitTestRegion.Element) // && resultElement.Parent == null)
				{
					this.contextualMenuPointedElement = resultElement;
					this.gotoXSDFileToolStripMenuItem.Enabled = true;
					this.removeFromDiagramToolStripMenuItem.Enabled = true;
				}
			}
		}

		private void gotoXSDFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.contextualMenuPointedElement != null)
			{
				XSDObject xsdObject = this.hashtableElementsByName[this.contextualMenuPointedElement.FullName] as XSDObject;
				if (xsdObject != null)
				{
					TabPage tabPage = this.hashtableTabPageByFilename[xsdObject.Filename] as TabPage;
					if (tabPage != null)
						this.tabControlView.SelectedTab = tabPage;
				}
			}
			this.contextualMenuPointedElement = null;
		}

		private void removeFromDiagramToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.contextualMenuPointedElement != null)
			{
				DiagramBase parentDiagram = this.contextualMenuPointedElement.Parent;
				this.diagram.Remove(this.contextualMenuPointedElement);
				UpdateDiagram();
				if (parentDiagram != null)
					this.panelDiagram.ScrollTo(this.diagram.ScalePoint(parentDiagram.Location), true);
				else
					this.panelDiagram.ScrollTo(new Point(0, 0));
			}
			this.contextualMenuPointedElement = null;
		}

		private void tabControlView_Selected(object sender, TabControlEventArgs e)
		{
			if (tabControlView.SelectedTab.Tag != null)
			{
				WebBrowser webBrowser = tabControlView.SelectedTab.Controls[0] as WebBrowser;
				if (webBrowser != null)
				{
					if(webBrowser.Url == null || webBrowser.Url != new Uri(tabControlView.SelectedTab.Tag as string))
						webBrowser.Navigate(tabControlView.SelectedTab.Tag as string);
					webBrowser.Select();
				}
			}
		}

		private void tabControlView_Click(object sender, EventArgs e)
		{
			if (tabControlView.SelectedTab.Tag != null)
			{
				WebBrowser webBrowser = tabControlView.SelectedTab.Controls[0] as WebBrowser;
				if (webBrowser != null)
					webBrowser.Select();
			}
		}

		private void tabControlView_Enter(object sender, EventArgs e)
		{
			if (tabControlView.SelectedTab.Tag != null)
			{
				WebBrowser webBrowser = tabControlView.SelectedTab.Controls[0] as WebBrowser;
				if (webBrowser != null)
					webBrowser.Focus();
			}
			else
				this.panelDiagram.Focus();
		}

		private void registerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileShellExtension.Register(Microsoft.Win32.Registry.GetValue("HKEY_CLASSES_ROOT\\.xsd", null, "xsdfile") as string, "XSDDiagram", "XSD Diagram", string.Format("\"{0}\" \"%L\"", Application.ExecutablePath));
		}

		private void unregisterToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileShellExtension.Unregister(Microsoft.Win32.Registry.GetValue("HKEY_CLASSES_ROOT\\.xsd", null, "xsdfile") as string, "XSDDiagram");
		}

		private void toolStripButtonShowReferenceBoundingBox_Click(object sender, EventArgs e)
		{
			this.diagram.ShowBoundingBox = this.toolStripButtonShowReferenceBoundingBox.Checked;
			UpdateDiagram();
		}

		private void toolStripComboBoxAlignement_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (this.toolStripComboBoxAlignement.SelectedItem as string)
			{
				case "Top": this.diagram.Alignement = DiagramBase.Alignement.Near; break;
				case "Center": this.diagram.Alignement = DiagramBase.Alignement.Center; break;
				case "Bottom": this.diagram.Alignement = DiagramBase.Alignement.Far; break;
			}
		    UpdateDiagram();
		}

		void diagram_RequestAnyElement(DiagramBase diagramElement, out XMLSchema.element element, out string nameSpace)
		{
			element = null;
			nameSpace = "";

			//ElementsForm elementsForm = new ElementsForm();
			//elementsForm.Location = MousePosition; //diagramElement.Location //MousePosition;
			//elementsForm.ListBoxElements.Items.Clear();
			//elementsForm.ListBoxElements.Items.Insert(0, "(Cancel)");
			//foreach (XSDObject xsdObject in this.hashtableElementsByName.Values)
			//    if (xsdObject != null && xsdObject.Type == "element")
			//        elementsForm.ListBoxElements.Items.Add(xsdObject);
			//if (elementsForm.ShowDialog(this.diagramControl) == DialogResult.OK && (elementsForm.ListBoxElements.SelectedItem as XSDObject) != null)
			//{
			//    XSDObject xsdObject = elementsForm.ListBoxElements.SelectedItem as XSDObject;
			//    element = xsdObject.Tag as XMLSchema.element;
			//    nameSpace = xsdObject.NameSpace;
			//}
		}

		private void listViewElement_Click(object sender, EventArgs e)
		{
			if (this.listViewElements.SelectedItems.Count > 0)
				SelectSchemaElement(this.listViewElements.SelectedItems[0].Tag as XSDObject);
		}

		private void listViewElement_DoubleClick(object sender, EventArgs e)
		{
			if (this.listViewElements.SelectedItems.Count > 0)
			{
				foreach (ListViewItem lvi in this.listViewElements.SelectedItems)
				{
					XSDObject xsdObject = lvi.Tag as XSDObject;
					switch (xsdObject.Type)
					{
						case "element":
							this.diagram.AddElement(xsdObject.Tag as XMLSchema.element, xsdObject.NameSpace);
							break;
						case "group":
							this.diagram.AddCompositors(xsdObject.Tag as XMLSchema.group, xsdObject.NameSpace);
							break;
						case "complexType":
							this.diagram.AddComplexType(xsdObject.Tag as XMLSchema.complexType, xsdObject.NameSpace);
							break;
					}
				}
				UpdateDiagram();
			}
		}

		private void expandOneLevelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.diagram.ExpandOneLevel();
			UpdateDiagram();
		}

		// Implements the manual sorting of items by columns.
		class ListViewItemComparer : IComparer
		{
			private int column;
			private ListView listView;
			public ListViewItemComparer(int column, ListView listView)
			{
				this.column = column;
				this.listView = listView;

				switch (this.listView.Sorting)
				{
					case SortOrder.None: this.listView.Sorting = SortOrder.Ascending; break;
					case SortOrder.Ascending: this.listView.Sorting = SortOrder.Descending; break;
					case SortOrder.Descending: this.listView.Sorting = SortOrder.Ascending; break;
				}
			}
			public int Compare(object x, object y)
			{
				int result = 0;
				if (this.listView.Sorting == SortOrder.Ascending)
					result = String.Compare(((ListViewItem)x).SubItems[this.column].Text, ((ListViewItem)y).SubItems[column].Text);
				if (this.listView.Sorting == SortOrder.Descending)
					result = -String.Compare(((ListViewItem)x).SubItems[this.column].Text, ((ListViewItem)y).SubItems[column].Text);

				return result;
			}
		}

		private void listViewElement_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			this.listViewElements.ListViewItemSorter = new ListViewItemComparer(e.Column, this.listViewElements);
		}

		private void listViewAttributes_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			this.listViewAttributes.ListViewItemSorter = new ListViewItemComparer(e.Column, this.listViewAttributes);
		}

		private void toolStripButtonRemoveAllFromDiagram_Click(object sender, EventArgs e)
		{
			this.diagram.RemoveAll();
			UpdateDiagram();
			this.panelDiagram.VirtualPoint = new Point(0, 0);
			this.panelDiagram.Clear();
		}

		private void listView_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			e.CancelEdit = true;
		}

		private void nextTabToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int index = this.tabControlView.SelectedIndex;
			++index;
			this.tabControlView.SelectedIndex = index % this.tabControlView.TabCount;
		}

		private void previousTabToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int index = this.tabControlView.SelectedIndex;
			--index;
			if (index < 0) index = this.tabControlView.TabCount - 1;
			this.tabControlView.SelectedIndex = index;
		}

		private void ListViewToString(ListView listView, bool selectedLineOnly)
		{
			string result = "";
			if (selectedLineOnly)
			{
				if (listView.SelectedItems.Count > 0)
				{
					foreach (ColumnHeader columnHeader in listView.Columns)
					{
						if (columnHeader.Index > 0) result += "\t";
						result += listView.SelectedItems[0].SubItems[columnHeader.Index].Text;
					}
				}
			}
			else
			{
				foreach (ListViewItem lvi in listView.Items)
				{
					foreach (ColumnHeader columnHeader in listView.Columns)
					{
						if (columnHeader.Index > 0) result += "\t";
						result += lvi.SubItems[columnHeader.Index].Text;
					}
					result += "\r\n";
				}
			}
			if (result.Length > 0)
				Clipboard.SetText(result);
		}

		private void toolStripMenuItemAttributesCopyLine_Click(object sender, EventArgs e)
		{
			ListViewToString(this.listViewAttributes, true);
		}

		private void toolStripMenuItemAttributesCopyList_Click(object sender, EventArgs e)
		{
			ListViewToString(this.listViewAttributes, false);
		}

		private void contextMenuStripAttributes_Opening(object sender, CancelEventArgs e)
		{
			this.toolStripMenuItemAttributesCopyLine.Enabled = (this.listViewAttributes.SelectedItems.Count == 1);
			this.toolStripMenuItemAttributesCopyList.Enabled = (this.listViewAttributes.Items.Count > 0);
		}

		private void toolStripMenuItemEnumerateCopyLine_Click(object sender, EventArgs e)
		{
			ListViewToString(this.listViewEnumerate, true);
		}

		private void toolStripMenuItemEnumerateCopyList_Click(object sender, EventArgs e)
		{
			ListViewToString(this.listViewEnumerate, false);
		}

		private void contextMenuStripEnumerate_Opening(object sender, CancelEventArgs e)
		{
			this.toolStripMenuItemEnumerateCopyLine.Enabled = (this.listViewEnumerate.SelectedItems.Count == 1);
			this.toolStripMenuItemEnumerateCopyList.Enabled = (this.listViewEnumerate.Items.Count > 0);
		}

		private void toolStripMenuItemElementsCopyLine_Click(object sender, EventArgs e)
		{
			ListViewToString(this.listViewElements, true);
		}

		private void toolStripMenuItemElementsCopyList_Click(object sender, EventArgs e)
		{
			ListViewToString(this.listViewElements, false);
		}

		private void contextMenuStripElements_Opening(object sender, CancelEventArgs e)
		{
			this.toolStripMenuItemElementsCopyLine.Enabled = (this.listViewElements.SelectedItems.Count == 1);
			this.toolStripMenuItemElementsCopyList.Enabled = (this.listViewElements.Items.Count > 0);
		}

		private void listViewElements_ItemDrag(object sender, ItemDragEventArgs e)
		{
			listViewElements.DoDragDrop(e.Item, DragDropEffects.Copy);
		}

		private void panelDiagram_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(ListViewItem)))
			{
				ListViewItem lvi = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
				if (lvi != null)
				{
					XSDObject xsdObject = lvi.Tag as XSDObject;
					switch (xsdObject.Type)
					{
						case "element":
						case "group":
						case "complexType":
							e.Effect = DragDropEffects.Copy;
							break;
					}
				}
			}
			else if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Move;
		}

		private void panelDiagram_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(ListViewItem)))
			{
				ListViewItem lvi = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
				if (lvi != null)
				{
					listViewElement_DoubleClick(sender, e);
				}
			}
			else if (e.Data.GetDataPresent(DataFormats.FileDrop))
				MainForm_DragDrop(sender, e);
		}

		//void DiagramControl_MouseMove(object sender, MouseEventArgs e)
		//{
			//System.Diagnostics.Trace.WriteLine("toolTipDiagramElement_Popup");
			//Point contextualMenuMousePosition = this.panelDiagram.DiagramControl.PointToClient(MousePosition);
			//contextualMenuMousePosition.Offset(this.panelDiagram.VirtualPoint);
			//DiagramBase resultElement;
			//DiagramBase.HitTestRegion resultRegion;
			//this.diagram.HitTest(contextualMenuMousePosition, out resultElement, out resultRegion);
			//if (resultRegion != DiagramBase.HitTestRegion.None)
			//{
			//    if (resultRegion == DiagramBase.HitTestRegion.Element) // && resultElement.Parent == null)
			//    {
			//        //this.contextualMenuPointedElement = resultElement;
			//        //toolTipDiagramElement.SetToolTip(this.panelDiagram.DiagramControl, "coucou");
			//        e.Cancel = true;
			//        toolTipElement.Show("Coucou", this);
			//    }
			//}
		//}
	}

	public class XSDObject
	{
		private string filename = "";
		private string name = "";
		private string nameSpace = "";
		private string type = "";
		private string fullNameType = "";
		private XMLSchema.openAttrs tag = null;

		public string Filename { get { return this.filename; } set { this.filename = value; } }
		public string Name { get { return this.name; } set { this.name = value; } }
		public string NameSpace { get { return this.nameSpace; } set { this.nameSpace = value; } }
		public string Type { get { return this.type; } set { this.type = value; } }
		public XMLSchema.openAttrs Tag { get { return this.tag; } set { this.tag = value; } }

		public string FullName { get { return this.nameSpace + ':' + this.fullNameType + ':' + this.name; } }

		public XSDObject(string filename, string name, string nameSpace, string type, XMLSchema.openAttrs tag)
		{
			this.filename = filename;
			this.name = name;
			this.nameSpace = (nameSpace == null ? "" : nameSpace);
			this.type = type;
			if (this.type == "simpleType" || this.type == "complexType")
				this.fullNameType = "type";
			else
				this.fullNameType = this.type;
			this.tag = tag;
		}

		public override string ToString()
		{
			return this.type + ": " + this.name + " (" + this.nameSpace + ")";
		}
	}

	public class XSDAttribute
	{
		private string filename = "";
		private string name = "";
		private string nameSpace = "";
		private string type = "";
		private bool isReference = false;
		private string defaultValue = "";
		private string use = "";
		private XMLSchema.attribute tag = null;

		public string Filename { get { return this.filename; } set { this.filename = value; } }
		public string Name { get { return this.name; } set { this.name = value; } }
		public string NameSpace { get { return this.nameSpace; } set { this.nameSpace = value; } }
		public string Type { get { return this.type; } set { this.type = value; } }
		public bool IsReference { get { return this.isReference; } set { this.isReference = value; } }
		public string DefaultValue { get { return this.defaultValue; } set { this.defaultValue = value; } }
		public string Use { get { return this.use; } set { this.use = value; } }
		public XMLSchema.attribute Tag { get { return this.tag; } set { this.tag = value; } }

		public string FullName { get { return this.nameSpace + ":attribute:" + this.name; } }

		public XSDAttribute(string filename, string name, string nameSpace, string type, bool isReference, string defaultValue, string use, XMLSchema.attribute attribute)
		{
			this.filename = filename;
			this.name = name;
			this.nameSpace = (nameSpace == null ? "" : nameSpace);
			this.type = type;
			this.isReference = isReference;
			this.defaultValue = defaultValue;
			this.use = use;
			this.tag = attribute;
		}

		public override string ToString()
		{
			return this.name + " (" + this.nameSpace + ")";
		}
	}

	public class XSDAttributeGroup
	{
		private string filename = "";
		private string name = "";
		private string nameSpace = "";
		private string type = "";
		private bool isReference = false;
		private XMLSchema.attributeGroup tag = null;

		public string Filename { get { return this.filename; } set { this.filename = value; } }
		public string Name { get { return this.name; } set { this.name = value; } }
		public string NameSpace { get { return this.nameSpace; } set { this.nameSpace = value; } }
		public string Type { get { return this.type; } set { this.type = value; } }
		public bool IsReference { get { return this.isReference; } set { this.isReference = value; } }
		public XMLSchema.attributeGroup Tag { get { return this.tag; } set { this.tag = value; } }

		public string FullName { get { return this.nameSpace + ":attributeGroup:" + this.name; } }

		public XSDAttributeGroup(string filename, string name, string nameSpace, string type, bool isReference, XMLSchema.attributeGroup attributeGroup)
		{
			this.filename = filename;
			this.name = name;
			this.nameSpace = (nameSpace == null ? "" : nameSpace);
			this.type = type;
			this.isReference = isReference;
			this.tag = attributeGroup;
		}

		public override string ToString()
		{
			return this.name + " (" + this.nameSpace + ")";
		}
	}
}