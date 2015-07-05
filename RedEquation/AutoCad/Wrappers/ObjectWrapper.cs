using Autodesk.AutoCAD.DatabaseServices;
using RedConn;

namespace RedEquation.AutoCad.Wrappers
{
    internal abstract class ObjectWrapper
    {
        internal abstract Entity CreateEntityToDrawing(RemoteObj remoteObject);
    }
}