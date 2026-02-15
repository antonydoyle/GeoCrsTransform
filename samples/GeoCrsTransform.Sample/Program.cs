using GeoCrsTransform;

var transformer = CoordinateTransform.CreateManaged();
var wgs84 = CrsId.Parse("EPSG:4326");
var webMercator = CrsId.Parse("EPSG:3857");
var geo = new GeoCoordinate(51.5074, -0.1278, 0);
var result = transformer.Transform(geo, wgs84, webMercator);
var proj = (ProjectedCoordinate)result.Output!;
Console.WriteLine($"WGS84 {geo.LatitudeDeg}, {geo.LongitudeDeg} -> Web Mercator E:{proj.EastingMeters:F0} N:{proj.NorthingMeters:F0}");
Console.WriteLine($"Path: {result.TransformPath}");
