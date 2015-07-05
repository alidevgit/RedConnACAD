using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RedConn;
using RedEquation.AutoCad.Helpers;

namespace RedEquation.Stark.Helpers
{
    internal static class PointHelper
    {
        internal static String AddPointObjectToCurrentObjectByAutoCadDbPoint(DBObject autoCadObject)
        {
            DBPoint autoCadDbPoint = (DBPoint) autoCadObject;
            Point3d autoPoint3D = new Point3d(autoCadDbPoint.Position.X, autoCadDbPoint.Position.Y, autoCadDbPoint.Position.Z);
            String parentObjectId = Main.ProjectExplorerControl.CurrentObject.GetID();
            String newStarkPointObjectId = AddPointObjectToCurrentObjectByAutoCadPoint3D(autoPoint3D, parentObjectId);
            DatabaseHelper.DictionaryOfIdsMatching.Add(autoCadObject.ObjectId, newStarkPointObjectId);
            return newStarkPointObjectId;
        }

        internal static String AddPointObjectToCurrentObjectByAutoCadPoint3D(Point3d point3d, String parentObjectId)
        {
            RemoteObj newStarkPointObject = Main.MainRemoteConnection.CreateObject(parentObjectId, Enums.ObjectType.Point.ToString(), "");
            Main.MainRemoteConnection.CreateParam(newStarkPointObject.ID, "X", "number", point3d.X.ToString(), "", "None", "");
            Main.MainRemoteConnection.CreateParam(newStarkPointObject.ID, "Y", "number", point3d.Y.ToString(), "", "None", "");
            Main.MainRemoteConnection.CreateParam(newStarkPointObject.ID, "Z", "number", point3d.Z.ToString(), "", "None", "");
            return newStarkPointObject.ID;
        }

    }
}
