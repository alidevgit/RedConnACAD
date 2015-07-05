using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using RedConn;
using RedEquation;
using RedEquation.AutoCad.Helpers;
using RedEquation.Common.Controls;
using RedEquation.Stark.Enums;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using MenuItem= Autodesk.AutoCAD.Windows.MenuItem;

[assembly: CommandClass(typeof(Main))]
namespace RedEquation
{
    public class Main:IExtensionApplication
    {
        internal static ProjectExplorerControl ProjectExplorerControl;
        internal static MenuItem AutoAddPrimitiveObjectsMenuItem, AutoEditObjectsMenuItem, AutoRemoveObjectsMenuItem;
        internal static RemoteConn MainRemoteConnection;

        private PaletteSet _paletteSet;
        private Int32 _countToDelete = 0;
        private ContextMenuExtension _commonContextMenuExtension, 
                                     _pointContextMenuExtension,
                                     _lineContextMenuExtension,
                                     _circleContextMenuExtension,
                                     _ellipseContextMenuExtension, 
                                     _arcContextMenuExtension,
                                     _polylineContextMenuExtension;
        

        public Main()
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Module is initializing now, please wait...\n");
            InitializePalleteSet();
            AddContextMenus();
            InitializeConnection();
        }

        private void AddContextMenus()
        {
            AddCommonContextMenu();
            AddObjectsContextMenus();
        }

        private void AddObjectsContextMenus()
        {
            MenuItem addToObjectAsNewPoint = new MenuItem("Add To Active Object As New Point");
            addToObjectAsNewPoint.Click += (sender, args) => DatabaseHelper.CreateNewObjectsBySelection(ObjectType.Point);
            MenuItem addToObjectAsNewLine = new MenuItem("Add To Active Object As New Line");
            addToObjectAsNewLine.Click += (sender, args) => DatabaseHelper.CreateNewObjectsBySelection(ObjectType.Line);
            MenuItem addToObjectAsNewSurface = new MenuItem("Add To Active Object As New Surface");
            addToObjectAsNewSurface.Click += (sender, args) => DatabaseHelper.CreateNewObjectsBySelection(ObjectType.Surface);
            MenuItem addToObjectAsNewCircle = new MenuItem("Add To Active Object As New Circle");
            addToObjectAsNewCircle.Click += (sender, args) => DatabaseHelper.CreateNewObjectsBySelection(ObjectType.Circle);
            
            _pointContextMenuExtension = new ContextMenuExtension();
            MenuItem mainItemGroup = new MenuItem("STARK");
            mainItemGroup.MenuItems.Add(addToObjectAsNewPoint);
            _pointContextMenuExtension.MenuItems.Add(mainItemGroup);
            Autodesk.AutoCAD.ApplicationServices.Application.AddObjectContextMenuExtension(RXObject.GetClass(typeof(DBPoint)), _pointContextMenuExtension);

            _lineContextMenuExtension = new ContextMenuExtension();
            mainItemGroup = new MenuItem("STARK");
            mainItemGroup.MenuItems.Add(addToObjectAsNewLine);
            _lineContextMenuExtension.MenuItems.Add(mainItemGroup);
            Autodesk.AutoCAD.ApplicationServices.Application.AddObjectContextMenuExtension(RXObject.GetClass(typeof(Line)), _lineContextMenuExtension);

            _circleContextMenuExtension = new ContextMenuExtension();
            mainItemGroup = new MenuItem("STARK");
            mainItemGroup.MenuItems.Add(addToObjectAsNewCircle);
            _circleContextMenuExtension.MenuItems.Add(mainItemGroup);
            Autodesk.AutoCAD.ApplicationServices.Application.AddObjectContextMenuExtension(RXObject.GetClass(typeof(Circle)), _circleContextMenuExtension);

            _ellipseContextMenuExtension = new ContextMenuExtension();
            mainItemGroup = new MenuItem("STARK");
            mainItemGroup.MenuItems.Add(addToObjectAsNewSurface);
            _ellipseContextMenuExtension.MenuItems.Add(mainItemGroup);
            Autodesk.AutoCAD.ApplicationServices.Application.AddObjectContextMenuExtension(RXObject.GetClass(typeof(Ellipse)), _ellipseContextMenuExtension);

            _arcContextMenuExtension = new ContextMenuExtension();
            mainItemGroup = new MenuItem("STARK");
            mainItemGroup.MenuItems.Add(addToObjectAsNewSurface);
            _arcContextMenuExtension.MenuItems.Add(mainItemGroup);
            Autodesk.AutoCAD.ApplicationServices.Application.AddObjectContextMenuExtension(RXObject.GetClass(typeof(Arc)), _arcContextMenuExtension);

            _polylineContextMenuExtension = new ContextMenuExtension();
            mainItemGroup = new MenuItem("STARK");
            mainItemGroup.MenuItems.Add(addToObjectAsNewSurface);
            mainItemGroup.MenuItems.Add(addToObjectAsNewCircle);
            _polylineContextMenuExtension.MenuItems.Add(mainItemGroup);
            Autodesk.AutoCAD.ApplicationServices.Application.AddObjectContextMenuExtension(RXObject.GetClass(typeof(Polyline)), _polylineContextMenuExtension);

        }

