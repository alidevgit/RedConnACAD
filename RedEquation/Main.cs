using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using RedConn;
using RedEquation.Controls;
using RedEquation.Helpers;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using MessageBox = System.Windows.MessageBox;

namespace RedEquation
{
    public class Main
    {
        private PaletteSet _paletteSet;
        private ProjectExplorerControl _projectExplorerControl;
        private RemoteConn _mainRemoteConnection;
        private static readonly Document CurrentDocument = Application.DocumentManager.MdiActiveDocument;
        private static readonly Database CurrentDatabase = CurrentDocument.Database;
        private Int32 _countToDelete = 0;
        private Boolean _isSubscribedOnAddRemoveObjectEvents = false;

        private String _drawedRedObjectId;
        private readonly Dictionary<ObjectId, String> _dictionaryOfIdsMatching = new Dictionary<ObjectId, String>();

        public Main()
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Module is initializing now, please wait...");
            InitializePalleteSet();
            InitializeConnection();
        }

        private void InitializeConnection()
        {
            _mainRemoteConnection = new RemoteConn(true);
            _mainRemoteConnection.Browser.Explorer.DocumentCompleted += Explorer_DocumentCompleted;
        }

        private void Explorer_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_mainRemoteConnection.Browser.Explorer.ReadyState == WebBrowserReadyState.Complete && IsJsScriptAvailable())
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Module initilized successful");
                _mainRemoteConnection.Browser.Explorer.DocumentCompleted -= Explorer_DocumentCompleted;
                _projectExplorerControl.RemoteConnection = _mainRemoteConnection;
                _projectExplorerControl.RefreshControl();
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
            UnsubscribeAddRemoveEvents();
            ObjectDrawer.DrawGeometriesOfObject(currentObject, _dictionaryOfIdsMatching);
            _drawedRedObjectId = currentObject.GetID();
            SubscribeAddRemoveEvents();

        }

        private void SubscribeAddRemoveEvents()
        {
            CurrentDatabase.ObjectAppended += ObjectAppendedHandler;
            CurrentDatabase.ObjectErased += ObjectErasedHandle;
            _isSubscribedOnAddRemoveObjectEvents = true;
        }

        private void UnsubscribeAddRemoveEvents()
        {
            if (_isSubscribedOnAddRemoveObjectEvents)
            {
                CurrentDatabase.ObjectAppended -= ObjectAppendedHandler;
                CurrentDatabase.ObjectErased -= ObjectErasedHandle;
                _isSubscribedOnAddRemoveObjectEvents = false;
            }
        }
        private void ObjectErasedHandle (object sender, ObjectErasedEventArgs e)
        {
            if (!IsCurrentAndDrawedObjectSynchronized())
            {
                ShowNotSinchronizedCurrentAndDrawedObjectMessage();
                return;
            }
            if (_dictionaryOfIdsMatching.ContainsKey(e.DBObject.ObjectId))
            {
               if (!_projectExplorerControl.DeleteObject(_dictionaryOfIdsMatching[e.DBObject.Id]))
                   DrawGeometriesOfObject(_projectExplorerControl.CurrentObject);
            }
            
        }

        private void ObjectAppendedHandler (object sender, ObjectEventArgs e)
        {
            if (!IsCurrentAndDrawedObjectSynchronized())
            {
                ShowNotSinchronizedCurrentAndDrawedObjectMessage();
                return;
            }
            MessageBox.Show("Not Implemented Yet");
        }

        private void ShowNotSinchronizedCurrentAndDrawedObjectMessage()
        {
            MessageBox.Show("At the time drawed not Active Object. If you want manipulate to object throw Drawing, you should to redraw Activ Object.",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                
        }

        private bool IsCurrentAndDrawedObjectSynchronized()
        {
            return _projectExplorerControl.CurrentObject != null &&
                   _projectExplorerControl.CurrentObject.GetID() == _drawedRedObjectId;
        }


        [CommandMethod("ShowProjectExplorer")]
        public void ShowProjectExplorer()
        {
            _paletteSet.Visible = true;
        }
 
       public void InitializePalleteSet()
        {
            _paletteSet = new PaletteSet("Project Explorer", new Guid("F7FD4571-9923-4031-981F-3B9729FA7E0D"));
            _paletteSet.DockEnabled = (DockSides)((Int32)DockSides.Left + (Int32)DockSides.Right + (Int32)DockSides.Bottom + (Int32)DockSides.Top);
            _projectExplorerControl = new ProjectExplorerControl(this);
            _paletteSet.AddVisual("Project Explorer", _projectExplorerControl);
        }
    }
}
