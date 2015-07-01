using Autodesk.AutoCAD.DatabaseServices;
using RedConn;
using RedEquation.Helpers;

namespace RedEquation.Classes
{
    internal class Point : Object
    {
        internal override Entity CreateEntityToDrawing(RemoteObj remoteObject)
        {
            return GetDbPoint(remoteObject);
        }

        private DBPoint GetDbPoint(RemoteObj remoteObject)
        {
           return new DBPoint(RemoteObjectHelper.CreatePoint3DFromRemoteObjectPoint(remoteObject));
        }
    }
}
