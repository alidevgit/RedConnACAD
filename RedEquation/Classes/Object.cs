using Autodesk.AutoCAD.DatabaseServices;
using RedConn;

namespace RedEquation.Classes
{
    internal abstract class Object
    {
        internal abstract Entity CreateEntityToDrawing(RemoteObj remoteObject);
    }
}