        private void AddCommonContextMenu()
        {
            _commonContextMenuExtension = new ContextMenuExtension {Title = "STARK"};

            MenuItem showProjectExplorerMenuItem = new MenuItem("Show Project Explorer");
            showProjectExplorerMenuItem.Click += showProjectExplorerMenuItem_Click;
            _commonContextMenuExtension.MenuItems.Add(showProjectExplorerMenuItem);
            
            MenuItem monitoringGroupMenuItem = new MenuItem("Monitoring");
            AutoAddPrimitiveObjectsMenuItem = new MenuItem("Auto Add Primitive Object");
            AutoAddPrimitiveObjectsMenuItem.Click += AutoAddPrimitiveObjectsMenuItem_Click;
            AutoEditObjectsMenuItem = new MenuItem("Auto Edit Object");
            AutoEditObjectsMenuItem.Click += AutoEditObjectsMenuItem_Click;
            AutoRemoveObjectsMenuItem = new MenuItem("Auto Remove Object");
            AutoRemoveObjectsMenuItem.Click += AutoRemoveObjectsMenuItem_Click;
            monitoringGroupMenuItem.MenuItems.Add(AutoAddPrimitiveObjectsMenuItem);
            monitoringGroupMenuItem.MenuItems.Add(AutoEditObjectsMenuItem);
            monitoringGroupMenuItem.MenuItems.Add(AutoRemoveObjectsMenuItem);
            _commonContextMenuExtension.MenuItems.Add(monitoringGroupMenuItem);

            Autodesk.AutoCAD.ApplicationServices.Application.AddDefaultContextMenuExtension(_commonContextMenuExtension);
        }

        static void AutoRemoveObjectsMenuItem_Click(object sender, EventArgs e)
        {
            if (AutoRemoveObjectsMenuItem.Checked)
                DatabaseHelper.UnsubscribeFromRemoveObjectEvent();
            else
                DatabaseHelper.SubscribeOnRemoveObjectEvent();
        }

        static void AutoEditObjectsMenuItem_Click(object sender, EventArgs e)
        {
            if (AutoEditObjectsMenuItem.Checked)
                DatabaseHelper.UnsubscribeFromEditObjectEvent();
            else
                DatabaseHelper.SubscribeOnEditObjectEvent();
        }

        static void AutoAddPrimitiveObjectsMenuItem_Click(object sender, EventArgs e)
        {
            if (AutoAddPrimitiveObjectsMenuItem.Checked)
                DatabaseHelper.UnsubscribeFromCreateNewObjectEvent();
            else
                DatabaseHelper.SubscribeOnCreateNewObjectEvent();
        }

