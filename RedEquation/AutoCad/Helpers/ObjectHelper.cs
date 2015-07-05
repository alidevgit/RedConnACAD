using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RedConn;
using RedEquation.Stark.Enums;

namespace RedEquation.AutoCad.Helpers
{
    internal static class ObjectHelper
    {
        internal static Point3d CreatePoint3DFromRemoteObjectPoint(RemoteObj remoteObject)
        {
            Double x, y, z;
            var pointXParameter = remoteObject.GetParam("X");
            var pointYParameter = remoteObject.GetParam("Y");
            var pointZParameter = remoteObject.GetParam("Z");
            x = pointXParameter != null ? pointXParameter.Value.AsNumber() : 0;
            y = pointYParameter != null ? pointYParameter.Value.AsNumber() : 0;
            z = pointZParameter != null ? pointZParameter.Value.AsNumber() : 0;
            return new Point3d(x,y,z);
        }

        public static List<Point3d> CreatePoint3DCollectionFromRemoteObject(RemoteObj remoteObject)
        {
            var points = new List<Point3d>();
            for (Int32 i = 0; i < remoteObject.Objects.Count(); i++)
            {
                var currentObject = remoteObject.Objects.Get(i);
                if (currentObject.GetType() == ObjectType.Point.ToString())
                    points.Add(CreatePoint3DFromRemoteObjectPoint(currentObject));
            }
            return points;
        }

        internal static readonly Dictionary<String, ObjectType> DefaultTranslationPrimitiveAutoCadObjectTypeToStarkObjectTypeDictionary = new Dictionary<String, ObjectType>()
        {
            {"DBPoint", ObjectType.Point},
            {"Line", ObjectType.Line},
            {"Circle", ObjectType.Circle}
        };
    }
}
