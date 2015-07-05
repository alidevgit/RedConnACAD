using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using RedConn;
using RedEquation.AutoCad.Helpers;
using RedEquation.Common.Helpers;

namespace RedEquation.Stark.Helpers
{
    internal static class SurfaceHelper
    {
        internal static void AddSurfaceObjectToCurrentObjectByAutoPolyline(DBObject autoCadObject)
        {
            Polyline autoCadPolyline = (Polyline)autoCadObject;
            DBObjectCollection polylineContainsObjectCollection = new DBObjectCollection();
            autoCadPolyline.Explode(polylineContainsObjectCollection);
            if (polylineContainsObjectCollection.Cast<DBObject>().Any(dbObject => dbObject.GetType().Name != "Line"))
            {
                DialogMessagesHelper.PolylineToSurfaceMustContainOnlyLines();
                return;
            }

            String parentObjectId = Main.ProjectExplorerControl.CurrentObject.GetID();
            RemoteObj newStarkSurfaceObject = Main.MainRemoteConnection.CreateObject(parentObjectId, Enums.ObjectType.Surface.ToString(), "");
            foreach (var dbObject in polylineContainsObjectCollection)
            {
                Line line = (Line)dbObject;
                PointHelper.AddPointObjectToCurrentObjectByAutoCadPoint3D(line.StartPoint, newStarkSurfaceObject.ID);
            }

            Main.MainRemoteConnection.CreateParam(newStarkSurfaceObject.ID, "Thickness", "Expr", autoCadPolyline.Thickness.ToString(), "", "None", "");
            DatabaseHelper.DictionaryOfIdsMatching.Add(autoCadObject.ObjectId, newStarkSurfaceObject.ID);
        }
    }
}
