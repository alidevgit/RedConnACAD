using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using RedConn;
using RedEquation.Common.Helpers;
using RedEquation.Stark.Enums;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RedEquation.AutoCad.Helpers
{
    internal static class DatabaseHelper
    {
        internal static Dictionary<ObjectId, String> DictionaryOfIdsMatching = new Dictionary<ObjectId, String>();
        internal static readonly Document CurrentDocument = Application.DocumentManager.MdiActiveDocument;
        internal static readonly Database CurrentDatabase = CurrentDocument.Database;

        private static Boolean _isSubscribedOnRemoveObjectEvent = false;
        private static Boolean _isSubscribedOnAddNewObjectEvent = false;
        private static Boolean _isSubscribedOnEditObjectEvent = false;
        private static String _lastDrawedRedObjectId;


        #region Internal Methods
        internal static void DrawGeometriesOfObject(RemoteObj remoteObject)
        {
            CurrentDocument.Editor.WriteMessage("Objects drawing, please wait...\n");
            UnsubscribeFromRemoveObjectEvent();
            UnsubscribeFromCreateNewObjectEvent();
            UnsubscribeFromEditObjectEvent();
            ObjectDrawer.DrawGeometriesOfObject(remoteObject, DictionaryOfIdsMatching);
            _lastDrawedRedObjectId = remoteObject.GetID();
            SubscribeOnRemoveObjectEvent();
            RefreshMenuItemsStatuses();
            CurrentDocument.Editor.WriteMessage("Objects successfully drawed\n");
        }

        internal static void CreateNewObjectsBySelection(ObjectType starkObjectType)
        {
            var selectedObjectsIds = GetSelectedObjectsIds();
            if (selectedObjectsIds == null) return;
            var selectedAutoCadObjects = AutoCADHelper.MainHelper.GetDbObjectListByObjectIdCollection(selectedObjectsIds);
            foreach (var dbObject in selectedAutoCadObjects)
            {
                TryToAddAutoCadObjectToStarkObjectAsSpecificStarkObjectType(dbObject, starkObjectType);
            }
        }

        #region Database Events Subscribers
        internal static void SubscribeOnRemoveObjectEvent()
        {
            if (!_isSubscribedOnRemoveObjectEvent)
            {
                CurrentDatabase.ObjectErased += ObjectErasedHandle;
                _isSubscribedOnRemoveObjectEvent = true;
                RefreshMenuItemsStatuses();
            }
        }

        internal static void UnsubscribeFromRemoveObjectEvent()
        {
            if (_isSubscribedOnRemoveObjectEvent)
            {
                CurrentDatabase.ObjectErased -= ObjectErasedHandle;
                _isSubscribedOnRemoveObjectEvent = false;
                RefreshMenuItemsStatuses();
            }
        }
        internal static void SubscribeOnEditObjectEvent()
        {
            if (!_isSubscribedOnEditObjectEvent)
            {
                CurrentDatabase.ObjectModified += ObjectModifiedHandle;
                _isSubscribedOnEditObjectEvent = true;
                RefreshMenuItemsStatuses();
            }
        }
        internal static void UnsubscribeFromEditObjectEvent()
        {
            if (_isSubscribedOnEditObjectEvent)
            {
                CurrentDatabase.ObjectModified -= ObjectModifiedHandle;
                _isSubscribedOnEditObjectEvent = false;
                RefreshMenuItemsStatuses();
            }
        }
        internal static void SubscribeOnCreateNewObjectEvent()
        {
            if (!_isSubscribedOnAddNewObjectEvent)
            {
                CurrentDatabase.ObjectAppended += ObjectAppendedHandle;
                _isSubscribedOnAddNewObjectEvent = true;
                RefreshMenuItemsStatuses();
            }
        }
        internal static void UnsubscribeFromCreateNewObjectEvent()
        {
            if (_isSubscribedOnAddNewObjectEvent)
            {
                CurrentDatabase.ObjectAppended -= ObjectAppendedHandle;
                _isSubscribedOnAddNewObjectEvent = false;
                RefreshMenuItemsStatuses();
            }
        }

        private static void RefreshMenuItemsStatuses()
        {
            Main.AutoAddPrimitiveObjectsMenuItem.Checked = _isSubscribedOnAddNewObjectEvent;
            Main.AutoEditObjectsMenuItem.Checked = _isSubscribedOnEditObjectEvent;
            Main.AutoRemoveObjectsMenuItem.Checked = _isSubscribedOnRemoveObjectEvent;
        }

        #endregion
        #endregion

        #region Private Methods
        private static Boolean IsCurrentAndDrawedObjectSynchronized()
        {
            return Main.ProjectExplorerControl.CurrentObject != null && Main.ProjectExplorerControl.CurrentObject.GetID() == _lastDrawedRedObjectId;
        }



        private static ObjectIdCollection GetSelectedObjectsIds()
        {
            var selectionResult = DatabaseHelper.CurrentDocument.Editor.SelectImplied();
            return selectionResult.Status == PromptStatus.OK ? new ObjectIdCollection(selectionResult.Value.GetObjectIds()) : null;

        }
    
        #region Database Events Handlers
        private static void ObjectModifiedHandle(object sender, ObjectEventArgs e)
        {
            DialogMessagesHelper.ShowNotImplementedYetMessage();
        }

        private static Boolean IsCurrentObjectIsPrimitiveObject(String currentObjectAutoCadType)
        {
            return ObjectHelper.DefaultTranslationPrimitiveAutoCadObjectTypeToStarkObjectTypeDictionary.ContainsKey(currentObjectAutoCadType);
        }

        internal static void TryToAddAutoCadObjectToStarkObjectAsSpecificStarkObjectType(DBObject autocadObject, ObjectType starkObjectType)
        {
            if (IsCurrentObjectALreadyExist(autocadObject))
            {
                DialogMessagesHelper.ShowObjectAlreadyExistInParentObject();
                return;
            }

            if (!IsCurrentObjectAllowedToAddToParrentObject(starkObjectType))
            {
                DialogMessagesHelper.ShowNotAllowedToAddThatTypeOfObjectMessage(starkObjectType.ToString(), Main.ProjectExplorerControl.CurrentObject.GetType());
                return;
            }

            switch (autocadObject.GetType().Name)
            {
                case "DBPoint":
                    Stark.Helpers.ObjectHelper.CreateSpecificStarkObjectByAutoCadPoint(starkObjectType, autocadObject);
                    break;
                case "Line":
                    Stark.Helpers.ObjectHelper.CreateSpecificStarkObjectByAutoCadLine(starkObjectType, autocadObject);
                    break;
                case "Circle":
                    Stark.Helpers.ObjectHelper.CreateSpecificStarkObjectByAutoCadCircle(starkObjectType, autocadObject);
                    break;
                case "Polyline":
                    Stark.Helpers.ObjectHelper.CreateSpecificStarkObjectByAutoCadPolyline(starkObjectType, autocadObject);
                    break;
            }
        }

        private static Boolean IsCurrentObjectALreadyExist(DBObject autocadObject)
        {
            return DictionaryOfIdsMatching.ContainsKey(autocadObject.ObjectId);
        }

        private static Boolean IsCurrentObjectAllowedToAddToParrentObject(ObjectType starkObjectType)
        {
            var currentObjectType = Stark.Helpers.ObjectHelper.GetObjectTypeByObjectTypeString(Main.ProjectExplorerControl.CurrentObject.GetType());
            if (currentObjectType == null) return false;
            return Stark.Helpers.ObjectHelper.GetAllowedTypes(currentObjectType.Value).Any(t => t == starkObjectType);
        }

        private static void ObjectAppendedHandle(object sender, ObjectEventArgs e)
        {
            if (!IsCurrentAndDrawedObjectSynchronized())
            {
                DialogMessagesHelper.ShowNotSinchronizedCurrentAndDrawedObjectMessage();
                return;
            }
            String currentObjectAutoCadType = e.DBObject.GetType().Name;
            if (!IsCurrentObjectIsPrimitiveObject(currentObjectAutoCadType)) return;
            TryToAddAutoCadObjectToStarkObjectAsSpecificStarkObjectType(e.DBObject, ObjectHelper.DefaultTranslationPrimitiveAutoCadObjectTypeToStarkObjectTypeDictionary[currentObjectAutoCadType]);
        }

        private static void ObjectErasedHandle(object sender, ObjectErasedEventArgs e)
        {
            if (!IsCurrentAndDrawedObjectSynchronized())
            {
                DialogMessagesHelper.ShowNotSinchronizedCurrentAndDrawedObjectMessage();
                return;
            }
            if (DictionaryOfIdsMatching.ContainsKey(e.DBObject.ObjectId) && DialogMessagesHelper.DeleteConfirmationDialog(String.Format("Are you sure you want to delete {0} object?", Main.ProjectExplorerControl.CurrentObject.GetName())) == MessageBoxResult.Yes)
            {
                Main.MainRemoteConnection.DeleteObject(DictionaryOfIdsMatching[e.DBObject.ObjectId]);
                DictionaryOfIdsMatching.Remove(e.DBObject.ObjectId);
            }
        }
        #endregion
        #endregion

    }
}
