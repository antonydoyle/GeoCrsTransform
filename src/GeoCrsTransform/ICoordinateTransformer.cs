namespace GeoCrsTransform;

/// <summary>Transforms coordinates between CRSs.</summary>
public interface ICoordinateTransformer
{
    /// <summary>Transform a geographic coordinate from source CRS to destination CRS. Result is GeoCoordinate or ProjectedCoordinate when dest is projected.</summary>
    TransformResult<object> Transform(GeoCoordinate input, CrsId sourceCrs, CrsId destCrs, TransformOptions? options = null);

    /// <summary>Transform a projected coordinate from source CRS to destination CRS. Result is geographic or projected depending on destCrs.</summary>
    TransformResult<object> Transform(ProjectedCoordinate input, CrsId sourceCrs, CrsId destCrs, TransformOptions? options = null);
}
