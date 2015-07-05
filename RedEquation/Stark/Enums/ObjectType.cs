using System.ComponentModel;

namespace RedEquation.Stark.Enums
{
    public enum ObjectType
    {
        [Description("Project")]
        Project,

        [Description("Unit")]
        Unit,
        
        [Description("Material")]
        Material,

        [Description("Alignment")]
        Alignment,

        [Description("Straight")]
        Straight,

        [Description("Circular")]
        Circular,

        [Description("Spiral")]
        Spiral,

        [Description("Elevation Point")]
        ElevationPoint,

        [Description("Cross Section")]
        CrossSection,
        
        [Description("Cross Section Segment")]
        CrossSectionSegment,

        [Description("Group")]
        Group,

        [Description("Point")]
        Point,

        [Description("Surface")]
        Surface,

        [Description("Volume")]
        Volume,

        [Description("Line")]
        Line,

        [Description("Circle")]
        Circle,

        [Description("Section")]
        Section,

        [Description("Shape")]
        Shape,

        [Description("Rebar Profile Definition")]
        RebarProfileDefinition,

        [Description("Rebar Line Layout")]
        RebarLineLayout,

        [Description("Rebar Circular Layout")]
        RebarCircularLayout,

        [Description("Tendon Layout")]
        TendonLayout,

        [Description("Repeat")]
        Repeat,

        [Description("Design Code")]
        DesignCode,

        [Description("Design Check")]
        DesignCheck,

        [Description("Design Run")]
        DesignRun
    }
}
