using Autodesk.AutoCAD.DatabaseServices;
using RedConn;
using RedEquation.AutoCad.Helpers;
using RedEquation.Common.Helpers;

namespace RedEquation.AutoCad.Wrappers
{
    internal class PointWrapper : ObjectWrapper
    {
        internal override Entity CreateEntityToDrawing(RemoteObj remoteObject)
        {
            return GetDbPoint(remoteObject);
        }

        private DBPoint GetDbPoint(RemoteObj remoteObject)
        {
            return new DBPoint(ObjectHelper.CreatePoint3DFromRemoteObjectPoint(remoteObject));
        }
    }
}
