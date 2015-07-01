using System;
using System.Collections.Generic;
using AutoCADHelper;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RedConn;
using RedEquation.Classes;
using RedEquation.Classes.Enums;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Surface = RedEquation.Classes.Surface;

namespace RedEquation.Helpers
{
    internal static class ObjectDrawer
    {
        private static Document _currentDocument;
        private static Database _currentDatabase;
        internal static void DrawGeometriesOfObject(RemoteObj remoteObject, Dictionary<ObjectId, String> dictionaryOfIdsMatching)
        {
            ClearLayer();
            dictionaryOfIdsMatching.Clear();
            _currentDocument = Application.DocumentManager.MdiActiveDocument;
            _currentDatabase = _currentDocument.Database;

            for (Int32 objectIndex = 0; objectIndex < remoteObject.Objects.Count(); objectIndex++)
            {
                var currentRemoteObject = remoteObject.Objects.Get(objectIndex);
                ObjectType currentObjectType;
                if (Enum.TryParse(currentRemoteObject.GetType(), true, out currentObjectType))
                {
                    var currentRemoteObjectId = currentRemoteObject.GetID();
                    if (String.IsNullOrEmpty(currentRemoteObjectId)) continue;
                    if (currentObjectType == ObjectType.Point)
                    {
                        DrawAndRegisterEntity(new Point().CreateEntityToDrawing(currentRemoteObject), currentRemoteObjectId, dictionaryOfIdsMatching);
                    }
                    else if (currentObjectType == ObjectType.Surface)
                    {
                        DrawAndRegisterEntity(new Surface().CreateEntityToDrawing(currentRemoteObject), currentRemoteObjectId, dictionaryOfIdsMatching);
                    }
                        
                }
            }
        }

        
        private static void ClearLayer()
        {
            MainHelper.DeleteAllObjectsFromLayer("0");
        }

        private static void DrawAndRegisterEntity(Entity entity, String remoteObjectId, Dictionary<ObjectId, String> dictionaryOfIdsMatching)
        {
            using (_currentDocument.LockDocument())
            {
                using (var transaction = _currentDatabase.TransactionManager.StartTransaction())
                {
                    BlockTable blockTable = transaction.GetObject(_currentDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord modelSpaceRecord = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    modelSpaceRecord.AppendEntity(entity);
                    transaction.AddNewlyCreatedDBObject(entity, true);
                    dictionaryOfIdsMatching.Add(entity.ObjectId, remoteObjectId);
                    transaction.Commit();
                }
            }
        }
    }
}
