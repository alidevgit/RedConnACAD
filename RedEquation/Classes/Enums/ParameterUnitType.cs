using System.ComponentModel;

namespace RedEquation.Classes.Enums
{
    public enum ParameterUnitType
    {
        [Description("None")]
        None,

        [Description("Length (ex. in)")]
        Length,

        [Description("Force (ex. kip)")]
        Force,

        [Description("Angle (ex. rad)")]
        Angle,

        [Description("Temperature (ex. F)")]
        Temperature,

        [Description("Moment (ex. kip-in)")]
        Moment,

        [Description("Stress (ex. kip/in^2)")]
        Stress,

        [Description("Density (ex. kip/in^3)")]
        Density,

        [Description("PerTemperature (ex. 1/F)")]
        PerTemperature,

        [Description("Area (ex. in^2)")]
        Area,

        [Description("Inertia (ex. in^4)")]
        Inertia,

        [Description("Velocity (ex. in/sec)")]
        Velocity,

        [Description("Acceleration (ex. in/sec^2)")]
        Acceleration,

        [Description("ForcePerLength (ex. kip/in)")]
        ForcePerLength,

        [Description("MomentPerLength (ex. kip-in/in)")]
        MomentPerLength,

        [Description("Volume (ex. in^3)")]
        Volume,

        [Description("Curvature (ex. 1/in)")]
        Curvature,

        [Description("Warp (ex. in^6)")]
        Warp
    }
}
