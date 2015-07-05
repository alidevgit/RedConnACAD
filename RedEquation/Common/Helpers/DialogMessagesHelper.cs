using System;
using System.Windows;

namespace RedEquation.Common.Helpers
{
    internal static class DialogMessagesHelper
    {
        internal static void ShowNotSinchronizedCurrentAndDrawedObjectMessage()
        {
            MessageBox.Show("At the time drawed not Active Object. If you want manipulate to object throw Drawing, you should to redraw Activ Object.",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

        }

        internal static void ShowNotImplementedYetMessage()
        {
            MessageBox.Show("Not Implemented Yet!");
        }

        internal static void ShowNotAllowedToAddThatTypeOfObjectMessage(String objectAutoCadType, String parentObjectType)
        {
            MessageBox.Show(String.Format("Created object is {0}, that is not allowed to add to {1}", objectAutoCadType, parentObjectType), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        internal static void ShowObjectAlreadyExistInParentObject()
        {
            MessageBox.Show("Object already exist in Active object", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        internal static MessageBoxResult DeleteConfirmationDialog(String message)
        {
            return MessageBox.Show(message, "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        }

        internal static void PolylineToSurfaceMustContainOnlyLines()
        {
            MessageBox.Show("To create surface polyline have to contain only lines", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