        void showProjectExplorerMenuItem_Click(object sender, EventArgs e)
        {
            ShowProjectExplorer();
        }




        public void InitializePalleteSet()
        {
            _paletteSet = new PaletteSet("Project Explorer", new Guid("F7FD4571-9923-4031-981F-3B9729FA7E0D"));
            _paletteSet.DockEnabled = (DockSides)((Int32)DockSides.Left + (Int32)DockSides.Right + (Int32)DockSides.Bottom + (Int32)DockSides.Top);
            ProjectExplorerControl = new ProjectExplorerControl(this);
            _paletteSet.AddVisual("Project Explorer", ProjectExplorerControl);
        }

        private void InitializeConnection()
        {
            MainRemoteConnection = new RemoteConn(true);
            MainRemoteConnection.Browser.Explorer.DocumentCompleted += Explorer_DocumentCompleted;
        }

        private void Explorer_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (MainRemoteConnection.Browser.Explorer.ReadyState == WebBrowserReadyState.Complete && IsJsScriptAvailable())
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Module initilized successful\n");
                MainRemoteConnection.Browser.Explorer.DocumentCompleted -= Explorer_DocumentCompleted;
                ProjectExplorerControl.RemoteConnection = MainRemoteConnection;
                ProjectExplorerControl.RefreshControl();
            }
        }

        private bool IsJsScriptAvailable()
        {
            //TO DO: Create "Test-Connection with JS method"
            _countToDelete++;
            return _countToDelete == 2;
        }

        internal void DrawGeometriesOfObject(RemoteObj currentObject)
        {
            DatabaseHelper.DrawGeometriesOfObject(currentObject);
        }






        #region Commands
        [CommandMethod("ShowProjectExplorer")]
        public void ShowProjectExplorer()
        {
            _paletteSet.Visible = true;
        }

       
        #endregion

        public void Initialize(){}
        public void Terminate()
        {
            RemoveContexMenus();
        }

        private void RemoveContexMenus()
        {
            RemoveCommonContextMenu();
            RemoveObjectsContextMenus();
        }

        private void RemoveObjectsContextMenus()
        {
            if (_pointContextMenuExtension != null)
                Autodesk.AutoCAD.ApplicationServices.Application.RemoveObjectContextMenuExtension(RXObject.GetClass(typeof(DBPoint)), _pointContextMenuExtension);
            if (_lineContextMenuExtension != null)
                Autodesk.AutoCAD.ApplicationServices.Application.RemoveObjectContextMenuExtension(RXObject.GetClass(typeof(Line)), _lineContextMenuExtension);
            if (_circleContextMenuExtension != null)
                Autodesk.AutoCAD.ApplicationServices.Application.RemoveObjectContextMenuExtension(RXObject.GetClass(typeof(Circle)), _circleContextMenuExtension);
            if (_ellipseContextMenuExtension != null)
                Autodesk.AutoCAD.ApplicationServices.Application.RemoveObjectContextMenuExtension(RXObject.GetClass(typeof(Ellipse)), _ellipseContextMenuExtension);
            if (_arcContextMenuExtension != null)
                Autodesk.AutoCAD.ApplicationServices.Application.RemoveObjectContextMenuExtension(RXObject.GetClass(typeof(Arc)), _arcContextMenuExtension);
            if (_polylineContextMenuExtension != null)
                Autodesk.AutoCAD.ApplicationServices.Application.RemoveObjectContextMenuExtension(RXObject.GetClass(typeof(Polyline)), _polylineContextMenuExtension);
        }

        private void RemoveCommonContextMenu()
        {
            if (_commonContextMenuExtension != null)
                Autodesk.AutoCAD.ApplicationServices.Application.RemoveDefaultContextMenuExtension(_commonContextMenuExtension);
        }
    }
}
