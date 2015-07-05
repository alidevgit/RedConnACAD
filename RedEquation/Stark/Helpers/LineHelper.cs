using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using RedConn;
using RedEquation.AutoCad.Helpers;

namespace RedEquation.Stark.Helpers
{
    internal static class LineHelper
    {
        internal static void AddLineObjectToCurrentObjectByAutoCadLine(DBObject autoCadObject)
        {
            Line autoCadLine = (Line)autoCadObject;
            String parentObjectId = Main.ProjectExplorerControl.CurrentObject.GetID();
            RemoteObj newStarkLineObject = Main.MainRemoteConnection.CreateObject(parentObjectId, Enums.ObjectType.Line.ToString(), "");
            PointHelper.AddPointObjectToCurrentObjectByAutoCadPoint3D(autoCadLine.StartPoint, newStarkLineObject.ID);
            PointHelper.AddPointObjectToCurrentObjectByAutoCadPoint3D(autoCadLine.EndPoint, newStarkLineObject.ID);
            DatabaseHelper.DictionaryOfIdsMatching.Add(autoCadObject.ObjectId, newStarkLineObject.ID);
        }
    }
}
