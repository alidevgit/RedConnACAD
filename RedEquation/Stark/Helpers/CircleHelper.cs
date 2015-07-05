using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using RedConn;
using RedEquation.AutoCad.Helpers;

namespace RedEquation.Stark.Helpers
{
    internal class CircleHelper
    {
        internal static void AddCircleObjectToCurrentObjectByAutoCadCircle(DBObject autoCadObject)
        {
            Circle autoCadCircle = (Circle)autoCadObject;
            String parentObjectId = Main.ProjectExplorerControl.CurrentObject.GetID();
            RemoteObj newStarkCircleObject = Main.MainRemoteConnection.CreateObject(parentObjectId, Enums.ObjectType.Circle.ToString(), "");
            Main.MainRemoteConnection.CreateParam(newStarkCircleObject.ID, "Radius", "Expr", autoCadCircle.Radius.ToString(), "", "None", "");
            DatabaseHelper.DictionaryOfIdsMatching.Add(autoCadObject.ObjectId, newStarkCircleObject.ID);
        }
    }
}
