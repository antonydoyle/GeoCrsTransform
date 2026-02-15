# Extensibility and future PROJ-backed option

## Stable API

- **ICoordinateTransformer** is the main contract. All transform entry points use it.
- **ICrsCatalog** allows custom or extended CRS sets.
- **CoordinateTransform.CreateManaged()** returns the dependency-free implementation (this library).

## Future: PROJ-backed transformer

v1 is dependency-free and does **not** ship a PROJ-based implementation. The design allows a **separate package** (e.g. `GeoCrsTransform.Proj`) to provide:

- **CoordinateTransform.CreateProjBacked()** (or similar) returning `ICoordinateTransformer` implemented with PROJ/native or managed PROJ bindings.
- The same **Transform(GeoCoordinate/ProjectedCoordinate, CrsId, CrsId, TransformOptions?)** and **TransformResult&lt;T&gt;** contract, so callers can swap implementations without changing usage.

No such package is implemented in v1. This document is a placeholder for that future option.
