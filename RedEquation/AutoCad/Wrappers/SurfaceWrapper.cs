using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RedConn;
using RedEquation.AutoCad.Helpers;
using RedEquation.Common.Helpers;


namespace RedEquation.AutoCad.Wrappers
{
    internal class SurfaceWrapper : ObjectWrapper
    {
        internal override Entity CreateEntityToDrawing(RemoteObj remoteObject)
        {
            return GetPolilyne(remoteObject);
        }

        private Entity GetPolilyne(RemoteObj remoteObject)
        {
            Int32 pointNumber = 0;
            Polyline polyline = new Polyline();
            var thicknessParameter = remoteObject.GetParam("Thickness");
            polyline.Thickness = thicknessParameter != null ? thicknessParameter.Value.AsNumber() : 0;
            List<Point3d> points = ObjectHelper.CreatePoint3DCollectionFromRemoteObject(remoteObject);
            foreach (var point in points)
            {
                polyline.AddVertexAt(pointNumber, new Point2d(point.X, point.Y), 0, 0, 0);
                pointNumber++;
            }
            polyline.Closed = true;
            return polyline;
        }
    }
}
