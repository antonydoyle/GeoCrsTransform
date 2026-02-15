# GeoCRS Transform

A **dependency-free** .NET library for converting coordinates between major **Coordinate Reference Systems (CRSs)**:

- **Geographic CRSs** (latitude/longitude, optional height)
- **Projected CRSs** (eastings/northings in metres, e.g. Web Mercator, British National Grid, UTM)

The library uses **WGS84** as a hub datum, **Helmert 7-parameter** transforms for datum shifts, and pure-managed **projection** code (no native/PROJ dependency).


GitHub - https://github.com/antonydoyle/GeoCrsTransform

## Supported in v1

- Geographic ↔ Geographic (datum shift via Helmert)
- Geographic ↔ Projected (forward/inverse projection)
- Projected ↔ Projected (via geographic hub)
- **CRSs**: WGS84 (4326), ETRS89 (4258), OSGB36 (4277), Web Mercator (3857), British National Grid (27700), UTM zones 30N/31N/33N (32630, 32631, 32633), plus aliases (e.g. `WGS84`, `BNG`, `UTM30N`)

See [docs/scope.md](https://github.com/antonydoyle/GeoCrsTransform/blob/main/docs/scope.md) for full scope and limitations.

## Installation

```bash
dotnet add package GeoCrsTransform
```

## Examples

### WGS84 (4326) ↔ Web Mercator (3857)

```csharp
using GeoCrsTransform;

var transformer = CoordinateTransform.CreateManaged();
var wgs84 = CrsId.Parse("EPSG:4326");
var webMercator = CrsId.Parse("EPSG:3857");

var geo = new GeoCoordinate(51.5074, -0.1278, 0);
var result = transformer.Transform(geo, wgs84, webMercator);
var projected = (ProjectedCoordinate)result.Output;
Console.WriteLine($"E: {projected.EastingMeters}, N: {projected.NorthingMeters}");
Console.WriteLine($"Path: {result.TransformPath}");
```

### WGS84 ↔ British National Grid (27700)

```csharp
var transformer = CoordinateTransform.CreateManaged();
var geo = new GeoCoordinate(51.5, -2.0, 0);
var result = transformer.Transform(geo, CrsId.Parse("EPSG:4326"), CrsId.Parse("EPSG:27700"));
var bng = (ProjectedCoordinate)result.Output;
foreach (var w in result.Warnings) Console.WriteLine($"Warning: {w}");
```

### UTM

```csharp
var transformer = CoordinateTransform.CreateManaged();
var utm30n = CrsId.Parse("EPSG:32630");  // or alias "UTM30N"
var result = transformer.Transform(geo, CrsId.Parse("EPSG:4326"), utm30n);
```

### Projected → Geographic

```csharp
var bngCoord = new ProjectedCoordinate(400000, 200000, 0);
var result = transformer.Transform(bngCoord, CrsId.Parse("EPSG:27700"), CrsId.Parse("EPSG:4326"));
var backGeo = (GeoCoordinate)result.Output;
```

## Accuracy and warnings

- **AccuracyClass** on each result: `High`, `Medium`, `Low`, or `Unknown`.
- **Warnings** list when parameters are approximate or when grid-based shifts (e.g. OSTN for BNG) are not applied.
- Survey-grade transforms that require **grid shifts** (NTv2, etc.) are **not** guaranteed in v1; Helmert-only datum shifts are used. See [docs/scope.md](https://github.com/antonydoyle/GeoCrsTransform/blob/main/docs/scope.md).

## Extending CRS data

CRS definitions are in the embedded `Data/major_crs.json`. The format includes:

- `ellipsoids`: id → `a`, `invF`
- `geographic`: `id`, `name`, `aliases`, `ellipsoid`, `toWgs84` (Helmert: `tx`, `ty`, `tz`, `rx`, `ry`, `rz`, `scalePpm`), `accuracy`, `warnings`
- `projected`: `id`, `name`, `aliases`, `base` (geographic CRS id), `projection` (`WebMercator` or `TransverseMercator`), plus `centralMeridian`, `latitudeOfOrigin`, `scaleFactor`, `falseEasting`, `falseNorthing`

Parameter sources and notes are recorded in [docs/parameter_sources.md](https://github.com/antonydoyle/GeoCrsTransform/blob/main/docs/parameter_sources.md).

## Extensibility

- **CreateManaged()** returns the built-in transformer (this package).
- A future optional package may provide **CreateProjBacked()** for PROJ/grid-based accuracy; the `ICoordinateTransformer` interface is designed to stay stable.

## License

MIT. See [LICENSE](https://github.com/antonydoyle/GeoCrsTransform/blob/main/LICENSE).
